open System
open System.IO

module String =
    let indent = List.map (sprintf "    %s")

let generateStringInputValidationCode key errorText = function
    | Forms.NotEmptyOrWhitespace ->
        [
            sprintf "if (empty($formData[\"%s\"]) || trim($formData[\"%s\"]) == \"\") $errors[\"%s\"] = \"%s\";" key key key errorText
        ]
    | Forms.ContainsCharacter c ->
        [
            sprintf "if (strpos($formData[\"%s\"], \"%c\") === false) $errors[\"%s\"] = \"%s\";" key c key errorText
        ]
let generateBoolInputValidationCode key errorText = function
    | Forms.MustBeTrue ->
        [
            sprintf "if (empty($formData[\"%s\"])) $errors[\"%s\"] = \"%s\";" key key errorText
        ]

let generateInputValidationCode input =
    let key = Forms.Unterstuetzen.getKey input
    let errorText = Forms.Unterstuetzen.getErrorText input
    match input with
    | Forms.Unterstuetzen.FirstName (_, validation)
    | Forms.Unterstuetzen.LastName (_, validation)
    | Forms.Unterstuetzen.Street (_, validation)
    | Forms.Unterstuetzen.City (_, validation)
    | Forms.Unterstuetzen.Email (_, validation) ->
        generateStringInputValidationCode key errorText validation
    | Forms.Unterstuetzen.DataUsageConsent (_, validation) ->
        generateBoolInputValidationCode key errorText validation

let generateReportGenerationCode input =
    let title= Forms.Unterstuetzen.getTitle input
    let key = Forms.Unterstuetzen.getKey input
    match input with
    | Forms.Unterstuetzen.FirstName _
    | Forms.Unterstuetzen.LastName _
    | Forms.Unterstuetzen.Street _
    | Forms.Unterstuetzen.City _
    | Forms.Unterstuetzen.Email _ ->
        [
            sprintf "$report .= \"* %s: $formData[%s]\\r\\n\";" title key
        ]
    | Forms.Unterstuetzen.DataUsageConsent _ ->
        [
            sprintf "$report .= ($formData[%s] ? '✓' : '✗') . \" %s\\r\\n\";" key title
        ]

let getEnvVarOrFail name =
    let value = Environment.GetEnvironmentVariable name
    if isNull value
    then failwithf "Environment variable \"%s\" not set" name
    else value

[<EntryPoint>]
let main argv =
    [
        yield "<?php"
        yield ""
        yield "use PHPMailer\\PHPMailer\\PHPMailer;"
        yield "use PHPMailer\\PHPMailer\\Exception;"
        yield ""
        yield "require __DIR__ . '/vendor/autoload.php';"
        yield ""
        yield "function validate($formData)"
        yield "{"
        yield!
            [
                yield "$errors = array();"
                yield!
                    Forms.Unterstuetzen.inputs
                    |> List.collect generateInputValidationCode 
                yield "return $errors;"
            ]
            |> String.indent
        yield "}"
        yield ""
        yield "function generateReport($formData)"
        yield "{"
        yield!
            [
                yield "$report = \"Juhuuu, ein neues unterstützendes Mitglied hat sich über das Online-Formular auf wk-laufen.at angemeldet.\\r\\n\";"
                yield!
                    Forms.Unterstuetzen.inputs
                    |> List.collect generateReportGenerationCode
                yield "return $report;"
            ]
            |> String.indent
        yield "}"
        yield "function initMail()"
        yield "{"
        yield!
            [
                yield "$mail = new PHPMailer;"
                yield "$mail->CharSet = 'UTF-8';"

                yield "$mail->isSMTP();"

                yield sprintf "$mail->Host = '%s';" (getEnvVarOrFail "MAIL_HOST")
                yield sprintf "$mail->Username = '%s';" (getEnvVarOrFail "MAIL_USERNAME")
                yield sprintf "$mail->Password = '%s';" (getEnvVarOrFail "MAIL_PASSWORD")
                yield sprintf "$mail->Port = %s;" (getEnvVarOrFail "MAIL_PORT")

                yield "$mail->SMTPAuth = true;"
                yield "$mail->SMTPSecure = 'tls';"

                yield "$mail->isHTML(false);"
                yield "return $mail;"
            ]
            |> String.indent
        yield "}"
        yield ""
        yield "function sendMail($content)"
        yield "{"
        yield!
            [
                yield "$mail = initMail();"

                yield "$mail->setFrom('unterstuetzende-mitglieder@wk-laufen.at', 'Registrierungsservice unterstützende Mitglieder');"
                yield "$mail->addAddress('marketing@wk-laufen.at');"
                yield "$mail->addCC('obmann@wk-laufen.at');"
                yield "$mail->addBCC('j.egger@posteo.at');"

                yield "$mail->Subject = 'Neues unterstützendes Mitglied';"
                yield "$mail->Body = $content;"

                yield "if(!$mail->send())"
                yield "{"
                yield "    http_response_code(500);"
                yield "    $response = array("
                yield "        'message' => 'Leider ist bei der Anmeldung ein Fehler aufgetreten. Bitte versuchen Sie es später erneut.'"
                yield "        //'debug-error' => $mail->ErrorInfo"
                yield "    );"
                yield "    exit(json_encode($response));"
                yield "}"
            ]
            |> String.indent
        yield "}"
        yield "$postDataString = file_get_contents(\"php://input\");"
        yield "$postData = json_decode($postDataString, true);"
        yield "$errors = validate($postData);"
        yield "if (count($errors) > 0)"
        yield "{"
        yield "    http_response_code(400);"
        yield "    exit(json_encode($errors));"
        yield "}"
        yield "$report = generateReport($postData);"
        yield "sendMail($report);"
        yield "?>"
    ]
    |> fun lines -> File.WriteAllLines(sprintf "src\\WkLaufen.Website.Server\\%s" Forms.Unterstuetzen.path, lines)
    0
