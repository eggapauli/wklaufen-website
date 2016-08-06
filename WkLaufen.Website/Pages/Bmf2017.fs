module WkLaufen.Website.Pages.Bmf2017

open WkLaufen.Website
open WebSharper.Html.Server

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
    Templating.Main ctx EndPoint.BMF2017Register
        {
            Id = "bmf-2017-register"
            Title = "BMF 2017 Anmeldung"
            Css = [ "bmf-2017-register.css" ]
            BackgroundImageUrl = Html.pages.Bmf2017.BackgroundImage
            Body =
            [
                Div [Class "header"] -< [
                    H1 [Class "prefix"] -< [Text "Ja, wir wollen zum &hellip;"]
                    H1 [Class "main"] -<
                        [
                            Span [Text "Bezirksmusikfest Gmunden der WK Laufen"]
                            Br []
                            Span [Text "von 9. bis 11. Juni 2017"]
                        ]
                    H1 [Class "postfix"] -< [Text "&hellip;auf Musiausflug foan!"]
                ]

                Div [Class "info"] -< [
                    Div [Class "row"] -< [
                        Input [Type "text"; PlaceHolder "Vereinsname"]
                    ]
                    Div [Class "row"] -< [
                        Input [Type "text"; PlaceHolder "Eure Ansprechperson"]
                    ]
                    Div [Class "row"] -< [
                        Span [Class "title"] -< [Text "Anzahl TeilnehmerInnen:"]
                        Input [Type "number"]
                        Span [Class "title2"] -< [Text "davon MusikerInnen:"]
                        Input [Type "number"]
                    ]
                    Div [Class "row"] -< [
                        Input [Type "tel"; PlaceHolder "Telefon"]
                    ]
                    Div [Class "row"] -< [
                        Input [Type "email"; PlaceHolder "E-Mail"]
                    ]
                    Div [Class "row"] -< [
                        Input [Type "text"; PlaceHolder "Adresse"]
                        HR []
                    ]
                    Div [Class "row"] -< [
                        Input [Type "text"; PlaceHolder "PLZ/Ort"]
                    ]
                ]

                Div [Class "logo"] -< [
                    Asset.htmlImage "bmf" "logo.png" (None, None)
                ]

//                Table [] -< [
//                    THead [] -< [
//                        TR [Class "h1"] -< [
//                            TH [ColSpan "3"] -< [Text "Marschwertung &amp; Festakt"]
//                            TH [] -< [Text "Zimmerreservierung"]
//                        ]
//                        TR [Class "h2"] -< [
//                            TH [] -< [Text "Datum"]
//                            TH [] -< [Text "Teilnahme als"]
//                            TH [] -< [Text "Bitte bei &Auml;nderungen Ferienregion benachrichtigen!"]
//                        ]
//                    ]
//                    TBody [] -< [
//                        TR [] -< [
//                            TD [] -< [
//                                Label [] -< [
//                                    Input [Type "checkbox"]
//                                    Span [Text "Freitag, 9. Juni 2017"]
//                                ]
//                            ]
//                            TD [] -< [
//                                Input [Type "radio"; Name "participation_kind_friday"]
//                            ]
//                        ]
//                    ]
//                ]
            ]
        }