module OOEBV

open System
open System.Globalization
open System.IO
open System.Net.Http
open System.Text
open System.Text.RegularExpressions
open HtmlAgilityPack
open DataModels

// http://stackoverflow.com/a/293357
HtmlNode.ElementsFlags.Remove "option" |> ignore

let private baseUri = Uri "https://mv.ooe-bv.at/"

let private loadHtmlResponse (response: System.Net.Http.HttpResponseMessage) = async {
    let! content = Http.getContentStringFromISO88591 response
    let doc = HtmlDocument()
    doc.LoadHtml content
    return doc
}

/// <summary>
/// Log in with username and password. Returns overview page as HTML document.
/// </summary>
let login (username, password) =
    let getRedirectUrl loginPageContent =
        let redirectUrlMatch = Regex.Match(loginPageContent, @"location.href=""(?<url>[^""]+)""")
        if redirectUrlMatch.Success
        then Uri(baseUri, redirectUrlMatch.Groups.["url"].Value) |> Choice.success 
        else Choice.error "Can't get redirect URL from login page"
    
    let uri = Uri(baseUri, "index_n1.php?slid=%3D%3DwMT0cjN1BgDN0QTMuVGZsVWbuF2Xh1zYul2M0cjN1gDN0QTM")
    let formParams = [
        "b_benutzername", username
        "b_kennwort", password
        "pw_vergessen", ""
        "anmelden", "anmelden"
    ]
    Http.postForm uri formParams
    |> AsyncChoice.bindAsync Http.getContentString
    |> AsyncChoice.bindChoice getRedirectUrl
    |> AsyncChoice.bind Http.get
    |> AsyncChoice.bindAsync loadHtmlResponse
    |> AsyncChoice.mapError (sprintf "Error while logging in: %s")

let loadAndResetMemberOverviewPage (startPage: HtmlDocument) =
    startPage.DocumentNode.SelectNodes("//a[@class=\"menu_0\"]")
    |> Seq.filter (fun n -> n.InnerText.Equals("mitglieder", StringComparison.OrdinalIgnoreCase))
    |> Seq.exactlyOne
    |> fun n -> Uri(baseUri, n.Attributes.["href"].Value)
    |> Http.get
    |> AsyncChoice.bindAsync loadHtmlResponse
    |> AsyncChoice.mapError (sprintf "Error while loading member page: %s")
    |> AsyncChoice.bind (fun memberPage ->
        let relativeUri = memberPage.DocumentNode.SelectSingleNode("//form[@name=\"frm_liste\"]").Attributes.["action"].Value
        let uri = Uri(baseUri, relativeUri)
        let formParams = [
            "order", "mit_name,mit_vorname"
            "loeschen", ""
            "CurrentPage", "1"
            "seite_vor", ""
            "seite_rueck", ""
            "del_id", ""
            "PageSize", "1000"
            "end_suche", "Alle"
            "smit_status", ""
            "smit_name", ""
            "smit_strasse", ""
            "smit_plz", ""
            "smit_ort", ""
            "smit_funktion[]", ""
            "smit_hauptinstrument", ""
            "smit_jugend", ""
        ]
        Http.postForm uri formParams
        |> AsyncChoice.bindAsync loadHtmlResponse
        |> AsyncChoice.mapError (sprintf "Error while resetting member page: %s")
    )

