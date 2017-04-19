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
                Div [Class "content rich-text"] -< [
                    Div [Class "text"] -< [
                        H1 [Text Html.pages.Bmf2017.Headline]
                        VerbatimContent (Html.md.Transform Html.pages.Bmf2017.Content)
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
                    Text "Wir bitten um eure verbindliche Anmeldung bis 3. Mai 2017!"
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
                                Text (sprintf "à %.2f €" price)
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
    let renderSection = function
    | SponsorForm.Info data ->
        Div [Class "info"] -< [
            yield!
                data
                |> List.map (fun input ->
                    Div[] -< [ HtmlForm.getInput input ]
                )
        ]
    | SponsorForm.Package data ->
        let packages = [
            (true, false, false, false, Text "Logo auf Orderman-Kassazettel, die bei jedem Getr\u00e4nke- oder Essenskauf ausgegeben werden")
            (true, false, false, false, Span [] -< [Text "Auf Wunsch benennen wir eine Bar nach Ihnen!"; Br []; Text "z.B.: DRIMAS-Bierbar, Schrabacher-Metallwerkst\u00e4tte-Kuchenbar\u2026"])
            (true, false, false, false, Text "Die Tischreservierung eines Vereins enth\u00e4lt Logo \u0026 \"Wir w\u00fcnschen dem Musikverein X ein fantastischen Bezirksmusikfest \u0026 viel Erfolg bei der Marschwertung!\"")
            (true, false, false, false, Text "Logo auf Transparente. Diese h\u00e4ngen im Raum Gmunden. Aus Brandschutzgr\u00fcnden d\u00fcrfen keine Transparente im Zelt h\u00e4ngen")
            (true, true, false, false, Text "Logo auf Bezirksmusikfest-Leiberl")
            (true, true, false, false, Text "Logo auf Preislisten \u0026 Speisekarten im Zelt")
            (true, true, false, false, Text "Sie werden als Sponsor auf der Facebook-Veranstaltung gelistet")
            (true, true, true, false, Text "Logo auf Plakate")
            (true, true, true, false, Text "Logo auf Flyer")
            (true, true, true, true, Text "3 Tage Logo in der PowerPoint-Pr\u00e4sentation, die via Beamer durchgehend im Festzelt l\u00e4uft (50 \u20ac pro Tag)")
        ]
        Div [Class "package"] -< [
            Div [Class "choice"] -< [
                HtmlForm.getInput data
            ]
            Table [Class "packagelist"] -< [
                THead [] -< [
                    TR [] -< [
                        TD [] -< [
                            B [
                                yield Text "Bitte senden Sie Ihr Logo an "
                                yield! Html.obfuscateEmail (Some "obmann@wk-laufen.at")
                            ]
                        ]
                        TD [Class "bronze"] -< [ Text "Bronze" ]
                        TD [Class "silver"] -< [ Text "Silber" ]
                        TD [Class "gold"] -< [ Text "Gold" ]
                        TD [Class "platinum"] -< [ Text "Platin" ]
                    ]
                ]
                TBody [] -< [
                    yield!
                        packages
                        |> List.map (fun (isPlatinum, isGold, isSilver, isBronze, content) ->
                            TR [] -< [
                                TD [] -< [ content ]
                                TD [Class "tick"] -< [ if isBronze then yield Text "\u2714" ]
                                TD [Class "tick"] -< [ if isSilver then yield Text "\u2714" ]
                                TD [Class "tick"] -< [ if isGold then yield Text "\u2714" ]
                                TD [Class "tick"] -< [ if isPlatinum then yield Text "\u2714" ]
                            ]
                        )
                ]
            ]
        ]
    | SponsorForm.Notes data ->
        Div [Class "contact"] -< [
            contact
            HtmlForm.getInput data
        ]

    let formAction = "bmf-sponsor.php"
    Templating.Main ctx EndPoint.BMF2017Register
        {
            Id = "bmf-2017-sponsor"
            Title = "BMF 2017 Sponsoring"
            Css = [ "bmf-2017-sponsor.css" ]
            BackgroundImageUrl = Html.pages.Bmf2017.BackgroundImage
            Body =
            [
                Div [Class "rich-text"] -< [
                    Div [Class "scroll-container"] -< [
                        Form [Attr.Action formAction] -< [
                            yield Span [] -< [
                                Text "Bezirksmusikfest Gmunden der WK Laufen"
                                Br []
                                Text "von 9. bis 11. Juni 2017"
                            ]
                            |> Html.modernHeader "Ja, wir wollen das" "unterst\u00fctzen!"

                            yield Div [Class "logo"] -< [
                                Asset.htmlImage "bmf" "logo.png" (None, Some 300)
                            ]

                            yield!
                                SponsorForm.formSections
                                |> List.map renderSection

                            yield Text "Durch Ihre Unterst\u00fctzung unserer Veranstaltung f\u00f6rdern Sie nicht nur die WK Laufen Gmunden-Engelhof und ihre Jugendarbeit, sondern auch die Traditionen der heimischen Blasmusik \u0026 somit \u00f6sterreichisches Kulturgut."

                            yield Div [Class "send"] -< [
                                Input [Type "submit"; Value "Abschicken"]
                                Div [Class "success hidden"] -< [
                                    Span [] -< [Text "Danke f\u00fcr Ihre Unterst\u00fctzung!"]
                                ]
                                Div [Class "clear"]
                            ]
                        ]
                    ]
                ]
            ]
        }