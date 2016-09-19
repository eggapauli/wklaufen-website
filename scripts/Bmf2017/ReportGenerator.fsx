#if COMPILED
module ReportGenerator
open WkLaufen.Bmf2017.Form
#endif

#if INTERACTIVE
//#load @"..\..\WkLaufen.Bmf2017\Form.fsx"
open Form
#endif

let getFormDataVar = sprintf "$formData[\"%s\"]"

let indent cols = sprintf "%s%s" (String.replicate (cols * 4) " ")

module Report =
    let getPostVarText = getFormDataVar >> sprintf "htmlentities(%s)"
    let getPostVarInString = getPostVarText >> sprintf "\" . %s . \""
    let getPostIntegerInString = getPostVarText >> sprintf "\" . intval(%s) . \""

    let isValueChecked subKey key =
        let postVar = getFormDataVar key
        sprintf "((is_array(%s) && in_array(\"%s\", %s)) || %s === \"%s\")" postVar subKey postVar postVar subKey


    let addReport format =
        let add = sprintf "$report .= \"%s\\r\\n\";"
        Printf.ksprintf add format

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
                        addReport "* [\" . (%s ? \"x\" : \" \") . \"] %s" (isValueChecked item.Value data.Name) item.Description
                    )
            ]

    let generate getSectionReportGenerator =
        List.collect getSectionReportGenerator
        >> List.map (indent 1)
        >> String.concat System.Environment.NewLine
        >> sprintf """function generateReport($formData)
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
        sprintf "$error = validate%s(%s); if ($error) { $errors[\"%s\"] = $error; }" (getPhpIdentifier name) (getFormDataVar name) name

    let getValidatorName input =
        match input with
        | TextInput data
        | NumberInputWithPrefixTitle data
        | NumberInputWithPostfixTitle data
        | TextAreaInput { Common = data } -> data.Name
        | CheckboxInput data
        | RadioboxInput data -> data.Name

    let generate getSectionValidatorNames sections =
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

let generateValidatedReportGenerator validation reportGeneration =
    [
        "<?php"
        "include_once \"common.php\";"
        validation
        reportGeneration
        "function validateAndGenerateReport()"
        "{"
        "    $errors = validate($_POST);"
        "    if (count($errors) > 0) {"
        "        http_response_code(400);"
        "        exit(json_encode($errors));"
        "    }"
        "    return generateReport($_POST);"
        "}"
        "?>"
    ]
    |> String.concat System.Environment.NewLine