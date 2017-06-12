module WkLaufen.Website.Pages.Bmf2017

open WkLaufen.Website
open WebSharper.Html.Server
open WebSharper
open WebSharper.Sitelets

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
                        P [] -< [
                            Text "Es war uns eine Riesenfreude, das Bezirksmusikfest 2017 in Gmunden veranstalten zu dürfen. Unser großer Dank gilt sowohl den fleißigen Helfern, die Tag und Nacht im Einsatz waren, als auch den zahlreichen Besuchern, durch die uns dieses Fest noch lange in Erinnerung bleiben wird. Vielen Dank auch an unsere "
                            A [ HRef "bmf-2017-sponsoren.html" ] -< [ Text "großzügigen Sponsoren" ]
                            Text "."
                        ]
                        P [] -< [
                            Text "Ausgewählte Fotos werden hier in Kürze veröffentlicht."
                        ]
                    ]
                ]
            ]
        }

let BMF2017SponsorListPage ctx =
    Templating.Main ctx EndPoint.BMF2017SponsorList
        {
            Id = "bmf-2017-sponsorlist"
            Title = Html.pages.Bmf2017.Title
            Css = [ "bmf-2017-sponsorlist.css" ]
            BackgroundImageUrl = Html.pages.News.BackgroundImage
            Body =
            [
                Div [Class "rich-text"] -< [
                    Div [Class "image"] -< [
                        Asset.htmlImageNoCrop "bmf" "sponsoren.jpg" (Some 940, Some 480)
                    ]
                ]
            ]
        }
