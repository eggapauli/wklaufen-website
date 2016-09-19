#load @"..\..\WkLaufen.Bmf2017\RegistrationForm.fs"

open WkLaufen.Bmf2017.RegistrationForm

let getSocialProgramInputs programs noProgram =
    [
        yield!
            programs
            |> List.collect (fun (p1, p2) ->
                [
                    yield! p1 |> Option.toList
                    yield! p2 |> Option.toList
                ]
            )
        yield noProgram
    ]

let getFormDataVar = sprintf "$formData[\"%s\"]"

let indent cols = sprintf "%s%s" (String.replicate (cols * 4) " ")

module Report =
    let addReport format =
        let add = sprintf "$report .= \"%s\\r\\n\";"
        Printf.ksprintf add format

    let getPostVarText = getFormDataVar >> sprintf "htmlentities(%s)"
    let getPostVarInString = getPostVarText >> sprintf "\" . %s . \""
    let getPostIntegerInString = getPostVarText >> sprintf "\" . intval(%s) . \""

    let inPostArray subKey key =
        let postVar = getFormDataVar key
        sprintf "(is_array(%s) && in_array(\"%s\", %s))" postVar subKey postVar

    let participatesAt day =
        inPostArray day.Key participationDaysKey

    let getInputReportGenerator = function
        | TextInput data
        | TextAreaInput { Common = data } -> [ addReport "* %s: %s" data.Description (getPostVarInString data.Name) ]
        | NumberInputWithPostfixTitle data ->
            [ addReport "* %s %s" (getPostIntegerInString data.Name) data.Description ]
        | NumberInputWithPrefixTitle data ->
            [ addReport "* %s: %s" data.Description (getPostIntegerInString data.Name) ]
        | CheckboxInput data
        | RadioboxInput data ->
            [
                match data.Description with
                | Some description -> yield addReport "=== %s" description
                | None -> ()
                yield!
                    data.Items
                    |> List.map (fun item ->
                        addReport "* [\" . (%s ? \"x\" : \" \") . \"] %s" (inPostArray item.Value data.Name) item.Description
                    )
            ]

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

    let generate sections =
        sections
        |> List.collect getSectionReportGenerator
        |> List.map (indent 1)
        |> String.concat System.Environment.NewLine
        |> sprintf """function generateReport($formData)
{
    $report = "";
%s
    return $report;
}"""

module Validation =
    let getPhpIdentifier text =
        System.Text.RegularExpressions.Regex.Replace(
            text,
            "(?:^|-)(\w)",
            System.Text.RegularExpressions.MatchEvaluator(fun m -> m.Groups.[1].Value.ToUpper())
        )

    let getValidator name =
        sprintf "$errors[\"%s\"] = validate%s(%s);" name (getPhpIdentifier name) (getFormDataVar name)

    let getValidatorName input =
        match input with
        | TextInput data
        | NumberInputWithPrefixTitle data
        | NumberInputWithPostfixTitle data
        | TextAreaInput { Common = data } -> data.Name
        | CheckboxInput data
        | RadioboxInput data -> data.Name

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

    let generate sections =
        sections
        |> List.collect getSectionValidatorNames
        |> List.distinct
        |> List.map (getValidator >> indent 1)
        |> String.concat System.Environment.NewLine
        |> sprintf """function validate($formData)
{
    $errors = array();
%s
    return $errors;
}"""

let generateRegistrationHandler() =
    [
        sprintf "<?php"
        Validation.generate formSections |> sprintf "%s"
        Report.generate formSections |> sprintf "%s"
        sprintf "?>"
    ]
    |> String.concat System.Environment.NewLine
