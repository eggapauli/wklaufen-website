module WkLaufen.Website.Pages.Bmf2017

open WkLaufen.Website
open WkLaufen.Bmf2017
open WkLaufen.Bmf2017.Form
open WebSharper.Html.Server
open WebSharper

let BMF2017Overview ctx =
    Templating.Main ctx EndPoint.BMF2017Overview
        {
            Id = "bmf-2017"
            Title = Html.pages.Bmf2017.Title
            Css = [ "bmf-2017.css" ]
            BackgroundImageUrl = Html.pages.Bmf2017.BackgroundImage
            Body =
            [
                Div [ Class "bmf-videos" ] -< (
                    [
                        "https://www.youtube.com/embed/Jfe6rPUB9b4"
                        "https://www.youtube.com/embed/iyM-thn_KvU"
                    ]
                    |> List.map (fun url ->
                        IFrame [
                            Width "300"
                            Height "160"
                            Src url
                            FrameBorder "0"
                            NewAttr "allowfullscreen" "allowfullscreen"
                        ]
                        |> List.singleton
                    )
                    |> List.reduceBack (fun item state -> item @ [ Br [] ] @ state)
                )
                Div [Class "bmf-program rich-text"] -< [
                    Div [Class "text"] -< [
                        B [] -< [ Text "Freitag ab 18.00 Uhr:" ]
                        Br []
                        Text "Marschwertung"
                        Br []
                        Text "JPT - Junge Pongauer Tanzlmusi"
                        Br []
                        Br []
                        B [] -< [ Text "Samstag ab 14.00 Uhr:" ]
                        Br []
                        Text "Marschwertung"
                        Br []
                        Text "Viera Blech"
                        Br []
                        Br []
                        B [] -< [ Text "Sonntag:" ]
                        Br []
                        Text "9.30 Uhr Feldmesse"
                        Br []
                        Text "10.30 Uhr Frühschoppen + Oldtimer-Treffen"
                    ]
                ]
                Div [Class "content rich-text"] -< [
                    Div [Class "text"] -< [
                        H1 [Text Html.pages.Bmf2017.Headline]
                        P [] -< [
                            Text "Wir haben die Ehre & das Vergnügen, das BMF Gmunden 2017 ausrichten zu dürfen."
                            Br []
                            B [Style "font-variant: small-caps"] -< [
                                Text "Musikvereine können sich "
                                A [ HRef "bmf-2017-do-meld-i-mi-on.html" ] -< [ Text "hier" ]
                                Text " anmelden."
                            ]
                            Br []
                            Span [] -< [
                                Text "Nähere Infos zum Fest auf unserem "
                                A [ HRef "bmf-2017-flyer.html" ] -< [ Text "Flyer" ]
                                Text "."
                            ]
                            Br []
                            Span [] -< [
                                Text "Die Veranstaltung eines Bezirksmusikfests funktioniert nur mit finanzieller Unterstützung. Werden Sie "
                                A [ HRef "bmf-2017-do-unterstuetzen-wir-euch.html" ] -< [ Text "Sponsor" ]
                                Text "."
                            ]
                        ]
                    ]
                ]
            ]
        }

let BMF2017Flyer ctx =
    Templating.Main ctx EndPoint.BMF2017Flyer
        {
            Id = "bmf-2017-flyer"
            Title = Html.pages.Bmf2017.Title
            Css = [ "bmf-2017-flyer.css" ]
            BackgroundImageUrl = Html.pages.Bmf2017.BackgroundImage
            Body =
            [
                Div [Class "content rich-text"] -< [
                    Tags.Object [Class "flyer"; Deprecated.Data ("assets/" + Html.pages.Bmf2017.Flyer); Attr.Type "application/pdf"]
                ]
            ]
        }

let BMF2017Musiausflug ctx =
    Templating.Main ctx EndPoint.BMF2017Musiausflug
        {
            Id = "bmf-2017-musiausflug"
            Title = Html.pages.Bmf2017.Title
            Css = [ "bmf-2017-musiausflug.css" ]
            BackgroundImageUrl = Html.pages.Bmf2017.BackgroundImage
            Body =
            [
                Div [Class "content rich-text"] -< [
                    Tags.Object [Class "musiausflug"; Deprecated.Data ("assets/binary/BMF2017-Gmunden-Musiausflug.pdf"); Attr.Type "application/pdf"]
                ]
            ]
        }

module HtmlForm =
    let getCheckboxOrRadioboxInput inputType data nameFn =
        Span [] -< (
            match data.Description with
            | Some description -> [ Span [] -< [Text (sprintf "%s:" description)] ]
            | None -> []
            @
            (
                data.Items
                |> List.map (fun item ->
                    Label [] -< [
                        Input [
                            yield Type inputType
                            yield Name (nameFn data.Name)
                            if item.Checked then yield Checked "checked"
                            yield Value item.Value
                        ]
                        Span [] -< [Text item.Description]
                    ]
                )
            )
        )

    let getCheckboxInput data = getCheckboxOrRadioboxInput "checkbox" data (sprintf "%s[]")
    let getRadioboxInput data = getCheckboxOrRadioboxInput "radio" data id

    let getInput = function
        | TextInput data -> Input [Type "text"; Name data.Name; PlaceHolder data.Description]
        | NumberInputWithPrefixTitle data ->
            Label [] -< [
                Span [Class "title"] -< [Text (sprintf "%s:" data.Description)]
                Input [Type "number"; Name data.Name]
            ]
        | NumberInputWithPostfixTitle data ->
            Label [] -< [
                Input [Type "number"; Name data.Name]
                Span [Class "title"] -< [Text data.Description]
            ]
        | CheckboxInput data -> getCheckboxInput data
        | RadioboxInput data -> getRadioboxInput data
        | TextAreaInput data ->
            TextArea [Name data.Common.Name; Rows data.Rows; Cols data.Cols; PlaceHolder data.Common.Description]

