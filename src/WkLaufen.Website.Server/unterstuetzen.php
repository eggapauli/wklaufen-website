<?php

use PHPMailer\PHPMailer\PHPMailer;
use PHPMailer\PHPMailer\Exception;

require __DIR__ . '/vendor/autoload.php';

function validate($formData)
{
    $errors = array();
    if (empty($formData["firstName"])) $errors["firstName"] = "Bitte geben Sie Ihren Namen an, damit wir sie persönlich anschreiben können.";
    if (empty($formData["lastName"])) $errors["lastName"] = "Bitte geben Sie Ihren Namen an, damit wir sie persönlich anschreiben können.";
    if (empty($formData["street"])) $errors["street"] = "Bitte geben Sie Ihre Adresse an, an die wir z.B. Einladungen senden können.";
    if (empty($formData["city"])) $errors["city"] = "Bitte geben Sie Ihre Adresse an, an die wir z.B. Einladungen senden können.";
    if (strpos($formData["email"], "@") === false) $errors["email"] = "Bitte geben Sie eine E-Mail-Adresse an, an die wir z.B. unseren Newsletter senden können.";
    return $errors;
}

function generateReport($formData)
{
    $report = "Juhuuu, ein neues unterstützendes Mitglied hat sich über das Online-Formular auf wk-laufen.at angemeldet.\r\n";
    $report .= "* Vorname: $formData[firstName]\r\n";
    $report .= "* Nachname: $formData[lastName]\r\n";
    $report .= "* Straße + Hausnummer: $formData[street]\r\n";
    $report .= "* PLZ + Ort: $formData[city]\r\n";
    $report .= "* E-Mail: $formData[email]\r\n";
    return $report;
}
function initMail()
{
    $mail = new PHPMailer;
    $mail->CharSet = 'UTF-8';
    $mail->isSMTP();
    $mail->Host = 's1.meinehp.at';
    $mail->Username = 'web@wk-laufen.at';
    $mail->Password = 'aWWBneXrnPf5#';
    $mail->Port = 587;
    $mail->SMTPAuth = true;
    $mail->SMTPSecure = 'tls';
    $mail->isHTML(false);
    return $mail;
}

function sendMail($content)
{
    $mail = initMail();
    $mail->setFrom('unterstuetzende-mitglieder@wk-laufen.at', 'Registrierungsservice unterstützende Mitglieder');
    $mail->addAddress('j.egger@posteo.at');
    $mail->Subject = 'Neues unterstützendes Mitglied';
    $mail->Body = $content;
    if(!$mail->send())
    {
        http_response_code(500);
        $response = array(
            'message' => 'Leider ist bei der Anmeldung ein Fehler aufgetreten. Bitte versuchen Sie es später erneut.'
            //'debug-error' => $mail->ErrorInfo
        );
        exit(json_encode($response));
    }
}
$postDataString = file_get_contents("php://input");
$postData = json_decode($postDataString, true);
$errors = validate($postData);
if (count($errors) > 0)
{
    http_response_code(400);
    exit(json_encode($errors));
}
$report = generateReport($postData);
sendMail($report);
?>
