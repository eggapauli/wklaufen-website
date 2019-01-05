module Kontakte.View

open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fulma
open global.Data

let root =
  Layout.page
    "contacts"
    Images.kontakte_w1000h600
    [
      div [ ClassName "info" ] [
        Heading.h1 [ Heading.Is3 ] [ str "Kontakte" ]
        div [] [
          str "WK Laufen Gmunden-Engelhof"
          br []
          str "Engelhofstraße 7-9"
          br []
          str "4810 Gmunden am Traunsee"
        ]
      ]
      div [ ClassName "contacts" ] (
        [ 31180; 87181 ]
        |> List.map (fun memberId ->
          MemberGroups.getIndexed()
          |> Map.find memberId
          |> App.Html.contact
        )
      )
    ]
