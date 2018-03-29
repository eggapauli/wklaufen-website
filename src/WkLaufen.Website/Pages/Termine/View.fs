module Termine.View

open Fable.Core
open Fable.Core.JsInterop
open Fable.Helpers.React
open Fable.Helpers.React.Props
open System
open Global
open Generated

type Activity = {
  Title: string
  BeginTime: DateTime
  EndTime: DateTime option
  Location: string
  CssClass: string option
}

let data =
  [
    {
        Title = "Palmsonntag"
        BeginTime = DateTime(2018, 3, 25, 09, 00, 00)
        EndTime = None
        Location = "Gmunden"
        CssClass = None
    }
    {
        Title = "Festzug 110 Jahre Gamundia"
        BeginTime = DateTime(2018, 4, 21, 18, 00, 00)
        EndTime = None
        Location = "Stadtpfarrkirche"
        CssClass = None
    }
    {
        Title = "Maibaum aufstellen"
        BeginTime = DateTime(2018, 4, 30, 16, 30, 00)
        EndTime = None
        Location = "Schloss Cumberland / Rathausplatz"
        CssClass = None
    }
    {
        Title = "Weckruf"
        BeginTime = DateTime(2018, 5, 1, 00, 00, 00)
        EndTime = None
        Location = "Gmunden"
        CssClass = None
    }
    {
        Title = "Erstkommunion"
        BeginTime = DateTime(2018, 5, 27, 09, 00, 00)
        EndTime = None
        Location = "Stadtpfarrkirche"
        CssClass = None
    }
    {
        Title = "Fronleichnam"
        BeginTime = DateTime(2018, 5, 31, 08, 00, 00)
        EndTime = None
        Location = "Stadtpfarrkirche"
        CssClass = None
    }
    {
        Title = "BMF MV Roitham am Traunfall"
        BeginTime = DateTime(2018, 6, 9, 16, 00, 00)
        EndTime = None
        Location = "Roitham"
        CssClass = None
    }
    {
        Title = "Schlosskonzert"
        BeginTime = DateTime(2018, 6, 20, 19, 30, 00)
        EndTime = None
        Location = "Schloss Ort"
        CssClass = Some "highlight"
    }
    {
        Title = "BMF MV Hofkirchen an der Trattnach"
        BeginTime = DateTime(2018, 6, 30, 14, 00, 00)
        EndTime = None
        Location = "Hofkirchen"
        CssClass = None
    }
    {
        Title = "Festzug zum Rathausplatz"
        BeginTime = DateTime(2018, 8, 15, 17, 30, 00)
        EndTime = None
        Location = "Yachtclub Gmunden"
        CssClass = None
    }
    {
        Title = "Töpfermarkt"
        BeginTime = DateTime(2018, 8, 24, 17, 00, 00)
        EndTime = None
        Location = "Stadtplatz"
        CssClass = None
    }
    {
        Title = "Tag der Tracht"
        BeginTime = DateTime(2018, 9, 09, 10, 00, 00)
        EndTime = None
        Location = ""
        CssClass = None
    }
    {
        Title = "Konzertwertung"
        BeginTime = DateTime(2018, 11, 03, 00, 00, 00)
        EndTime = None
        Location = ""
        CssClass = Some "highlight"
    }
    {
        Title = "Adventkonzert"
        BeginTime = DateTime(2018, 12, 16, 18, 00, 00)
        EndTime = None
        Location = "Kapuzinerkloster"
        CssClass = Some "highlight"
    }
  ]

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
              data
              |> List.filter (fun act -> act.BeginTime > DateTime.Today.AddDays -7.)
              |> List.groupBy (fun act -> act.BeginTime.Year)
              |> List.collect (fun (year, entries) ->
                let entryNodes =
                  entries
                  |> List.map (fun entry ->
                    let rowAttributes =
                      match entry.CssClass with
                      | Some v -> [ Class v :> IHTMLProp ]
                      | None -> []
                    tr rowAttributes [
                      td [] [ str (formatTime entry.BeginTime entry.EndTime) ]
                      td [] [ str entry.Title ]
                      td [] [ str entry.Location ]
                    ]
                  )
                tr [] [ th [ ColSpan 3. ] [ str (string year) ] ] :: entryNodes
              )
            )
          ]
        ]
      ]
    ]
