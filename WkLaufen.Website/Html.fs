module WkLaufen.Website.Html

open System
open System.Text.RegularExpressions
open WebSharper.Html.Server

let pages = Data.Pages.getAll()
let members = Data.Members.getGroups()
let memberLookup = Data.Members.getIndexed()

let menuItem title href bgImage =
    LI [] -< [
        A [Class "menu-item"; HRef href] -< [
            Div [ Class "bg"; Style (sprintf "background-image: url(%s)" bgImage) ]
            Div [ Class "title-bar" ] -< [
                Span [ Class "title" ] -< [ Text title ]
            ]
        ]
    ]

let slug (text: string) =
    text
        .ToLowerInvariant()
        .Replace("\u00C4", "Ae")
        .Replace("\u00D6", "Oe")
        .Replace("\u00DC", "Ue")
        .Replace("\u00E4", "ae")
        .Replace("\u00F6", "oe")
        .Replace("\u00FC", "ue")
        .Replace("&", "und")
    |> fun t -> Regex.Replace(t, "[^a-zA-Z0-9]+", "-")

let getHref text =
    slug text
    |> sprintf "%s.html"

let md = new MarkdownSharp.Markdown()

let random = new Random();
let obfuscate (text: string) =
    text
    |> Seq.map (fun ch ->
        let chars = "abcdefghijklmnopqrstuvwxyz0123456789"
        let randomChar = chars.[random.Next(chars.Length)] |> string
        [
            Span [Style "display: none"] -< [Text randomChar]
            Span [Text (string ch)]
        ]
    )
    |> List.concat

let obfuscatePhone phoneNumber =
    phoneNumber |> string |> obfuscate

let obfuscateEmail emailAddress =
    emailAddress
    |> Option.map obfuscate
    |> Option.toList
    |> List.concat

let (|Uri|_|) str =
    match Uri.TryCreate(str, UriKind.Absolute) with
    | true, uri when uri.Scheme = "http" || uri.Scheme = "https" -> Some uri
    | _ -> None

let htmlify (text: string) =
    Regex.Matches(text, @"\S+|(\r\n|\r|\n)|\s+")
    |> Seq.cast<System.Text.RegularExpressions.Match>
    |> Seq.map (fun m ->
        match m.Value with
        | Uri uri -> A [HRef (uri.ToString())] -< [ Text (uri.ToString()) ]
        | "\r" | "\n" | "\r\n" -> Br []
        | text -> Text text
    )
    |> Seq.toList