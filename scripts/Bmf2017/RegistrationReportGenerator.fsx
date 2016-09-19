#if COMPILED
module RegistrationReportGenerator
open WkLaufen.Bmf2017.Form
open WkLaufen.Bmf2017.RegistrationForm
#endif

#if INTERACTIVE
#load @"..\..\WkLaufen.Bmf2017\RegistrationForm.fsx"
//#load "ReportGenerator.fsx"
open Form
open RegistrationForm
#endif

module Report =
    open ReportGenerator
    open ReportGenerator.Report

    let participatesAt day =
        isValueChecked day.Key participationDaysKey

    let getSectionReportGenerator = function
        | Info data ->
            [
                yield addReport "== Info"
                yield!
                    data
                    |> List.collect id
                    |> List.collect getInputReportGenerator
                yield addReport ""
            ]
        | Participation data ->
            [
                yield addReport "== Marschwertung & Festakt"
                yield!
                    data
                    |> List.collect (fun (day, participate, participationType) ->
                        let getReport (day: Day) = function
                            | CheckboxInput participate, Some (RadioboxInput participationType) ->
                                let participationTypes =
                                    participationType.Items
                                    |> List.map (fun item ->
                                        sprintf "\"%s\" => \"%s\"" item.Value item.Description
                                    )
                                    |> String.concat ", "
                                [
                                    sprintf "if (!%s)" (participatesAt day)
                                    sprintf "{"
                                    addReport "%s: Keine Teilnahme" day.Name |> indent 1
                                    sprintf "}"
                                    sprintf "else"
                                    sprintf "{"
                                    sprintf "$participationTypes = array(%s);" participationTypes |> indent 1
                                    addReport "%s: {$participationTypes[%s]}" day.Name (getFormDataVar participationType.Name) |> indent 1
                                    sprintf "}"
                                ]
                            | CheckboxInput participate, None ->
                                [
                                    sprintf "if (!%s)" (participatesAt day)
                                    sprintf "{"
                                    addReport "%s: \u2718" day.Name |> indent 1
                                    sprintf "}"
                                    sprintf "else"
                                    sprintf "{"
                                    addReport "%s: \u2714" day.Name |> indent 1
                                    sprintf "}"
                                ]
                            | _ -> failwith "not implemented"
                        getReport day (participate, participationType)
                    )
                yield addReport ""
            ]
        | Reservations (enabled, data) ->
            [
                yield addReport "== Zimmerreservierung"
                yield sprintf "if(%s[0])" (getFormDataVar enabled.Name)
                yield "{"
                yield!
                    data
                    |> List.collect (fun (day, reservations) ->
                        [
                            yield sprintf "if(%s)" (participatesAt day)
                            yield sprintf "{"
                            yield!
                                [
                                    yield addReport "=== %s" day.Name
                                    yield!
                                        reservations
                                        |> List.collect getInputReportGenerator
                                    yield addReport ""
                                ]
                                |> List.map (indent 1)
                            yield sprintf "}"
                        ]
                    )
                    |> List.map (indent 1)
                yield "}"
                yield "else"
                yield "{"
                yield addReport "Nicht ben\u00f6tigt" |> indent 1
                yield addReport "" |> indent 1
                yield "}"
            ]
        | Food data -> 
            [
                yield addReport "== Vorbestellung Festzelt"
                yield!
                    data
                    |> List.collect (fun (day, reservations) ->
                        [
                            yield sprintf "if(%s)" (participatesAt day)
                            yield sprintf "{"
                            yield!
                                [
                                    yield addReport "=== %s" day.Name
                                    yield!
                                        reservations
                                        |> List.collect (fst >> getInputReportGenerator)
                                    yield addReport ""
                                ]
                                |> List.map (indent 1)
                            yield sprintf "}"
                        ]
                    )
            ]
        | Notes data ->
            match data with
            | TextAreaInput data ->
                [
                    addReport "== %s" data.Common.Description
                    addReport "%s" (getPostVarInString data.Common.Name)
                ]
            | _ -> failwith "not implemented"

    let generate = ReportGenerator.Report.generate getSectionReportGenerator

module Validation =
    open ReportGenerator
    open ReportGenerator.Validation

    let getSectionValidatorNames = function
        | Info data ->
            data
            |> List.collect id
            |> List.map getValidatorName
        | Participation data ->
            data
            |> List.collect (fun (_, p1, p2) -> [ yield p1; yield! p2 |> Option.toList] )
            |> List.map getValidatorName
        | Reservations (enabled, data) ->
            [
                yield enabled.Name
                yield!
                    data
                    |> List.collect snd
                    |> List.map getValidatorName
            ]
        | Food data ->
            data
            |> List.collect snd
            |> List.map (fst >> getValidatorName)
        | Notes data -> [ getValidatorName data ]

    let generate = ReportGenerator.Validation.generate getSectionValidatorNames

let generateRegistrationHandler() =
    ReportGenerator.generateValidatedReportGenerator (Validation.generate formSections) (Report.generate formSections)
