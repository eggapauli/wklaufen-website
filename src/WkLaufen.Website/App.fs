module App.View

open Elmish
open Fable.React
open Fable.React.Props
open Fable.Core.JsInterop
open Types
open App.State
open Global
open Elmish.React
open Elmish.Debug
open Elmish.HMR
open Elmish.UrlParser

importAll "../../sass/main.sass"

let view model dispatch =
  let backArrow = function
    | Home -> []
    | _ ->
      [
        a [ ClassName "back"; OnClick (fun _ev -> dispatch GoBack) ] [
          span [ ClassName "far fa-arrow-alt-circle-left fa-5x"; Style [ Color "rgba(169,132,20,0.7)" ] ] []
        ]
      ]

  let pageHtml = function
    | Home -> Home.View.root
    | Kontakte -> Kontakte.View.root
    | News -> div [] []
    | Termine -> Termine.View.root
    | Musiker -> Musiker.View.root
    | MusikerRegister groupId -> Musiker.View.detail groupId
    | Unterstuetzen -> Unterstuetzen.View.root model.UnterstuetzenModel (UnterstuetzenMsg >> dispatch)
    | WirUeberUns -> WirUeberUns.View.root model.WirUeberUnsModel (WirUeberUnsMsg >> dispatch)
    | MitgliedWerden -> MitgliedWerden.View.root
    | Wertungen -> Wertungen.View.root
    | Jugend -> Jugend.View.root
    | Floetenkids -> Floetenkids.View.root
    | Impressum -> Impressum.View.root
    | Instrumentenfindung -> Instrumentenfindung.View.root

  div [] [
    yield pageHtml model.CurrentPage
    yield a [ ClassName "impressum"; toLink Impressum |> Href ] [ Helpers.str "Impressum" ]
    yield! backArrow model.CurrentPage
  ]


// App
Program.mkProgram init update view
|> Program.toNavigable (parseHash pageParser) urlUpdate
|> Program.withReactBatched "elmish-app"
#if DEBUG
|> Program.withConsoleTrace
|> Program.withDebugger
#endif
|> Program.run
