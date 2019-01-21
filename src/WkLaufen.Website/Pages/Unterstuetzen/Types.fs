module Unterstuetzen.Types

open System
open Elmish
open Fable.Core.JsInterop
open Fetch
open Thoth.Json

type ValidationResult =
  | NotValidated
  | Valid
  | ValidationError of string
type ValidatedInput =
  { Input: Forms.Input<Forms.Unterstuetzen.Field>
    Error: ValidationResult }

type FormState = NotSent | Sending | SendSuccess | SendError

type Model =
  { Inputs: ValidatedInput list
    FormState: FormState }

type Msg =
  | Update of Forms.InputType<Forms.Unterstuetzen.Field>
  | Validate of Forms.InputType<Forms.Unterstuetzen.Field>
  | Submit
  | SubmitResponse of Result<Result<Response, Map<string, string>>, exn>

let init =
  { Inputs =
      Forms.Unterstuetzen.inputs
      |> List.map (fun input -> { Input = input; Error = NotValidated })
    FormState = NotSent
  }

let isStringInputValid validation value =
  match validation with
  | Forms.NotEmptyOrWhitespace -> not <| String.IsNullOrWhiteSpace value
  | Forms.ContainsCharacter c -> String.exists ((=) c) value

let isBoolInputValid validation value =
  match validation with
  | Forms.MustBeTrue -> value

let private validateInput (input: Forms.Input<_>) =
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


let private encodeForm inputs =
  let encodeInput inputType =
    match inputType with
    | Forms.StringInput inputProps -> Encode.option Encode.string inputProps.Value
    | Forms.BoolInput inputProps -> Encode.option Encode.bool inputProps.Value

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

let private postJson url data properties =
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

let update msg model =
  match msg with
  | Update newInputType ->
    let inputs =
      model.Inputs
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
      )
    { model with Inputs = inputs }, Cmd.none
  | Validate inputToValidate ->
    let inputs =
      model.Inputs
      |> List.map (fun input ->
        if inputToValidate = input.Input.Type
        then { input with Error = validateInput input.Input }
        else input
      )
    { model with Inputs = inputs }, Cmd.none
  | Submit ->
    let validatedInputs =
      model.Inputs
      |> List.map (fun input -> { input with Error = validateInput input.Input })
    let inputsValid =
      validatedInputs
      |> List.forall (fun input ->
        match input.Error with
        | Valid -> true
        | NotValidated
        | ValidationError _ -> false
      )
    let model' = { model with Inputs = validatedInputs }

    if inputsValid
    then
      let cmd =
        Cmd.ofPromise
          (postJson Forms.Unterstuetzen.path (encodeForm model.Inputs))
          []
          (Ok >> SubmitResponse)
          (Error >> SubmitResponse)
      { model' with FormState = Sending }, cmd
    else
      { model' with FormState = NotSent }, Cmd.none
  | SubmitResponse (Ok (Ok _)) ->
    let model' = { model with FormState = SendSuccess }
    model', Cmd.none
  | SubmitResponse (Ok (Error messages)) ->
    // TODO show validation errors
    model, Cmd.none
  | SubmitResponse (Error e) -> 
    let model' = { model with FormState = SendError }
    model', Cmd.none
