<?php
// see http://php.net/manual/en/function.http-response-code.php
if (!function_exists('http_response_code')) {
    function http_response_code($code)
    {
       switch ($code)
       {
            case 400: $text = 'Bad Request'; break;
            case 500: $text = 'Internal Server Error'; break;
            default:
                exit('Unknown http status code "' . htmlentities($code) . '"');
            break;
        }

        $protocol = (isset($_SERVER['SERVER_PROTOCOL']) ? $_SERVER['SERVER_PROTOCOL'] : 'HTTP/1.0');

        header($protocol . ' ' . $code . ' ' . $text);
    }
}

function validateNotEmpty($value) {
    if (empty($value)) {
        return "Darf nicht leer sein";
    }
    return null;
}

function validateNumberGTE($value, $comparand) {
    if (intval($value) < $comparand) {
        return "Muss größer oder gleich {$comparand} sein";
    }
    return null;
}

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

include "bmf-registration-helper.php";

$errors = validate($_POST);
$filteredErrors = array();
foreach ($errors as $fieldName => $error)
{
    if (!is_null($error))
    {
        $filteredErrors[$fieldName] = $error;
    }
}
if (count($filteredErrors) > 0) {
    http_response_code(400);
    exit(json_encode($filteredErrors));
}

$report = generateReport($_POST);

function sendMail($content) {
    require 'vendor/phpmailer/phpmailer/PHPMailerAutoload.php';

    $mail = new PHPMailer;
    $mail->CharSet = 'UTF-8';
    
    $mail->isSMTP();
    
    $mail->Host = '%MailHost%';
    $mail->Username = '%MailUsername%';
    $mail->Password = '%MailPassword%';
    $mail->Port = %MailPort%;
    
    $mail->SMTPAuth = true;
    $mail->SMTPSecure = 'tls';

    $mail->setFrom('bmf-registration@wk-laufen.at', 'Registrierungsservice BMF 2017');
    $mail->addAddress('marketing@wk-laufen.at');
    $mail->addCC('obmann@wk-laufen.at');
    $mail->addBCC('j.egger@posteo.at');
    $mail->isHTML(false);

    $mail->Subject = 'Registrierung BMF2017';
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