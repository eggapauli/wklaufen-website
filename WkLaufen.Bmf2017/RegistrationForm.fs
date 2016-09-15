module WkLaufen.Bmf2017.RegistrationForm

type Day = { Key : string; Name: string }
type SimpleInputData = { Name: string; Description: string }
type CheckboxInputItem = { Value: string; Description: string; Checked: bool }
type CheckboxInputData = { Name: string; Description: string option; Items: CheckboxInputItem list }
type TextAreaInputData = { Common: SimpleInputData; Rows: string; Cols: string }

type FormData =
    | TextInput of SimpleInputData
    | NumberInputWithPrefixTitle of SimpleInputData
    | NumberInputWithPostfixTitle of SimpleInputData
    | CheckboxInput of CheckboxInputData
    | RadioboxInput of CheckboxInputData
    | TextAreaInput of TextAreaInputData

type FormSection =
    | Info of FormData list list
    | Participation of (Day * FormData * FormData) list
    | Reservations of (Day * FormData list) list
    | Food of (Day * (FormData * int) list) list
    | Notes

let friday = { Key = "friday"; Name = "Freitag, 9. Juni 2017" }
let saturday = { Key = "saturday"; Name = "Samstag, 10. Juni 2017" }
let days = [ friday; saturday ]

let private info =
    Info [
        [ TextInput { Name = "club-name"; Description = "Vereinsname" } ]
        [ TextInput { Name = "contact-person"; Description = "Eure Ansprechperson" } ]
        [
            NumberInputWithPrefixTitle { Name = "number-of-participants"; Description = "Anzahl TeilnehmerInnen" }
            NumberInputWithPrefixTitle { Name = "number-of-club-members"; Description = "davon MusikerInnen" }
        ]
        [ TextInput { Name = "phone"; Description = "Telefon" } ]
        [ TextInput { Name = "email"; Description = "E-Mail" } ]
        [ TextInput { Name = "address"; Description = "Adresse" } ]
        [ TextInput { Name = "city"; Description = "PLZ/Ort" } ]
    ]

let participationDaysKey = "participation-days"

let private participation =
    let emptyParticipation = { Name = participationDaysKey; Description = None; Items = [ ] }
    let participateOnFriday = CheckboxInput {
        emptyParticipation with
            Items =
            [
                { Value = "friday"; Description = "Freitag, 9. Juni 2017"; Checked = true }
            ]
    }

    let participateOnSaturday = CheckboxInput {
        emptyParticipation with
            Items =
            [
                { Value = "saturday"; Description = "Samstag, 10. Juni 2017"; Checked = true }
            ]
    }

    let participationTypeTemplate = {
        Name = "participation-type-"; Description = Some "Teilnahme"
        Items =
        [
            { Value = "marschwertung"; Description = "Marschwertung"; Checked = true }
            { Value = "gastkapelle"; Description = "Gastkapelle"; Checked = false }
        ]
    }

    let participationTypeOnFriday = RadioboxInput {
        participationTypeTemplate with
            Name = participationTypeTemplate.Name + "friday"
    }

    let participationTypeOnSaturday = RadioboxInput {
        participationTypeTemplate with
            Name = participationTypeTemplate.Name + "saturday"
    }

    Participation [
        friday, participateOnFriday, participationTypeOnFriday
        saturday, participateOnSaturday, participationTypeOnSaturday
    ]

let private reservations =
    days
    |> List.map (fun day ->
        day,
        [
            NumberInputWithPostfixTitle { Name = (sprintf "%s-reservation-doppelzimmer" day.Key); Description = "Doppelzimmer" }
            NumberInputWithPostfixTitle { Name = (sprintf "%s-reservation-einzelzimmer" day.Key); Description = "Einzelzimmer" }
            NumberInputWithPostfixTitle { Name = (sprintf "%s-reservation-mehrbettzimmer" day.Key); Description = "Personen in Mehrbettzimmern" }
        ]
    )
    |> Reservations

let private food =
    days
    |> List.map (fun day ->
        day,
        [
            NumberInputWithPostfixTitle { Name = (sprintf "%s-food-schnitzel" day.Key); Description = "Schnitzerl" }, 9
            NumberInputWithPostfixTitle { Name = (sprintf "%s-food-bratwuerstel" day.Key); Description = "Bratw\u00fcrstel" }, 100
            NumberInputWithPostfixTitle { Name = (sprintf "%s-food-hendl" day.Key); Description = "Hendl" }, 101
            NumberInputWithPostfixTitle { Name = (sprintf "%s-food-bierfass" day.Key); Description = "15 l Fass am Tisch" }, 102
            NumberInputWithPostfixTitle { Name = (sprintf "%s-food-veggie" day.Key); Description = "Vegetarisch" }, 0
            NumberInputWithPostfixTitle { Name = (sprintf "%s-food-anti" day.Key); Description = "Kiste Anti gemischt" }, 5
        ]
    )
    |> Food

let formSections = [
    info
    participation
    reservations
    food
    Notes
]
