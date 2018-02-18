module App.State

open Elmish
open Elmish.Browser.Navigation
open Elmish.Browser.UrlParser
open Fable.Import.Browser
open Global
open Types

let pageParser: Parser<Page->Page,Page> =
  oneOf [
    map Home (s "home")
    map Kontakte (s "kontakte")
    map News (s "news")
    map NewsDetails (s "news" </> str)
    map Termine (s "termine")
    map Musiker (s "musiker")
    map MusikerRegister (s "musiker" </> str)
    map BMF2017 (s "bmf-2017")
    map WirUeberUns (s "wir-ueber-uns")
    map Vision2020 (s "vision-2020")
    map Wertungen (s "wertungen")
    map Jugend (s "jugend")
    map Floetenkids (s "floetenkids")
  ]

let urlUpdate (result: Option<Page>) model =
  match result with
  | None ->
    console.error("Error parsing url")
    model,Navigation.modifyUrl (toHash model.currentPage)
  | Some page ->
      { model with currentPage = page }, []

let init result =
  urlUpdate result { currentPage = Home }

let update msg model =
  model, []
