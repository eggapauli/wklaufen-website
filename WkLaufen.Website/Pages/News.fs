module WkLaufen.Website.Pages.News

open WkLaufen.Website
open WebSharper.Sitelets
open WebSharper.Html.Server

let OverviewPage (ctx: Context<EndPoint>) =
    Templating.Main ctx EndPoint.News
        {
            Id = "news"
            Title = Html.pages.News.Title
            Css = [ "news.css" ]
            BackgroundImageUrl = Html.pages.News.BackgroundImage
            Body =
            [
                H1 [Text Html.pages.News.Title]
                Div [Id "news-frame"; Class "rich-text"] -< [
                    Div [Id "news-container"; Class "carousel"] -< (
                        Data.News.getAll()
                        |> Seq.map (fun news ->
                            Div [Class "news"] -< (
                                [
                                    Div [Class "date"] -< [
                                        Strong [Text (news.Timestamp.ToString "dd.MM.yyyy")]
                                    ]
                                    Div [Class "content"] -< (
                                        Html.htmlify news.Content
                                        @
                                        [ Br [] ]
                                        @
                                        (match news.Images with
                                        | [||] -> []
                                        | _ ->
                                            [
                                                A [Class "details-link"; HRef (ctx.Link (NewsDetails news.FacebookId))] -< [
                                                    Img [Src "assets/images/camera.png"]
                                                ]
                                            ]
                                        )
                                        @
                                        [
                                            A [HRef ("https://facebook.com/" + news.FacebookId)] -< [
                                                Img [Src "assets/images/fb-logo-bw.png"]
                                            ]
                                        ]
                                    )
                                ]
                            )
                        )
                    )
                ]
                A [Id "facebook-news-hint"; HRef "https://www.facebook.com/Werkskapelle-Laufen-Gmunden-Engelhof-890998107646493"] -< [
                    Text "Aktuelle Infos"
                    Br []
                    Text "auch auf "
                    Img [Src "assets/images/fb-logo.png"]
                ]
            ]
        }

let DetailsPage ctx newsId =
    let news = Data.News.getAll() |> List.find (fun n -> n.FacebookId = newsId)
    Templating.Main ctx EndPoint.NewsDetails
        {
            Id = "news-details"
            Title = Html.pages.News.Title
            Css = [ "news-details.css" ]
            BackgroundImageUrl = Html.pages.News.BackgroundImage
            Body =
            [
                Div [Class "carousel rich-text"] -< (
                    news.Images
                    |> Seq.map (fun image ->
                        Div [Class "image"] -< [
                            Asset.htmlImageNoCrop "news" image (Some 940, Some 480)
                        ]
                    )
                )
            ]
        }