module private MemberParsing =
    let normalizeName (name: string) =
        name
        |> fun n -> n.Replace("&nbsp;", " ")
        |> fun n -> n.Trim()
        |> fun n -> n.ToLowerInvariant()
        |> fun n -> Regex.Replace(n, @"\b(\w)", new MatchEvaluator(fun m -> m.Value.ToUpper()))

    let isMember (status: string) =
        [ "akt"; "mim"; "mip"; "ten" ]
        |> List.exists (fun s -> status.Equals(s, StringComparison.InvariantCultureIgnoreCase))

    let parseDate dateString =
        match dateString with
        | "" -> None
        | x -> DateTime.ParseExact(x, "d", CultureInfo.GetCultureInfo("de-AT")) |> Some

    let parsePhones (phoneString: string) =
        phoneString.Split ';'
        |> List.ofArray
        |> List.filter (String.IsNullOrWhiteSpace >> not)
        |> List.map (fun phoneNumber ->
            phoneNumber.Trim()
            |> fun x -> Regex.Replace(x, @"^(43|0043|\+43)", "0")
            |> fun x -> Regex.Replace(x, @"\D", "")
        )

    type Gender = | Male | Female | Unspecified

    let parseGender text =
        match text with
        | "m\u00E4nnlich" -> Male
        | "weiblich" -> Female
        | _ -> Unspecified

    let genderRole gender role =
        let replacements =
            match gender with
            | Male -> [("/in", ""); ("/obfrau", "")]
            | Female -> [("/in", "in"); ("obmann/", "")]
            | Unspecified -> []

        let folder (s: string) (p: string, r: string) =
            Regex.Replace(s, p, r, RegexOptions.IgnoreCase)

        List.fold folder role replacements

let private rand = Random()

let private tryGetMemberImage memberId =
    Uri(baseUri, sprintf "/core/inc_cms/hole_ajax_div_up.php?thumbnail_aktiv=1&cmyk_aktiv=&big_aktiv=&bereich=mitglieder_bild&bereich_tabelle=mitglieder_bild_archiv&bereich_verzeichniss=mitglieder_bild&a_id=ua&anzeige_form=&rand=%d&del_rec=x&id=%d&sprache=deu" (rand.Next()) memberId)
    |> Http.get
    |> Async.bind (
        Choice.mapAsync loadHtmlResponse
    )
    |> Async.map (Choice.map (fun doc ->
        doc.DocumentNode.SelectNodes("//img[@src]")
        |> Seq.map (fun n -> n.Attributes.["src"].Value)
        |> Seq.filter (fun src -> src.StartsWith("uploads/mitglieder_bild/", StringComparison.InvariantCultureIgnoreCase))
        |> Seq.tryHead
        |> Option.map (fun src -> Uri(baseUri, src.Replace("_small_", "_")))
    ))

