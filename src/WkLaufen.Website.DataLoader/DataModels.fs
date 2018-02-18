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

type LocalMember = {
    Member: Member
    Image: string option
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
