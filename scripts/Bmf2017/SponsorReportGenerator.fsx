#if COMPILED
module SponsorReportGenerator
open WkLaufen.Bmf2017.Form
open WkLaufen.Bmf2017.SponsorForm
#endif

#if INTERACTIVE
#load @"..\..\WkLaufen.Bmf2017\SponsorForm.fsx"
//#load "ReportGenerator.fsx"
open Form
open SponsorForm
#endif

module Report =
    open ReportGenerator
    open ReportGenerator.Report

    let getSectionReportGenerator = function
        | Info data ->
            [
                yield addReport "== Info"
                yield!
                    data
                    |> List.collect getInputReportGenerator
                yield addReport ""
            ]
        | Package data ->
            [
                yield addReport "== Sponsoring-Paket"
                yield! getInputReportGenerator data
                yield addReport ""
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
            |> List.map getValidatorName
        | Package data -> [ getValidatorName data ]
        | Notes data -> [ getValidatorName data ]

    let generate = ReportGenerator.Validation.generate getSectionValidatorNames

let generateSponsorSubscriptionHandler() =
    ReportGenerator.generateValidatedReportGenerator (Validation.generate formSections) (Report.generate formSections)
