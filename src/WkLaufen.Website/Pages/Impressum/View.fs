module Impressum.View

open Fable.Helpers.React
open Fable.Helpers.React.Props
open Generated

let obmann =
  Members.items
  |> Seq.find (fun m -> m.Roles |> Seq.contains "Obmann")

let root =
  Layout.page
    "impressum"
    Images.impressum_w1000h600
    [
      h1 [] [ str "Impressum" ]
      div [Class "top-content"] [
        p [] [
          yield b [] [ str "Für den Inhalt verantwortlich" ]
          yield str ":"
          yield br []
          yield str "Werkskapelle Laufen Gmunden-Engelhof"
          yield br []
          yield str "Engelhofstraße 7-9, 4810 Gmunden"
          yield br []
          yield str "ZVR: 651398436"
          yield br []
          yield sprintf "Obmann: %s %s, " obmann.FirstName obmann.LastName |> str
          yield!
              match obmann.EmailAddresses |> List.tryHead with
              | Some email ->
                [
                  br []
                  span [] (App.Html.obfuscate email)
                ]
              | None -> []
        ]
      ]
      div [Class "bottom-content rich-text"] [
        div [] [
          p [] [
            b [] [ str "Offenlegung gem. § 25 MedG" ]
            str ": Diese Webseite dient dem öffentlichen Auftritt der Werkskapelle Laufen Gmunden-Engelhof (Medieninhaber). Der Zweck dieser Website ist es demnach den Verein und seine Tätigkeiten vorzustellen."
          ]
          p [] [
            b [] [ str "Haftungsausschluss" ]
            str ": Die Inhalte dieser Webseite (Texte, Bilder, Grafiken usw.) sind urheberrechtlich geschützt und dürfen nur mit ausdrücklicher Zustimmung der WK Laufen Gmunden-Engelhof genützt oder weiterverwendet werden. Für Vollständigkeit, Richtigkeit, Verfügbarkeit oder Aktualität der Inhalte wird keine Haftung übernommen. Die WK Laufen Gmunden-Engelhof haftet nicht für den Inhalt externer Links."
          ]
        ]
      ]
    ]
