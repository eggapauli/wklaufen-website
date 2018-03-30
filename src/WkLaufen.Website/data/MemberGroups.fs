module Data.MemberGroups

type MemberGroup = {
    Id: string
    Name: string
}

let private isMember groupId (m: DataModels.Member) =
    let hasAnyInstrument instruments =
        m.Instruments
        |> List.truncate 1
        |> Set.ofList
        |> Set.intersect (instruments |> Set.ofList)
        |> Set.isEmpty
        |> not
    match groupId with
    | "vorstandsteam" -> m.Roles |> List.isEmpty |> not
    | "saxophon" -> hasAnyInstrument [ "Saxophon" ]
    | "klarinette-und-fagott" -> hasAnyInstrument [ "B-Klarinette"; "Bass-Klarinette"; "Fagott" ]
    | "marketenderinnen" -> m.Instruments |> List.isEmpty || hasAnyInstrument [ "Sonstige" ]
    | "tiefes-blech" -> hasAnyInstrument [ "Tenorhorn"; "Tuba"; "Posaune"; "Horn" ]
    | "hohes-blech" -> hasAnyInstrument [ "Fl\u00fcgelhorn"; "Trompete" ]
    | "schlagzeug" -> hasAnyInstrument [ "Schlagzeug" ]
    | "querfloete-und-oboe" -> hasAnyInstrument [ "Querfl\u00F6te"; "Oboe" ]
    | _ -> false

let getIndexed() =
    Members.items
    |> List.map (fun m -> m.OoebvId, m)
    |> Map.ofList

let getGroups() =
    [
        {
            Id = "querfloete-und-oboe"
            Name = "Querflöte & Oboe"
        }
        {
            Id = "klarinette-und-fagott"
            Name = "Klarinette & Fagott"
        }
        {
            Id = "saxophon"
            Name = "Saxophon"
        }
        {
            Id = "hohes-blech"
            Name = "Hohes Blech"
        }
        {
            Id = "vorstandsteam"
            Name = "Vorstandsteam"
        }
        {
            Id = "tiefes-blech"
            Name = "Tiefes Blech"
        }
        {
            Id = "schlagzeug"
            Name = "Schlagzeug"
        }
        {
            Id = "marketenderinnen"
            Name = "Marketenderinnen"
        }
    ]
    |> List.map (fun g ->
        let groupMembers =
            Data.Members.items
            |> List.filter (isMember g.Id)
        g, groupMembers
    )
