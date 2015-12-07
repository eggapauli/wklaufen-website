namespace website_fsharp

open WebSharper.Html.Server
open WebSharper
open WebSharper.Sitelets
open WebSharper.JQuery
open WebSharper.JavaScript

type EndPoint =
    | [<EndPoint "GET /">] Home
    | [<EndPoint "GET /kontakte">] Contacts
    | [<EndPoint "GET /news">] News
    | [<EndPoint "GET /">] NewsDetails of string
    | [<EndPoint "GET /termine">] Activities
    | [<EndPoint "GET /musiker">] MemberGroups
    | [<EndPoint "GET /">] Members of string
    | [<EndPoint "GET /bmf-2017">] BMF2017
    | [<EndPoint "GET /wir-ueber-uns">] AboutUs
    | [<EndPoint "GET /vision-2020">] Vision2020
    | [<EndPoint "GET /wertungen">] Contests
    | [<EndPoint "GET /jugend">] Youths
    | [<EndPoint "GET /floetenkids">] RecorderKids
    | [<EndPoint "GET /impressum">] Impressum

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
            .With("css", fun x ->
                x.Css
                |> List.map (sprintf "assets/css/%s")
                |> List.map (fun href -> Link [HRef href; Rel "stylesheet"])
            )
            .With("bgUrl", fun x -> Asset.resize "pages" x.BackgroundImageUrl (Some 1000, Some 600))
            .With("body", fun x -> x.Body @ [ Span [ ClientSide <@ Client.CheckRedirect() @> ] ])

    let Main ctx endpoint page : Async<Content<EndPoint>> =
        Content.WithTemplate MainTemplate page

