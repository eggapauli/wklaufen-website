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
            Title : string
            MenuBar : list<Element>
            Body : list<Element>
        }

    let MainTemplate =
        Content.Template<Page>("~/Main.html")
            .With("title", fun x -> x.Title)
            .With("css", fun x -> Link [HRef "test.css"; Rel "stylesheet"])
            .With("bgUrl", fun x -> "background-image.jpg")
            .With("body", fun x -> x.Body)

    // Compute a menubar where the menu item for the given endpoint is active
    let MenuBar (ctx: Context<EndPoint>) endpoint =
        let ( => ) txt act =
             LI [if endpoint = act then yield Attr.Class "active"] -< [
                A [Attr.HRef (ctx.Link act)] -< [Text txt]
             ]
        [
            LI ["Home" => EndPoint.Home]
            LI ["Contacts" => EndPoint.Contacts]
        ]

    let Main ctx endpoint title body : Async<Content<EndPoint>> =
        Content.WithTemplate MainTemplate
            {
                Title = title
                MenuBar = MenuBar ctx endpoint
                Body = body
            }

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

        Templating.Main ctx EndPoint.Home "Home" [
            UL [ Id "top-menu"; Class "menu" ] -< [
                for item in topMenuItems ->
                    menuItem item.Title (getHref item.Title) (Asset.resize "menu-items" item.BackgroundImage (Some 150, Some 100))
            ]
            H1 [Text (md.Transform pages.Home.Content)]
//            Div [ClientSide <@ Client.Main() @>]
            UL [ Id "bottom-menu"; Class "menu" ] -< [
                for item in bottomMenuItems ->
                    menuItem item.Title (getHref item.Title) (Asset.resize "menu-items" item.BackgroundImage (Some 150, Some 100))
            ]
        ]

    let ContactsPage ctx =
        Templating.Main ctx EndPoint.Contacts "About" [
            H1 [Text "About"]
            P [Text "This is a template WebSharper HTML application."]
        ]

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
