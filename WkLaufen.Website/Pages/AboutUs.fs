module WkLaufen.Website.Pages.AboutUs

open WkLaufen.Website
open WebSharper.Html.Server

let Page ctx =
    Templating.Main ctx EndPoint.AboutUs
        {
            Id = "about-us"
            Title = Html.pages.AboutUs.Title
            Css = [ "about-us.css" ]
            BackgroundImageUrl = Html.pages.AboutUs.BackgroundImage
            Body =
            [
                H1 [Text Html.pages.AboutUs.Title]
                Div [Class "characteristics rich-text"] -< [
                    H2 [Text Html.pages.AboutUs.Characteristics.Title]
                    Div [Class "carousel"] -< (
                        Html.pages.AboutUs.Characteristics.Items
                        |> Seq.map (fun item ->
                            Div [Class "about-us-section"] -< [
                                H3 [Text item.Title]
                                Div [VerbatimContent (Html.md.Transform item.Text)]
                            ]
                        )
                    )
                ]
                Div [Class "samples rich-text"] -< [
                    VerbatimContent (Html.md.Transform Html.pages.AboutUs.Videos)
                ]
            ]
        }