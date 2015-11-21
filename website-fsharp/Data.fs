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