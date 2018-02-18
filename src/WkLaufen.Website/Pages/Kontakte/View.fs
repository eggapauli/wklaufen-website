module Kontakte.View

open Fable.Helpers.React
open Fable.Helpers.React.Props

let root =
  Layout.page
    "contacts"
    "kontakte.jpg"
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
            div [ ClassName "image" ] (
              m.Image
              |> Option.map (fun p -> App.Html.image "members" p (Some 110, Some 160))
              |> Option.toList
            )
            span [] [ str (sprintf "%s %s" m.Member.FirstName m.Member.LastName) ]
            br []
            span [] [ str (m.Member.Roles |> String.concat ", ") ]
            br []
            span [] (m.Member.Phones |> String.concat ", " |> App.Html.obfuscatePhone)
            br []
            span [] (App.Html.obfuscateEmail m.Member.Email)
          ]
        )
      )
    ]
