module Jugend.View

open Fable.Helpers.React
open Fable.Helpers.React.Props
open Generated

let root =
  Layout.page
    "youths"
    Images.jugendreferat_w1000h600
    [
        div [Class "rich-text text"] [
            h1 [] [ str "Jugendreferat" ]
            p [] [ str "Wie man uns ansieht, wird Jugendarbeit bei uns groß geschrieben! Bereits im Kindergartenalter können Kinder bei uns spielerisch Blockflöte erlernen. Hat man sich dann für ein Instrument entschieden, beginnt das gemeinsame Musizieren im Jugendorchester. Durch frühes, gemeinsames Proben und Auftritte (Adventkonzert, Jahreskonzert, Sommerfest…) ist nicht nur die musikalische Weiterentwicklung gewährleistet, sondern es entwickeln sich auch Freundschaften für's Leben. Kein Wunder, denn in der Vereinsjugend unternehmen wir viel gemeinsam (Malwettbewerb, Spielenachmittage, Ausflüge etc.). Interessierte können sich jederzeit beim Jugendreferat der Werkskapelle Laufen informieren."]
        ]

        div [Class "contacts-container"] [
            div [Class "rich-text contacts"] (
                [
                    39627
                    600
                    31145
                    49155
                ]
                |> List.map (fun memberId ->
                    let m =
                        App.Data.Members.getIndexed()
                        |> Map.find memberId
                    div [Class "contact"] [
                        yield strong [] [ sprintf "%s %s" m.FirstName m.LastName |> str ]
                        yield m.Roles |> String.concat ", " |> sprintf " (%s)" |> str
                        yield!
                            match App.Html.phone m, App.Html.emailAddress m with
                            | Some phone, Some email ->
                                [
                                    str ": "
                                    phone
                                    str ", "
                                    email
                                ]
                            | None, Some x
                            | Some x, None ->
                                [
                                    str ": "
                                    x
                                ]
                            | None, None -> []
                    ]
                )
            )
        ]
    ]