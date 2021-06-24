module Unterstuetzen.Types

open Elmish
open Fetch
open App.Forms

type Model =
  { Inputs: ValidatedInput<Forms.Unterstuetzen.Field> list
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

let update msg model =
  match msg with
  | Update newInputType ->
    let inputs = updateInput model.Inputs newInputType
    { model with Inputs = inputs }, Cmd.none
  | Validate inputToValidate ->
    let inputs = validateSingleInput model.Inputs inputToValidate
    { model with Inputs = inputs }, Cmd.none
  | Submit ->
    let (validatedInputs, inputsValid) = validateAllInputs model.Inputs
    let model' = { model with Inputs = validatedInputs }

    if inputsValid
    then
      let cmd =
        Cmd.OfPromise.either
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
