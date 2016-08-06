module WkLaufen.Website.Pages.Activities

open System
open WkLaufen.Website
open WebSharper.Html.Server

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
                                    Data.Activities.getAll()
                                    |> Seq.filter (fun act -> act.IsPublic)
                                    |> Seq.filter (fun act -> act.BeginTime.IsSome)
                                    |> Seq.filter (fun act -> act.BeginTime.Value > DateTime.Today.AddDays(-7.))
                                    |> Seq.groupBy (fun act -> act.BeginTime.Value.Year)
                                    |> Seq.map (fun (year, entries) ->
                                        let entryNodes =
                                            entries
                                            |> Seq.map (fun entry ->
                                                TR [] -< [
                                                    TD [ Text (formatTime entry.BeginTime.Value entry.EndTime)]
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