module MitgliedWerden.View

open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fulma
open global.Data

let root =
  let contact =
    MemberGroups.getIndexed()
    |> Map.find 31180
  Layout.page
    "become-member"
    Images.mitglied_werden_w1000h600
    [
      Content.content [ Content.CustomClass "rich-text text" ] [
        Heading.h1 [ Heading.Is3 ] [ str "Wir suchen dich!"; br []; str "Werde Mitglied in unserem Musikverein!" ]
        p [] [ str "Du spielst ein Musikinstrument und hast Lust, dieses Hobby auch mit anderen zu teilen? Dann sind wir, die Werkskapelle Laufen Gmunden-Engelhof, genau der richtige Verein für dich!" ]
        p [] [ str "Egal ob du erst angefangen hast oder schon seit längerer Zeit ein Instrument spielst, bei uns bist du jederzeit willkommen. Du kannst dich in unser Jugendorchester integrieren oder je nach Können auch gleich in unseren Musikverein einsteigen, wo Jung und Alt einmal pro Woche das schönste Hobby der Welt teilen." ]
        p [] [ str "Na, neugierig geworden? Wenn du vorbeischauen möchtest oder Fragen hast, melde dich einfach jederzeit bei uns. Wir freuen uns auf dich!" ]
        App.Html.contact contact
      ]
    ]
