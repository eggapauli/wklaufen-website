module App.State

open Elmish.Browser.Navigation
open Elmish.Browser.UrlParser
open Fable.Import.Browser
open Global
open Types
open Elmish

let pageParser: Parser<Page->Page,Page> =
  oneOf [
    map Home (s "home")
    map Kontakte (s "kontakte")
    map News (s "news")
    map NewsDetails (s "news" </> str)
    map Termine (s "termine")
    map Musiker (s "musiker")
    map MusikerRegister (s "musiker" </> str)
    map Unterstuetzen (s "unterstuetzen")
    map WirUeberUns (s "wir-ueber-uns")
    map MitgliedWerden (s "mitglied-werden")
    map Wertungen (s "wertungen")
    map Jugend (s "jugend")
    map Floetenkids (s "floetenkids")
    map Impressum (s "impressum")
    map Jahreskonzert (s "jahreskonzert")
  ]

let updateWindowTitle page dispatch =
  let title =
    match page with
    | Home -> "Willkommen"
    | Kontakte -> "Kontakte"
    | Termine -> "Termine"
    | News
    | NewsDetails _ -> "News"
    | Musiker
    | MusikerRegister _ -> "Musiker"
    | Unterstuetzen -> "Unterstützen"
    | WirUeberUns -> "Wir über uns"
    | MitgliedWerden -> "Mitglied werden"
    | Wertungen -> "Wertungen"
    | Jugend -> "Jugend"
    | Floetenkids -> "Flötenkids"
    | Impressum -> "Impressum"
    | Jahreskonzert -> "Jahreskonzert"
    |> sprintf "%s - WK Laufen"
  Fable.Import.Browser.document.title <- title

let trackPage page dispatch =
  GTag.setPage GTag.trackingId (toUrl page)

let urlUpdate (result: Option<Page>) model =
  match result with
  | None ->
    console.error("Error parsing url")
    model, Cmd.batch [ Navigation.modifyUrl (toHash model.CurrentPage) ]
  | Some page ->
    { model with CurrentPage = page }, [ updateWindowTitle page; trackPage page ]

let init result =
  let model = {
    CurrentPage = Home
    NewsModel = News.Types.init
    UnterstuetzenModel = Unterstuetzen.Types.init
    WirUeberUnsModel = WirUeberUns.Types.init
    JahreskonzertModel = Jahreskonzert.Types.init
  }

  let (model', cmd) =
    match result with
    | None ->
      model, Navigation.modifyUrl (toHash model.CurrentPage)
    | Some Home ->
      model, []
    | Some page ->
      { model with CurrentPage = page },
      Cmd.batch [
        Navigation.modifyUrl (toHash Home)
        Navigation.newUrl (toHash page)
      ]
  model', Cmd.batch [ [ updateWindowTitle model'.CurrentPage ]; cmd ]

let update msg model =
  match msg with
  | GoBack when model.CurrentPage = News ->
    { model with NewsModel = News.Types.init }, Navigation.jump -1
  | GoBack -> model, Navigation.jump -1
  | NewsMsg msg -> { model with NewsModel = News.Types.update msg model.NewsModel }, Cmd.none
  | UnterstuetzenMsg msg ->
    let model', cmd' = Unterstuetzen.Types.update msg model.UnterstuetzenModel
    { model with UnterstuetzenModel = model' }, Cmd.map UnterstuetzenMsg cmd'
  | WirUeberUnsMsg msg ->
    let model', cmd' = WirUeberUns.Types.update msg model.WirUeberUnsModel
    { model with WirUeberUnsModel = model' }, Cmd.map WirUeberUnsMsg cmd'
  | JahreskonzertMsg msg ->
    let model', cmd' = Jahreskonzert.Types.update msg model.JahreskonzertModel
    { model with JahreskonzertModel = model' }, Cmd.map JahreskonzertMsg cmd'

