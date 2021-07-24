module BMV

open DataModels
open FSharp.Data
open FSharp.Interop.Excel
open OpenQA.Selenium
open OpenQA.Selenium.Chrome
open OpenQA.Selenium.Support.UI
open Polly
open SixLabors.ImageSharp
open System
open System.Globalization
open System.IO
open System.Net
open System.Net.Http
open System.Text.RegularExpressions

let private baseUrl = Uri("https://bmv.ooe-bv.at")

let private createWebDriver chromeLocation =
    let options = ChromeOptions()
    options.BinaryLocation <- chromeLocation
    let driver = new ChromeDriver(options) :> IWebDriver
    driver.Manage().Timeouts().ImplicitWait <- TimeSpan.FromSeconds(30.)
    driver

let private login (driver: IWebDriver) (userName, password) =
    driver.Navigate().GoToUrl(Uri(baseUrl, "/Account/Login"))
    let userNameElement = driver.FindElement(By.Name "UserName")
    userNameElement.SendKeys(userName)
    let passwordElement = driver.FindElement(By.Name "Password")
    passwordElement.SendKeys(password)

    let wait = WebDriverWait(driver, TimeSpan.FromSeconds(5.))
    wait.Until(fun d -> d.FindElement(By.Name "GoogleCaptchaToken").GetAttribute("value") <> "")|> ignore

    System.Threading.Thread.Sleep(5000)

    driver.FindElement(By.TagName "form").Submit()

    driver.Manage().Cookies.GetCookieNamed("BMVOnline")
    |> Option.ofObj
    |> Option.defaultWith (fun () -> failwith "Session Cookie not found - Login might have failed.")
    |> fun v -> v.Value

let logout (driver: IWebDriver) =
    driver.Navigate().GoToUrl(baseUrl)
    driver.FindElement(By.Id "logoutForm").Submit()

let createLoggedInHttpClient sessionCookie = async {
    let httpClient = Http.createClientWithCookies [
        Cookie("BMVOnline", sessionCookie, "/", baseUrl.Host)
        Cookie("chacheAuswahlfilter", "kein%20Auswahlfilter", "/", baseUrl.Host)
    ]
    do! httpClient.PostAsync(Uri(baseUrl, "/Personen/Read"), new StringContent("sort=&group=&filter=&Personenfilter=&Auswahlfilter=kein+Auswahlfilter")) |> Async.AwaitTask |> Async.Ignore
    return httpClient
}

let runAsLoggedIn chromeLocation credentials fn = async {
    use driver = createWebDriver chromeLocation
    let sessionCookie = login driver credentials
    try
        use! httpClient = createLoggedInHttpClient sessionCookie
        return! fn httpClient
    finally
        logout driver
}

[<Literal>]
let private PersonenSample = __SOURCE_DIRECTORY__ + "\\data\\Personen.csv"
type private Personen = CsvProvider<PersonenSample, Separators=";", Schema="geb_dat=string">

[<Literal>]
let private MitgliederuebersichtSample = __SOURCE_DIRECTORY__ + "\\data\\Mitgliederuebersicht.csv"
type private Mitgliederuebersicht = CsvProvider<MitgliederuebersichtSample, Separators=";", SkipRows=1, Schema="GebDat=string,von=string,bis=string">

[<Literal>]
let private FunktionaereSample = __SOURCE_DIRECTORY__ + "\\data\\Funktionaere.csv"
type private Funktionaere = CsvProvider<FunktionaereSample, Separators=";", Schema="GebDat=string,Funktion von=string,Funktion bis=string">

[<Literal>]
let private MitgliederInstrumenteSample = __SOURCE_DIRECTORY__ + "\\data\\MitgliederInstrumente.csv"
type private MitgliederInstrumente = CsvProvider<MitgliederInstrumenteSample, Separators=";", SkipRows=1, Schema="GebDat=string,von=string,bis=string">

let private parseGender text =
    if String.equalsIgnoreCase text "herr" then Male
    elif String.equalsIgnoreCase text "frau" then Female
    else Unspecified

let private tryParsePhoneNumber text =
    if String.IsNullOrWhiteSpace text then None
    else
        text
        |> fun x -> Regex.Replace(x, @"^(43|0043|\+43)", "0")
        |> fun x -> Regex.Replace(x, @"\D", "")
        |> Some

let private tryParseEmail text =
    if String.IsNullOrWhiteSpace text then None
    else Some text

let private tryParseDateTime (v: string) =
    match DateTime.TryParse(v, CultureInfo.GetCultureInfo("de-AT"), DateTimeStyles.None) with
    | (true, v) -> Some v
    | _ -> None

let private parseMembershipEntry (row: Mitgliederuebersicht.Row) =
    let memberId = (row.Vorname, row.Zuname, tryParseDateTime row.GebDat)
    (memberId, (tryParseDateTime row.Von, tryParseDateTime row.Bis))

