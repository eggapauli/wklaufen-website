module Fable.Import.Slick

open Fable.Core
open Fable.Core.JsInterop
open Fable.React.Props
open Fable.React.Helpers

type SliderProps =
    | Draggable of bool
    | Infinite of bool
    | AdaptiveHeight of bool
    | InitialSlide of int
    | AfterChange of (int -> unit)
    interface IHTMLProp

let slickStyles = importAll<obj> "slick-carousel/slick/slick.scss"
let slickThemeStyles = importAll<obj> "slick-carousel/slick/slick-theme.scss"
let slider (b: IHTMLProp list) c = ofImport "default" "react-slick/lib/slider" (keyValueList CaseRules.LowerFirst b) c
