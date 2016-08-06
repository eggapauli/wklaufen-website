module WkLaufen.Website.Pages.Impressum

open WkLaufen.Website
open WebSharper.Html.Server

let Page ctx =
    let obmann =
        Html.members
        |> Seq.collect snd
        |> Seq.find (fun m -> m.Roles |> Seq.contains "Obmann")
    Templating.Main ctx EndPoint.Impressum
        {
            Id = "impressum"
            Title = Html.pages.Impressum.Title
            Css = [ "impressum.css" ]
            BackgroundImageUrl = Html.pages.Impressum.BackgroundImage
            Body =
            [
                H1 [Text Html.pages.Impressum.Title]
                Div [Class "top-content"] -< (
                    [
                        VerbatimContent (Html.md.Transform Html.pages.Impressum.TopContent)
                        Text (sprintf "Obmann: %s %s, " obmann.FirstName obmann.LastName)
                    ]
                    @
                    Html.obfuscateEmail obmann.Email
                )
                Div [Class "bottom-content rich-text"] -< [
                    Div [] -< [VerbatimContent (Html.md.Transform Html.pages.Impressum.BottomContent)]
                ]
            ]
        }