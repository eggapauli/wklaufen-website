module WirUeberUns.View

open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fable.Import.React
open Fable.Import.Slick
open Generated

type CharacteristicPage = {
  Title: string
  Content: ReactElement list
}

type Sample = {
  Title: string
  Link: string
}

let characteristicPages = [
  {
    Title = "Musikalische Flexibilität"
    Content =
      [
        str "Wir spielen alles! Marschmusik, Klassisches, Big Band Sound, böhmische Blasmusik, Filmmusik, moderne Konzertliteratur, Polka, Walzer, Ouvertüre(…) von Avsenik & Doss über Mozart & Schwarz bis hin zu Wonder & Zeman. Und das auf Konzerten, Früh- & Dämmerschoppen, Hochzeiten, Begräbnissen, Konzertreisen, Wertungsspiele und kirchliche Anlässen. Auch bei unserem traditionellen Jahreskonzert im Februar zeigen wir gerne, dass wir es als traditioneller Blasmusikverein auch Big-Band-technisch richtig krachen lassen können. Dieses abwechslungsreiche Repertoire ist eine Herausforderung, der wir uns mit größtem Vergnügen jede Probe wieder stellen."
      ]
  }
  {
    Title = "Eine lange & aufregende Geschichte"
    Content =
      [
        str "Gegründet wurde unser Verein im Jahr 1951. Zunächst bestand der Klangkörper vor allem aus Mitarbeitern der Keramik- & Sanitärproduktionsstätte im Gmundner Stadtteil Engelhof. Unser Probenzimmer befindet sich direkt am Werksgelände, aktuell sind zwei unserer 52 Musikerinnen und Musiker im Werk beschäftigt. Zu unseren größten Erfolgen bzw. bedeutendsten Ereignissen der Vereinsgeschichte zählen definitiv die internationalen Konzertreisen nach Zypern und Korfu."
        br []
        br []
        str "Im Jahr 2013 übergab Konsulent Franz Schindlauer nach 30 Jahren als Kapellmeister den Taktstock an seine Tochter Christa Doblmair, die den Verein nun gemeinsam mit Obmann Mathias Schrabacher leitet. Der nächste große Meilenstein unseres Vereins ist (neben der Realisierung eines neuen Probenlokals) die Ausrichtung des Bezirksmusikfestes Gmunden 2017."
      ]
  }
  {
    Title = "Ein großartiges Vereinsklima"
    Content =
      [
        str "Unser größter Trumpf sind unsere Musikerinnen und Musiker! Egal ob 6-jähriges Blockflötenkind oder 60-jähriger Blasmusikexperte – jeder ist mit viel Freude & Engagement an der Musik dabei. So entstehen nicht nur Freundschaften über Generationen hinweg, sondern auch ein Vereinsklima, bei dem man auch nach einem langen und anstrengenden Arbeits- oder Schultag gerne das Probenlokal betritt."
      ]
  }
  {
    Title = "Junger Schwung & wertvolle Erfahrungen im Vorstandsteam"
    Content =
      [
        str "Ziele braucht der Vorstand und hat deshalb eine Vision 2020 ausgearbeitet. Diese Planung beinhaltet Meilensteine wie ein neues Probenlokal, das Bezirksmusikfest 2017, Ausflüge, musikalische Höhenflüge und Jugendprojekte."
        br []
        br []
        str "Aktuell besteht unser Vorstandsteam aus 14 Personen. Was uns hier ausmacht ist die Balance zwischen langjährigen & erfahrenen Vorstandsmitgliedern (wie unserem ehemaligen Kapellmeister Konsulent Franz Schindlauer und unserem ehemaligen Obmann Franz Prentner) und jungen, motivierten und kreativen Kräften. Derzeit sind 9 von 14 Vorstandsmitgliedern 30 Jahre oder jünger."
      ]
  }
  {
    Title = "Unsere jungen Wilden"
    Content =
      [
        str "Jung sind bei uns nämlich nicht nur die Blockflötenkids und die Musikerinnen und Musiker im Jugendorchester. Unsere Jugendreferentinnen Maria und Christina sind selbst noch keine 20 Jahre alt, aber bereits voller Pläne und Ideen für die Jugendarbeit. Neben der musikalischen Weiterbildung kommt auch der Spaß bei unserer Vereinsjugend nicht zu kurz. Gerade erst haben sie den dritten Platz bei einem Malwettbewerb zum Thema \"Wir verändern die Welt\" mit ihrem Bild \"Musik verändert die Welt\" erreicht."
      ]
  }
]

let samples = [
  {
    Title = "The Dream of Freedom"
    Link = "https://www.youtube.com/watch?v=WKsrP1cfs8Q"
  }
  {
    Title = "Schönbrunn Suite"
    Link = "https://www.youtube.com/watch?v=o63IJHlv4Ik"
  }
  {
    Title = "Saxpack"
    Link = "https://www.youtube.com/watch?v=k4sBY68C0_o"
  }
  {
    Title = "Cape Horn"
    Link = "https://www.youtube.com/watch?v=kMNL4Y4XMhc"
  }
  {
    Title = "Children of Sanchez"
    Link = "https://www.youtube.com/watch?v=f48X0KubvnQ"
  }
  {
    Title = "Montana Fanfare"
    Link = "https://www.youtube.com/watch?v=C6gkZudc7oo"
  }
  {
    Title = "Ouverture Festive"
    Link = "https://www.youtube.com/watch?v=TNoIbz_PcJ4"
  }
]

let root =
  Layout.page
    "about-us"
    Images.wir_ueber_uns_w1000h600
    [
      h1 [] [ str "Wir über uns" ]
      div [Class "characteristics rich-text"] [
          h2 [] [ str "Was uns auszeichnet" ]
          slider
            [
              Draggable false
              Infinite false
              AdaptiveHeight true
            ]
            (
              characteristicPages
              |> List.map (fun item ->
                  div [Class "about-us-section"] [
                      h3 [] [ str item.Title ]
                      div [] item.Content
                  ]
              )
            )
      ]
      div [Class "samples rich-text"] [
        yield h2 [] [ str "Videos & Hörproben" ]
        yield!
          samples
          |> List.map (fun sample ->
            a [ Href sample.Link; Target "_blank" ] [ str sample.Title ]
          )
          |> List.intersperse (br [])
      ]
    ]