let private parseRole v =
    if Regex.IsMatch(v, "^(Obmann|Obfrau)$") then Obmann
    elif Regex.IsMatch(v, "^Kapellmeister(in)?$") then Kapellmeister
    elif Regex.IsMatch(v, "^Jugendorchesterleiter(in)?$") then Jugendorchesterleiter
    elif Regex.IsMatch(v, "^Jugendreferent(in)?$") then Jugendreferent
    else Other v

let private tryParseRoleEntry (row: Funktionaere.Row) =
    if String.equalsIgnoreCase row.Funktion "sonstige Funktion" then None
    else
        let memberId = (row.Vorname, row.Zuname, tryParseDateTime row.GebDat)
        Some (memberId, parseRole row.Funktion)

let private parseInstrumentEntry (row: MitgliederInstrumente.Row) =
    let memberId = (row.Vorname, row.Zuname, tryParseDateTime row.GebDat)
    (memberId, (row.Instrument, tryParseDateTime row.Von, tryParseDateTime row.Bis))

let private httpRetryPolicy =
    Policy
        .Handle<HttpRequestException>(fun e -> e.StatusCode = Nullable HttpStatusCode.InternalServerError)
        .WaitAndRetryAsync(5, fun retryAttempt -> TimeSpan.FromSeconds(2. ** float retryAttempt))

let private retryHttpAction action = async {
    let! result = httpRetryPolicy.ExecuteAndCaptureAsync(fun () -> Async.StartAsTask action) |> Async.AwaitTask
    if result.Outcome = OutcomeType.Successful then return Ok result.Result
    else return Error result.FinalException
}

let private normalizeName (v: string) =
    v
    |> fun v -> v.ToLowerInvariant()
    |> fun v -> Regex.Replace(v, @"\b(\w)", new MatchEvaluator(fun m -> m.Value.ToUpper()))

