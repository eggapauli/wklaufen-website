#if INTERACTIVE
#I @"..\Common\"
#load "Members.fsx"
#endif

open System.IO

let credentials = ("<username>", "<password>")

let memberImageDir = @"C:\Users\Johannes\Workspace\wklaufen\design\mitglieder"

let tryFindImage memberImageDir (m: DataModels.Member) =
    let memberName = sprintf "%s %s" m.LastName m.FirstName
    Directory.EnumerateFiles memberImageDir
    |> Seq.tryFind (fun file -> Path.GetFileNameWithoutExtension file = memberName)

let getNewPath oldPath newName =
    Path.Combine(Path.GetDirectoryName oldPath, newName + Path.GetExtension oldPath)

Members.download credentials
|> Async.map
    (Choice.map
        (List.choose <| fun m ->
            tryFindImage memberImageDir m.Member
            |> Option.map (fun image -> image, getNewPath image (string m.Member.OOEBVId))
        )
    )
|> Async.RunSynchronously
|> function
| Choice1Of2 images ->
    images
    |> List.iter File.Move
| Choice2Of2 error -> eprintfn "ERROR: %s" error
