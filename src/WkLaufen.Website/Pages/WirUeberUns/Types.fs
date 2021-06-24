module WirUeberUns.Types

open Elmish
open Fable.Core.JsInterop
open Fable.Import

let mutable sliderRef: Browser.Types.Element option = None

let gotoSlide n =
    match sliderRef with
    | None -> ()
    | Some slider -> slider?slickGoTo(n, false)

type Model = {
  SlideNumber: int
}

type Msg =
  | SlideTo of int
let init =
  { SlideNumber = 0 }

let update msg model =
  match msg with
  | SlideTo idx -> { model with SlideNumber = idx }, Cmd.OfFunc.attempt gotoSlide idx raise

