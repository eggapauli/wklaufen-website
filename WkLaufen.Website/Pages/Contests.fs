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
                        Html.pages.Contests.Results
                        |> Seq.map (fun item ->
                            Div [Class "contest"] -< [
                                H2 [Text item.Title]
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
                                        item.Entries
                                        |> Seq.map (fun entry ->
                                            TR [] -< [
                                                TD [Text (entry.Year.ToString())]
                                                TD [Text entry.Category]
                                                TD [Text (entry.Points.ToString("F2", CultureInfo.GetCultureInfo "de-AT"))]
                                                TD [Text (if entry.Result.IsSome then entry.Result.Value else "")]
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