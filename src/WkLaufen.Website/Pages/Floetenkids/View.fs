module Floetenkids.View

open Fable.Helpers.React
open Fable.Helpers.React.Props
open global.Data

let root =
  let contact =
    MemberGroups.getIndexed()
    |> Map.find 39627
  Layout.page
    "recorder-kids"
    Images.blockfloetenkids_w1000h600
    [
        div [Class "rich-text text"] [
            h1 [] [ str "Liebes zukünftiges Blockflötenkind!"; br []; str "Liebe Eltern!" ]
            p [] [ str "Die Blockflöte eine tolle Vorbereitung für jedes weiterführende Instrument. Viele Musikerinnen und Musiker der Werkskapelle Laufen Gmunden-Engelhof haben durch den frühen Blockflötenunterricht ihre Leidenschaft für die Musik entdeckt und sind bis heute begeistert von diesem wundervollen Hobby. Das Blockflötenspielen vermittelt Rhythmusgefühl, Selbstbewusstsein und Notenkunde, aber vor allem macht es jede Menge Spaß!" ]
            p [] [ str "Weil uns als Musikverein die Jugendarbeit besonders wichtig ist, bieten wir für Kinder ab dem Kindergartenalter Blockflötenunterricht an. Der Unterricht findet meist in Gruppen von 3-5 Jahren und 6-8 Jahren statt. In unserem Musikheim in der Engelhofstraße in Gmunden wird spielerisch einmal pro Woche der Umgang mit dem ersten Instrument vermittelt." ]
            p [] [ str "Wenn du gerne ein Blockflötenprofi werden, oder einfach nur mal reinschnuppern möchtest, dann melde dich doch bei uns. Wir freuen uns auf dich!" ]
            p [] [ str "Auf deine Anmeldung und/oder deine Fragen freut sich:" ]
            App.Html.contact contact
        ]
    ]
