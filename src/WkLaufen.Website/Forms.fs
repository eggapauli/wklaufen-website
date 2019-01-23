module App.Forms

open System
open Fable.Core.JsInterop
open Fetch
open Thoth.Json

type ValidationResult =
  | NotValidated
  | Valid
  | ValidationError of string
type ValidatedInput<'a> =
  { Input: Forms.Input<'a>
    Error: ValidationResult }

type FormState = NotSent | Sending | SendSuccess | SendError

let private isStringInputValid validation value =
  match validation with
  | Forms.NotEmptyOrWhitespace -> not <| String.IsNullOrWhiteSpace value
  | Forms.ContainsCharacter c -> String.exists ((=) c) value

let private isBoolInputValid validation value =
  match validation with
  | Forms.MustBeTrue -> value

let private isIntegerInputValid validation value =
  match validation with
  | Forms.NonNegative -> value >= 0

let validateInput (input: Forms.Input<_>) =
  match input.Type with
  | Forms.StringInput inputProps ->
    match inputProps.Value with
    | Some value when isStringInputValid inputProps.Validation value -> Valid
    | Some _
    | None -> ValidationError input.Props.ErrorText
  | Forms.BoolInput inputProps ->
    match inputProps.Value with
    | Some value when isBoolInputValid inputProps.Validation value -> Valid
    | Some _
    | None -> ValidationError input.Props.ErrorText
  | Forms.IntegerInput inputProps ->
    match inputProps.Value with
    | Some value when isIntegerInputValid inputProps.Validation value -> Valid
    | Some _
    | None -> ValidationError input.Props.ErrorText

let encodeForm inputs =
  let encodeInput inputType =
    match inputType with
    | Forms.StringInput inputProps -> Encode.option Encode.string inputProps.Value
    | Forms.BoolInput inputProps -> Encode.option Encode.bool inputProps.Value
    | Forms.IntegerInput inputProps -> Encode.option Encode.int inputProps.Value

  inputs
  |> List.map (fun input -> input.Input.Props.Key, encodeInput input.Input.Type)
  |> Encode.object

let formErrorsDecoder =
  Decode.object (fun get ->
    let keys =
      Forms.Unterstuetzen.inputs
      |> List.map (fun input -> input.Props.Key)
    (Map.empty, keys) ||> List.fold (fun map key ->
      match get.Optional.Field key Decode.string with
      | Some value -> Map.add key value map
      | None -> map)
  )

let private errorString (response: Response) =
  string response.Status + " " + response.StatusText + " for URL " + response.Url

let postJson url data properties =
  let defaultProps =
    [ RequestProperties.Method HttpMethod.POST
      requestHeaders [ContentType "application/json"]
      RequestProperties.Body !^(Encode.toString 0 data)]
  // Append properties after defaultProps to make sure user-defined values
  // override the default ones if necessary
  let props = List.append defaultProps properties
  
  GlobalFetch.fetch(RequestInfo.Url url, requestProps props)
  |> Promise.map (fun response ->
    if response.Ok
    then Ok response
    elif response.Status = 400
    then Error response
    else failwith (errorString response))
  |> Promise.bind (function
    | Ok response -> Ok response |> Promise.lift
    | Error response -> response.text() |> Promise.map Error)
  |> Promise.map (Result.mapError (fun res ->
    match Decode.fromString formErrorsDecoder res with
    | Ok successValue -> successValue
    | Error error -> failwith error))

let updateInput inputs newInputType =
  inputs
  |> List.map (fun input ->
    match input.Input.Type with
    | Forms.StringInput inputProps ->
      match newInputType with
      | Forms.StringInput ({ Field = newInputField }) when inputProps.Field = newInputField ->
        { input with Input = { input.Input with Type = newInputType } }
      | _ -> input
    | Forms.BoolInput inputProps ->
      match newInputType with
      | Forms.BoolInput ({ Field = newInputField }) when inputProps.Field = newInputField ->
        { input with Input = { input.Input with Type = newInputType } }
      | _ -> input
    | Forms.IntegerInput inputProps ->
      match newInputType with
      | Forms.IntegerInput ({ Field = newInputField }) when inputProps.Field = newInputField ->
        { input with Input = { input.Input with Type = newInputType } }
      | _ -> input
  )

let validateSingleInput inputs inputToValidate =
  inputs
  |> List.map (fun input ->
    if inputToValidate = input.Input.Type
    then { input with Error = validateInput input.Input }
    else input
  )

let validateAllInputs inputs =
  let validatedInputs =
    inputs
    |> List.map (fun input -> { input with Error = validateInput input.Input })
  let inputsValid =
    validatedInputs
    |> List.forall (fun input ->
      match input.Error with
      | Valid -> true
      | NotValidated
      | ValidationError _ -> false
    )
  validatedInputs, inputsValid

module UI =
  open Fulma
  open Fable.Helpers.React
  open Fable.Helpers.React.Props
  open Fable.FontAwesome

  let textInput placeholder value icon error onChange onBlur =
    Field.div []
      [ yield Control.div [ Control.HasIconLeft ]
          [ Input.text
              [ yield Input.Placeholder placeholder
                yield Input.DefaultValue value
                match error with
                | NotValidated -> ()
                | Valid ->
                  yield Input.Color IsSuccess
                | ValidationError _ ->
                  yield Input.Color IsDanger
                yield Input.OnChange (fun ev -> onChange ev.Value)
                yield Input.Props [ OnBlur (ignore >> onBlur) ] ]
            Icon.icon
              [ Icon.Size IsSmall
                Icon.IsLeft ]
              [ Fa.i [ icon ] [] ] ]
        match error with
        | NotValidated
        | Valid -> ()
        | ValidationError message ->
          yield Help.help [ Help.Color IsDanger ]
            [ str message ]
      ]

  let boolInput title value error onChange onBlur =
    Field.div []
      [ yield Control.div []
          [ Checkbox.checkbox []
              [ Checkbox.input [ Props [ Checked value; OnChange (fun ev -> onChange ev.target?``checked``) ] ]
                span [ Style [ MarginLeft "5px" ] ] [ str title ] ] ]
        match error with
        | NotValidated
        | Valid -> ()
        | ValidationError message ->
          yield Help.help [ Help.Color IsDanger ]
            [ str message]
      ]

  let private tryParseInt value =
    match Int32.TryParse value with
    | (true, v) -> Some v
    | (false, _) -> None

  let integerInput placeholder value icon error onChange onBlur =
    Field.div []
      [ yield Control.div [ Control.HasIconLeft ]
          [ Input.number
              [ yield Input.Placeholder placeholder
                yield Input.DefaultValue value
                match error with
                | NotValidated -> ()
                | Valid ->
                  yield Input.Color IsSuccess
                | ValidationError _ ->
                  yield Input.Color IsDanger
                yield Input.OnChange (fun ev -> onChange (tryParseInt ev.Value))
                yield Input.Props [ OnBlur (ignore >> onBlur) ] ]
            Icon.icon
              [ Icon.Size IsSmall
                Icon.IsLeft ]
              [ Fa.i [ icon ] [] ] ]
        match error with
        | NotValidated
        | Valid -> ()
        | ValidationError message ->
          yield Help.help [ Help.Color IsDanger ]
            [ str message ]
      ]

