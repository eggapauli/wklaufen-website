module Members

open System
open DataModels

let serializeMember (m: DataModels.Member) =
    [
        yield "{"
        yield!
            [
                yield sprintf "BMVId = %s" (Serialize.string m.BMVId)
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
