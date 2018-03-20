module Floetenkids.View

open Fable.Core.JsInterop
open Fable.Helpers.React
open Fable.Helpers.React.Props
open Generated

let root =
  Layout.page
    "recorder-kids"
    Images.blockfloetenkids_w1000h600
    [
        h1 [] [ str "Blockflötenkids" ]
        div [ Class "flyer" ] [ App.Html.pdfDoc "binary/Blockfloetenflyer.pdf" ]
    ]
