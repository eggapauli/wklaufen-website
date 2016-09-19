<?php
include_once 'common.php';

function validateOrganisation($value) { return validateNotEmpty($value); }
function validateContactPerson($value) { return validateNotEmpty($value); }
function validateEmail($value) { return validateNotEmpty($value); }
function validatePhone($value) { return validateNotEmpty($value); }
function validatePackage($value) { return validateNotEmpty($value); }
function validateNotes($value) { return null; }

include_once "bmf-sponsor-helper.php";

$report = validateAndGenerateReport($_POST);

function sendMail($content) {
    $mail = initMail();

    $mail->setFrom('bmf-sponsoring@wk-laufen.at', 'Sponsoringservice BMF 2017');
    $mail->addAddress('marketing@wk-laufen.at');
    $mail->addCC('obmann@wk-laufen.at');
    $mail->addBCC('j.egger@posteo.at');

    $mail->Subject = 'Sponsoring BMF 2017';
    $mail->Body    = $content;

    if(!$mail->send())
    {
        http_response_code(500);
        $response = array(
            "message" => "Leider ist bei der Anmeldung ein Fehler aufgetreten. Bitte versuchen Sie es sp\u00e4ter erneut."
            //"debug-error" => $mail->ErrorInfo
        );
        exit(json_encode($response));
    }
}

sendMail($report);
?>