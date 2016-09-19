namespace WkLaufen.Website

open WebSharper.Sitelets

type EndPoint =
    | [<EndPoint "GET /">] Home
    | [<EndPoint "GET /kontakte">] Contacts
    | [<EndPoint "GET /news">] News
    | [<EndPoint "GET /">] NewsDetails of string
    | [<EndPoint "GET /termine">] Activities
    | [<EndPoint "GET /musiker">] MemberGroups
    | [<EndPoint "GET /">] Members of string
    | [<EndPoint "GET /bmf-2017">] BMF2017Overview
    | [<EndPoint "GET /bmf-2017-do-meld-i-mi-on">] BMF2017Register
    | [<EndPoint "GET /bmf-2017-do-unterstuetzen-wir-euch">] BMF2017Sponsor
    | [<EndPoint "GET /bmf-2017-flyer">] BMF2017Flyer
    | [<EndPoint "GET /wir-ueber-uns">] AboutUs
    | [<EndPoint "GET /vision-2020">] Vision2020
    | [<EndPoint "GET /wertungen">] Contests
    | [<EndPoint "GET /jugend">] Youths
    | [<EndPoint "GET /floetenkids">] RecorderKids
    | [<EndPoint "GET /impressum">] Impressum

module Templating =
    open System.Web
    open WebSharper
    open WebSharper.Html.Server

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
