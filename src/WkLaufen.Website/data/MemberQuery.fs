module Data.MemberQuery

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
    | "saxophon" -> hasAnyInstrument [ "Saxophon Alt" ]
    | "klarinette-und-fagott" -> hasAnyInstrument [ "Klarinette B"; "Klarinette Baß"; "Fagott" ]
    | "marketenderinnen" -> hasAnyInstrument [ "sonstiges Instrument" ]
    | "tiefes-blech" -> hasAnyInstrument [ "Tenorhorn"; "Tuba B"; "Zugposaune"; "Waldhorn" ]
    | "hohes-blech" -> hasAnyInstrument [ "Flügelhorn"; "Trompete" ]
    | "schlagzeug" -> hasAnyInstrument [ "Schlagzeug" ]
    | "querfloete-und-oboe" -> hasAnyInstrument [ "Querflöte"; "Oboe" ]
    | _ -> false

let getIndexed() =
    Members.items
    |> List.map (fun m -> m.BMVId, m)
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

let firstWithRole role =
    Members.items
    |> Seq.find (fun m -> m.Roles |> Seq.contains role)
