module App.Data

open FSharp.Data

module Members =
  type MemberGroup = {
    Id: string
    Name: string
    Photo: string
  }

  let private isMember groupId (m: DataModels.LocalMember) =
    let hasAnyInstrument instruments =
      m.Member.Instruments
      |> List.truncate 1
      |> Set.ofList
      |> Set.intersect (instruments |> Set.ofList)
      |> Set.isEmpty
      |> not
    match groupId with
    | "vorstandsteam" -> m.Member.Roles |> List.isEmpty |> not
    | "saxophon" -> hasAnyInstrument [ "Saxophon" ]
    | "klarinette-und-fagott" -> hasAnyInstrument [ "B-Klarinette"; "Fagott" ]
    | "marketenderinnen" -> m.Member.Instruments |> List.isEmpty || hasAnyInstrument [ "Sonstige" ]
    | "tiefes-blech" -> hasAnyInstrument [ "Tenorhorn"; "Tuba"; "Posaune"; "Horn" ]
    | "hohes-blech" -> hasAnyInstrument [ "Fl\u00fcgelhorn"; "Trompete" ]
    | "schlagzeug" -> hasAnyInstrument [ "Schlagzeug" ]
    | "querfloete" -> hasAnyInstrument [ "Querfl\u00F6te" ]
    | _ -> false

  let getIndexed() =
    Generated.Members.items
    |> List.map (fun m -> m.Member.OoebvId, m)
    |> Map.ofList

  let getGroups() =
    let groups = [
      {
        Id = "querfloete"
        Name = "Querflöte"
        Photo = "querfloete.png"
      }
      {
        Id = "klarinette-und-fagott"
        Name = "Klarinette & Fagott"
        Photo = "klarinette-fagott.png"
      }
      {
        Id = "saxophon"
        Name = "Saxophon"
        Photo = "saxophon.png"
      }
      {
        Id = "hohes-blech"
        Name = "Hohes Blech"
        Photo = "hohes-blech.png"
      }
      {
        Id = "vorstandsteam"
        Name = "Vorstandsteam"
        Photo = "vorstandsteam.png"
      }
      {
        Id = "tiefes-blech"
        Name = "Tiefes Blech"
        Photo = "tiefes-blech.png"
      }
      {
        Id = "schlagzeug"
        Name = "Schlagzeug"
        Photo = "schlagzeug.png"
      }
      {
        Id = "marketenderinnen"
        Name = "Marketenderinnen"
        Photo = "marketenderinnen.jpg"
      }
    ]
    groups
    |> List.map (fun g ->
      let groupMembers =
        Generated.Members.items
        |> List.filter (isMember g.Id)
      g, groupMembers
    )

module News =
  open System

  type NewsItem = {
    Timestamp: DateTime
  }

  let items = Generated.News.items