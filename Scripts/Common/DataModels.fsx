#if COMPILED
module DataModels
#endif

open System

type Member = {
    OOEBVId: int
    FirstName: string
    LastName: string
    DateOfBirth: DateTime option
    City: string
    Phone: string option
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

type ContentfulMember = {
    Id: string
    Version: int
    Member: Member
    ImageId: string option
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

type ContentfulNews = {
    Id: string
    FacebookId: string option
    Version: int
    News: NewsEntry
    Title: string
    ImageIds: string list
}

type ContestType =
    | Concert
    | Marching

module ContestType =
    let toString = function
        | Concert -> "Konzertwertung"
        | Marching -> "Marschwertung"
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
