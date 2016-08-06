module WkLaufen.Website.Pages.Contacts

open WkLaufen.Website
open WebSharper.Html.Server

let Page ctx =
    Templating.Main ctx EndPoint.Contacts
        {
            Id = "contacts"
            Title = Html.pages.Contacts.Title
            Css = [ "contacts.css" ]
            BackgroundImageUrl = Html.pages.Contacts.BackgroundImage
            Body =
            [
                Div [Class "info"] -< [
                    H1 [Text Html.pages.Contacts.Title]
                    Div [VerbatimContent (Html.md.Transform Html.pages.Contacts.Address)]
                ]
                Div [Class "contacts"] -< (
                    Html.pages.Contacts.Members
                    |> Seq.map (fun memberId ->
                        let m =
                            Html.memberLookup
                            |> Map.find memberId
                        Div [Class "contact"] -< [
                            Div [Class "image"] -<
                                (m.Photo
                                |> Option.map (fun p -> Asset.htmlImage "members" p (Some 110, Some 160))
                                |> Option.toList)
                            Span [Text (sprintf "%s %s" m.FirstName m.LastName)]
                            Br []
                            Span [Text (m.Roles |> String.concat ", ")]
                            Br []
                            Span [] -< Html.obfuscatePhone m.Phone
                            Br []
                            Span [] -< Html.obfuscateEmail m.Email
                        ]
                    )
                )
            ]
        }