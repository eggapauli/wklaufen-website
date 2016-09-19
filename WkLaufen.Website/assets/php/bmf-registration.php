<?php
include_once 'common.php';

function validateClubName($value) { return validateNotEmpty($value); }
function validateContactPerson($value) { return validateNotEmpty($value); }
function validateNumberOfParticipants($value) { return validateNumberGTE($value, 1); }
function validateNumberOfClubMembers($value) { return validateNumberGTE($value, 1); }
function validatePhone($value) { return validateNotEmpty($value); }
function validateEmail($value) { return validateNotEmpty($value); }
function validateAddress($value) { return validateNotEmpty($value); }
function validateCity($value) { return validateNotEmpty($value); }
function validateParticipationDays($value) {
    if (empty($value)) {
        return "Ein Tag muss ausgewählt werden.";
    }
    return null;
}
function validateParticipationTypeFriday($value) { return null; }
function validateParticipationTypeSaturday($value) { return null; }
function validateEnableReservation($value) { return null; }
function validateFridayReservationDoppelzimmer($value) { return validateNumberGTE($value, 0); }
function validateFridayReservationEinzelzimmer($value) { return validateNumberGTE($value, 0); }
function validateFridayReservationMehrbettzimmer($value) { return validateNumberGTE($value, 0); }
function validateSaturdayReservationDoppelzimmer($value) { return validateNumberGTE($value, 0); }
function validateSaturdayReservationEinzelzimmer($value) { return validateNumberGTE($value, 0); }
function validateSaturdayReservationMehrbettzimmer($value) { return validateNumberGTE($value, 0); }
function validateFridayFoodSchnitzel($value) { return validateNumberGTE($value, 0); }
function validateFridayFoodBratwuerstel($value) { return validateNumberGTE($value, 0); }
function validateFridayFoodHendl($value) { return validateNumberGTE($value, 0); }
function validateFridayFoodBierfass($value) { return validateNumberGTE($value, 0); }
function validateFridayFoodVeggie($value) { return validateNumberGTE($value, 0); }
function validateFridayFoodAnti($value) { return validateNumberGTE($value, 0); }
function validateSaturdayFoodSchnitzel($value) { return validateNumberGTE($value, 0); }
function validateSaturdayFoodBratwuerstel($value) { return validateNumberGTE($value, 0); }
function validateSaturdayFoodHendl($value) { return validateNumberGTE($value, 0); }
function validateSaturdayFoodBierfass($value) { return validateNumberGTE($value, 0); }
function validateSaturdayFoodVeggie($value) { return validateNumberGTE($value, 0); }
function validateSaturdayFoodAnti($value) { return validateNumberGTE($value, 0); }
function validateSundayFoodSchnitzel($value) { return validateNumberGTE($value, 0); }
function validateSundayFoodBratwuerstel($value) { return validateNumberGTE($value, 0); }
function validateSundayFoodHendl($value) { return validateNumberGTE($value, 0); }
function validateSundayFoodBierfass($value) { return validateNumberGTE($value, 0); }
function validateSundayFoodVeggie($value) { return validateNumberGTE($value, 0); }
function validateSundayFoodAnti($value) { return validateNumberGTE($value, 0); }
function validateNotes($value) { return null; }

include_once "bmf-registration-helper.php";

$report = validateAndGenerateReport($_POST);

function sendMail($content) {
    $mail = initMail();

    $mail->setFrom('bmf-registration@wk-laufen.at', 'Registrierungsservice BMF 2017');
    $mail->addAddress('marketing@wk-laufen.at');
    $mail->addCC('obmann@wk-laufen.at');
    $mail->addBCC('j.egger@posteo.at');

    $mail->Subject = 'Registrierung BMF 2017';
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