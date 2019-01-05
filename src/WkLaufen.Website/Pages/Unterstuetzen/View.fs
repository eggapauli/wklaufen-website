module Unterstuetzen.View

open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fulma
open global.Data

let root =
  Layout.page
    "support"
    Images.unterstuetzen_w1000h600
    [
        Heading.h1 [ Heading.Is3 ] [ str "Unterstützendes Mitglied werden" ]
        div [ Class "flyer" ] [ App.Html.pdfDoc "binary/FormularUnterstuetzendesMitglied.pdf" ]
    ]
