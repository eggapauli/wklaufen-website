#if INTERACTIVE
#load "AsyncChoice.fsx"
#load "DataModels.fsx"
#load "Http.fsx"
#load "Serialize.fsx"
#load "OOEBV.fsx"
#load "DownloadHelper.fsx"
#endif

#if COMPILED
module Members
#endif

open System
open System.IO
open System.Text.RegularExpressions
open DataModels

let download credentials =
    OOEBV.login credentials
    |> AsyncChoice.bind OOEBV.Members.loadAndResetMemberOverviewPage
    |> AsyncChoice.bind OOEBV.Members.loadActiveMembers

let serializeMember (m: DataModels.Member) =
    [
        yield "{"
        yield!
            [
                yield sprintf "OoebvId = %d" m.OoebvId
                yield sprintf "FirstName = %s" (Serialize.string m.FirstName)
                yield sprintf "LastName = %s" (Serialize.string m.LastName)
                yield sprintf "DateOfBirth = %s" (Serialize.dateOption m.DateOfBirth)
                yield sprintf "Gender = %O" m.Gender
                yield sprintf "City = %s" (Serialize.string m.City)
                yield "Phones ="
                yield! Serialize.stringSeq m.Phones |> List.map (sprintf "  %s")
                yield "EmailAddresses ="
                yield! Serialize.stringSeq m.EmailAddresses |> List.map (sprintf "  %s")
                yield sprintf "MemberSince = %s" (Serialize.dateOption m.MemberSince)
                yield "Roles ="
                yield! m.Roles |> List.map (sprintf "%O") |> Serialize.seq |> List.map (sprintf "  %s")
                yield "Instruments ="
                yield! Serialize.stringSeq m.Instruments |> List.map (sprintf "  %s")
            ]
            |> List.map (sprintf "  %s")
        yield "}"
    ]

let serialize members =
    members
    |> Seq.map (
        serializeMember
        >> List.map (sprintf "    %s")
        >> String.concat Environment.NewLine
    )
    |> String.concat Environment.NewLine
    |> (sprintf """module Data.Members

open DataModels

let items =
  [
%s
  ]""")

let tryDownloadImage baseDir (m: DataModels.OoebvMember) =
    match m.Image with
    | Some imageUri ->
        let fileName = sprintf "%d%s" m.Member.OoebvId (DownloadHelper.getExtension imageUri)
        let filePath = Path.Combine(baseDir, "members", fileName)
        DownloadHelper.tryDownload imageUri filePath
    | None ->
        AsyncChoice.success ()
