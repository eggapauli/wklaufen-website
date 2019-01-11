module Forms

type HttpMethod = Get | Post

type StringInputValidation =
  | NotEmptyOrWhitespace
  | ContainsCharacter of char
type StringValue = string option
type StringInput = StringValue * StringInputValidation

module Unterstuetzen =
  type Input =
    | FirstName of StringInput
    | LastName of StringInput
    | Street of StringInput
    | City of StringInput
    | Email of StringInput

  let httpMethod = Post
  let path = "unterstuetzen.php"
  let getKey = function
    | FirstName _ -> "firstName"
    | LastName _ -> "lastName"
    | Street _ -> "street"
    | City _ -> "city"
    | Email _ -> "email"
  let getTitle = function
    | FirstName _ -> "Vorname"
    | LastName _ -> "Nachname"
    | Street _ -> "Straße + Hausnummer"
    | City _ -> "PLZ + Ort"
    | Email _ -> "E-Mail"
  let getErrorText = function
    | FirstName _
    | LastName _ -> "Bitte geben Sie Ihren Namen an, damit wir sie persönlich anschreiben können."
    | Street _
    | City _ -> "Bitte geben Sie Ihre Adresse an, an die wir z.B. Einladungen senden können."
    | Email _ -> "Bitte geben Sie eine E-Mail-Adresse an, an die wir z.B. unseren Newsletter senden können."
  let inputs =
    [
      FirstName (None, NotEmptyOrWhitespace)
      LastName (None, NotEmptyOrWhitespace)
      Street (None, NotEmptyOrWhitespace)
      City (None, NotEmptyOrWhitespace)
      Email (None, ContainsCharacter '@')
    ]
