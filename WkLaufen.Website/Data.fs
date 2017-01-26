module WkLaufen.Website.Data

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
            |> Array.truncate 1
            |> Set.ofArray
            |> Set.intersect (instruments |> Set.ofList)
            |> Set.isEmpty
            |> not
        match groupId with
        | "vorstandsteam" -> m.Roles |> Array.isEmpty |> not
        | "saxophon" -> hasAnyInstrument [ "Saxophon" ]
        | "klarinette-und-fagott" -> hasAnyInstrument [ "B-Klarinette"; "Fagott" ]
        | "marketenderinnen" -> m.Instruments |> Array.isEmpty || hasAnyInstrument [ "Sonstige" ]
        | "tiefes-blech" -> hasAnyInstrument [ "Tenorhorn"; "Tuba"; "Posaune"; "Horn" ]
        | "hohes-blech" -> hasAnyInstrument [ "Fl\u00fcgelhorn"; "Trompete" ]
        | "schlagzeug" -> hasAnyInstrument [ "Schlagzeug" ]
        | "querfloete" -> hasAnyInstrument [ "Querfl\u00F6te" ]
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

module News =
    [<Literal>]
    let DataPath = @"data\news.json"

    type private News = JsonProvider<DataPath>

    let getAll() =
        News.GetSamples()
        |> Array.toList
