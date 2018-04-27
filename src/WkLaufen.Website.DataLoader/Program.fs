module Main

open System
open System.IO
open DataModels

let downloadMembers credentials dataDir imageBaseDir =
    Directory.CreateDirectory dataDir |> ignore

    let result =
        Members.download credentials
        |> Async.bind (
            Choice.bindAsync (
                List.map (fun m ->
                    Members.tryDownloadImage imageBaseDir m
                    |> AsyncChoice.map (fun () -> m.Member)
                )
                >> Async.ofList
                >> Async.map Choice.ofList
            )
        )
        |> Async.RunSynchronously

    match result with
    | Choice1Of2 members ->
        File.WriteAllText(dataDir @@ "Members.generated.fs", Members.serialize members)
        printfn "Successfully downloaded members."
    | Choice2Of2 x -> failwithf "Error while downloading members. %s" x

let downloadContests credentials dataDir =
    Contests.download credentials
    |> Async.RunSynchronously
    |> Choice.map Contests.serialize
    |> Choice.map (fun s -> File.WriteAllText(dataDir @@ "Contests.generated.fs", s))
    |> function
    | Choice1Of2 () -> printfn "Successfully downloaded contests."
    | Choice2Of2 x -> failwithf "Error while downloading contests. %s" x

let downloadNews accessToken dataDir imageBaseDir =
    let newsBaseDir = imageBaseDir @@ "news"
    Directory.CreateDirectory newsBaseDir |> ignore

    News.download accessToken
    |> Async.bind (Choice.bindAsync ((List.map (News.downloadImages newsBaseDir)) >> Async.ofList >> Async.map Choice.ofList))
    |> Async.RunSynchronously
    |> Choice.map News.serialize
    |> Choice.map (fun s -> File.WriteAllText(dataDir @@ "News.generated.fs", s))
    |> function
    | Choice1Of2 () -> printfn "Successfully downloaded news."
    | Choice2Of2 x -> failwithf "Error while downloading news. %s" x

let downloadActivities publicCalendarUrl internalCalendarUrl dataDir targetDir =
    Directory.CreateDirectory targetDir |> ignore

    [
        "public.ics", publicCalendarUrl
        "internal.ics", internalCalendarUrl
    ]
    |> List.iter (fun (fileName, url) ->
        Http.get url
        |> AsyncChoice.bindAsync Http.getContentString
        |> AsyncChoice.map (fun content -> File.WriteAllText(targetDir @@ fileName, content))
        |> Async.RunSynchronously
        |> function
        | Choice1Of2 () -> printfn "Successfully downloaded calendar %s." fileName
        | Choice2Of2 x -> failwithf "Error while downloading calendar %s. %s" fileName x
    )

    try
        Activities.fromFile (targetDir @@ "public.ics")
        |> Seq.sortBy (fun a -> ActivityTimestamp.unwrap a.BeginTime)
        |> Activities.serialize
        |> fun s -> File.WriteAllText(dataDir @@ "Activities.generated.fs", s)
        printfn "Successfully generated data for activities."
    with e ->
        failwithf "Error while generated data for activities. %O" e

let tryGetArg args name =
    args
    |> Seq.skipWhile (fun v -> String.equalsIgnoreCase v ("--" + name) |> not)
    |> Seq.truncate 2
    |> Seq.tryLast

[<EntryPoint>]
let main argv =
    match
        tryGetArg argv "ooebv-username",
        tryGetArg argv "ooebv-password",
        tryGetArg argv "facebook-access-token",
        tryGetArg argv "public-calendar-url",
        tryGetArg argv "internal-calendar-url" with
    | Some ooebvUsername,
      Some ooebvPassword,
      Some facebookAccessToken,
      Some publicCalendarUrl,
      Some internalCalendarUrl ->
        let rootDir = Path.GetFullPath @"..\.."
        let dataDir = rootDir @@ "src" @@ "WkLaufen.Website" @@ "data"
        let imageDir = rootDir @@ "assets" @@ "images"
        let deployDir = rootDir @@ "public"

        downloadMembers (ooebvUsername, ooebvPassword) dataDir imageDir
        downloadContests (ooebvUsername, ooebvPassword) dataDir
        downloadNews facebookAccessToken dataDir imageDir
        downloadActivities (Uri publicCalendarUrl) (Uri internalCalendarUrl) dataDir (deployDir @@ "calendar")
        ImageResize.resizeImages dataDir imageDir deployDir "images"
        0
    | _ ->
        eprintfn "Usage: dotnet run -- --ooebv-username <username> --ooebv-password <password> --facebook-access-token <access-token> --public-calendar-url <url> --internal-calendar-url <url>"
        1
