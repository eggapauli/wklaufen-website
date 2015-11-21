namespace website_fsharp

open WebSharper.Html.Server
open WebSharper
open WebSharper.Sitelets

type EndPoint =
    | [<EndPoint "GET /">] Home
    | [<EndPoint "GET /kontakte">] Contacts
    | [<EndPoint "GET /termine">] Activities

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

    let private MainTemplate =
        Content.Template<Page>("~/Main.html")
            .With("title", fun x -> x.Title)
            .With("contentId", fun x -> x.Id)
            .With("css", fun x -> x.Css |> List.map (fun path -> Link [HRef (sprintf "assets/css/%s" path); Rel "stylesheet"]))
            .With("bgUrl", fun x -> sprintf "assets/images/pages/%s" x.BackgroundImageUrl)
            .With("body", fun x -> x.Body)

    let Main ctx endpoint page : Async<Content<EndPoint>> =
        Content.WithTemplate MainTemplate page

module Site =
    open System
    open System.Text.RegularExpressions

    let private pages = Data.getPages()
    let private members = Data.getMembers()
    let private memberLookup =
        members
        |> Array.map (fun m -> m.OoebvId, m)
        |> Map.ofArray

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

    let private random = new Random();
    let private obfuscate (text: string) =
        text
        |> Seq.map (fun ch ->
            let chars = "abcdefghijklmnopqrstuvwxyz0123456789"
            let randomChar = chars.[random.Next(chars.Length)] |> string
            [
                Span [Style "display: none"] -< [Text randomChar]
                Span [Text (string ch)]
            ]
        )
        |> List.concat

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
        let contactNodes =
            pages.Contacts.Members
            |> Seq.map (fun memberId ->
                let m =
                    memberLookup
                    |> Map.find memberId
                Div [Class "contact"] -< [
                    Div [Class "image"] -<
                        (m.Photo
                        |> Option.map (fun p -> Asset.htmlImage "members" p (Some 100, Some 150))
                        |> Option.toList)
                    Span [Text (sprintf "%s %s" m.FirstName m.LastName)]
                    Br []
                    Span [Text (m.Roles |> String.concat ", ")]
                    Br []
                    Span [] -< obfuscate (string m.Phone)
                    Br []
                    Span [] -< (m.Email |> Option.map obfuscate |> Option.toList |> List.concat)
                ]
            )
            |> Seq.toList

        Templating.Main ctx EndPoint.Contacts
            {
                Id = "contacts"
                Title = pages.Contacts.Title
                Css = [ "contacts.css" ]
                BackgroundImageUrl = pages.Contacts.BackgroundImage
                Body =
                [
                    Div [Class "info"] -< [
                        H1 [Text pages.Contacts.Title]
                        Div [VerbatimContent (md.Transform pages.Contacts.Address)]
                    ]
                    Div [Class "contacts"] -< contactNodes
                ]
            }

    let ActivitiesPage ctx =
        let formatTime (beginTime: DateTime) endTime =
            let endTime = endTime |> function | Some x -> x | None -> beginTime
            let showTime = beginTime.TimeOfDay <> TimeSpan.Zero || endTime.TimeOfDay <> TimeSpan.Zero
            let sameTime = beginTime.ToString("dd.MM.yyyy HH:mm") = endTime.ToString("dd.MM.yyyy HH:mm")
            let sameDate = beginTime.ToString("dd.MM.yyyy") = endTime.ToString("dd.MM.yyyy")
            let sameMonth = beginTime.ToString("MM.yyyy") = endTime.ToString("MM.yyyy")
            let sameYear = beginTime.ToString("yyyy") = endTime.ToString("yyyy")
            let dateTimeFormat = sprintf "dd.MM.yyyy%s" (if showTime then " HH:mm" else "")
            let formattedTime =
                if sameTime then beginTime.ToString dateTimeFormat
                elif sameDate then (beginTime.ToString dateTimeFormat) + (if showTime then " - " + endTime.ToString "HH:mm" else "")
                elif sameMonth && not showTime then beginTime.ToString("dd.") + " - " + endTime.ToString("dd.") + beginTime.ToString("MM.yyyy")
                else beginTime.ToString(dateTimeFormat) + " - " + endTime.ToString(dateTimeFormat)
            formattedTime

        let activityRows =
            Data.getActivities()
            |> Seq.filter (fun act -> act.IsPublic)
            |> Seq.filter (fun act -> act.BeginTime.IsSome)
            |> Seq.groupBy (fun act -> act.BeginTime.Value.Year)
            |> Seq.map (fun (year, entries) ->
                let entryNodes =
                    entries
                    |> Seq.map (fun entry ->
                        TR [] -< [
                            TD [ Text (formatTime entry.BeginTime.Value entry.EndTime)]
                            TD [ Text entry.Title]
                            TD [ Text entry.Location]
                        ]
                    )
                    |> Seq.toList
                TR [] -< (TH [ColSpan "3"] -< [Text (string year)] :: entryNodes)
            )
            |> Seq.toList

        Templating.Main ctx EndPoint.Activities
            {
                Id = "activities"
                Title = pages.Activities.Title
                Css = [ "activities.css" ]
                BackgroundImageUrl = pages.Activities.BackgroundImage
                Body =
                [
                    Div [Class "activities rich-text"] -< [
                        H1 [Text pages.Activities.Title]
                        Div [Class "list"] -< [
                            Table [] -< [
                                TBody [] -< activityRows
                            ]
                        ]
                    ]
                ]
            }

    [<Website>]
    let Main =
        Application.MultiPage (fun ctx action ->
            match action with
            | Home -> HomePage ctx
            | Contacts -> ContactsPage ctx
            | Activities -> ActivitiesPage ctx
        )

[<Sealed>]
type Website() =
    interface IWebsite<EndPoint> with
        member this.Sitelet = Site.Main
        member this.Actions = [Home; Contacts; Activities]

[<assembly: Website(typeof<Website>)>]
do ()
