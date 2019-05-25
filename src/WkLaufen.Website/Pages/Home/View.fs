module Home.View

open Microsoft.FSharp.Reflection
open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fulma
open Global
open global.Data

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
  | Unterstuetzen -> Some ((1, 5), render Images.unterstuetzen_w150h100 "Unterstützen")
  | WirUeberUns -> Some ((2, 1), render Images.wir_ueber_uns_w150h100 "Wir über uns")
  | MitgliedWerden -> Some ((2, 2), render Images.mitglied_werden_w150h100 "Mitglied werden")
  | Wertungen -> Some ((2, 3), render Images.wertungen_w150h100 "Wertungen")
  | Jugend -> Some ((2, 4), render Images.jugend_w150h100 "Jugend")
  | Floetenkids -> Some ((2, 5), render Images.floetenkids_w150h100 "Flötenkids")
  | Impressum -> None
  | Familiennachmittag -> None

let private pages =
  Microsoft.FSharp.Reflection.FSharpType.GetUnionCases typeof<Page>
  |> Seq.filter (fun info -> info.GetFields().Length = 0)
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
      Heading.h1 [ Heading.Is3 ] [ str "Willkommen bei der"; br []; str "Werkskapelle Laufen Gmunden-Engelhof" ]
      a
        [ Href (toHash Familiennachmittag)
          Style
            [ Display "inline-block"
              Position "absolute"
              Right "50px"
              Transform "rotate(-5deg)"
              BoxShadow "0px 0px 15px 15px white"
              LineHeight "0" ]
        ]
        [ img [ Src Images.familiennachmittag_w250 ] ]
      div [ Id "bottom-menu-container" ] [
        ul [ Id "bottom-menu"; ClassName "menu" ] bottomMenuItems
      ]
    ]
