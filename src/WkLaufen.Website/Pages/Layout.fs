module Layout

open Fable.Helpers.React
open Fable.Helpers.React.Props

let page pageId bgImageUrl content =
  div
    [
      sprintf "content-background content-background-%s" pageId |> ClassName
      Style [ BackgroundImage (sprintf "url(%s)" bgImageUrl) ]
    ]
    [ div [ Id pageId; ClassName "inner-main"] content ]
