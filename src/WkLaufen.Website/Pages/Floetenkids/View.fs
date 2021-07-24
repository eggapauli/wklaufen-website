module Floetenkids.View

open global.Data
open DataModels
open Fable.React
open Fulma

let root =
  let contact = MemberQuery.firstWithRole Jugendorchesterleiter
  Layout.page
    "recorder-kids"
    Images.blockfloetenkids_w1000h600
    [
        App.Html.pdfDoc "binary/2021_Laufenenten-Club_Broschüre_PDF.pdf"
    ]
