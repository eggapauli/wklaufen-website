module WkLaufen.Website.Pages.Youths

open WkLaufen.Website
open WebSharper.Html.Server

let Page ctx =
    Templating.Main ctx EndPoint.Youths
        {
            Id = "youths"
            Title = Html.pages.Youths.Title
            Css = [ "youths.css" ]
            BackgroundImageUrl = Html.pages.Youths.BackgroundImage
            Body =
            [
                Div [Class "rich-text text"] -< [
                    H1 [Text Html.pages.Youths.Title]
                    Div [VerbatimContent (Html.md.Transform Html.pages.Youths.Content)]
                ]

                Div [Class "contacts-container"] -< [
                    Div [Class "rich-text contacts"] -< (
                        Html.pages.Youths.Members
                        |> Seq.map (fun memberId ->
                            let m = Html.memberLookup |> Map.find memberId
                            Div [Class "contact"] -< [
                                Strong [Text (sprintf "%s %s" m.FirstName m.LastName)]
                                Text (sprintf " (%s): " (m.Roles |> String.concat ", "))
                                Span [] -< Html.obfuscatePhone m.Phone
                                VerbatimContent "&nbsp;"
                                Span [] -< Html.obfuscateEmail m.Email
                            ]
                        )
                    )
                ]
            ]
        }