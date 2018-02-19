module App.View

open Elmish
open Elmish.Browser.Navigation
open Elmish.Browser.UrlParser
open Fable.Core
open Fable.Core.JsInterop
open Fable.Import
open Fable.Import.Browser
open Types
open App.State
open Global

importAll "../../sass/main.sass"

open Fable.Helpers.React
open Fable.Helpers.React.Props

let view model dispatch =
  let pageHtml = function
    | Page.Home -> Home.View.root
    | Kontakte -> Kontakte.View.root
    | Termine -> Termine.View.root
    | News -> News.View.root
    | NewsDetails newsId -> News.View.details newsId
    | Musiker -> Musiker.View.root
    | MusikerRegister groupId -> Musiker.View.detail groupId
    | BMF2017 -> div [] [ str "Not implemented" ]
    | WirUeberUns -> WirUeberUns.View.root
    | Vision2020 -> Vision2020.View.root
    | Wertungen -> Wertungen.View.root
    | Jugend -> Jugend.View.root
    | Floetenkids -> Floetenkids.View.root

  pageHtml model.currentPage

open Elmish.React
open Elmish.Debug
open Elmish.HMR

// App
Program.mkProgram init update view
|> Program.toNavigable (parseHash pageParser) urlUpdate
|> Program.withReact "elmish-app"
#if DEBUG
|> Program.withDebugger
|> Program.withHMR
#endif
|> Program.run
