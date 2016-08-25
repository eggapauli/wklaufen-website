module WkLaufen.Website.Pages.Bmf2017

open WkLaufen.Website
open WkLaufen.Bmf2017.RegistrationForm
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

let Register ctx =
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

    let renderSection = function
        | Info data ->
            Div [Class "section info"] -< [
                Div [Class "inputs"] -< (
                    data
                    |> List.map (fun row ->
                        Div [Class "row"] -< (row |> List.map getInput)
                    )
                )
                Div [Class "logo"] -< [
                    Asset.htmlImage "bmf" "logo.png" (None, Some 220)
                ]

                Div [Class "clear"]
            ]
        | Participation data ->
            Div [Class "section participation"] -< [
                yield H2 [] -< [Text "Marschwertung \u0026 Festakt"]
                yield!
                    data
                    |> List.map (fun (day, participate, participationType) ->
                        Div [Class "day"] -< [
                            getInput participate
                            Br []
                            Div [Class (sprintf "show_on_%s" day.Key)] -< [
                                getInput participationType
                            ]
                        ]
                    )
            ]
        | Reservations data ->
            Div [Class "section room-reservation"] -< [
                yield H2 [] -< [Text "Zimmerreservierung"]
                yield Div [] -< [
                    Span [Class "hint"] -< [Text "Bitte bei \u00c4nderungen Ferienregion benachrichtigen!"]
                ]
                yield!
                    data
                    |> List.map (fun (day, reservations) ->
                        let rec join separator = function
                            | [] -> []
                            | [ head ] -> [ head ]
                            | head :: tail -> head :: separator :: (join separator tail)

                        Div [Class (sprintf "day show_on_%s" day.Key)] -< [
                            H3 [] -< [Text day.Name]
                            Span [] -< (
                                reservations
                                |> List.map getInput
                                |> join (Br [])
                            )
                        ]
                    )
                yield Div [Class "clear"]
            ]
        | Food data ->
            Div [Class "section food"] -< [
                yield H2 [] -< [Text "Vorbestellung Festzelt:"]
                yield!
                    data
                    |> List.map (fun (day, items) ->
                        let foodInput item price =
                            Div [Class "item"] -< [
                                getInput item
                                Span [Class "price"] -< [Text (sprintf "%d \u20ac" price)]
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
                            @
                            [ Div [Class "clear"] ]
                        )
                    )
                yield Span [Class "hint"] -< [
                    Text "Bestellte F\u00e4sser \u0026 Kisten stehen auf dem f\u00fcr euch reservierten Tisch - selbst zapfbar!"
                ]
            ]
        | SocialPrograms (programs, noProgram) ->
            Div [Class "section social-program"] -< [
                H2 [] -< [Text "Rahmenprogramm - Das w\u00fcrde uns gefallen:"]
                Table [] -< [
                    THead [] -< [
                        TR [] -< [
                            TH [] -< [Text "Sch\u00f6nwetterprogramm"]
                            TH [] -< [Text "Schlechtwettertauglich"]
                        ]
                    ]
                    TBody [] -< (
                        programs
                        |> List.map (fun (niceWeatherProgram, badWeatherProgram) ->
                            let getSocialProgramHtml = function
                            | Some socialProgram -> TD [] -< [ getInput socialProgram ]
                            | None -> TD [] -< [VerbatimContent "&nbsp;"]

                            TR [] -< [
                                getSocialProgramHtml niceWeatherProgram
                                getSocialProgramHtml badWeatherProgram
                            ]
                        )
                    )
                ]
                getInput noProgram
            ]
        | Notes data ->
            Div [Class "section contact"] -< [
                Table [] -< [
                    THead [] -< [
                        TR [] -< [
                            TH [ColSpan "4"] -< [Text "F\u00fcr Fragen \u0026 W\u00fcnsche stehen wir euch jederzeit zur Verf\u00fcgung:"]
                        ]
                    ]
                    TBody [] -< (
                        [
                            ( "Ramona Leb", "0699 17 252 334", "ramonaleb@gmx.at", "BMF Marketing")
                            ( "Mathias Schrabacher", "0699 16 601 702", "obmann@wk-laufen.at", "BMF Organisation/Obmann")
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

                getInput data

                Div [Class "hint"] -< [
                    Text "Aus organisatorischen Gr\u00fcnden sind kurzfristige Stornierungen von \u00dcbernachtungen nach verbindlicher Anmeldung nicht m\u00f6glich."
                ]
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
                            yield Div [Class "section header"] -< [
                                H1 [Class "prefix"] -< [Text "Ja, wir wollen zum \u2026"]
                                H1 [Class "main"] -<
                                    [
                                        Span [Text "Bezirksmusikfest Gmunden der WK Laufen"]
                                        Br []
                                        Span [Text "von 9. bis 11. Juni 2017"]
                                    ]
                                H1 [Class "postfix"] -< [Text "\u2026auf Musiausflug foan!"]
                            ]

                            yield!
                                formSections
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