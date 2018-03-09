module Unterstuetzen.View

open Fable.Core.JsInterop
open Fable.Helpers.React
open Fable.Helpers.React.Props
open Generated

let root =
  Layout.page
    "support"
    Images.unterstuetzen_w1000h600
    [
        h1 [] [ str "Unterstützendes Mitglied werden" ]
        object [Class "flyer"; !!("data", "binary/FormularUnterstuetzendesMitglied.pdf"); Type "application/pdf"]
            [
                App.Html.pdfNotSupportedWarning
            ]
    ]
