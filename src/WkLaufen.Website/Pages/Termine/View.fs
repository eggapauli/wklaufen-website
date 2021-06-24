module Termine.View

open Fable.React
open Fable.React.Props
open Fulma
open global.Data
open DataModels

let formatTime (beginTime: ActivityTimestamp) endTime =
    let endTime = endTime |> Option.defaultValue beginTime
    match beginTime, endTime with
    | DateTime beginTime, DateTime endTime
    | Date beginTime, DateTime endTime
    | DateTime beginTime, Date endTime ->
        let sameTime = beginTime.ToString("dd.MM.yyyy HH:mm") = endTime.ToString("dd.MM.yyyy HH:mm")
        let sameDate = beginTime.ToString("dd.MM.yyyy") = endTime.ToString("dd.MM.yyyy")
        let dateTimeFormat = sprintf "dd.MM.yyyy HH:mm"

        if sameTime || sameDate then beginTime.ToString dateTimeFormat
        else sprintf "%s - %s" (beginTime.ToString dateTimeFormat) (endTime.ToString dateTimeFormat)
    | Date beginTime, Date endTime ->
        let endTime = endTime.AddDays -1.
        let sameDate = beginTime.ToString("dd.MM.yyyy") = endTime.ToString("dd.MM.yyyy")
        let sameMonth = beginTime.ToString("MM.yyyy") = endTime.ToString("MM.yyyy")
        let sameYear = beginTime.ToString("yyyy") = endTime.ToString("yyyy")
        let dateTimeFormat = sprintf "dd.MM.yyyy"

        if sameDate then beginTime.ToString dateTimeFormat
        elif sameMonth then sprintf "%s - %s" (beginTime.ToString "dd.") (endTime.ToString "dd.MM.yyyy")
        elif sameYear then sprintf "%s - %s" (beginTime.ToString "dd.MM") (endTime.ToString "dd.MM.yyyy")
        else sprintf "%s - %s" (beginTime.ToString dateTimeFormat) (endTime.ToString dateTimeFormat)

let root =
  Layout.page
    "activities"
    Images.terminkalender_w1000h600
    [
      div [ ClassName "activities rich-text" ] [
        Heading.h1 [ Heading.Is3 ] [ str "Terminkalender" ]
        div [ ClassName "activity-list" ] [
          table [] [
            tbody [] (
              Activities.items
              |> List.groupBy (fun act -> ActivityTimestamp.unwrap(act.BeginTime).Year)
              |> List.sortBy fst
              |> List.collect (fun (year, entries) ->
                let entryNodes =
                  entries
                  |> List.map (fun entry ->
                    let rowAttributes =
                      match entry.Importance with
                      | Normal -> []
                      | Important -> [ Class "highlight" :> IHTMLProp ]
                    tr rowAttributes [
                      td [] [ str (formatTime entry.BeginTime entry.EndTime) ]
                      td [] [ str entry.Title ]
                      td [] [ entry.Location |> Option.defaultValue "" |> str ]
                    ]
                  )
                tr [] [ th [ ColSpan 3 ] [ str (string year) ] ] :: entryNodes
              )
            )
          ]
        ]
      ]
    ]
