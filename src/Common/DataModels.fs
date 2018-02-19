module DataModels

open System

type Member = {
    OoebvId: int
    FirstName: string
    LastName: string
    DateOfBirth: DateTime option
    City: string
    Phones: string list
    Email: string option
    MemberSince: DateTime option
    Roles: string list
    Instruments: string list
}

type OoebvMember = {
    Member: Member
    Image: Uri option
    IsActive: bool
}

type NewsEntry = {
    Timestamp: DateTime
    Content: string
}

type FacebookNews = {
    Id: string
    News: NewsEntry
    Images: Uri list
}

type LocalNews = {
    Id: string
    News: NewsEntry
    Images: string list
    SourceUri: string
}

[<AutoOpen>]
module ContestType_ =
    type ContestType =
        | Concert
        | Marching

module ContestType =
    let toString = function
        | Concert -> "Konzertwertung"
        | Marching -> "Marschwertung"
    let toPluralString = function
        | Concert -> "Konzertwertungen"
        | Marching -> "Marschwertungen"
    let fromString = function
        | "Konzertwertung" -> Some Concert
        | "Marschwertung" -> Some Marching
        | _ -> None

type Contest = {
    Year: int
    Type: ContestType
    Category: string
    Points: float
    Result: string
    Location: string
}