let private loadMemberFromDetailsPage memberId isActive (doc: HtmlDocument) = async {
    let getTextBoxValue name =
        let xPath = sprintf "//input[@name=\"%s\"]" name
        doc.DocumentNode.SelectSingleNode(xPath).Attributes.["value"].Value

    let memberSince =
        doc.DocumentNode.Descendants("td")
        |> Seq.filter (fun n -> n.InnerText.Equals("eintrittsdatum", StringComparison.InvariantCultureIgnoreCase))
        |> Seq.exactlyOne
        |> fun n -> n.ParentNode.SelectSingleNode("td[2]").InnerText
        |> MemberParsing.parseDate

    let firstName = getTextBoxValue "mit_vorname" |> MemberParsing.normalizeName
    let lastName = getTextBoxValue "mit_name" |> MemberParsing.normalizeName
            
    let image =
        if isActive
        then
            tryGetMemberImage memberId
            |> Async.map (Choice.mapError (fun _ -> sprintf "Error while getting HTML document for photo for %s %s" firstName lastName))
        else
            async { return Choice1Of2 None }
    return!
        image
        |> Async.map (Choice.map (fun image ->
            let getSelectedOption (comboBox: HtmlNode) =
                comboBox.Descendants("option")
                |> Seq.filter (fun o -> o.Attributes.["selected"] <> null && not <| String.IsNullOrEmpty(o.Attributes.["value"].Value))
                |> Seq.tryHead
                |> Option.map (fun o -> MemberParsing.normalizeName o.InnerText)

            let gender =
                doc.DocumentNode.SelectNodes("//input[@type=\"radio\"][@name=\"mit_geschlecht\"]")
                |> Seq.tryFind(fun n -> n.Attributes.["checked"] <> null)
                |> function
                | Some n when n.Attributes.["value"].Value.Equals("m", StringComparison.InvariantCultureIgnoreCase) -> MemberParsing.Gender.Male
                | Some n when n.Attributes.["value"].Value.Equals("w", StringComparison.InvariantCultureIgnoreCase) -> MemberParsing.Gender.Female
                | _ -> MemberParsing.Gender.Unspecified
            
            let roles =
                doc.DocumentNode.SelectNodes("//select")
                |> Seq.filter (fun n -> n.Id.StartsWith("mf_function_", StringComparison.InvariantCultureIgnoreCase))
                |> Seq.choose (fun cbxRole ->
                    let roleName = getSelectedOption cbxRole

                    let roleEnded =
                        cbxRole.Ancestors("tr")
                        |> Seq.head
                        |> fun p -> p.SelectSingleNode "td[3]/input[@type=\"text\"][@name]"
                        |> fun n -> not <| String.IsNullOrWhiteSpace(n.Attributes.["value"].Value)

                    match roleName, roleEnded with
                    | Some n, false -> n |> MemberParsing.genderRole gender |> Some
                    | _ -> None
                )
                |> Seq.toList

            let instruments =
                let mainInstrument =
                    doc.GetElementbyId("mit_hauptinstrument")
                    |> getSelectedOption
                match mainInstrument with
                | Some x ->
                    let otherInstruments =
                        doc.DocumentNode.Descendants("input")
                        |> Seq.filter (fun n ->
                            n.Attributes.["name"].Value.StartsWith("mit_nebeninstrumente[", StringComparison.InvariantCultureIgnoreCase)
                            && n.Attributes.["checked"] <> null
                        )
                        |> Seq.map (fun n -> n.Attributes.["value"].Value)
                        |> Seq.toList
                    x :: otherInstruments
                | None -> []

            let noneWhenNullOrWhitespace s =
                if String.IsNullOrWhiteSpace s
                then None
                else Some s

            {
                Member =
                    {
                        OoebvId = memberId
                        FirstName = firstName
                        LastName = lastName
                        DateOfBirth = getTextBoxValue "mit_geburtsdatum" |> MemberParsing.parseDate
                        City = getTextBoxValue "mit_ort" |> MemberParsing.normalizeName
                        Phones = [ getTextBoxValue "mit_mobil"; getTextBoxValue "mit_telefon1"; getTextBoxValue "mit_telefon2" ] |> List.collect MemberParsing.parsePhones
                        Email = getTextBoxValue "mit_email" |> noneWhenNullOrWhitespace
                        MemberSince = memberSince
                        Roles = roles
                        Instruments = instruments
                    }
                Image = image
                IsActive = isActive
            }
        ))
}

let private getMemberIdFromOverviewTableRowId (rowId: string) =
    rowId.Replace("dsz_", "") |> int

let private loadOverviewRowAction (row: HtmlNode) actionTitle =
    row.Descendants("a")
    |> Seq.filter(fun a ->
        a.Attributes.["href"] <> null
        && a.Attributes.["title"] <> null
        && a.Attributes.["title"].Value.Equals actionTitle
    )
    |> Seq.exactlyOne
    |> fun a -> Uri(baseUri, a.Attributes.["href"].Value)
    |> Http.get
    |> Async.bind (Choice.mapAsync loadHtmlResponse)

let private loadMemberFromOverviewTableRow (row: HtmlNode) =
    let memberId = getMemberIdFromOverviewTableRowId row.Id

    let fullName = row.SelectSingleNode("td[2]").InnerText |> MemberParsing.normalizeName
    printfn "Getting details for member %s (Id = %d)" fullName memberId

    let memberStatus = row.SelectSingleNode("td[1]").InnerText
    let isActive = MemberParsing.isMember memberStatus

    loadOverviewRowAction row "bearbeiten"
    |> Async.bind (
        Choice.mapError (fun _ -> sprintf "Error while getting details document of member with id %d" memberId)
        >> Choice.bindAsync (loadMemberFromDetailsPage memberId isActive)
        >> Async.perform (fun _ -> printfn "Got details for member %s" fullName)
    )

let private getMemberOverviewRows (memberPage: HtmlDocument) =
    memberPage.DocumentNode.SelectNodes("//table[@id=\"mytable\"]/tr[@id]")

