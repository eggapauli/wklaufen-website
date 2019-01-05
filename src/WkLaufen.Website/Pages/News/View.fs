module News.View

open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fable.Import.Slick
open Fulma
open global.Data
open Global
open News.Types

let root model dispatch =
  Layout.page
    "news"
    Images.news_w1000h600
    [
      div [ ClassName "info" ] [
        Heading.h1 [ Heading.Is3 ] [ str "News" ]
        div [ Id "news-frame"; ClassName "rich-text" ] [
          Fable.Import.Slick.slider
            [
              Draggable false
              Infinite false
              AdaptiveHeight true
              InitialSlide model.SlideNumber
              AfterChange (SlideTo >> dispatch)
            ]
            (
              News.items
              |> List.map (fun n ->
                div [ ClassName "news" ] [
                  div [ ClassName "date" ] [
                      b [] [ str (n.News.Timestamp.ToString "dd.MM.yyyy") ]
                  ]
                  div [ ClassName "text" ] (
                    App.Html.htmlify n.News.Content
                    @
                    [ br [] ]
                    @
                    (
                      match n.Images with
                      | [] -> []
                      | _ ->
                        [
                          a [ ClassName "details-link"; Href (toHash (NewsDetails n.Id)) ] [
                            img [ Src "images/camera.png" ]
                          ]
                        ]
                    )
                    @
                    [
                      a
                        [ Href (n.SourceUri.ToString())
                          Target "_blank" ]
                        [ img [ Src "images/fb-logo-bw.png" ] ]
                    ]
                  )
                ]
              )
            )
        ]
      ]
      a
        [ Id "facebook-news-hint"
          Href "https://www.facebook.com/Werkskapelle-Laufen-Gmunden-Engelhof-890998107646493"
          Target "_blank"
        ]
        [ str "Aktuelle Infos"
          br []
          str "auch auf "
          img [ Src "images/fb-logo.png" ]
        ]
    ]

let details model dispatch newsId =
  News.items
  |> List.tryFind (fun n -> n.Id = newsId)
  |> function
    | Some news ->
      [
        div [ ClassName "rich-text" ]
          [
            Fable.Import.Slick.slider
              [
                Draggable false
                Infinite false
                // AdaptiveHeight true
              ]
              (
                Images.news
                |> Map.tryFind news.Id
                |> Option.defaultValue []
                |> List.map (fun imageUrl ->
                  div [ ClassName "image"; Style [ Height 480 ] ] [
                    App.Html.image imageUrl (Some 940, Some 480)
                  ]
                )
              )
          ]
      ]
      |> Layout.page "news-details" Images.news_w1000h600
    | None ->
      Fable.Import.Browser.console.error ("Can't find news with id " + newsId)
      root model dispatch
