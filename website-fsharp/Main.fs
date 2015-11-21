namespace website_fsharp

open WebSharper.Html.Server
open WebSharper
open WebSharper.Sitelets

type EndPoint =
    | [<EndPoint "GET /">] Home
    | [<EndPoint "GET /kontakte">] Contacts

module Templating =
    open System.Web

    type Page =
        {
            Id: string
            Title: string
            Css: string list
            BackgroundImageUrl: string
            Body: Element list
        }

    let MainTemplate =
        Content.Template<Page>("~/Main.html")
            .With("title", fun x -> x.Title)
            .With("contentId", fun x -> x.Id)
            .With("css", fun x -> x.Css |> List.map (fun path -> Link [HRef (sprintf "assets/css/%s" path); Rel "stylesheet"]))
            .With("bgUrl", fun x -> sprintf "assets/images/pages/%s" x.BackgroundImageUrl)
            .With("body", fun x -> x.Body)

    let Main ctx endpoint page : Async<Content<EndPoint>> =
        Content.WithTemplate MainTemplate page

module Site =
    open System.Text.RegularExpressions

    let private pages = Data.getPages()

    let private menuItem title href bgImage =
        LI [] -< [
            A [Class "menu-item"; HRef href] -< [
                Div [ Class "bg"; Style (sprintf "background-image: url(%s)" bgImage) ]
                Div [ Class "title-bar" ] -< [
                    Span [ Class "title" ] -< [ Text title ]
                ]
            ]
        ]

    let private slug (text: string) =
        text
            .ToLowerInvariant()
            .Replace("\u00C4", "Ae")
            .Replace("\u00D6", "Oe")
            .Replace("\u00DC", "Ue")
            .Replace("\u00E4", "ae")
            .Replace("\u00F6", "oe")
            .Replace("\u00FC", "ue")
            .Replace("&", "und")
        |> fun t -> Regex.Replace(t, "[^a-zA-Z0-9]+", "-")

    let private getHref text =
        slug text
        |> sprintf "%s.html"

    let private md = new MarkdownDeep.Markdown();

    let HomePage ctx =
        let (topMenuItems, bottomMenuItems) =
            Data.getMenuItems()
            |> Array.partition (fun i -> i.Location = "Top")

        Templating.Main ctx EndPoint.Home
            {
                Id = "home"
                Title = pages.Home.Title
                Css = [ "home.css" ]
                BackgroundImageUrl = pages.Home.BackgroundImage
                Body =
                [
                    UL [ Id "top-menu"; Class "menu" ] -< [
                        for item in topMenuItems ->
                            menuItem item.Title (getHref item.Title) (Asset.resize "menu-items" item.BackgroundImage (Some 150, Some 100))
                    ]
                    H1 [ VerbatimContent (md.Transform pages.Home.Content)]
                    Div [ Id "bottom-menu-container" ] -< [
                        UL [ Id "bottom-menu"; Class "menu" ] -< [
                            for item in bottomMenuItems ->
                                menuItem item.Title (getHref item.Title) (Asset.resize "menu-items" item.BackgroundImage (Some 150, Some 100))
                        ]
                    ]
                ]
            }

    let ContactsPage ctx =
        Templating.Main ctx EndPoint.Contacts
            {
                Id = "contacts"
                Title = pages.Contacts.Title
                Css = [ "contacts.css" ]
                BackgroundImageUrl = pages.Contacts.BackgroundImage
                Body =
                [
                    H1 [Text "About"]
                    P [Text "This is a template WebSharper HTML application."]
                ]
            }

    [<Website>]
    let Main =
        Application.MultiPage (fun ctx action ->
            match action with
            | Home -> HomePage ctx
            | Contacts -> ContactsPage ctx
        )

[<Sealed>]
type Website() =
    interface IWebsite<EndPoint> with
        member this.Sitelet = Site.Main
        member this.Actions = [Home; Contacts]

[<assembly: Website(typeof<Website>)>]
do ()
