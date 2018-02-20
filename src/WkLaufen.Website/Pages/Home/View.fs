module Home.View

open Microsoft.FSharp.Reflection
open Fable.Helpers.React
open Fable.Helpers.React.Props
open Global
open Generated

let private menuItem page =
  let href = toHash page
  let render bgImagePath text =
    App.Html.menuItem bgImagePath text href
    
  match page with
  | Home -> None
  | Kontakte -> Some ((1, 1), render Images.kontakte_w150h100 "Kontakte")
  | News -> Some ((1, 2), render Images.news_w150h100 "News")
  | NewsDetails _ -> None
  | Termine -> Some ((1, 3), render Images.termine_w150h100 "Termine")
  | Musiker -> Some ((1, 4), render Images.musiker_w150h100 "Musiker")
  | MusikerRegister _ -> None
  | BMF2017 -> Some ((1, 5), render Images.bmf_2017_w150h100 "BMF 2017")
  | WirUeberUns -> Some ((2, 1), render Images.wir_ueber_uns_w150h100 "Wir über uns")
  | Vision2020 -> Some ((2, 2), render Images.vision_2020_w150h100 "Vision 2020")
  | Wertungen -> Some ((2, 3), render Images.wertungen_w150h100 "Wertungen")
  | Jugend -> Some ((2, 4), render Images.jugend_w150h100 "Jugend")
  | Floetenkids -> Some ((2, 5), render Images.floetenkids_w150h100 "Flötenkids")
  | Impressum -> None

let private pages =
  Microsoft.FSharp.Reflection.FSharpType.GetUnionCases typeof<Page>
  |> Seq.map (fun caseInfo -> FSharpValue.MakeUnion(caseInfo, [||]) :?> Page)
  |> Seq.toList

let root =
  let topMenuItems, bottomMenuItems =
    pages
    |> List.choose menuItem
    |> List.partition (fst >> fst >> ((=) 1))
    |> fun (a, b) ->
      a |> List.sortBy (fst >> snd) |> List.map snd
      , b |> List.sortBy (fst >> snd) |> List.map snd
  
  Layout.page
    "home"
    Images.home_w1000h600
    [
      ul [ Id "top-menu"; ClassName "menu" ] topMenuItems
      h1 [] [ str "Willkommen bei der"; br []; str "Werkskapelle Laufen Gmunden-Engelhof" ]
      div [ Id "bottom-menu-container" ] [
        ul [ Id "bottom-menu"; ClassName "menu" ] bottomMenuItems
      ]
    ]
