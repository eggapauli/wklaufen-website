module Home.View

open Microsoft.FSharp.Reflection
open Fable.Helpers.React
open Fable.Helpers.React.Props
open Global

let private menuItem page =
  let href = toHash page
  let render (bgImage: string) text =
    let lastDotIndex = bgImage.LastIndexOf '.'
    let fileName = bgImage.Substring(0, lastDotIndex)
    let extension = bgImage.Substring lastDotIndex
    let bgImagePath = sprintf "images/menu-items/%s_w150h100%s" fileName extension

    li
      [ ClassName "menu-item"; Style [ BackgroundImage (sprintf "url(%s)" bgImagePath) ] ]
      [
        a
          [ ClassName "menu-item"; Href href ]
          [
            span [ ClassName "bg"; Style [ BackgroundImage (sprintf "url(%s)" bgImagePath) ] ] []      
            span [ ClassName "title-bar" ] [ span [ ClassName "title" ] [ str text ] ]
          ]
      ]
  match page with
  | Home -> None
  | Kontakte -> Some ((1, 1), render "kontakte.png" "Kontakte")
  | News -> Some ((1, 2), render "news.jpg" "News")
  | NewsDetails _ -> None
  | Termine -> Some ((1, 3), render "termine.png" "Termine")
  | Musiker -> Some ((1, 4), render "musiker.png" "Musiker")
  | MusikerRegister _ -> None
  | BMF2017 -> Some ((1, 5), render "bmf-2017.jpg" "BMF 2017")
  | WirUeberUns -> Some ((2, 1), render "wir-ueber-uns.jpg" "Wir über uns")
  | Vision2020 -> Some ((2, 2), render "vision-2020.png" "Vision 2020")
  | Wertungen -> Some ((2, 3), render "wertungen.png" "Wertungen")
  | Jugend -> Some ((2, 4), render "jugend.png" "Jugend")
  | Floetenkids -> Some ((2, 5), render "floetenkids.png" "Flötenkids")

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
    "home.jpg"
    [
      ul [ Id "top-menu"; ClassName "menu" ] topMenuItems
      h1 [] [ str "Willkommen bei der"; br []; str "Werkskapelle Laufen Gmunden-Engelhof" ]
      div [ Id "bottom-menu-container" ] [
        ul [ Id "bottom-menu"; ClassName "menu" ] bottomMenuItems
      ]
    ]
