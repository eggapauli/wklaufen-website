module Main

open System
open System.IO
open DataModels

let downloadMembers httpClient dataDir imageBaseDir = async {
    let! members = BMV.getMembers httpClient
    members
    |> List.iter (fun m ->
        match m.ImageContent with
        | Some imageContent ->
            let memberImageDir = Path.Combine(imageBaseDir, "members")
            Directory.CreateDirectory memberImageDir |> ignore
            File.WriteAllBytes(Path.Combine(memberImageDir, sprintf "%s.jpg" m.Member.BMVId), imageContent)
            // File.WriteAllBytes(Path.Combine(memberImageDir, sprintf "%s %s.jpg" m.Member.LastName m.Member.FirstName), imageContent)
        | None -> ()
    )
    File.WriteAllText(dataDir @@ "Members.generated.fs", members |> List.map (fun v -> v.Member) |> Members.serialize)
}

let downloadContests httpClient clubId dataDir = async {
    let! contests = BMV.getContests httpClient clubId

    File.WriteAllText(dataDir @@ "Contests.generated.fs", Contests.serialize contests)
}

let downloadActivities calendarUrl dataDir targetDir =
    Directory.CreateDirectory targetDir |> ignore

    Http.get calendarUrl
    |> AsyncChoice.bindAsync Http.getContentString
    |> AsyncChoice.map (Activities.fixKonzertmeisterCalendar >> fun content -> File.WriteAllText(targetDir @@ "public.ics", content))
    |> Async.RunSynchronously
    |> function
    | Choice1Of2 () -> printfn "Successfully downloaded calendar."
    | Choice2Of2 x -> failwithf "Error while downloading calendar: %s" x

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
    let bmvUsername = getEnvVarOrFail "BMV_USERNAME"
    let bmvPassword = getEnvVarOrFail "BMV_PASSWORD"
    let bmvClubId = getEnvVarOrFail "BMV_CLUB_ID"
    let calendarUrl = getEnvVarOrFail "CALENDAR_URL"

    let rootDir = Path.GetFullPath "."
    let dataDir = rootDir @@ "src" @@ "WkLaufen.Website" @@ "data"
    Directory.CreateDirectory dataDir |> ignore
    let imageDir = rootDir @@ "assets" @@ "images"
    let deployDir = rootDir @@ "public"

    BMV.runAsLoggedIn (bmvUsername, bmvPassword) (fun httpClient -> async {
        do! downloadMembers httpClient dataDir imageDir
        do! downloadContests httpClient bmvClubId dataDir
    })
    |> Async.RunSynchronously
    downloadActivities (Uri calendarUrl) dataDir (deployDir @@ "calendar")
    ImageResize.resizeImages dataDir imageDir deployDir "images"
    0
