open System
open System.IO

module String =
    let indent = List.map (sprintf "    %s")

type Recipient =
    | Main of string
    | CC of string
    | BCC of string

type PhpFormReport<'a> =
    { FormInputs: Forms.Input<'a> list
      FilePath: string
      EmailReportHeadline: string
      EmailRecipients: Recipient list
      EmailSubject: string }

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

let generateIntegerInputValidationCode key errorText = function
    | Forms.NonNegative ->
        [
            sprintf "if (!is_int($formData[\"%s\"]) || $formData[\"%s\"] < 0) $errors[\"%s\"] = \"%s\";" key key key errorText
        ]

let generateInputValidationCode (input: Forms.Input<_>) =
    match input.Type with
    | Forms.StringInput inputProps ->
        generateStringInputValidationCode input.Props.Key input.Props.ErrorText inputProps.Validation
    | Forms.BoolInput inputProps ->
        generateBoolInputValidationCode input.Props.Key input.Props.ErrorText inputProps.Validation
    | Forms.IntegerInput inputProps ->
        generateIntegerInputValidationCode input.Props.Key input.Props.ErrorText inputProps.Validation

let generateReportGenerationCode (input: Forms.Input<_>) =
    match input.Type with
    | Forms.StringInput _
    | Forms.IntegerInput _ ->
        [
            sprintf "$report .= \"* %s: $formData[%s]\\r\\n\";" input.Props.Title input.Props.Key
        ]
    | Forms.BoolInput _ ->
        [
            sprintf "$report .= ($formData[%s] ? '✓' : '✗') . \" %s\\r\\n\";" input.Props.Key input.Props.Title
        ]

let getEnvVarOrFail name =
    let value = Environment.GetEnvironmentVariable name
    if isNull value
    then failwithf "Environment variable \"%s\" not set" name
    else value

let generateServerForm formReport =
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
                yield! List.collect generateInputValidationCode formReport.FormInputs
                yield "return $errors;"
            ]
            |> String.indent
        yield "}"
        yield ""
        yield "function generateReport($formData)"
        yield "{"
        yield!
            [
                yield sprintf "$report = \"%s\\r\\n\";" formReport.EmailReportHeadline
                yield! List.collect generateReportGenerationCode formReport.FormInputs
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

                yield sprintf "$mail->setFrom('%s', 'WK Laufen Website');" (getEnvVarOrFail "MAIL_USERNAME")
                yield!
                    formReport.EmailRecipients
                    |> List.map (function
                        | Main address -> sprintf "$mail->addAddress('%s');" address
                        | CC address -> sprintf "$mail->addCC('%s');" address
                        | BCC address -> sprintf "$mail->addBCC('%s');" address
                    )

                yield sprintf "$mail->Subject = '%s';" formReport.EmailSubject
                yield "$mail->Body = $content;"

                yield "if(!$mail->send())"
                yield "{"
                yield "    http_response_code(500);"
                yield "    $response = array("
                yield "        'message' => 'Leider ist beim Absenden ein Fehler aufgetreten. Bitte versuchen Sie es später erneut.'"
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
    |> fun lines -> File.WriteAllLines(sprintf "src\\WkLaufen.Website.Server\\%s" formReport.FilePath, lines)

[<EntryPoint>]
let main argv =
    let supportFormReport =
        { FormInputs = Forms.Unterstuetzen.inputs
          FilePath = Forms.Unterstuetzen.path
          EmailReportHeadline = "Juhuuu, ein neues unterstützendes Mitglied hat sich über das Online-Formular auf wk-laufen.at angemeldet."
          EmailRecipients =
            // [ Main "marketing@wk-laufen.at"
            //   CC "obmann@wk-laufen.at"
            //   BCC "j.egger@posteo.at" ]
            [ Main "j.egger@posteo.at" ]
          EmailSubject = "Neues unterstützendes Mitglied" }
    generateServerForm supportFormReport

    let ticketFormReport =
        { FormInputs = Forms.Kartenreservierung.inputs
          FilePath = Forms.Kartenreservierung.path
          EmailReportHeadline = "Juhuuu, über das Online-Formular auf wk-laufen.at sind Karten für das Jahreskonzert reserviert worden."
          EmailRecipients =
            // [ Main "obmann@wk-laufen.at"
            //   CC "marketing@wk-laufen.at"
            //   BCC "j.egger@posteo.at" ]
            [ Main "j.egger@posteo.at" ]
          EmailSubject = "Kartenreservierung Jahreskonzert 2019" }
    generateServerForm ticketFormReport
    0
