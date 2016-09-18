<?php
function validate($formData)
{
    $errors = array();
    $errors["club-name"] = validateClubName($formData["club-name"]);
    $errors["contact-person"] = validateContactPerson($formData["contact-person"]);
    $errors["number-of-participants"] = validateNumberOfParticipants($formData["number-of-participants"]);
    $errors["number-of-club-members"] = validateNumberOfClubMembers($formData["number-of-club-members"]);
    $errors["phone"] = validatePhone($formData["phone"]);
    $errors["email"] = validateEmail($formData["email"]);
    $errors["address"] = validateAddress($formData["address"]);
    $errors["city"] = validateCity($formData["city"]);
    $errors["participation-days"] = validateParticipationDays($formData["participation-days"]);
    $errors["participation-type-friday"] = validateParticipationTypeFriday($formData["participation-type-friday"]);
    $errors["participation-type-saturday"] = validateParticipationTypeSaturday($formData["participation-type-saturday"]);
    $errors["friday-reservation-doppelzimmer"] = validateFridayReservationDoppelzimmer($formData["friday-reservation-doppelzimmer"]);
    $errors["friday-reservation-einzelzimmer"] = validateFridayReservationEinzelzimmer($formData["friday-reservation-einzelzimmer"]);
    $errors["friday-reservation-mehrbettzimmer"] = validateFridayReservationMehrbettzimmer($formData["friday-reservation-mehrbettzimmer"]);
    $errors["saturday-reservation-doppelzimmer"] = validateSaturdayReservationDoppelzimmer($formData["saturday-reservation-doppelzimmer"]);
    $errors["saturday-reservation-einzelzimmer"] = validateSaturdayReservationEinzelzimmer($formData["saturday-reservation-einzelzimmer"]);
    $errors["saturday-reservation-mehrbettzimmer"] = validateSaturdayReservationMehrbettzimmer($formData["saturday-reservation-mehrbettzimmer"]);
    $errors["friday-food-schnitzel"] = validateFridayFoodSchnitzel($formData["friday-food-schnitzel"]);
    $errors["friday-food-bratwuerstel"] = validateFridayFoodBratwuerstel($formData["friday-food-bratwuerstel"]);
    $errors["friday-food-hendl"] = validateFridayFoodHendl($formData["friday-food-hendl"]);
    $errors["friday-food-bierfass"] = validateFridayFoodBierfass($formData["friday-food-bierfass"]);
    $errors["friday-food-veggie"] = validateFridayFoodVeggie($formData["friday-food-veggie"]);
    $errors["friday-food-anti"] = validateFridayFoodAnti($formData["friday-food-anti"]);
    $errors["saturday-food-schnitzel"] = validateSaturdayFoodSchnitzel($formData["saturday-food-schnitzel"]);
    $errors["saturday-food-bratwuerstel"] = validateSaturdayFoodBratwuerstel($formData["saturday-food-bratwuerstel"]);
    $errors["saturday-food-hendl"] = validateSaturdayFoodHendl($formData["saturday-food-hendl"]);
    $errors["saturday-food-bierfass"] = validateSaturdayFoodBierfass($formData["saturday-food-bierfass"]);
    $errors["saturday-food-veggie"] = validateSaturdayFoodVeggie($formData["saturday-food-veggie"]);
    $errors["saturday-food-anti"] = validateSaturdayFoodAnti($formData["saturday-food-anti"]);
    $errors["sunday-food-schnitzel"] = validateSundayFoodSchnitzel($formData["sunday-food-schnitzel"]);
    $errors["sunday-food-bratwuerstel"] = validateSundayFoodBratwuerstel($formData["sunday-food-bratwuerstel"]);
    $errors["sunday-food-hendl"] = validateSundayFoodHendl($formData["sunday-food-hendl"]);
    $errors["sunday-food-bierfass"] = validateSundayFoodBierfass($formData["sunday-food-bierfass"]);
    $errors["sunday-food-veggie"] = validateSundayFoodVeggie($formData["sunday-food-veggie"]);
    $errors["sunday-food-anti"] = validateSundayFoodAnti($formData["sunday-food-anti"]);
    return $errors;
}
function generateReport($formData)
{
    $report = "";
    $report .= "== Info\r\n";
    $report .= "* Vereinsname: " . htmlentities($formData["club-name"]) . "\r\n";
    $report .= "* Eure Ansprechperson: " . htmlentities($formData["contact-person"]) . "\r\n";
    $report .= "* Anzahl TeilnehmerInnen: " . intval(htmlentities($formData["number-of-participants"])) . "\r\n";
    $report .= "* davon MusikerInnen: " . intval(htmlentities($formData["number-of-club-members"])) . "\r\n";
    $report .= "* Telefon: " . htmlentities($formData["phone"]) . "\r\n";
    $report .= "* E-Mail: " . htmlentities($formData["email"]) . "\r\n";
    $report .= "* Adresse: " . htmlentities($formData["address"]) . "\r\n";
    $report .= "* PLZ/Ort: " . htmlentities($formData["city"]) . "\r\n";
    $report .= "\r\n";
    $report .= "== Marschwertung & Festakt\r\n";
    if (!(is_array($formData["participation-days"]) && in_array("friday", $formData["participation-days"])))
    {
        $report .= "Freitag, 9. Juni 2017: Keine Teilnahme\r\n";
    }
    else
    {
        $participationTypes = array("marschwertung" => "Marschwertung", "gastkapelle" => "Gastkapelle");
        $report .= "Freitag, 9. Juni 2017: {$participationTypes[$formData["participation-type-friday"]]}\r\n";
    }
    if (!(is_array($formData["participation-days"]) && in_array("saturday", $formData["participation-days"])))
    {
        $report .= "Samstag, 10. Juni 2017: Keine Teilnahme\r\n";
    }
    else
    {
        $participationTypes = array("marschwertung" => "Marschwertung", "gastkapelle" => "Gastkapelle");
        $report .= "Samstag, 10. Juni 2017: {$participationTypes[$formData["participation-type-saturday"]]}\r\n";
    }
    if (!(is_array($formData["participation-days"]) && in_array("sunday", $formData["participation-days"])))
    {
        $report .= "Sonntag, 11. Juni 2017: ✘\r\n";
    }
    else
    {
    $report .= "Sonntag, 11. Juni 2017: ✔\r\n";
    }
    $report .= "\r\n";
    $report .= "== Zimmerreservierung\r\n";
    if((is_array($formData["participation-days"]) && in_array("friday", $formData["participation-days"])))
    {
        $report .= "=== Freitag, 9. Juni 2017\r\n";
        $report .= "* " . intval(htmlentities($formData["friday-reservation-doppelzimmer"])) . " Doppelzimmer\r\n";
        $report .= "* " . intval(htmlentities($formData["friday-reservation-einzelzimmer"])) . " Einzelzimmer\r\n";
        $report .= "* " . intval(htmlentities($formData["friday-reservation-mehrbettzimmer"])) . " Personen in Mehrbettzimmern\r\n";
        $report .= "\r\n";
    }
    if((is_array($formData["participation-days"]) && in_array("saturday", $formData["participation-days"])))
    {
        $report .= "=== Samstag, 10. Juni 2017\r\n";
        $report .= "* " . intval(htmlentities($formData["saturday-reservation-doppelzimmer"])) . " Doppelzimmer\r\n";
        $report .= "* " . intval(htmlentities($formData["saturday-reservation-einzelzimmer"])) . " Einzelzimmer\r\n";
        $report .= "* " . intval(htmlentities($formData["saturday-reservation-mehrbettzimmer"])) . " Personen in Mehrbettzimmern\r\n";
        $report .= "\r\n";
    }
    $report .= "== Vorbestellung Festzelt\r\n";
    if((is_array($formData["participation-days"]) && in_array("friday", $formData["participation-days"])))
    {
        $report .= "=== Freitag, 9. Juni 2017\r\n";
        $report .= "* " . intval(htmlentities($formData["friday-food-schnitzel"])) . " Schnitzerl\r\n";
        $report .= "* " . intval(htmlentities($formData["friday-food-bratwuerstel"])) . " Bratwürstel\r\n";
        $report .= "* " . intval(htmlentities($formData["friday-food-hendl"])) . " Hendl\r\n";
        $report .= "* " . intval(htmlentities($formData["friday-food-bierfass"])) . " 15 l Fass am Tisch\r\n";
        $report .= "* " . intval(htmlentities($formData["friday-food-veggie"])) . " Vegetarisch\r\n";
        $report .= "* " . intval(htmlentities($formData["friday-food-anti"])) . " Kiste Anti gemischt\r\n";
        $report .= "\r\n";
    }
    if((is_array($formData["participation-days"]) && in_array("saturday", $formData["participation-days"])))
    {
        $report .= "=== Samstag, 10. Juni 2017\r\n";
        $report .= "* " . intval(htmlentities($formData["saturday-food-schnitzel"])) . " Schnitzerl\r\n";
        $report .= "* " . intval(htmlentities($formData["saturday-food-bratwuerstel"])) . " Bratwürstel\r\n";
        $report .= "* " . intval(htmlentities($formData["saturday-food-hendl"])) . " Hendl\r\n";
        $report .= "* " . intval(htmlentities($formData["saturday-food-bierfass"])) . " 15 l Fass am Tisch\r\n";
        $report .= "* " . intval(htmlentities($formData["saturday-food-veggie"])) . " Vegetarisch\r\n";
        $report .= "* " . intval(htmlentities($formData["saturday-food-anti"])) . " Kiste Anti gemischt\r\n";
        $report .= "\r\n";
    }
    if((is_array($formData["participation-days"]) && in_array("sunday", $formData["participation-days"])))
    {
        $report .= "=== Sonntag, 11. Juni 2017\r\n";
        $report .= "* " . intval(htmlentities($formData["sunday-food-schnitzel"])) . " Schnitzerl\r\n";
        $report .= "* " . intval(htmlentities($formData["sunday-food-bratwuerstel"])) . " Bratwürstel\r\n";
        $report .= "* " . intval(htmlentities($formData["sunday-food-hendl"])) . " Hendl\r\n";
        $report .= "* " . intval(htmlentities($formData["sunday-food-bierfass"])) . " 15 l Fass am Tisch\r\n";
        $report .= "* " . intval(htmlentities($formData["sunday-food-veggie"])) . " Vegetarisch\r\n";
        $report .= "* " . intval(htmlentities($formData["sunday-food-anti"])) . " Kiste Anti gemischt\r\n";
        $report .= "\r\n";
    }
    return $report;
}
?>