module Instrumentenfindung.View

open Microsoft.FSharp.Reflection
open Fable.React
open Fable.React.Props
open Fulma
open Global
open global.Data

let root =
  Layout.page
    "instrumentenfindung"
    Images.instrumentenfindung_w1000h600
    [
        App.Html.pdfDoc "binary/2021_A4_Finde dein Instrument_PDF.pdf"
    ]