module Site =
    open System
    open System.Text.RegularExpressions

    let private pages = Data.Pages.getAll()
    let private members = Data.Members.getGroups()
    let private memberLookup = Data.Members.getIndexed()

    let private menuItem title href bgImage =
        LI [] -< [
            A [Class "menu-item info-link"; HRef href] -< [
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

    let private md = new MarkdownDeep.Markdown(ExtraMode = true)

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

    let private obfuscatePhone phoneNumber =
        phoneNumber |> string |> obfuscate

    let private obfuscateEmail emailAddress =
        emailAddress
        |> Option.map obfuscate
        |> Option.toList
        |> List.concat

    let HomePage ctx =
        let (topMenuItems, bottomMenuItems) =
            Data.MenuItems.getAll()
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
                    Span [] -< [ ClientSide <@ Client.Main() @> ]
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
                    Div [Class "info"] -< [
                        H1 [Text pages.Contacts.Title]
                        Div [VerbatimContent (md.Transform pages.Contacts.Address)]
                    ]
                    Div [Class "contacts"] -< (
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
                                Span [] -< obfuscatePhone m.Phone
                                Br []
                                Span [] -< obfuscateEmail m.Email
                            ]
                        )
                    )
                ]
            }

    let (|Uri|_|) str =
        match Uri.TryCreate(str, UriKind.Absolute) with
        | true, uri when uri.Scheme = "http" || uri.Scheme = "https" -> Some uri
        | _ -> None

    let htmlify (text: string) =
        Regex.Matches(text, @"\S+|(\r\n|\r|\n)|\s+")
        |> Seq.cast<System.Text.RegularExpressions.Match>
        |> Seq.map (fun m ->
            match m.Value with
            | Uri uri -> A [HRef (uri.ToString())] -< [ Text (uri.ToString()) ]
            | "\r" | "\n" | "\r\n" -> Br []
            | text -> Text text
        )
        |> Seq.toList

    let NewsPage (ctx: Context<EndPoint>) =
        Templating.Main ctx EndPoint.News
            {
                Id = "news"
                Title = pages.News.Title
                Css = [ "news.css" ]
                BackgroundImageUrl = pages.News.BackgroundImage
                Body =
                [
                    H1 [Text pages.News.Title]
                    Div [Id "news-frame"; Class "rich-text"] -< [
                        Div [Id "news-container"; Class "carousel"] -< (
                            Data.News.getAll()
                            |> Seq.map (fun news ->
                                Div [Class "news"] -< (
                                    [
                                        Div [Class "date"] -< [
                                            Strong [Text (news.Timestamp.ToString "dd.MM.yyyy")]
                                        ]
                                    ]
                                    @
                                    htmlify news.Content
                                    @
                                    (match news.Images with
                                    | [||] -> []
                                    | _ ->
                                        [
                                            Br []
                                            A [Class "details-link info-link"; HRef (ctx.Link (NewsDetails news.FacebookId))] -< [
                                                Img [Src "assets/images/camera.png"]
                                            ]
                                            A [HRef ("https://facebook.com/" + news.FacebookId)] -< [
                                                Img [Src "assets/images/fb-logo-bw.png"]
                                            ]
                                        ]
                                    )
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

    let NewsDetailsPage ctx newsId =
        let news = Data.News.getAll() |> List.find (fun n -> n.FacebookId = newsId)
        Templating.Main ctx EndPoint.NewsDetails
            {
                Id = "news-details"
                Title = pages.News.Title
                Css = [ "news-details.css" ]
                BackgroundImageUrl = pages.News.BackgroundImage
                Body =
                [
                    Div [Class "carousel rich-text"] -< (
                        news.Images
                        |> Seq.map (fun image ->
                            Div [Class "image"] -< [
                                Asset.htmlImage "news" image (Some 940, Some 480)
                            ]
                        )
                    )
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
                                TBody [] -< (
                                    Data.Activities.getAll()
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
                                )
                            ]
                        ]
                    ]
                ]
            }

    let MemberGroupsPage (ctx: Context<EndPoint>) =
        Templating.Main ctx EndPoint.MemberGroups
            {
                Id = "member-groups"
                Title = pages.Members.Title
                Css = [ "member-groups.css" ]
                BackgroundImageUrl = pages.Members.BackgroundImage
                Body =
                [
                    H1 [Text pages.Members.Title]
                    UL [Class "menu"] -< (
                        members
                        |> List.map (fun (group, _) ->
                            Asset.resize "member-groups" group.Photo (Some 200, Some 130)
                            |> menuItem group.Name (ctx.Link (group.Name |> slug |> EndPoint.Members))
                        )
                    )
                ]
            }

    let MembersPage ctx groupId =
        let (group, members) = members |> List.find (fun (g, _) -> g.Id = groupId)

        Templating.Main (ctx: Context<EndPoint>) EndPoint.Members
            {
                Id = "members"
                Title = group.Name
                Css = [ "members.css" ]
                BackgroundImageUrl = pages.Members.BackgroundImage
                Body =
                [
                    H1 [Text group.Name]
                    Div [Class "rich-text"] -< [
                        Div [Class "carousel"] -< (
                            members
                            |> Seq.map (fun m ->
                                Div [Class "member"] -< seq {
                                    yield H2 [Text (sprintf "%s %s" m.FirstName m.LastName)]
                                    match m.Photo with
                                    | Some photo -> yield Div [Class "image"] -< [Asset.htmlImage "members" photo (None, Some 270)]
                                    | None -> ()
                                    yield UL [] -< seq {
                                        if m.Instruments |> Array.isEmpty |> not
                                        then
                                            yield LI [Text (if m.Instruments.Length = 1 then "Instrument: " else "Instrumente: ")] -< [
                                                Text (m.Instruments |> String.concat ", ")
                                            ]
                                        if m.Roles |> Array.isEmpty |> not
                                        then
                                            yield LI [Text (if m.Roles.Length = 1 then "Funktion: " else "Funktionen: ")] -< [
                                                Text (m.Roles |> String.concat ", ")
                                            ]
                                        yield LI [Text (sprintf "Aktiv seit: %d" (DateTime.Parse(m.MemberSince).Year))]
                                        yield LI [Text (sprintf "Wohnort: %s" m.City)]
                                    }
                                    yield Div [Class "clear"]
                                }
                            )
                        )
                    ]
                ]
            }

    let BMF2017Page ctx =
        Templating.Main ctx EndPoint.Activities
            {
                Id = "bmf-2017"
                Title = pages.Bmf2017.Title
                Css = [ "bmf-2017.css" ]
                BackgroundImageUrl = pages.Bmf2017.BackgroundImage
                Body =
                [
                    Div [Class "text"] -< [
                        H1 [Text pages.Bmf2017.Headline]
                        VerbatimContent (md.Transform pages.Bmf2017.Content)
                    ]
                ]
            }

    let AboutUsPage ctx =
        Templating.Main ctx EndPoint.AboutUs
            {
                Id = "about-us"
                Title = pages.AboutUs.Title
                Css = [ "about-us.css" ]
                BackgroundImageUrl = pages.AboutUs.BackgroundImage
                Body =
                [
                    H1 [Text pages.AboutUs.Title]
                    Div [Class "characteristics rich-text"] -< [
                        H2 [Text pages.AboutUs.Characteristics.Title]
                        Div [Class "carousel"] -< (
                            pages.AboutUs.Characteristics.Items
                            |> Seq.map (fun item ->
                                Div [Class "about-us-section"] -< [
                                    H3 [Text item.Title]
                                    Div [VerbatimContent (md.Transform item.Text)]
                                ]
                            )
                        )
                    ]
                    Div [Class "samples rich-text"] -< [
                        VerbatimContent (md.Transform pages.AboutUs.Videos)
                    ]
                ]
            }

    let Vision2020Page ctx =
        Templating.Main ctx EndPoint.Vision2020
            {
                Id = "vision-2020"
                Title = pages.Vision2020.Title
                Css = [ "vision-2020.css" ]
                BackgroundImageUrl = pages.Vision2020.BackgroundImage
                Body =
                [
                    H1 [Text pages.Vision2020.Title]
                    Div [Class "rich-text"] -< [
                        Div [Class "rich-text-content"] -< [
                            VerbatimContent (md.Transform pages.Vision2020.Content)
                        ]
                    ]
                ]
            }

    let ContestsPage ctx =
        Templating.Main ctx EndPoint.Contests
            {
                Id = "contests"
                Title = pages.Contests.Title
                Css = [ "contests.css" ]
                BackgroundImageUrl = pages.Contests.BackgroundImage
                Body =
                [
                    Div [Class "rich-text contest-container"] -< [
                        H1 [Text pages.Contests.Title]
                        Div [Class "rich-text-content"] -< (
                            pages.Contests.Results
                            |> Seq.map (fun item ->
                                Div [Class "contest"] -< [
                                    H2 [Text item.Title]
                                    VerbatimContent (md.Transform item.Content)
                                ]
                            )
                        )
                    ]
                ]
            }

    let YouthsPage ctx =
        Templating.Main ctx EndPoint.Youths
            {
                Id = "youths"
                Title = pages.Youths.Title
                Css = [ "youths.css" ]
                BackgroundImageUrl = pages.Youths.BackgroundImage
                Body =
                [
                    Div [Class "rich-text text"] -< [
                        H1 [Text pages.Youths.Title]
                        Div [VerbatimContent (md.Transform pages.Youths.Content)]
                    ]

                    Div [Class "contacts-container"] -< [
                        Div [Class "rich-text contacts"] -< (
                            pages.Youths.Members
                            |> Seq.map (fun memberId ->
                                let m = memberLookup |> Map.find memberId
                                Div [Class "contact"] -< [
                                    Strong [Text (sprintf "%s %s" m.FirstName m.LastName)]
                                    Text (sprintf " (%s): " (m.Roles |> String.concat ", "))
                                    Span [] -< obfuscatePhone m.Phone
                                    VerbatimContent "&nbsp;"
                                    Span [] -< obfuscateEmail m.Email
                                ]
                            )
                        )
                    ]
                ]
            }

    let RecorderKidsPage ctx =
        Templating.Main ctx EndPoint.RecorderKids
            {
                Id = "recorder-kids"
                Title = pages.RecorderKids.Title
                Css = [ "recorder-kids.css" ]
                BackgroundImageUrl = pages.RecorderKids.BackgroundImage
                Body =
                [
                    H1 [Text pages.RecorderKids.Title]
                    Tags.Object [Class "flyer"; Deprecated.Data ("assets/" + pages.RecorderKids.Flyer); Attr.Type "application/pdf"]
                ]
            }

    let ImpressumPage ctx =
        let obmann =
            members
            |> Seq.collect snd
            |> Seq.find (fun m -> m.Roles |> Seq.contains "Obmann")
        Templating.Main ctx EndPoint.Impressum
            {
                Id = "impressum"
                Title = pages.Impressum.Title
                Css = [ "impressum.css" ]
                BackgroundImageUrl = pages.Impressum.BackgroundImage
                Body =
                [
                    H1 [Text pages.Impressum.Title]
                    Div [Class "top-content"] -< (
                        [
                            VerbatimContent (md.Transform pages.Impressum.TopContent)
                            Text (sprintf "Obmann: %s %s, " obmann.FirstName obmann.LastName)
                        ]
                        @
                        obfuscateEmail obmann.Email
                    )
                    Div [Class "bottom-content rich-text"] -< [
                        Div [] -< [VerbatimContent (md.Transform pages.Impressum.BottomContent)]
                    ]
                ]
            }

    [<Website>]
    let Main =
        Application.MultiPage (fun ctx action ->
            match action with
            | Home -> HomePage ctx
            | Contacts -> ContactsPage ctx
            | News -> NewsPage ctx
            | NewsDetails newsId -> NewsDetailsPage ctx newsId
            | Activities -> ActivitiesPage ctx
            | MemberGroups -> MemberGroupsPage ctx
            | Members groupId -> MembersPage ctx groupId
            | BMF2017 -> BMF2017Page ctx
            | AboutUs -> AboutUsPage ctx
            | Vision2020 -> Vision2020Page ctx
            | Contests -> ContestsPage ctx
            | Youths -> YouthsPage ctx
            | RecorderKids -> RecorderKidsPage ctx
            | Impressum -> ImpressumPage ctx
        )

[<Sealed>]
type Website() =
    interface IWebsite<EndPoint> with
        member this.Sitelet = Site.Main
        member this.Actions =
            [
                Home
                Contacts
                News
            ]
            @
            (Data.News.getAll()
            |> List.filter (fun n -> n.Images.Length > 0)
            |> List.map (fun n -> NewsDetails n.FacebookId))
            @
            [
                Activities
                MemberGroups
            ]
            @
            (Data.Members.getGroups()
            |> List.map (fun (g, _) -> Members g.Id))
            @
            [
                BMF2017
                AboutUs
                Vision2020
                Contests
                Youths
                RecorderKids
                Impressum
            ]

[<assembly: Website(typeof<Website>)>]
do ()
