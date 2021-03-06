module BMV

open DataModels
open FSharp.Data
open OpenQA.Selenium
open OpenQA.Selenium.Chrome
open OpenQA.Selenium.Support.UI
open SixLabors.ImageSharp
open System
open System.Globalization
open System.IO
open System.Net.Http
open System.Text.RegularExpressions

let private baseUrl = Uri("https://bmv.ooe-bv.at")

let login (userName, password) =
    let options = ChromeOptions()
    options.BinaryLocation <- @"C:\Program Files\BraveSoftware\Brave-Browser\Application\brave.exe"
    #if !DEBUG
    options.AddArgument("headless")
    #endif
    use driver = new ChromeDriver(options) :> IWebDriver
    driver.Manage().Timeouts().ImplicitWait <- TimeSpan.FromSeconds(30.)

    driver.Navigate().GoToUrl(Uri(baseUrl, "/Account/Login"))
    let userNameElement = driver.FindElement(By.Name "UserName")
    userNameElement.SendKeys(userName)
    let passwordElement = driver.FindElement(By.Name "Password")
    passwordElement.SendKeys(password)

    let wait = WebDriverWait(driver, TimeSpan.FromSeconds(5.))
    wait.Until(fun d -> d.FindElement(By.Name "GoogleCaptchaToken").GetAttribute("value") <> "")|> ignore

    driver.FindElement(By.TagName "form").Submit()

    driver.Manage().Cookies.GetCookieNamed("BMVOnline").Value

let createLoggedInHttpClient sessionCookie = async {
    let httpClient = Http.createClientWithCookies [
        Net.Cookie("BMVOnline", sessionCookie, "/", baseUrl.Host)
        Net.Cookie("chacheAuswahlfilter", "kein%20Auswahlfilter", "/", baseUrl.Host)
    ]
    do! httpClient.PostAsync(Uri(baseUrl, "/Personen/Read"), new StringContent("sort=&group=&filter=&Personenfilter=&Auswahlfilter=kein+Auswahlfilter")) |> Async.AwaitTask |> Async.Ignore
    return httpClient
}

[<Literal>]
let PersonenSample = __SOURCE_DIRECTORY__ + "\\data\\Personen.csv"
type Personen = CsvProvider<PersonenSample, Separators=";", Schema="geb_dat=string">

[<Literal>]
let MitgliederuebersichtSample = __SOURCE_DIRECTORY__ + "\\data\\Mitgliederuebersicht.csv"
type Mitgliederuebersicht = CsvProvider<MitgliederuebersichtSample, Separators=";", SkipRows=1, Schema="GebDat=string,von=string,bis=string">

[<Literal>]
let FunktionaereSample = __SOURCE_DIRECTORY__ + "\\data\\Funktionaere.csv"
type Funktionaere = CsvProvider<FunktionaereSample, Separators=";", Schema="Funktion von=string,Funktion bis=string">

[<Literal>]
let InstrumentenlisteSample = __SOURCE_DIRECTORY__ + "\\data\\Instrumentenliste.json"
type Instrumentenliste = JsonProvider<InstrumentenlisteSample>

[<Literal>]
let InstrumenteSample = __SOURCE_DIRECTORY__ + "\\data\\Instrumente.json"
type Instrumente = JsonProvider<InstrumenteSample>

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
    else
        text
        |> fun x -> Regex.Replace(x, @"^(43|0043|\+43)", "0")
        |> fun x -> Regex.Replace(x, @"\D", "")
        |> Some

let private tryParseDateTime (v: string) =
    match DateTime.TryParse(v, CultureInfo.GetCultureInfo("de-AT"), DateTimeStyles.None) with
    | (true, v) -> Some v
    | _ -> None

let private parseMembershipEntry (row: Mitgliederuebersicht.Row) =
    let memberId = (row.Vorname, row.Zuname, tryParseDateTime row.GebDat)
    (memberId, (tryParseDateTime row.Von, tryParseDateTime row.Bis))

let private parseRole v =
    if String.equalsIgnoreCase v "Obmann" then Obmann
    elif String.equalsIgnoreCase v "Kapellmeister" then Kapellmeister
    elif String.equalsIgnoreCase v "Jugendorchesterleiter" then Jugendorchesterleiter
    elif String.equalsIgnoreCase v "Jugendreferent" then Jugendreferent
    else Other v

let private parseRoleEntry (row: Funktionaere.Row) =
    let memberId = (row.Vorname, row.Zuname, row.GebDat)
    (memberId, parseRole row.Funktion)

