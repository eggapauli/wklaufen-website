module Jugend.View

open global.Data
open DataModels
open Fable.React
open Fulma

let root =
  let contact = MemberQuery.firstWithRole Jugendreferent
  Layout.page
    "youths"
    Images.jugendreferat_w1000h600
    [
        Content.content [ Content.CustomClass "rich-text text" ] [
            Heading.h1 [ Heading.Is3 ] [ str "Lieber zukünftiger Musikprofi!"; br []; str "Liebe Eltern!" ]
            p [] [ str "Weil uns als Musikverein die Jugendarbeit besonders wichtig ist, bieten wir Kindern, die ein Instrument erlernen möchten eine Rundumbetreuung. Neben der Suche nach einem Musikschulplatz, stellen wir auch das Instrument gegen eine geringe Leihgebühr zur Verfügung. Außerdem helfen wir gerne bei der Suche nach dem richtigen Musikinstrument, denn nicht selten entwickelt sich dies zum lebenslangen Begleiter." ]
            p [] [ str "Es besteht außerdem die Möglichkeit zum Gruppenspiel durch die Bildung einer Bläserklasse speziell für Instrumentenanfänger. Die sofortige Integration in den Verein fördert nicht nur die persönliche Weiterentwicklung, es entstehen auch tolle Freundschaften bei den wöchentlichen Proben im Musikheim in der Engelhofstraße in Gmunden oder bei lustigen Ausflügen." ]
            p [] [ str "Du kannst dich noch nicht entscheiden, welches Instrument du lernen willst? Kein Problem! Melde dich einfach bei uns und komm vorbei zum Ausprobieren. Wir freuen uns auf dich!" ]
            p [] [ str "Für Anmeldungen und/oder Fragen steht jederzeit bereit:" ]
            App.Html.contact contact
        ]
    ]
