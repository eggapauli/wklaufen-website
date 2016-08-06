module WkLaufen.Website.Pages.Vision2020

open WkLaufen.Website
open WebSharper.Html.Server

let Page ctx =
    Templating.Main ctx EndPoint.Vision2020
        {
            Id = "vision-2020"
            Title = Html.pages.Vision2020.Title
            Css = [ "vision-2020.css" ]
            BackgroundImageUrl = Html.pages.Vision2020.BackgroundImage
            Body =
            [
                H1 [Text Html.pages.Vision2020.Title]
                Div [Class "rich-text"] -< [
                    Div [Class "rich-text-content"] -< [
                        VerbatimContent (Html.md.Transform Html.pages.Vision2020.Content)
                    ]
                ]
            ]
        }