let getMembers (httpClient: HttpClient) = async {
    let! persons = async {
        let url = Uri(baseUrl, "/Personen/CsvExport/?page=1&pageSize=1000&filter=~&sort=ZUNAME-asc&Auswahlfilter=kein%20Auswahlfilter&kurzbezeichnung=undefined")
        let! responseContent = httpClient.GetStringAsync(url) |> Async.AwaitTask
        return Personen.Parse(responseContent).Rows |> Seq.toList
    }
    let! activeMembers = async {
        let url = Uri(baseUrl, "/Personen/ReportMitgliederlistecsv/?page=1&pageSize=1000&filter=~&sort=ZUNAME-asc&Kurzbezeichnung=undefined&Auswahlfilter=kein%20Auswahlfilter&offen=true&Bereichsfilter=undefined&nurBereich=true")
        let! responseContent = httpClient.GetStringAsync(url) |> Async.AwaitTask
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
    let! roles = async {
        let url = Uri(baseUrl, "/Personen/ReportFunktion%C3%A4rs%C3%BCbersichtCSV/?page=1&pageSize=1000&filter=~&sort=ZUNAME-asc&Kurzbezeichnung=undefined&Auswahlfilter=kein%20Auswahlfilter&offen=true&Bereichsfilter=undefined&nurBereich=true")
        let! responseContent = httpClient.GetStringAsync(url) |> Async.AwaitTask
        return
            (Funktionaere.Parse responseContent).Rows
            |> Seq.map parseRoleEntry
            |> Seq.groupBy fst
            |> Seq.map (fun (key, value) -> key, value |> Seq.map snd |> Seq.toList)
            |> Map.ofSeq
    }
    let allInstruments =
        Instrumentenliste.GetSamples()
        |> Seq.map (fun row -> row.Id, row.Name)
        |> Map.ofSeq
    let! instruments =
        persons
        |> Seq.map (fun person -> async {
            let url = Uri(baseUrl, "/Personen/GetMSK_INST")
            let content =
                [
                    "sort", ""
                    "page", "1"
                    "pageSize", "50"
                    "group", ""
                    "filter", ""
                    "id", sprintf "%O" person.M_nr
                ]
                |> List.map System.Collections.Generic.KeyValuePair<_, _>
            let! response = httpClient.PostAsync(url, new FormUrlEncodedContent(content)) |> Async.AwaitTask
            response.EnsureSuccessStatusCode() |> ignore
            let! responseContent = response.Content.ReadAsStringAsync() |> Async.AwaitTask
            let instruments =
                Instrumente.Parse(responseContent).Data
                |> Seq.filter (fun row -> row.VereinId = 96 && Option.isNone row.Aktmusb)
                |> Seq.map (fun row ->
                    Map.tryFind row.InstrumentId allInstruments
                    |> Option.defaultWith (fun () -> failwithf "Can't find instrument with id %d" row.InstrumentId)
                )
                |> Seq.toList
            return (person.M_nr, instruments)
        })
        |> Async.Parallel
        |> Async.map Map.ofArray

    let! images =
        persons
        |> Seq.map (fun person -> async {
            let url = Uri(baseUrl, sprintf "/Personen/Edit/?id=%O" person.M_nr)
            let! responseContent = httpClient.GetStringAsync(url) |> Async.AwaitTask
            let m = Regex.Match(responseContent, @"id=""Personenfotoanzeigen""[^>]*>\s*<img src=""data:(?<imageType>[^;]+);base64,(?<content>[^""]*)""")
            if not m.Success then
                failwithf "Can't find photo of %s %s (%O) in %s" person.Zuname person.Vorname person.M_nr responseContent
            let imageType = m.Groups.["imageType"].Value
            if not <| String.equalsIgnoreCase imageType "image/png" then
                failwithf "Unknown image type \"%s\" for %s %s (%O)" imageType person.Zuname person.Vorname person.M_nr
            let imageContent = m.Groups.["content"].Value |> Convert.FromBase64String
            
            let isDummyImage =
                use image = Image.Load(imageContent)
                image.Width = 0 && image.Height = 0
            if isDummyImage then return None
            else return Some (person.M_nr, imageContent)
        })
        |> Async.Parallel
        |> Async.map (Array.choose id >> Map.ofArray)

    return
        persons
        |> List.choose (fun person ->
            match Map.tryFind (person.Vorname, person.Zuname, tryParseDateTime person.Geb_dat) activeMembers with
            | Some memberSince ->
                Some {
                    Member =
                        {
                            BMVId = sprintf "%O" person.M_nr
                            FirstName = person.Vorname
                            LastName = person.Zuname
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
                                instruments
                                |> Map.tryFind person.M_nr
                                |> Option.defaultWith (fun () -> failwithf "Can't find instruments for person %O" person.M_nr)
                        }
                    ImageContent = Map.tryFind person.M_nr images
                }
            | None -> None
        )
}