let getMembers (httpClient: HttpClient) = async {
    let! persons = async {
        printfn "=== Parsing persons"
        let url = Uri(baseUrl, "/Personen/CsvExport/?page=1&pageSize=1000&filter=~&sort=ZUNAME-asc&Auswahlfilter=kein%20Auswahlfilter&kurzbezeichnung=undefined")
        let! responseContent = async {
            match! retryHttpAction (async { return! httpClient.GetStringAsync(url) |> Async.AwaitTask }) with
            | Ok result -> return result
            | Error e -> return raise (Exception("Can't fetch persons", e))
        }
        return Personen.Parse(responseContent).Rows |> Seq.toList
    }
    let! activeMembers = async {
        printfn "=== Parsing active members"
        let url = Uri(baseUrl, "/Personen/ReportMitgliederlistecsv/?page=1&pageSize=1000&filter=~&sort=ZUNAME-asc&Kurzbezeichnung=undefined&Auswahlfilter=kein%20Auswahlfilter&offen=true&Bereichsfilter=undefined&nurBereich=true")
        let! responseContent = async {
            match! retryHttpAction (async { return! httpClient.GetStringAsync(url) |> Async.AwaitTask }) with
            | Ok result -> return result
            | Error e -> return raise (Exception("Can't fetch active members", e))
        }
        return
            (Mitgliederuebersicht.Parse responseContent).Rows
            |> Seq.filter (fun row -> String.equalsIgnoreCase row.Mitgliedsarten "aktives mitglied")
            |> Seq.map parseMembershipEntry
            |> Seq.groupBy fst
            |> Seq.map (fun (key, value) -> key, value |> Seq.map snd |> Seq.toList)
            |> Seq.choose (fun (key, activeMemberEntries) ->
                let isActiveMember =
                    activeMemberEntries
                    |> Seq.exists (fun (fromDate, dueDate) -> Option.isNone dueDate)
                if isActiveMember then
                    let memberSince =
                        activeMemberEntries
                        |> Seq.choose fst
                        |> Seq.min
                    Some (key, memberSince)
                else None
            )
            |> Map.ofSeq
    }
    let activePersons =
        persons
        |> List.filter (fun person -> Map.containsKey (person.Vorname, person.Zuname, tryParseDateTime person.Geb_dat) activeMembers)
    let! roles = async {
        printfn "=== Parsing roles"
        let url = Uri(baseUrl, "/Personen/ReportFunktion%C3%A4rs%C3%BCbersichtCSV/?page=1&pageSize=1000&filter=~&sort=ZUNAME-asc&Kurzbezeichnung=undefined&Auswahlfilter=kein%20Auswahlfilter&offen=true&Bereichsfilter=undefined&nurBereich=true")
        let! responseContent = async {
            match! retryHttpAction (async { return! httpClient.GetStringAsync(url) |> Async.AwaitTask }) with
            | Ok result -> return result
            | Error e -> return raise (Exception("Can't fetch roles", e))
        }
        return
            (Funktionaere.Parse responseContent).Rows
            |> Seq.choose tryParseRoleEntry
            |> Seq.groupBy fst
            |> Seq.map (fun (key, value) -> key, value |> Seq.map snd |> Seq.toList)
            |> Map.ofSeq
    }
    let! memberInstruments = async {
        printfn "=== Parsing instruments"
        let url = Uri("https://bmv.ooe-bv.at/Personen/ReportInstrumente%C3%BCbersichtCSV/?page=1&pageSize=30&filter=~&sort=ZUNAME-asc&Kurzbezeichnung=undefined&Auswahlfilter=null&offen=false&Bereichsfilter=undefined&nurBereich=true")
        let! responseContent = async {
            match! retryHttpAction (async { return! httpClient.GetStringAsync(url) |> Async.AwaitTask }) with
            | Ok result -> return result
            | Error e -> return raise (Exception("Can't fetch instruments", e))
        }
        return
            (MitgliederInstrumente.Parse responseContent).Rows
            |> Seq.map parseInstrumentEntry
            |> Seq.groupBy fst
            |> Seq.map (fun (key, value) -> key, value |> Seq.map snd |> Seq.toList)
            |> Map.ofSeq
    }

    let! images =
        printfn "=== Parsing images"
        activePersons
        |> Seq.map (fun person -> async {
            let url = Uri(baseUrl, sprintf "/Personen/Edit/?id=%O" person.M_nr)
            let! responseContent = async {
                match! retryHttpAction (async { return! httpClient.GetStringAsync(url) |> Async.AwaitTask }) with
                | Ok result -> return result
                | Error e -> return raise (Exception(sprintf "Can't fetch image of %s %s (%O)" person.Zuname person.Vorname person.M_nr, e))
            }
            let m = Regex.Match(responseContent, @"id=""Personenfotoanzeigen""[^>]*>\s*<img src=""data:(?<imageType>[^;]+);base64,(?<content>[^""]*)""")
            if not m.Success then
                failwithf "Can't find photo of %s %s (%O) in %s" person.Zuname person.Vorname person.M_nr responseContent
            let imageType = m.Groups.["imageType"].Value
            if not <| String.equalsIgnoreCase imageType "image/png" then
                failwithf "Unknown image type \"%s\" for %s %s (%O)" imageType person.Zuname person.Vorname person.M_nr
            let imageContent = m.Groups.["content"].Value |> Convert.FromBase64String

            use image = Image.Load(imageContent)
            if image.Width = 128 && image.Height = 128 then return None
            else
                use targetStream = new MemoryStream()
                do! image.SaveAsJpegAsync(targetStream) |> Async.AwaitTask
                return Some (person.M_nr, targetStream.ToArray())
        })
        |> fun x -> Async.Parallel(x, 10)
        |> Async.map (Array.choose id >> Map.ofArray)

    return
        activePersons
        |> List.choose (fun person ->
            match Map.tryFind (person.Vorname, person.Zuname, tryParseDateTime person.Geb_dat) activeMembers with
            | Some memberSince ->
                Some {
                    Member =
                        {
                            BMVId = sprintf "%O" person.M_nr
                            FirstName = normalizeName person.Vorname
                            LastName = normalizeName person.Zuname
                            DateOfBirth = tryParseDateTime person.Geb_dat
                            Gender = parseGender person.Anrede
                            City = person.Ort
                            Phones = [ person.Tel_nr; person.Tel_nr1; person.Tel_nr2 ] |> List.choose tryParsePhoneNumber
                            EmailAddresses = [ person.Email1; person.Email2 ] |> List.choose tryParseEmail
                            MemberSince = Some memberSince
                            Roles =
                                roles
                                |> Map.tryFind (person.Vorname, person.Zuname, tryParseDateTime person.Geb_dat)
                                |> Option.defaultValue []
                            Instruments =
                                memberInstruments
                                |> Map.tryFind (person.Vorname, person.Zuname, tryParseDateTime person.Geb_dat)
                                |> Option.defaultValue []
                                |> List.filter (fun (_, _, ``to``) -> Option.isNone ``to``)
                                |> List.map (fun (instrument, _, _) -> instrument)
                        }
                    ImageContent = Map.tryFind person.M_nr images
                }
            | None -> None
        )
}

[<Literal>]
let private ContestsSample = __SOURCE_DIRECTORY__ + "\\data\\Kapellenehrungen.xlsx"
type private Contests = ExcelFile<ContestsSample>

let getContests (httpClient: HttpClient) clubId = async {
    let tempFilePath = Path.Combine(Path.GetTempPath(), sprintf "%O.xlsx" (Guid.NewGuid()))
    do! async {
        use! sourceStream = httpClient.GetStreamAsync(Uri(baseUrl, sprintf "/Kapelle/ReportKapellenehrungenExcel/?NR=%s" clubId)) |> Async.AwaitTask
        use targetStream = File.Open(tempFilePath, FileMode.Create)
        do! sourceStream.CopyToAsync(targetStream) |> Async.AwaitTask
    }
    return
        Contests(tempFilePath).Data
        |> Seq.choose (fun row ->
            match ContestType.fromString row.Ehrungsbezeichnung with
            | Some contestType ->
                Some {
                    Contest.Year = row.am.Year
                    Type = contestType
                    Category = row.Stufe
                    Points = row.Punkte
                    Result = row.Prädikat
                    Location = row.Ort
                }
            | None -> None
        )
        |> Seq.sortByDescending (fun v -> v.Year)
        |> Seq.toList
}
