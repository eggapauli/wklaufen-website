module Unterstuetzen.View

open Fable.Helpers.React
open Fable.Helpers.React.Props
open global.Data

let root =
  Layout.page
    "support"
    Images.unterstuetzen_w1000h600
    [
        h1 [] [ str "Unterstützendes Mitglied werden" ]
        div [ Class "flyer" ] [ App.Html.pdfDoc "binary/FormularUnterstuetzendesMitglied.pdf" ]
    ]
