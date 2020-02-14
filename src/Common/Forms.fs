module Forms

type StringValue = string option
type StringInputValidation =
  | NotEmptyOrWhitespace
  | ContainsCharacter of char

type BoolValue = bool option
type BoolInputValidation =
  | MustBeTrue

type IntegerValue = int option
type IntegerInputValidation =
  | NonNegative

type InputProps =
  { Key: string
    Title: string
    ErrorText: string }

type StringInputProps<'a> =
  { Field: 'a
    Value: StringValue
    Validation: StringInputValidation }

type BoolInputProps<'a> =
  { Field: 'a
    Value: BoolValue
    Validation: BoolInputValidation }

type IntegerInputProps<'a> =
  { Field: 'a
    Value: IntegerValue
    Validation: IntegerInputValidation }

type InputType<'a> =
  | StringInput of StringInputProps<'a>
  | BoolInput of BoolInputProps<'a>
  | IntegerInput of IntegerInputProps<'a>

type Input<'a> =
  { Props: InputProps
    Type: InputType<'a> }

module Unterstuetzen =
  type Field =
    | FirstName
    | LastName
    | Street
    | City
    | Email
    | DataUsageConsent

  let path = "unterstuetzen.php"
  let inputs =
    [
      { Props =
          { Key = "firstName"
            Title = "Vorname"
            ErrorText = "Bitte geben Sie Ihren Namen an, damit wir sie persönlich anschreiben können." }
        Type =
          StringInput
            { Field = FirstName
              Value = None
              Validation = NotEmptyOrWhitespace } }

      { Props =
          { Key = "lastName"
            Title = "Nachname"
            ErrorText = "Bitte geben Sie Ihren Namen an, damit wir sie persönlich anschreiben können." }
        Type =
          StringInput
            { Field = LastName
              Value = None
              Validation = NotEmptyOrWhitespace } }

      { Props =
          { Key = "street"
            Title = "Straße + Hausnummer"
            ErrorText = "Bitte geben Sie Ihre Adresse an, an die wir z.B. Einladungen senden können." }
        Type =
          StringInput
            { Field = Street
              Value = None
              Validation = NotEmptyOrWhitespace } }

      { Props =
          { Key = "city"
            Title = "PLZ + Ort"
            ErrorText = "Bitte geben Sie Ihre Adresse an, an die wir z.B. Einladungen senden können." }
        Type =
          StringInput
            { Field = City
              Value = None
              Validation = NotEmptyOrWhitespace } }

      { Props =
          { Key = "email"
            Title = "E-Mail"
            ErrorText = "Bitte geben Sie eine E-Mail-Adresse an, an die wir z.B. unseren Newsletter senden können." }
        Type =
          StringInput
            { Field = Email
              Value = None
              Validation = ContainsCharacter '@' } }

      { Props =
          { Key = "dataUsageConsent"
            Title = "Ich bin damit einverstanden, dass diese Daten für die Zusendung des Newsletters, Einladungen und weitere Informationszwecke verwendet werden. Die Daten werden nicht an Dritte weitergegeben."
            ErrorText = "Bitte akzeptieren sie die Einwilligungserklärung gemäß Datenschutz zur Verarbeitung Ihrer Daten." }
        Type =
          BoolInput
            { Field = DataUsageConsent
              Value = None
              Validation = MustBeTrue } }
    ]
