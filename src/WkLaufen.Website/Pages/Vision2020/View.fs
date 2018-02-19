module Vision2020.View

open Fable.Helpers.React
open Fable.Helpers.React.Props
open Generated

let root =
  Layout.page
    "vision-2020"
    Images.vision_2020_w1000h600
    [
      h1 [] [ str "Vision 2020" ]
      div [Class "rich-text"] [
          div [Class "rich-text-content"] [
              i [] [ str "\"Die Vergangenheit muss ich zur Kenntnis nehmen. Mit der Gegenwart muss ich leben. Für die Zukunft aber, muss ich Visionen haben.\"" ]
              br []
              str "W. Kownatka"
              br []
              br []
              str "Unter diesem Motto hat der Vorstand der WK Laufen bei der Herbsttagung am 26. September 2014 eine Vision für die nächsten 6 Vereinsjahre formuliert. Mithilfe eines Meilensteinplans versuchen wir unsere Vereinsziele zu verwirklichen, um auch in Zukunft musikalische Qualität und ein lebendiges Miteinander garantieren zu können."
              br []
              br []
              h2 [] [ str "Unsere gemeinsame Vision 2020" ]
              b [] [ str "Engagierte" ]
              str " und leistungsorientierte "
              b [] [ str "Jugendarbeit" ]
              str " ist das Fundament für "
              b [] [ str "florierendes Vereinsleben" ]
              str "."
              br []
              b [] [ str "Kameradschaftliches" ]
              str " Zusammenspiel zwischen "
              b [] [ str "Jung und Alt" ]
              str " motiviert zu höchsten Leistungen in guter "
              b [] [ str "Qualität" ]
              str "."
              br []
              b [] [ str "Zielorientierte" ]
              str " Vereinsführung ermöglicht die Umsetzung von "
              b [] [ str "Konzertreisen" ]
              str ", "
              b [] [ str "Musikfesten" ]
              str " und anderen Großprojekten."
              br []
              str "Unser neues "
              b [] [ str "Probenheim" ]
              str " bietet beste Rahmenbedingungen für "
              b [] [ str "Spaß und Spiel" ]
              str "."
          ]
      ]
    ]
