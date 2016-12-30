#if COMPILED
module WkLaufen.Bmf2017.SponsorForm
open WkLaufen.Bmf2017.Form
#endif

#if INTERACTIVE
//#load "Form.fsx"
open Form
#endif

type FormSection =
    | Info of Input list
    | Package of Input
    | Notes of Input

let private info =
    Info [
        TextInput { Name = "organisation"; Description = "Organisation" }
        TextInput { Name = "contact-person"; Description = "Ansprechpartner" }
        TextInput { Name = "email"; Description = "E-Mail" }
        TextInput { Name = "phone"; Description = "Telefon" }
    ]

let private package =
    RadioboxInput {
        Name = "package"
        Description = Some "Hiermit beantrage ich f\u00fcr das Bezirksmusikfest Gmunden das Sponsoring-Paket"
        Items =
        [
            { Value = "bronze"; Description = "Bronze um 100 \u20ac"; Checked = false }
            { Value = "silver"; Description = "Silber um 500 \u20ac"; Checked = false }
            { Value = "gold"; Description = "Gold um 1.000 \u20ac"; Checked = true }
            { Value = "platinum"; Description = "Platin um 5.000 \u20ac"; Checked = false }
        ]
    }
    |> Package

let private notes =
    TextAreaInput { Common = { Name = "notes"; Description = "Fragen, Anmerkungen, etc."}; Rows = "7"; Cols = "50" }
    |> Notes

let formSections = [
    info
    package
    notes
]
