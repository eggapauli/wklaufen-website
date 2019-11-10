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

let getEnvVarOrFail name =
    let value = Environment.GetEnvironmentVariable name
    if isNull value
    then failwithf "Environment variable \"%s\" not set" name
    else value

[<EntryPoint>]
let main argv =
    let ooebvUsername = getEnvVarOrFail "OOEBV_USERNAME"
    let ooebvPassword = getEnvVarOrFail "OOEBV_PASSWORD"
    let publicCalendarUrl = getEnvVarOrFail "PUBLIC_CALENDAR_URL"
    let internalCalendarUrl = getEnvVarOrFail "INTERNAL_CALENDAR_URL"

    let rootDir = Path.GetFullPath "."
    let dataDir = rootDir @@ "src" @@ "WkLaufen.Website" @@ "data"
    let imageDir = rootDir @@ "assets" @@ "images"
    let deployDir = rootDir @@ "public"

    downloadMembers (ooebvUsername, ooebvPassword) dataDir imageDir
    downloadContests (ooebvUsername, ooebvPassword) dataDir
    downloadActivities (Uri publicCalendarUrl) (Uri internalCalendarUrl) dataDir (deployDir @@ "calendar")
    ImageResize.resizeImages dataDir imageDir deployDir "images"
    0
