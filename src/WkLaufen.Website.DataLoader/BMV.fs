module BMV

open DataModels
open FSharp.Data
open OpenQA.Selenium
open OpenQA.Selenium.Chrome
open OpenQA.Selenium.Support.UI
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
        return Personen.Parse responseContent
    }
    let! members = async {
        let url = Uri(baseUrl, "/Personen/ReportMitgliederlistecsv/?page=1&pageSize=1000&filter=~&sort=ZUNAME-asc&Kurzbezeichnung=undefined&Auswahlfilter=null&offen=false&Bereichsfilter=undefined&nurBereich=true")
        let! responseContent = httpClient.GetStringAsync(url) |> Async.AwaitTask
        return
            (Mitgliederuebersicht.Parse responseContent).Rows
            |> Seq.map parseMembershipEntry
            |> Seq.groupBy fst
            |> Seq.map (fun (key, value) -> key, value |> Seq.map snd |> Seq.toList)
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
    return
        persons.Rows
        |> Seq.map (fun row ->
            {
                Member =
                    {
                        BMVId = sprintf "%O" row.M_nr
                        FirstName = row.Vorname
                        LastName = row.Zuname
                        DateOfBirth = tryParseDateTime row.Geb_dat
                        Gender = parseGender row.Anrede
                        City = row.Ort
                        Phones = [ row.Tel_nr; row.Tel_nr1; row.Tel_nr2 ] |> List.choose tryParsePhoneNumber
                        EmailAddresses = [ row.Email1; row.Email2 ] |> List.choose tryParseEmail
                        MemberSince =
                            members
                            |> Map.tryFind (row.Vorname, row.Zuname, tryParseDateTime row.Geb_dat)
                            |> Option.bind (List.choose fst >> function | [] -> None | xs -> Some (List.min xs))
                        Roles =
                            roles
                            |> Map.tryFind (row.Vorname, row.Zuname, tryParseDateTime row.Geb_dat)
                            |> Option.defaultValue []
                        Instruments = []  // TODO
                    }
                Image = None // TODO
            }
        )
}
