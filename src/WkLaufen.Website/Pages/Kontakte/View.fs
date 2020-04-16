module Kontakte.View

open global.Data
open DataModels
open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fulma

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
        [ Obmann; Kapellmeister ]
        |> List.map (MemberQuery.firstWithRole >> App.Html.contact)
      )
    ]
