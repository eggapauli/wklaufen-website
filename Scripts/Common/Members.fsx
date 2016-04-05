#if INTERACTIVE
#I @"..\..\"
#r @"packages\FSharp.Data\lib\net40\FSharp.Data.dll"
#load "Async.fsx"
#load "Choice.fsx"
#load "DataModels.fsx"
#load "Http.fsx"
#load "Json.fsx"
#load "OOEBV.fsx"
#load "DownloadHelper.fsx"
#endif

#if COMPILED
module Members
#endif

open System
open System.IO
open System.Text.RegularExpressions
open DownloadHelper

let download credentials =
    OOEBV.login credentials
    |> Async.bind (Choice.bindAsync OOEBV.loadAndResetMemberOverviewPage)
    |> Async.bind (Choice.bindAsync OOEBV.loadActiveMembers)

let getJson (m: DataModels.OoebvMember, image) =
    let formatPhoneNumber text =
        Regex.Replace(text, @"^(43|0043|\+43)", "0")
        |> fun x -> Regex.Replace(x, @"\D", "")

    sprintf """{
        "OoebvId": %d,
        "FirstName": "%s",
        "LastName": "%s",
        "DateOfBirth": %s,
        "Roles": %s,
        "Phone": %s,
        "Email": %s,
        "Photo": %s,
        "Instruments": %s,
        "MemberSince": %s,
        "City": "%s"
    }"""
        m.Member.OOEBVId
        m.Member.FirstName
        m.Member.LastName
        (m.Member.DateOfBirth |> Json.fromDateOption)
        (m.Member.Roles |> Json.fromArray)
        (m.Member.Phone |> Option.map formatPhoneNumber |> Json.fromOption)
        (m.Member.Email |> Json.fromOption)
        (image |> Json.fromOption)
        (m.Member.Instruments |> Json.fromArray)
        (m.Member.MemberSince |> Json.fromDateOption)
        m.Member.City

let tryDownloadImage baseDir (m: DataModels.OoebvMember) =
    match m.Image with
    | Some imageUri ->
        let fileName = sprintf "%d%s" m.Member.OOEBVId (getExtension imageUri)
        let filePath = Path.Combine(baseDir, "members", fileName)
        tryDownload imageUri filePath
        |> Async.map (Choice.map (fun () -> m, Some fileName))
    | None -> Async.unit (Choice1Of2 (m, None))