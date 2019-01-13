module WirUeberUns.View

open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fable.Import.React
open Fable.Import.Slick
open Fulma
open Fulma.Extensions.Wikiki
open global.Data
open WirUeberUns.Types

type CharacteristicPage = {
  Title: string
  Content: ReactElement list
}

type Sample = {
  Title: string
  Link: string
}

let items = [
  "A", "Adventkonzert", "Jedes Jahr gestalten wir zusammen mit dem Jugendorchester und den Blockflötenkids ein Kirchenkonzert in der Vorweihnachtszeit. Außerdem stimmen wir uns mit Spielereien auf Weihnachtsmärkten, dem Keksebacken der Jugend und unserer Vereinsweihnachtsfeier auf die schönste Zeit des Jahres ein."
  "B", "Blockflötenkids", "Ab dem Kindergartenalter bieten wir Kindern einmal pro Woche Blockflötenunterricht bei uns im Probenheim. Dabei wird ihr Selbstbewusstsein gefördert und sie lernen Notenkunde und Rhythmusgefühl. Anmeldungen unter jugendorchester@wk-laufen.at."
  "C", "Chamäleons", "Wir sehen uns als musikalische Chamäleons. Ob Marschmusik, Klassisches, Big Band Sound, böhmische Blasmusik, Filmmusik, moderne Konzertwerke, Polka oder Walzer - wir spielen alles gerne und mit Begeisterung!"
  "D", "Dämmerschoppen", "Dämmerschoppen und Frühschoppen gehören bei einem österreichischen Musikverein einfach dazu. Bei uns unter anderem am Tag der Tracht oder abends an Sommertagen bei einem der Traunsee-Wirte."
  "E", "Engelhofstraße 7-9", "Hier sind wir daheim. Der Gmundner Stadtteil Engelhof ist sozusagen unser zweiter Vorname."
  "F", "Feste", "Feste feiern wir gerne. Auf den Musikfesten der Region sind wir stets gut vertreten und immer vorne dabei. Auch innerhalb des Vereins gibt es immer wieder Grund zu feiern."
  "G", "Gmunden", "Die Kulturregion der Keramikstadt Gmunden liegt uns am Herzen. Als Teil des oberösterreichischen Blasmusikverbandes im Bezirk Gmunden erhalten wir österreichisches Kulturgut und Traditionen."
  "H", "Humor", "Humor kommt im Verein nicht zu kurz. Wir lachen gerne gemeinsam und sind immer für einen guten „Schmäh“ zu haben. Gemeinsam musizieren macht Freude und auch außerhalb des Probenzimmers macht das Vereinsleben Spaß - besonders bei Ausflügen, gemeinsamen Veranstaltungen oder Konzertreisen."
  "I", "Instrumente", "Instrumente sind teuer, daher haben wir Leihinstrumente für interessierte Kinder und Jugendliche. Zudem unterstützen wir unsere Mitglieder bei Wartung und Finanzierung ihrer Instrumente."
  "J", "Jahreskonzert", "Das Jahreskonzert im Februar/März ist unser großes Highlight im Vereinsjahr. Für keine andere Veranstaltung proben wir so intensiv. Hier geben wir unser Bestes, um einen fulminanten Konzertabend bieten zu können."
  "K", "Konzertwertung", "Um unser musikalisches Niveau zu steigern und unserem Ehrgeiz gerecht zu werden, lassen wir uns jedes Jahr von einer fachkundigen Jury beurteilen - in den letzten Jahren mit außerordentlichem Erfolg! Auch bei der Marschwertung im Zuge der Bezirksmusikfeste sind wir immer gern dabei, um unser Können unter Beweis zu stellen."
  "L", "Laufen AG", "Die Keramik- und Sanitärproduktionsstätte der Laufen AG in Gmunden ist unser Geburtsort. Werksmitarbeiter haben unseren Verein 1951 gegründet."
  "M", "Mittwoch", "Seit Jahrzehnten ist der Mittwoch unser wöchentlicher Probentag. Das ist für Musikvereine in der Region eher ungewöhnlich und für Studierende im Verein nicht immer einfach, doch durch die Mitgliedschaft einiger MusikerInnen in anderen Vereinen unausweichlich."
  "N", "Neujahrsblasen", "Zwischen Weihnachten und Silvester gehen mehrere Ensembles der Werkskapelle in verschiedenen Stadtteilen Gmundens zwei Tage lang von Haus zu Haus, um musikalische Neujahrsgrüße und eine Einladung zum Jahreskonzert zu überbringen."
  "O", "ÖSPAG", "Bevor der Laufen-Konzern die Sanitärfabrik in Gmunden übernommen hat, waren wir als Werkskapelle ÖSPAG bekannt."
  "P", "Probenlokal", "Unser Probenlokal nennen wir liebevoll MuZi (= Musi-Zimmer). Die Adresse haben wir ja bereits verraten."
  "Q", "Quartett, Quintette ….", "Wir spielen nicht nur Konzerte, Früh- und Dämmerschoppen oder Wertungsspiele in großer Formation, sondern auch Begräbnisse, kirchliche Anlässe oder auf Hochzeiten. Vielfach sind hier kleinere Ensembles der WK Laufen im Einsatz."
  "R", "Rot, gelb & grün", "Das sind die Farben unserer Uniform, die wir seit 2011 stolz tragen."
  "S", "Sommerpause", "Während der heißen Jahreszeit gönnt sich unser Verein jedes Jahr eine wohlverdiente Sommerpause, um im Herbst wieder richtig durchstarten zu können."
  "T", "Taktstock", "Das Amt des Kapellmeisters ist ein ganz besonderes in jedem Musikverein. Die musikalische Leitung kümmert sich nicht nur um die Abhaltung der Proben, sondern ist auch verantwortlich für die Programmauswahl und ein wichtiger Bestandteil des Vorstandsteams."
  "U", "Unterstützende Mitglieder", "Wir freuen uns, wenn Sie unseren Verein mit 15 Euro Jahresbeitrag unterstützen möchten. Dafür erhalten Sie zweimal jährlich einen Newsletter und verpassen als VIP keine Veranstaltung von und mit der Werkskapelle."
  "V", "Vorstand", "Ein bunt gemischtes Team aus Jung und Alt bildet unseren Vorstand, der sich mit viel ehrenamtlichem Engagement um die Organisation und Weiterentwicklung des Vereins kümmert."
  "W", "Weckruf am ersten Mai", "Eine altbewährte Tradition. Ab 6.00 Uhr morgens ist die gesamte Werkskapelle mit Kleinbussen zu Stationen im gesamten Stadtgebiet unterwegs."
  "X", "Xylophon", "Auch das haben wir in unserem umfangreichen Vereinsinstrumentarium. Ankauf und Pflege von Instrumenten sind teuer, aber der interessierte Nachwuchs braucht ordentliche Instrumente. Daher freuen wir uns enorm über jedes Sponsoring und neue unterstützende Mitglieder."
  "Y", "Youngsters", "Die Jugendarbeit ist uns enorm wichtig. Neben dem Blockflötenunterricht bieten wir Hilfe bei weiterführender Instrumentenfindung, organisieren einen Musikschulplatz und unterstützen die musikalische Entwicklung durch das Jugendorchester - für Anmeldungen und Fragen wenden Sie sich an jugenreferat@wk.laufen.at. Mit dem Bronzenen Leistungsabzeichen erfolgt die Integration in das große Orchester."
  "Z", "Zusammenhalt", "Das wichtigste im Verein ist der Zusammenhalt. Wir sind Freunde und in allen Lebenslagen füreinander da. Der Verein ist ein unschätzbares soziales Netzwerk mit unterschiedlichen Charakteren und einer eigenen Kultur. Jede und jeder Musikinteressierte ist herzlich willkommen!"
]

