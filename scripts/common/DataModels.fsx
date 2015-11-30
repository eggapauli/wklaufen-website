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
