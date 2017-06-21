module WkLaufen.Website.Pages.Activities

open System
open WkLaufen.Website
open WebSharper.Html.Server

type Activity = {
    Title: string
    BeginTime: DateTime
    EndTime: DateTime option
    Location: string
}

let data =
    [
        {
            Title = "Jahreskonzert"
            BeginTime = DateTime(2017, 03, 04, 19, 43, 00)
            EndTime = None
            Location = "Toscana"
        }
        {
            Title = "Liebstattsonntag"
            BeginTime = DateTime(2017, 03, 26, 13, 00, 00)
            EndTime = None
            Location = "Rathausplatz"
        }
        {
            Title = "Glockenweihe"
            BeginTime = DateTime(2017, 04, 17, 09, 00, 00)
            EndTime = None
            Location = "Stadtpfarrkirche"
        }
        {
            Title = "Weckruf"
            BeginTime = DateTime(2017, 05, 01, 00, 00, 00)
            EndTime = None
            Location = "Gmunden"
        }
        {
            Title = "Erstkommunion"
            BeginTime = DateTime(2017, 05, 21, 08, 30, 00)
            EndTime = None
            Location = "Stadtpfarrkirche"
        }
        {
            Title = "Bezirksmusikfest"
            BeginTime = DateTime(2017, 06, 09, 00, 00, 00)
            EndTime = DateTime(2017, 06, 11, 00, 00, 00) |> Some
            Location = "Engelhof"
        }
        {
            Title = "Fronleichnam"
            BeginTime = DateTime(2017, 06, 15, 08, 00, 00)
            EndTime = None
            Location = "Stadtpfarrkirche"
        }
        {
            Title = "Festzug Kameradschaftsbund"
            BeginTime = DateTime(2017, 06, 25, 08, 30, 00)
            EndTime = None
            Location = "Altmünster"
        }
        {
            Title = "Schlosskonzert"
            BeginTime = DateTime(2017, 08, 23, 19, 30, 00)
            EndTime = None
            Location = "Seeschloss Ort"
        }
        {
            Title = "Töpfermarkt"
            BeginTime = DateTime(2017, 08, 25, 17, 00, 00)
            EndTime = None
            Location = "Rathausplatz"
        }
        {
            Title = "Tag der Tracht"
            BeginTime = DateTime(2017, 09, 10, 10, 00, 00)
            EndTime = None
            Location = "Kapuzinerkirche"
        }
        {
            Title = "Frühschoppen JVP"
            BeginTime = DateTime(2017, 09, 17, 10, 30, 00)
            EndTime = None
            Location = "Toscanaparkplatz"
        }
        {
            Title = "Heldenehrung"
            BeginTime = DateTime(2017, 10, 29, 10, 00, 00)
            EndTime = None
            Location = "Klosterplatz"
        }
        {
            Title = "Konzertwertung"
            BeginTime = DateTime(2017, 11, 04, 00, 00, 00)
            EndTime = None
            Location = ""
        }
        // {
        //     Title = "Weihnachtsfeier"
        //     BeginTime = DateTime(2017, 12, 16, 19, 30, 00)
        //     EndTime = None
        //     Location = "Engelhof"
        // }
        {
            Title = "Adventkonzert"
            BeginTime = DateTime(2017, 12, 17, 16, 30, 00)
            EndTime = None
            Location = "Kapuzinerkirche"
        }
        {
            Title = "Neujahrsblasen"
            BeginTime = DateTime(2017, 12, 29, 00, 00, 00)
            EndTime = DateTime(2017, 12, 30, 00, 00, 00) |> Some
            Location = "Gmunden"
        }
        {
            Title = "Silvester"
            BeginTime = DateTime(2017, 12, 31, 13, 00, 00)
            EndTime = None
            Location = "Rathausplatz"
        }
    ]

let Page ctx =
        let formatTime (beginTime: DateTime) endTime =
            let endTime = endTime |> function | Some x -> x | None -> beginTime
            let showTime = beginTime.TimeOfDay <> TimeSpan.Zero || endTime.TimeOfDay <> TimeSpan.Zero
            let sameTime = beginTime.ToString("dd.MM.yyyy HH:mm") = endTime.ToString("dd.MM.yyyy HH:mm")
            let sameDate = beginTime.ToString("dd.MM.yyyy") = endTime.ToString("dd.MM.yyyy")
            let sameMonth = beginTime.ToString("MM.yyyy") = endTime.ToString("MM.yyyy")
            let sameYear = beginTime.ToString("yyyy") = endTime.ToString("yyyy")
            let dateTimeFormat = sprintf "dd.MM.yyyy%s" (if showTime then " HH:mm" else "")
            let formattedTime =
                if sameTime then beginTime.ToString dateTimeFormat
                elif sameDate then (beginTime.ToString dateTimeFormat) + (if showTime then " - " + endTime.ToString "HH:mm" else "")
                elif sameMonth && not showTime then beginTime.ToString("dd.") + " - " + endTime.ToString("dd.") + beginTime.ToString("MM.yyyy")
                else beginTime.ToString(dateTimeFormat) + " - " + endTime.ToString(dateTimeFormat)
            formattedTime

        Templating.Main ctx EndPoint.Activities
            {
                Id = "activities"
                Title = Html.pages.Activities.Title
                Css = [ "activities.css" ]
                BackgroundImageUrl = Html.pages.Activities.BackgroundImage
                Body =
                [
                    Div [Class "activities rich-text"] -< [
                        H1 [Text Html.pages.Activities.Title]
                        Div [Class "list"] -< [
                            Table [] -< [
                                TBody [] -< (
                                    data
                                    |> Seq.filter (fun act -> act.BeginTime > DateTime.Today.AddDays(-7.))
                                    |> Seq.groupBy (fun act -> act.BeginTime.Year)
                                    |> Seq.map (fun (year, entries) ->
                                        let entryNodes =
                                            entries
                                            |> Seq.map (fun entry ->
                                                TR [] -< [
                                                    TD [ Text (formatTime entry.BeginTime entry.EndTime)]
                                                    TD [ Text entry.Title]
                                                    TD [ Text entry.Location]
                                                ]
                                            )
                                            |> Seq.toList
                                        TR [] -< (TH [ColSpan "3"] -< [Text (string year)] :: entryNodes)
                                    )
                                )
                            ]
                        ]
                    ]
                ]
            }