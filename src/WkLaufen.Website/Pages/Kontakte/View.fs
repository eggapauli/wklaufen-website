module Kontakte.View

open Fable.Helpers.React
open Fable.Helpers.React.Props
open global.Data

let root =
  Layout.page
    "contacts"
    Images.kontakte_w1000h600
    [
      div [ ClassName "info" ] [
        h1 [] [ str "Kontakte" ]
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
          let m =
           MemberGroups.getIndexed()
            |> Map.find memberId
          div [ ClassName "contact" ] [
            yield div [ ClassName "image" ] (
              Images.contacts
              |> Map.tryFind (string m.OoebvId)
              |> Option.map (fun p ->  App.Html.image p (Some 110, Some 160))
              |> Option.toList
            )
            yield span [] [ str (sprintf "%s %s" m.FirstName m.LastName) ]
            yield br []
            yield span [] [ str (m.Roles |> String.concat ", ") ]
            yield!
              match App.Html.phone m with
              | Some x -> [ br []; x ]
              | None -> [ ]
            yield!
              App.Html.emailAddress m
              |> Option.toList
              |> List.append [ br [] ]
          ]
        )
      )
    ]
