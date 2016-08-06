module WkLaufen.Website.Pages.Members

open System
open WkLaufen.Website
open WebSharper.Sitelets
open WebSharper.Html.Server

let GroupsPage (ctx: Context<EndPoint>) =
    Templating.Main ctx EndPoint.MemberGroups
        {
            Id = "member-groups"
            Title = Html.pages.Members.Title
            Css = [ "member-groups.css" ]
            BackgroundImageUrl = Html.pages.Members.BackgroundImage
            Body =
            [
                H1 [Text Html.pages.Members.Title]
                UL [Class "menu"] -< (
                    Html.members
                    |> List.map (fun (group, _) ->
                        Asset.resize "member-groups" group.Photo (Some 200, Some 130)
                        |> Html.menuItem group.Name (ctx.Link (group.Name |> Html.slug |> EndPoint.Members))
                    )
                )
            ]
        }

let MembersPage ctx groupId =
    let (group, members) = Html.members |> List.find (fun (g, _) -> g.Id = groupId)

    Templating.Main (ctx: Context<EndPoint>) EndPoint.Members
        {
            Id = "members"
            Title = group.Name
            Css = [ "members.css" ]
            BackgroundImageUrl = Html.pages.Members.BackgroundImage
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
                                | Some photo -> yield Div [Class "image"] -< [Asset.htmlImage "members" photo (Some 200, Some 270)]
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