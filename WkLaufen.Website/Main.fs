namespace WkLaufen.Website

open WebSharper.Html.Server
open WebSharper
open WebSharper.Sitelets
open WebSharper.JQuery
open WebSharper.JavaScript

module App =
    [<Website>]
    let Main =
        Application.MultiPage (fun ctx action ->
            match action with
            | Home -> WkLaufen.Website.Pages.Home.Page ctx
            | Contacts -> WkLaufen.Website.Pages.Contacts.Page ctx
            | News -> WkLaufen.Website.Pages.News.OverviewPage ctx
            | NewsDetails newsId -> WkLaufen.Website.Pages.News.DetailsPage ctx newsId
            | Activities -> WkLaufen.Website.Pages.Activities.Page ctx
            | MemberGroups -> WkLaufen.Website.Pages.Members.GroupsPage ctx
            | Members groupId -> WkLaufen.Website.Pages.Members.MembersPage ctx groupId
            | BMF2017Overview -> WkLaufen.Website.Pages.Bmf2017.BMF2017Overview ctx
            | BMF2017Flyer -> WkLaufen.Website.Pages.Bmf2017.BMF2017Flyer ctx
            | BMF2017Register -> WkLaufen.Website.Pages.Bmf2017.Register ctx
            | AboutUs -> WkLaufen.Website.Pages.AboutUs.Page ctx
            | Vision2020 -> WkLaufen.Website.Pages.Vision2020.Page ctx
            | Contests -> WkLaufen.Website.Pages.Contests.Page ctx
            | Youths -> WkLaufen.Website.Pages.Youths.Page ctx
            | RecorderKids -> WkLaufen.Website.Pages.RecorderKids.Page ctx
            | Impressum -> WkLaufen.Website.Pages.Impressum.Page ctx
        )

[<Sealed>]
type Website() =
    interface IWebsite<EndPoint> with
        member this.Sitelet = App.Main
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
                BMF2017Overview
                BMF2017Flyer
                BMF2017Register
                AboutUs
                Vision2020
                Contests
                Youths
                RecorderKids
                Impressum
            ]

[<assembly: Website(typeof<Website>)>]
do ()
