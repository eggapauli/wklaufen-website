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
    $errors["friday-food-bratl"] = validateFridayFoodBratl($formData["friday-food-bratl"]);
    $errors["friday-food-hendl"] = validateFridayFoodHendl($formData["friday-food-hendl"]);
    $errors["friday-food-bierfass"] = validateFridayFoodBierfass($formData["friday-food-bierfass"]);
    $errors["friday-food-veggie"] = validateFridayFoodVeggie($formData["friday-food-veggie"]);
    $errors["friday-food-anti"] = validateFridayFoodAnti($formData["friday-food-anti"]);
    $errors["saturday-food-schnitzel"] = validateSaturdayFoodSchnitzel($formData["saturday-food-schnitzel"]);
    $errors["saturday-food-bratl"] = validateSaturdayFoodBratl($formData["saturday-food-bratl"]);
    $errors["saturday-food-hendl"] = validateSaturdayFoodHendl($formData["saturday-food-hendl"]);
    $errors["saturday-food-bierfass"] = validateSaturdayFoodBierfass($formData["saturday-food-bierfass"]);
    $errors["saturday-food-veggie"] = validateSaturdayFoodVeggie($formData["saturday-food-veggie"]);
    $errors["saturday-food-anti"] = validateSaturdayFoodAnti($formData["saturday-food-anti"]);
    $errors["social-programs"] = validateSocialPrograms($formData["social-programs"]);
    $errors["notes"] = validateNotes($formData["notes"]);
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
        $report .= "* " . intval(htmlentities($formData["friday-food-bratl"])) . " Bratl\r\n";
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
        $report .= "* " . intval(htmlentities($formData["saturday-food-bratl"])) . " Bratl\r\n";
        $report .= "* " . intval(htmlentities($formData["saturday-food-hendl"])) . " Hendl\r\n";
        $report .= "* " . intval(htmlentities($formData["saturday-food-bierfass"])) . " 15 l Fass am Tisch\r\n";
        $report .= "* " . intval(htmlentities($formData["saturday-food-veggie"])) . " Vegetarisch\r\n";
        $report .= "* " . intval(htmlentities($formData["saturday-food-anti"])) . " Kiste Anti gemischt\r\n";
        $report .= "\r\n";
    }
    $report .= "== Rahmenprogramm - Das würde uns gefallen\r\n";
    $report .= "* [" . ((is_array($formData["social-programs"]) && in_array("gruenberg", $formData["social-programs"])) ? "x" : " ") . "] Grünberg: Seilbahn oder zu Fuß, Sommerrodelbahn, Gasthof Grünbergalm\r\n";
    $report .= "* [" . ((is_array($formData["social-programs"]) && in_array("eggenberg", $formData["social-programs"])) ? "x" : " ") . "] Besichtigung & Verkostung Brauerei Schloss Eggenberg (Vorchdorf)\r\n";
    $report .= "* [" . ((is_array($formData["social-programs"]) && in_array("traunsee", $formData["social-programs"])) ? "x" : " ") . "] Traunseeschifffahrt - Seerundfahrt\r\n";
    $report .= "* [" . ((is_array($formData["social-programs"]) && in_array("oldtimermuseum", $formData["social-programs"])) ? "x" : " ") . "] Oldtimermuseum Altmünster - Rund ums Rad!\r\n";
    $report .= "* [" . ((is_array($formData["social-programs"]) && in_array("esplanade", $formData["social-programs"])) ? "x" : " ") . "] Gemütliche Stunden an der Gmundner Esplanade (Tretboot, Schloss Ort, Bummelzug, …)\r\n";
    $report .= "* [" . ((is_array($formData["social-programs"]) && in_array("steyrermuehl", $formData["social-programs"])) ? "x" : " ") . "] Österreichisches Papiermacher- und Druckereimuseum (Steyrermühl)\r\n";
    $report .= "* [" . ((is_array($formData["social-programs"]) && in_array("altmuenster", $formData["social-programs"])) ? "x" : " ") . "] Minigolf & Sommerbar Coconut an der Esplanade Altmünster\r\n";
    $report .= "* [" . ((is_array($formData["social-programs"]) && in_array("klomuseum", $formData["social-programs"])) ? "x" : " ") . "] Klo & So Museum für historische Sanitärobjekte\r\n";
    $report .= "* [" . ((is_array($formData["social-programs"]) && in_array("badetag", $formData["social-programs"])) ? "x" : " ") . "] Badetag am Traunsee\r\n";
    $report .= "* [" . ((is_array($formData["social-programs"]) && in_array("keramik", $formData["social-programs"])) ? "x" : " ") . "] Manufakturführung Gmundner Keramik\r\n";
    $report .= "* [" . ((is_array($formData["social-programs"]) && in_array("none", $formData["social-programs"])) ? "x" : " ") . "] Danke, aber wir kümmern uns selbst um unser Rahmenprogramm.\r\n";
    $report .= "\r\n";
    $report .= "== Anmerkungen, Wünsche, spezielle Rahmenprogramm-Favoriten\r\n";
    $report .= "" . htmlentities($formData["notes"]) . "\r\n";
    return $report;
}
?>