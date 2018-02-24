module Fable.Import.Slick

open Fable.Core
open Fable.Core.JsInterop
open Fable.Helpers
open Fable.Helpers.React.Props
open Fable.Import.React

type SliderProps =
    | Draggable of bool
    | Infinite of bool
    | AdaptiveHeight of bool
    | InitialSlide of int
    | AfterChange of (int -> unit)
    interface IHTMLProp

let slickStyles = importAll<obj> "slick-carousel/slick/slick.scss"
let slickThemeStyles = importAll<obj> "slick-carousel/slick/slick-theme.scss"
let Slider = importDefault<ComponentClass<obj>> "react-slick/lib/slider"
let slider (b: IHTMLProp list) c = React.from Slider (keyValueList CaseRules.LowerFirst b) c
