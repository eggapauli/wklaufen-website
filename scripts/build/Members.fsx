#I @"..\..\"
#r @"packages\FSharp.Data\lib\net40\FSharp.Data.dll"

//#load @"..\common\DataModels.fsx"
//#load @"..\common\Json.fsx"
//#load @"..\common\OOEBV.fsx"
//#load @".\DownloadHelper.fsx"

open System
open System.IO
open System.Text.RegularExpressions
open DownloadHelper

let download (username, password) : Choice<DataModels.OoebvMember list, string> =
    OOEBV.login username password
    |> Choice.bind OOEBV.loadAndResetMemberOverviewPage
    |> Choice.bind OOEBV.loadActiveMembers

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
        |> Choice.map (fun () -> m, Some fileName)
    | None -> Choice1Of2 (m, None)