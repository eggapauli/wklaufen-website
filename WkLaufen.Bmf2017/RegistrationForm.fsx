#if COMPILED
module WkLaufen.Bmf2017.RegistrationForm
open WkLaufen.Bmf2017.Form
#endif

#if INTERACTIVE
//#load "Form.fsx"
open Form
#endif

type Day = { Key : string; Name: string }

type FormSection =
    | Info of Input list
    | Participation of (Day * Input * Input option) list
    | Food of (Day * (Input * decimal) list) list
    | ClubInfo of Input
    | Arrival of Input

let friday = { Key = "friday"; Name = "Freitag, 9. Juni 2017" }
let saturday = { Key = "saturday"; Name = "Samstag, 10. Juni 2017" }
let sunday = { Key = "sunday"; Name = "Sonntag, 11. Juni 2017" }

let private info =
    Info [
        TextInput { Name = "club-name"; Description = "Vereinsname" }
        TextInput { Name = "contact-person"; Description = "Eure Ansprechperson" }
        TextInput { Name = "phone"; Description = "Telefon" }
        TextInput { Name = "email"; Description = "E-Mail" }
        TextInput { Name = "address"; Description = "Adresse" }
        TextInput { Name = "city"; Description = "PLZ/Ort" }
        NumberInputWithPrefixTitle { Name = "adults"; Description = "MusikerInnen über 18 Jahren" }
        NumberInputWithPrefixTitle { Name = "teenagers"; Description = "MusikerInnen zwischen 16 und 18 Jahren" }
        NumberInputWithPrefixTitle { Name = "children"; Description = "MusikerInnen unter 16 Jahren" }
    ]

let participationDaysKey = "participation-days"

let private participation =
    let emptyParticipation = { Name = participationDaysKey; Description = None; Items = [ ] }
    let participateOnFriday = CheckboxInput {
        emptyParticipation with
            Items =
            [
                { Value = friday.Key; Description = friday.Name; Checked = true }
            ]
    }

    let participateOnSaturday = CheckboxInput {
        emptyParticipation with
            Items =
            [
                { Value = saturday.Key; Description = saturday.Name; Checked = true }
            ]
    }

    let participateOnSunday = CheckboxInput {
        emptyParticipation with
            Items =
            [
                { Value = sunday.Key; Description = sunday.Name; Checked = true }
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
        friday, participateOnFriday, Some participationTypeOnFriday
        saturday, participateOnSaturday, Some participationTypeOnSaturday
        sunday, participateOnSunday, None
    ]

let private food =
    [ friday; saturday; sunday ]
    |> List.map (fun day ->
        day,
        [
            if day = sunday then
                yield NumberInputWithPostfixTitle { Name = (sprintf "%s-food-surschopf" day.Key); Description = "Surschopf mit Knödel & Speckkrautsalat (A,C,G,L,M)" }, 10.5m
            yield NumberInputWithPostfixTitle { Name = (sprintf "%s-food-hendl" day.Key); Description = "\u00BD Grillhendl mit Pommes (M)" }, 9.0m
            yield NumberInputWithPostfixTitle { Name = (sprintf "%s-food-schnitzel" day.Key); Description = "Schnitzel mit Pommes (A,C,M)" }, 8.5m
            yield NumberInputWithPostfixTitle { Name = (sprintf "%s-food-gemueselaibchen" day.Key); Description = "Gemüselaibchen auf Salat mit Dip (A,C,F,G,H,L,M)" }, 7.5m
            yield NumberInputWithPostfixTitle { Name = (sprintf "%s-food-bratwuerstel" day.Key); Description = "Bratwürstel mit Sauerkraut, Senf & Brot (A,L,M,O)" }, 4.0m
            yield NumberInputWithPostfixTitle { Name = (sprintf "%s-food-bierfass" day.Key); Description = "Bierfass 15 Liter am Tisch" }, 90.0m
            yield NumberInputWithPostfixTitle { Name = (sprintf "%s-food-anti" day.Key); Description = "Kiste Anti gemischt (24 x 0,3 l Flaschen)" }, 50.0m
        ]
    )
    |> Food

let private arrival =
    {
        Name = "arrival"
        Description = None
        Items =
        [
            { Value = "bus"; Description = "Bus"; Checked = true }
            { Value = "car"; Description = "Pkw"; Checked = false }
            { Value = "train"; Description = "Zug (Möglichst bald anmelden! -> Organisation Sonderzüge)"; Checked = false }
        ]
    }
    |> RadioboxInput
    |> Arrival

let private clubInfo =
    TextAreaInput { Common = { Name = "club-info"; Description = "Infos über den Verein für den Platzsprecher"}; Rows = "7"; Cols = "70" }
    |> ClubInfo

let formSections = [
    info
    participation
    food
    arrival
    clubInfo
]
