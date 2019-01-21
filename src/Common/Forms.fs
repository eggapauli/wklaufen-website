module Forms

type HttpMethod = Get | Post

type StringValue = string option
type StringInputValidation =
  | NotEmptyOrWhitespace
  | ContainsCharacter of char

type BoolValue = bool option
type BoolInputValidation =
  | MustBeTrue

type StringInput = StringValue * StringInputValidation
type BoolInput = BoolValue * BoolInputValidation

module Unterstuetzen =
  type Input =
    | FirstName of StringInput
    | LastName of StringInput
    | Street of StringInput
    | City of StringInput
    | Email of StringInput
    | DataUsageConsent of BoolInput

  let httpMethod = Post
  let path = "unterstuetzen.php"
  let getKey = function
    | FirstName _ -> "firstName"
    | LastName _ -> "lastName"
    | Street _ -> "street"
    | City _ -> "city"
    | Email _ -> "email"
    | DataUsageConsent _ -> "dataUsageConsent"
  let getTitle = function
    | FirstName _ -> "Vorname"
    | LastName _ -> "Nachname"
    | Street _ -> "Straße + Hausnummer"
    | City _ -> "PLZ + Ort"
    | Email _ -> "E-Mail"
    | DataUsageConsent _ -> "Ich bin damit einverstanden, dass diese Daten für die Zusendung des Newsletters, Einladungen und weitere Informationszwecke verwendet werden. Die Daten werden nicht an Dritte weitergegeben."
  let getErrorText = function
    | FirstName _
    | LastName _ -> "Bitte geben Sie Ihren Namen an, damit wir sie persönlich anschreiben können."
    | Street _
    | City _ -> "Bitte geben Sie Ihre Adresse an, an die wir z.B. Einladungen senden können."
    | Email _ -> "Bitte geben Sie eine E-Mail-Adresse an, an die wir z.B. unseren Newsletter senden können."
    | DataUsageConsent _ -> "Bitte akzeptieren sie die Einwilligungserklärung gemäß Datenschutz zur Verarbeitung Ihrer Daten."
  let inputs =
    [
      FirstName (None, NotEmptyOrWhitespace)
      LastName (None, NotEmptyOrWhitespace)
      Street (None, NotEmptyOrWhitespace)
      City (None, NotEmptyOrWhitespace)
      Email (None, ContainsCharacter '@')
      DataUsageConsent (None, MustBeTrue)
    ]