let private contact =
    Table [] -< [
        THead [] -< [
            TR [] -< [
                TH [ColSpan "4"] -< [Text "F\u00fcr Fragen \u0026 weitere Infos stehen wir euch jederzeit zur Verf\u00fcgung:"]
            ]
        ]
        TBody [] -< (
            [
                ( "Ramona Leb", "+43 699 17 252 334", "marketing@wk-laufen.at", "BMF Marketing")
                ( "Mathias Schrabacher", "+43 699 16 601 702", "obmann@wk-laufen.at", "BMF Organisation/Obmann")
            ]
            |> List.map (fun (name, phone, email, role) ->
                TR [] -< [
                    TD [] -< [Text name]
                    TD [] -< (Html.obfuscatePhone phone)
                    TD [] -< (Html.obfuscateEmail (Some email))
                    TD [Class "role"] -< [Text role]
                ]
            )
        )
    ]

let Register ctx =

    let renderSection = function
        | RegistrationForm.Info data ->
            Div [Class "section info"] -< [
                Div [Class "inputs"] -< (
                    data
                    |> List.map (fun row ->
                        Div [Class "row"] -< [ HtmlForm.getInput row ]
                    )
                )
                Div [Class "logo"] -< [
                    Asset.htmlImage "bmf" "logo.png" (None, Some 300)
                ]

                Div [Class "deadline"] -< [
                    Text "Wir bitten um eure verbindliche Anmeldung bis 18. Mai 2017!"
                ]

                Div [Class "clear"]
            ]
        | RegistrationForm.Participation data ->
            Div [Class "section participation"] -< [
                yield H2 [] -< [Text "Marschwertung \u0026 Festakt"]
                yield!
                    data
                    |> List.map (fun (day, participate, participationType) ->
                        Div [Class "day"] -< [
                            yield HtmlForm.getInput participate
                            match participationType with
                            | Some participationType ->
                                yield Br []
                                yield Div [Class (sprintf "show_on_%s" day.Key)] -< [
                                    HtmlForm.getInput participationType
                                ]
                            | None -> ()
                        ]
                    )
                yield Span [Class "hint"] -< [
                    Text "Die Anmeldung für die Marschwertungsteilnahme muss wie üblich ZUSÄTZLICH direkt beim Blasmusikverband Gmunden erfolgen!"
                ]
            ]
        | RegistrationForm.Food data ->
            Div [Class "section food"] -< [
                yield H2 [] -< [Text "Vorbestellung Festzelt"]
                yield!
                    data
                    |> List.map (fun (day, items) ->
                        let foodInput item price =
                            Div [Class "item"] -< [
                                HtmlForm.getInput item
                                Text (sprintf " à %.2f €" price)
                            ]

                        Div [Class (sprintf "show_on_%s" day.Key)] -< (
                            [ H3 [] -< [Text day.Name] ]
                            @
                            (
                                items
                                |> List.map (fun (item, price) ->
                                    foodInput item price
                                )
                            )
                        )
                    )
                yield Span [Class "hint"] -< [
                    Text "Bestellte F\u00e4sser \u0026 Kisten stehen auf dem f\u00fcr euch reservierten Tisch - selbst zapfbar!"
                ]
            ]
        | RegistrationForm.Arrival data ->
            Div [Class "section arrival"] -< [
                H2 [] -< [Text "Anreise"]
                HtmlForm.getInput data
            ]
        | RegistrationForm.ClubInfo data ->
            Div [Class "section club-info"] -< [
                contact
                HtmlForm.getInput data
            ]

    let formAction = "bmf-registration.php"

    Templating.Main ctx EndPoint.BMF2017Register
        {
            Id = "bmf-2017-register"
            Title = "BMF 2017 Anmeldung"
            Css = [ "bmf-2017-register.css" ]
            BackgroundImageUrl = Html.pages.Bmf2017.BackgroundImage
            Body =
            [
                Div [Class "rich-text"] -< [
                    Div [Class "scroll-container"] -< [
                        Form [Attr.Action formAction] -< [
                            yield Div [Class "section"] -< [
                                Span [] -< [
                                    Text "Bezirksmusikfest Gmunden der WK Laufen"
                                    Br []
                                    Text "von 9. bis 11. Juni 2017"
                                ]
                                |> Html.modernHeader "Ja, wir m\u00f6chten uns f\u00fcr das" "anmelden!"
                            ]

                            yield!
                                RegistrationForm.formSections
                                |> List.map renderSection

                            yield Div [Class "send"] -< [
                                Input [Type "submit"; Value "Anmelden"]
                                Div [Class "success hidden"] -< [
                                    Span [] -< [Text "Danke f\u00fcr eure Anmeldung! Wir freuen uns auf euch!"]
                                ]
                                Div [Class "clear"]
                            ]
                        ]
                    ]
                ]
            ]
        }

let Sponsor ctx =
    Templating.Main ctx EndPoint.BMF2017Register
        {
            Id = "bmf-2017-sponsor"
            Title = "BMF 2017 Sponsoring"
            Css = [ "bmf-2017-sponsor.css" ]
            BackgroundImageUrl = Html.pages.Bmf2017.BackgroundImage
            Body =
            [
                Div [Class "content rich-text"] -< [
                    Tags.Object [Class "document"; Deprecated.Data ("assets/" + Html.pages.Bmf2017.SponsorFormular); Attr.Type "application/pdf"]
                ]
            ]
        }