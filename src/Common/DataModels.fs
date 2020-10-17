module DataModels

open System
open System.Text.RegularExpressions

type Role =
    | Obmann
    | Kapellmeister
    | Jugendorchesterleiter
    | Jugendreferent
    | Other of string

type Gender = | Male | Female | Unspecified

module Role =
    let private genderRole gender role =
        let replacements =
            match gender with
            | Male -> ["/in", ""; "/obfrau", ""]
            | Female ->
                [
                    "/in", "in"
                    "obmann/", ""
                    "Archivarstellvertreter", "Archivarstellvertreterin"
                    "Beirat", "Beirätin"
                ]
            | Unspecified -> []

        let folder (s: string) (p: string, r: string) =
            Regex.Replace(s, p, r, RegexOptions.IgnoreCase)

        List.fold folder role replacements

    let toString gender role =
        match gender, role with
        | Male, Obmann | Unspecified, Obmann -> "Obmann"
        | Female, Obmann -> "Obfrau"
        | Male, Kapellmeister | Unspecified, Kapellmeister -> "Kapellmeister"
        | Female, Kapellmeister -> "Kapellmeisterin"
        | Male, Jugendorchesterleiter | Unspecified, Jugendorchesterleiter -> "Jugendorchesterleiter"
        | Female, Jugendorchesterleiter -> "Jugendorchesterleiterin"
        | Male, Jugendreferent | Unspecified, Jugendreferent -> "Jugendreferent"
        | Female, Jugendreferent -> "Jugendreferentin"
        | role, Other name -> genderRole role name

type Member = {
    BMVId: string
    FirstName: string
    LastName: string
    DateOfBirth: DateTime option
    Gender: Gender
    City: string
    Phones: string list
    EmailAddresses: string list
    MemberSince: DateTime option
    Roles: Role list
    Instruments: string list
}

type BMVMember = {
    Member: Member
    Image: Uri option
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

type ActivityImportance = Important | Normal

type ActivityTimestamp =
    | Date of DateTimeOffset
    | DateTime of DateTimeOffset

module ActivityTimestamp =
    let unwrap = function
        | Date d -> d
        | DateTime d -> d

type Activity = {
    Title: string
    BeginTime: ActivityTimestamp
    EndTime: ActivityTimestamp option
    Location: string option
    Importance: ActivityImportance
}
