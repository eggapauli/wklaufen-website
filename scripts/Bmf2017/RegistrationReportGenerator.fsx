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
        | Arrival data ->
            match data with
            | RadioboxInput data ->
                [
                    yield addReport "== Anreise"
                    let map =
                        data.Items
                        |> List.map (fun i -> i.Value, i.Description)
                    yield! reportRadioboxDescription map data.Name
                    yield addReport ""
                ]
            | _ -> failwith "not implemented"
        | ClubInfo data ->
            match data with
            | TextAreaInput data ->
                [
                    addReport "== %s" data.Common.Description
                    addReport "%s" (getPostVarInString data.Common.Name)
                    addReport ""
                ]
            | _ -> failwith "not implemented"

    let generate = ReportGenerator.Report.generate getSectionReportGenerator

module Validation =
    open ReportGenerator
    open ReportGenerator.Validation

    let getSectionValidatorNames = function
        | Info data ->
            data
            |> List.map getValidatorName
        | Participation data ->
            data
            |> List.collect (fun (_, p1, p2) -> [ yield p1; yield! p2 |> Option.toList] )
            |> List.map getValidatorName
        | Food data ->
            data
            |> List.collect snd
            |> List.map (fst >> getValidatorName)
        | Arrival data -> [ getValidatorName data ]
        | ClubInfo data -> [ getValidatorName data ]

    let generate = ReportGenerator.Validation.generate getSectionValidatorNames

let generateRegistrationHandler() =
    ReportGenerator.generateValidatedReportGenerator (Validation.generate formSections) (Report.generate formSections)
