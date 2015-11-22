module Data

open FSharp.Data

module MenuItems =
    [<Literal>]
    let DataPath = @"data\menu-items.json"

    type private MenuItems = JsonProvider<DataPath>

    let getAll() =
        MenuItems.GetSamples()

module Pages =
    [<Literal>]
    let DataPath = @"data\pages.json"

    type private Pages = JsonProvider<DataPath>

    let getAll() =
        Pages.GetSample()

module Members =
    [<Literal>]
    let MembersPath = @"data\members.json"
    type private Members = JsonProvider<MembersPath, InferTypesFromValues=false>

    [<Literal>]
    let MemberGroupsPath = @"data\member-groups.json"
    type private MemberGroups = JsonProvider<MemberGroupsPath>

    type MemberGroup = {
        Name: string
        Photo: string
    }

    let private isMember groupId (m: Members.Root) =
        let hasAnyInstrument instruments =
            m.Instruments
            |> Set.ofArray
            |> Set.intersect (instruments |> Set.ofList)
            |> Set.isEmpty
            |> not
        match groupId with
        | "Vorstand" -> m.Roles |> Array.isEmpty |> not
        | "Saxophon" -> hasAnyInstrument [ "Saxophon" ]
        | "KlarinetteFagott" -> hasAnyInstrument [ "B-Klarinette"; "Fagott" ]
        | "Marketenderinnen" -> m.Instruments |> Array.isEmpty || hasAnyInstrument [ "Sonstige" ]
        | "TiefesBlech" -> hasAnyInstrument [ "Tenorhorn"; "Tuba"; "Posaune"; "Horn" ]
        | "HohesBlech" -> hasAnyInstrument [ "Fl\u00fcgelhorn"; "Trompete" ]
        | "Schlagzeug" -> hasAnyInstrument [ "Schlagzeug" ]
        | "Querfloete" -> hasAnyInstrument [ "Querfl\u00F6te" ]
        | _ -> false

    let getIndexed() =
        Members.GetSamples()
        |> Array.map (fun m -> int m.OoebvId, m)
        |> Map.ofArray

    let getGroups() =
        let members = Members.GetSamples()
        MemberGroups.GetSamples()
        |> Array.map (fun g ->
            let groupMembers =
                members
                |> Array.filter (isMember g.Id)
                |> Array.toList
            g, groupMembers
        )
        |> Array.toList

module Activities =
    [<Literal>]
    let DataPath = @"data\activities.json"

    type private Activities = JsonProvider<DataPath>

    let getAll() =
        Activities.GetSamples()

module News =
    [<Literal>]
    let DataPath = @"data\news.json"

    type private News = JsonProvider<DataPath>

    let getAll() =
        News.GetSamples()
