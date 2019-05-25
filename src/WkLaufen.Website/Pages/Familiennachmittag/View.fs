module Familiennachmittag.View

open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fulma
open global.Data

let root =
  Layout.page
    "familiennachmittag"
    Images.home_w1000h600
    [
      div [ Style [ TextAlign "center" ] ]
        [
          img [ Src Images.familiennachmittag_flyer_vorderseite_w450h560; Style [ BoxShadow "white -8px 0px 15px 10px" ] ]
          img [ Src Images.familiennachmittag_flyer_rueckseite_w450h560; Style [ BoxShadow "white 8px 0px 15px 10px" ] ]
        ]
    ]
