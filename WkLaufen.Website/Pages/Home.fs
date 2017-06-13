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
                H1 [] -< [
                    Text "Willkommen bei der"
                    Br []
                    Text "Werkskapelle Laufen Gmunden-Engelhof"
                ]
                Div [ Id "bottom-menu-container" ] -< [
                    UL [ Id "bottom-menu"; Class "menu" ] -< [
                        for item in bottomMenuItems ->
                            Html.menuItem item.Title (Html.getHref item.Title) (Asset.resize "menu-items" item.BackgroundImage (Some 150, Some 100))
                    ]
                ]
                Span [ClientSide <@ Client.Main() @>]
            ]
        }