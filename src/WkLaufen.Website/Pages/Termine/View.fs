module Termine.View

open Fable.Helpers.React
open Fable.Helpers.React.Props
open System
open global.Data

let formatTime (beginTime: DateTime) endTime =
  let endTime = endTime |> function | Some x -> x | None -> beginTime
  let showTime = beginTime.TimeOfDay <> TimeSpan.Zero || endTime.TimeOfDay <> TimeSpan.Zero
  let sameTime = beginTime.ToString("dd.MM.yyyy HH:mm") = endTime.ToString("dd.MM.yyyy HH:mm")
  let sameDate = beginTime.ToString("dd.MM.yyyy") = endTime.ToString("dd.MM.yyyy")
  let sameMonth = beginTime.ToString("MM.yyyy") = endTime.ToString("MM.yyyy")
  let sameYear = beginTime.ToString("yyyy") = endTime.ToString("yyyy")
  let dateTimeFormat = sprintf "dd.MM.yyyy%s" (if showTime then " HH:mm" else "")

  if sameTime then beginTime.ToString dateTimeFormat
  elif sameDate then (beginTime.ToString dateTimeFormat) + (if showTime then " - " + endTime.ToString "HH:mm" else "")
  elif sameMonth && not showTime then beginTime.ToString("dd.") + " - " + endTime.ToString("dd.") + beginTime.ToString("MM.yyyy")
  else beginTime.ToString(dateTimeFormat) + " - " + endTime.ToString(dateTimeFormat)

let root =
  Layout.page
    "activities"
    Images.terminkalender_w1000h600
    [
      div [ ClassName "activities rich-text" ] [
        h1 [] [ str "Terminkalender" ]
        div [ ClassName "list" ] [
          table [] [
            tbody [] (
              Activities.items
              |> List.choose (function | NotRecurring (Public p) -> Some p | _ -> None)
              |> List.filter (fun act -> act.Data.BeginTime > DateTime.Today.AddDays -7.)
              |> List.groupBy (fun act -> act.Data.BeginTime.Year)
              |> List.collect (fun (year, entries) ->
                let entryNodes =
                  entries
                  |> List.map (fun entry ->
                    let rowAttributes =
                      match entry.Importance with
                      | Normal -> []
                      | Important -> [ Class "highlight" :> IHTMLProp ]
                    tr rowAttributes [
                      td [] [ str (formatTime entry.Data.BeginTime entry.Data.EndTime) ]
                      td [] [ str entry.Data.Title ]
                      td [] [ str entry.Data.Location ]
                    ]
                  )
                tr [] [ th [ ColSpan 3. ] [ str (string year) ] ] :: entryNodes
              )
            )
          ]
        ]
      ]
    ]
