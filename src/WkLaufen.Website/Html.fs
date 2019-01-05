module App.Html

open Fable.Core.JsInterop
open Fable.Helpers.React
open Fable.Helpers.React.Props
open System
open System.Text.RegularExpressions
open global.Data

let splitFileName (fileName: string) =
  match fileName.LastIndexOf '.' with
  | -1 -> fileName, ""
  | idx -> fileName.Substring(0, idx), fileName.Substring idx

let image path (width: int option, height: int option) =
  [
    Src path |> Some
    // width |> Option.map (fun v -> Fable.Helpers.React.Props.Width v)
    // height |> Option.map (fun v -> Fable.Helpers.React.Props.Height v)
  ]
  |> List.choose id
  |> List.map (fun p -> p :> IHTMLProp)
  |> img

let menuItem bgImagePath text href =
  li
    [ ClassName "menu-item"; Style [ BackgroundImage (sprintf "url(%s)" bgImagePath) ] ]
    [
      a
        [ ClassName "menu-item"; Href href ]
        [
          span [ ClassName "bg"; Style [ BackgroundImage (sprintf "url(%s)" bgImagePath) ] ] []      
          span [ ClassName "title-bar" ] [ span [ ClassName "title" ] [ str text ] ]
        ]
    ]

let private random = Random()
let obfuscate (text: string) =
  text
  |> Seq.map (fun ch ->
    let chars = "abcdefghijklmnopqrstuvwxyz0123456789"
    let randomChar = chars.[random.Next(chars.Length)] |> string
    [
      span [ Style [ Display "none" ] ] [ str randomChar ]
      span [] [ str (string ch) ]
    ]
  )
  |> List.concat

let phone (m: DataModels.Member) =
  match m.Phones |> List.tryHead with
  | Some phone -> span [ ClassName "phone" ] (obfuscate phone) |> Some
  | None -> None

let emailAddress (m: DataModels.Member) =
  match m.EmailAddresses |> List.tryHead with
  | Some email -> span [ ClassName "email" ] (obfuscate email) |> Some
  | None -> None

let (|Uri|_|) str =
  if Regex.IsMatch(str, "^https?://")
  then Some str
  else None

let htmlify (text: string) =
  Regex.Matches(text, @"\S+|(\r\n|\r|\n)|\s+")
  |> Seq.cast<Match>
  |> Seq.map (fun m ->
    match m.Value with
    | Uri uri -> a [ Href (uri.ToString()) ] [ str (uri.ToString()) ]
    | "\r" | "\n" | "\r\n" -> br []
    | text -> str text
  )
  |> Seq.toList

let pdfDoc url =
  object [ Class "flyer"; !!("data", url); Fable.Helpers.React.Props.Type "application/pdf" ]
    [
      div [ Class "rich-text not-supported-warning" ]
        [
            h2 [] [ str "PDF kann nicht angezeigt werden" ]
            div [] [
              str "Klicken Sie "
              a [ Href url ] [ str "hier" ]
              str " um die Datei direkt herunterzuladen oder versuchen Sie es mit einem anderen Browser od. Endgerät."  
            ]
        ]
    ]

let contact (m: DataModels.Member) =
  div [ ClassName "contact" ] [
    yield div [ ClassName "image" ] (
      Images.contacts
      |> Map.tryFind (string m.OoebvId)
      |> Option.map (fun p ->  image p (Some 110, Some 160))
      |> Option.toList
    )
    yield span [ ClassName "name" ] [ str (sprintf "%s %s" m.FirstName m.LastName) ]
    yield br []
    yield span [ ClassName "roles" ] [ str (m.Roles |> String.concat ", ") ]
    yield!
      match phone m with
      | Some x -> [ br []; x ]
      | None -> [ ]
    yield!
      emailAddress m
      |> Option.toList
      |> List.append [ br [] ]
    yield div [ ClassName "clear" ] []
  ]
