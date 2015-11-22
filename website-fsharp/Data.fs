module Data

open FSharp.Data

[<Literal>]
let MenuItemsPath = @"data\menu-items.json"

type private MenuItems = JsonProvider<MenuItemsPath>

let getMenuItems() =
    MenuItems.GetSamples()

[<Literal>]
let PagesPath = @"data\pages.json"

type private Pages = JsonProvider<PagesPath>

let getPages() =
    Pages.GetSample()

[<Literal>]
let MembersPath = @"data\members.json"

type private Members = JsonProvider<MembersPath>

let getMembers() =
    Members.GetSamples()

[<Literal>]
let ActivitiesPath = @"data\activities.json"

type private Activities = JsonProvider<ActivitiesPath>

let getActivities() =
    Activities.GetSamples()

[<Literal>]
let NewsPath = @"data\news.json"

type private News = JsonProvider<NewsPath>

let getNews() =
    News.GetSamples()
