module Layout

open Fable.Helpers.React
open Fable.Helpers.React.Props

let page pageId bgImage content =
  let bgImageUrl = App.Html.imageUrl "pages" bgImage (Some 1000, Some 600)
  div
    [ ClassName "content-background"; Style [ BackgroundImage (sprintf "url(%s)" bgImageUrl) ] ]
    [
      div [ ClassName "horizontal-outer-border" ] [
        div [ ClassName "vertical-outer-border" ] [
          div [ Id pageId; ClassName "inner-main"] content
        ]
      ]
    ]
