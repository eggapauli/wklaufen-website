module WkLaufen.Website.Pages.RecorderKids

open WkLaufen.Website
open WebSharper.Html.Server

let Page ctx =
    Templating.Main ctx EndPoint.RecorderKids
        {
            Id = "recorder-kids"
            Title = Html.pages.RecorderKids.Title
            Css = [ "recorder-kids.css" ]
            BackgroundImageUrl = Html.pages.RecorderKids.BackgroundImage
            Body =
            [
                H1 [Text Html.pages.RecorderKids.Title]
                Tags.Object [Class "flyer"; Deprecated.Data ("assets/" + Html.pages.RecorderKids.Flyer); Attr.Type "application/pdf"]
            ]
        }