let root model dispatch =
  let letterView idx (letter, _title, _description) =
    let color =
      if idx = model.SlideNumber
      then IsPrimary
      else NoColor

    Button.button
      [ Button.IsRounded
        Button.Color color
        Button.OnClick (fun _ev -> dispatch (SlideTo idx)) ]
      [ str letter ]
  let lettersView =
    Content.content [] [ Button.list [ Button.List.IsCentered ] (List.mapi letterView items) ]

  let descriptionView (letter, title, description) =
      Content.content []
        [ Heading.h2 [ Heading.Is4 ] [ str (sprintf "%s wie %s" letter title) ]
          Content.content [ Content.CustomClass "description" ] [ str description ] ]
  let descriptionsView =
    List.map descriptionView items

  Layout.page
    "about-us"
    Images.wir_ueber_uns_w1000h600
    [
      div [ ClassName "rich-text" ]
        [ Heading.h1 [ Heading.Is3 ] [ str "Wir über uns - Musi-ABC" ]
          lettersView ]

      Divider.divider [ ]
      
      div [ClassName "rich-text"]
        [ Fable.Import.Slick.slider
            [ Draggable false
              Infinite true
              AdaptiveHeight false
              InitialSlide model.SlideNumber
              AfterChange (SlideTo >> dispatch)
              Key (string model.SlideNumber) ]
            descriptionsView ] ] 