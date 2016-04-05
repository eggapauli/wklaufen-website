#if INTERACTIVE
#I "..\Common"
#load "OOEBV.fsx"
#endif

open System.IO

let credentials = ("<username>", "<password>")

let memberImageDir = @"C:\Users\Johannes\Workspace\wklaufen\design\mitglieder\resized"
let memberImages =
    Directory.GetFiles memberImageDir
    |> Seq.map (fun f -> Path.GetFileNameWithoutExtension f |> int, f)
    |> Map.ofSeq

OOEBV.login credentials
|> Async.bind (Choice.bindAsync OOEBV.loadAndResetMemberOverviewPage)
|> Async.bind (Choice.bindAsync (OOEBV.replaceMemberImages memberImages))
|> Async.RunSynchronously
|> function
| Choice1Of2 () -> printfn "Replaced all images"
| Choice2Of2 error -> printfn "ERROR: %s" error