let private loadMembersFromOverviewTableRows rows =
    rows
    |> Seq.map loadMemberFromOverviewTableRow
    |> Async.Parallel
    |> Async.map (Array.toList >> Choice.ofList)

let loadAllMembers memberPage =
    memberPage
    |> getMemberOverviewRows
    |> loadMembersFromOverviewTableRows

let loadActiveMembers memberPage =
    memberPage
    |> getMemberOverviewRows
    |> Seq.filter (fun row ->
        row.SelectSingleNode("td[1]").InnerText
        |> MemberParsing.isMember
    )
    |> loadMembersFromOverviewTableRows

let private imageUrl = sprintf "/core/inc_cms/hole_ajax_div_up.php?thumbnail_aktiv=1&cmyk_aktiv=&big_aktiv=&bereich=mitglieder_bild&bereich_tabelle=mitglieder_bild_archiv&bereich_verzeichniss=mitglieder_bild&a_id=ua&anzeige_form=&b_session=%s&rand=%d&del_rec=%s&id=%d&sprache=deu"
let private getImageIds sessionId memberId =
    let listImagesUrl = Uri(baseUri, imageUrl sessionId (rand.Next()) "x" memberId)
    Http.get listImagesUrl
    |> Async.bind (Choice.mapAsync loadHtmlResponse)
    |> Async.map (Choice.map (fun doc ->
        doc.DocumentNode.SelectNodes("//span[@onclick]")
        |> fun n -> if n = null then Seq.empty else n :> seq<HtmlNode>
        |> Seq.map (fun node -> Regex.Match(node.Attributes.["onclick"].Value, "(?<=javascript:zeige_ajax_mitglieder_bild_deu\(')\d+(?='\);)"))
        |> Seq.filter (fun m -> m.Success)
        |> Seq.map (fun m -> int m.Value)
        |> Seq.toList
    ))

let private deleteImage sessionId memberId imageId =
    let deleteImageUrl = Uri(baseUri, imageUrl sessionId (rand.Next()) (string imageId) memberId)
    Http.postEmpty deleteImageUrl
    |> Async.map (Choice.map ignore)

let private deleteImages sessionId memberId imageIds =
    imageIds
    |> Seq.map (deleteImage sessionId memberId)
    |> Async.ofList
    |> Async.map (Choice.ofList >> Choice.map ignore)

let uploadImage memberId imagePath =
    let str =
        sprintf "ist_menueintnr=126|b_intnr=3911|bj_id=ist_id|ist_id=%d|temp_id=|bj_wert=%d|bereich=mitglieder_bild|sprache=deu|in_intnr=|check_box_org=2|uploaddirectory=../../uploads/mitglieder_bild/" memberId memberId
        |> Encoding.UTF8.GetBytes
        |> Convert.ToBase64String
    let url = Uri(baseUri, sprintf "/core/include/uploadify.php?str=%s" str)
    Http.uploadImageMultipart url imagePath
    |> Async.map (Choice.map ignore)

let replaceMemberImages images memberPage =
    memberPage
    |> getMemberOverviewRows
    |> Seq.choose (fun row ->
        let memberId = getMemberIdFromOverviewTableRowId row.Id
        Map.tryFind memberId images
        |> Option.map (fun imagePath ->
            loadOverviewRowAction row "Bild"
            |> Async.bind (Choice.bindAsync (fun doc ->
                let sessionId =
                    doc.DocumentNode.Descendants("script")
                    |> Seq.map (fun node -> node.InnerText)
                    |> Seq.map (fun text -> Regex.Match(text, "(?<=&b_session=)\w+(?=&)"))
                    |> Seq.filter (fun m -> m.Success)
                    |> Seq.map (fun m -> m.Value)
                    |> Seq.head
                getImageIds sessionId memberId
                |> Async.bind (Choice.bindAsync (deleteImages sessionId memberId))
                |> Async.bind (Choice.bindAsync (fun () -> uploadImage memberId imagePath))
            ))
        )
    )
    |> Async.ofList
    |> Async.map (Choice.ofList >> Choice.map ignore)
