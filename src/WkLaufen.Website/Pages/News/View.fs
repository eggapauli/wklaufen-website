module News.View

open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fable.Import.Slick
open Generated
open Global

let root =
  Layout.page
    "news"
    Images.news_w1000h600
    [
      div [ ClassName "info" ] [
        h1 [] [ str "News" ]
        div [ Id "news-frame"; ClassName "rich-text" ] [
          Fable.Import.Slick.slider
            [
              Draggable false
              Infinite false
              AdaptiveHeight true
            ]
            (
              App.Data.News.items
              |> List.map (fun n ->
                div [ ClassName "news" ] [
                  div [ ClassName "date" ] [
                      b [] [ str (n.News.Timestamp.ToString "dd.MM.yyyy") ]
                  ]
                  div [ ClassName "content" ] (
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
                      a [ Href (n.SourceUri.ToString()) ] [
                        img [ Src "images/fb-logo-bw.png" ]
                      ]
                    ]
                  )
                ]
              )
            )
        ]
      ]
      a [ Id "facebook-news-hint"; Href "https://www.facebook.com/Werkskapelle-Laufen-Gmunden-Engelhof-890998107646493" ] [
        str "Aktuelle Infos"
        br []
        str "auch auf "
        img [ Src "images/fb-logo.png" ]
      ]
    ]

let details newsId =
  App.Data.News.items
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
                AdaptiveHeight true
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
    | None ->
      Fable.Import.Browser.console.error ("Can't find news with id " + newsId)
      []
  |> Layout.page "news-details" "news.jpg"
