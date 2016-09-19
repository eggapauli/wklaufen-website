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

function initMail() {
    include_once 'vendor/phpmailer/phpmailer/PHPMailerAutoload.php';

    $mail = new PHPMailer;
    $mail->CharSet = 'UTF-8';
    
    $mail->isSMTP();
    
    $mail->Host = '%MailHost%';
    $mail->Username = '%MailUsername%';
    $mail->Password = '%MailPassword%';
    $mail->Port = %MailPort%;
    
    $mail->SMTPAuth = true;
    $mail->SMTPSecure = 'tls';

    $mail->isHTML(false);
    return $mail;
}
?>