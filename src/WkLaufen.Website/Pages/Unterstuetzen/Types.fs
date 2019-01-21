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
  { Input: Forms.Unterstuetzen.Input
    Error: ValidationResult }

type FormState = NotSent | Sending | SendSuccess | SendError

type Model =
  { Inputs: ValidatedInput list
    FormState: FormState }

type Msg =
  | Update of Forms.Unterstuetzen.Input
  | Validate of Forms.Unterstuetzen.Input
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

let private validateInput input =
  match input with
  | Forms.Unterstuetzen.FirstName (value, validation)
  | Forms.Unterstuetzen.LastName (value, validation)
  | Forms.Unterstuetzen.Street (value, validation)
  | Forms.Unterstuetzen.City (value, validation)
  | Forms.Unterstuetzen.Email (value, validation) ->
    match value with
    | Some value when isStringInputValid validation value -> Valid
    | Some _
    | None -> Forms.Unterstuetzen.getErrorText input |> ValidationError
  | Forms.Unterstuetzen.DataUsageConsent (value, validation) ->
    match value with
    | Some value when isBoolInputValid validation value -> Valid
    | Some _
    | None -> Forms.Unterstuetzen.getErrorText input |> ValidationError


let private encodeForm inputs =
  let encodeInput input =
    match input with
    | Forms.Unterstuetzen.FirstName (value, _)
    | Forms.Unterstuetzen.LastName (value, _)
    | Forms.Unterstuetzen.Street (value, _)
    | Forms.Unterstuetzen.City (value, _)
    | Forms.Unterstuetzen.Email (value, _) -> Encode.option Encode.string value
    | Forms.Unterstuetzen.DataUsageConsent (value, _) -> Encode.option Encode.bool value

  inputs
  |> List.map (fun input -> (Forms.Unterstuetzen.getKey input.Input), encodeInput input.Input)
  |> Encode.object

let formErrorsDecoder =
  Decode.object (fun get ->
    let keys =
      Forms.Unterstuetzen.inputs
      |> List.map Forms.Unterstuetzen.getKey
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
  | Update (Forms.Unterstuetzen.FirstName _ as newInput) ->
    let inputs =
      model.Inputs
      |> List.map (fun input ->
        match input.Input with
        | Forms.Unterstuetzen.FirstName _ -> { input with Input = newInput }
        | _ -> input
      )
    { model with Inputs = inputs }, Cmd.none
  | Update (Forms.Unterstuetzen.LastName _ as newInput) ->
    let inputs =
      model.Inputs
      |> List.map (fun input ->
        match input.Input with
        | Forms.Unterstuetzen.LastName _ -> { input with Input = newInput }
        | _ -> input
      )
    { model with Inputs = inputs }, Cmd.none
  | Update (Forms.Unterstuetzen.Street _ as newInput) ->
    let inputs =
      model.Inputs
      |> List.map (fun input ->
        match input.Input with
        | Forms.Unterstuetzen.Street _ -> { input with Input = newInput }
        | _ -> input
      )
    { model with Inputs = inputs }, Cmd.none
  | Update (Forms.Unterstuetzen.City _ as newInput) ->
    let inputs =
      model.Inputs
      |> List.map (fun input ->
        match input.Input with
        | Forms.Unterstuetzen.City _ -> { input with Input = newInput }
        | _ -> input
      )
    { model with Inputs = inputs }, Cmd.none
  | Update (Forms.Unterstuetzen.Email _ as newInput) ->
    let inputs =
      model.Inputs
      |> List.map (fun input ->
        match input.Input with
        | Forms.Unterstuetzen.Email _ -> { input with Input = newInput }
        | _ -> input
      )
    { model with Inputs = inputs }, Cmd.none
  | Update (Forms.Unterstuetzen.DataUsageConsent _ as newInput) ->
    let inputs =
      model.Inputs
      |> List.map (fun input ->
        match input.Input with
        | Forms.Unterstuetzen.DataUsageConsent _ -> { input with Input = newInput }
        | _ -> input
      )
    { model with Inputs = inputs }, Cmd.none
  | Validate inputToValidate ->
    let inputs =
      model.Inputs
      |> List.map (fun input ->
        if inputToValidate = input.Input
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
