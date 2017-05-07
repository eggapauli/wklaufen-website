module WkLaufen.Website.Pages.Home

open WkLaufen.Website
open WkLaufen.Website.Data
open WebSharper
open WebSharper.Html.Server

let Page ctx =
    let (topMenuItems, bottomMenuItems) =
        Data.MenuItems.getAll()
        |> Array.partition (fun i -> i.Location = "Top")

    Templating.Main ctx EndPoint.Home
        {
            Id = "home"
            Title = Html.pages.Home.Title
            Css = [ "home.css" ]
            BackgroundImageUrl = Html.pages.Home.BackgroundImage
            Body =
            [
                UL [ Id "top-menu"; Class "menu" ] -< [
                    for item in topMenuItems ->
                        Html.menuItem item.Title (Html.getHref item.Title) (Asset.resize "menu-items" item.BackgroundImage (Some 150, Some 100))
                ]
                H1 [ VerbatimContent (Html.md.Transform Html.pages.Home.Content)]
                Div [ Id "bottom-menu-container" ] -< [
                    UL [ Id "bottom-menu"; Class "menu" ] -< [
                        for item in bottomMenuItems ->
                            Html.menuItem item.Title (Html.getHref item.Title) (Asset.resize "menu-items" item.BackgroundImage (Some 150, Some 100))
                    ]
                ]
                A [ Id "home-bmf-logo"; HRef "bmf-2017.html" ] -< [
                    Asset.htmlImage "bmf" "logo.png" (Some 120, None)
                ]
                A [ Id "home-bmf-inserat"; HRef "bmf-2017.html" ] -< [
                    Asset.htmlImage "bmf" "inserat.jpg" (Some 150, None)
                ]
                Span [ClientSide <@ Client.Main() @>]
            ]
        }