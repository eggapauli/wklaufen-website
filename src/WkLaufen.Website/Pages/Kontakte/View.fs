module Kontakte.View

open Fable.Helpers.React
open Fable.Helpers.React.Props
open Generated

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
        [ 31180; 600 ]
        |> List.map (fun memberId ->
          let m =
            App.Data.Members.getIndexed()
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
            yield br []
            yield span [] (m.Phones |> List.map App.Html.obfuscate |> List.intersperse [ str ", " ] |> List.concat)
            yield!
              match m.EmailAddresses |> List.tryHead with
              | Some email ->
                [
                  br []
                  span [] (App.Html.obfuscate email)
                ]
              | None -> []
          ]
        )
      )
    ]
