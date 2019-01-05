module Wertungen.View

open Fable.Core
open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fable.Core.JsInterop
open Fulma
open DataModels
open global.Data

[<Emit("$0.toLocaleString($1, $2)")>]
let toLocaleString (_number: float) (_culture: string) (options: obj) = jsNative

let root =
  Layout.page
    "contests"
    Images.wertungsergebnisse_w1000h600
    [
      div [Class "rich-text contest-container"] [
        Heading.h1 [ Heading.Is3 ] [ str "Wertungsergebnisse" ]
        div [Class "rich-text-content"] (
          Contests.items
          |> List.sortByDescending (fun m -> m.Year)
          |> List.groupBy (fun m -> m.Type)
          |> List.sortBy fst
          |> List.map (fun (key, list) ->
              div [Class "contest"] [
                  Heading.h2 [ Heading.Is4 ] [ ContestType.toPluralString key |> str ]
                  table [ ] [
                      thead [] [
                          tr [] [
                              th [] [ str "Jahr" ]
                              th [] [ str "Leistungsstufe" ]
                              th [] [ str "Punkteanzahl" ]
                              th [] [ str "Ergebnis" ]
                          ]
                      ]
                      tbody [] (
                          list
                          |> List.take 5
                          |> List.rev
                          |> List.map (fun item ->
                              tr [] [
                                  td [] [ str (sprintf "%d" item.Year) ]
                                  td [] [ str item.Category ]
                                  td [] [ str (toLocaleString item.Points "de-AT" (createObj ["minimumFractionDigits" ==> 2])) ]
                                  td [] [ str item.Result ]
                              ]
                          )
                      )
                  ]
              ]
          )
        )
      ]
    ]
