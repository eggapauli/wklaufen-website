module WkLaufen.Website.Pages.Contests

open System.Globalization
open WkLaufen.Website
open WebSharper.Html.Server

let Page ctx =
    Templating.Main ctx EndPoint.Contests
        {
            Id = "contests"
            Title = Html.pages.Contests.Title
            Css = [ "contests.css" ]
            BackgroundImageUrl = Html.pages.Contests.BackgroundImage
            Body =
            [
                Div [Class "rich-text contest-container"] -< [
                    H1 [Text Html.pages.Contests.Title]
                    Div [Class "rich-text-content"] -< (
                        Data.Contests.getContests()
                        |> Seq.map (fun (title, items) ->
                            Div [Class "contest"] -< [
                                H2 [Text title]
                                Table [] -< [
                                    THead [] -< [
                                        TR [] -< [
                                            TH [Text "Jahr"]
                                            TH [Text "Leistungsstufe"]
                                            TH [Text "Punkteanzahl"]
                                            TH [Text "Ergebnis"]
                                        ]
                                    ]
                                    TBody [] -< (
                                        items
                                        |> Seq.map (fun item ->
                                            TR [] -< [
                                                TD [Text (sprintf "%d" item.Year)]
                                                TD [Text item.Category]
                                                TD [Text (item.Points.ToString("F2", CultureInfo.GetCultureInfo "de-AT"))]
                                                TD [Text (if item.Result.IsSome then item.Result.Value else "")]
                                            ]
                                        )
                                    )
                                ]
                            ]
                        )
                    )
                ]
            ]
        }