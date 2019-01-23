module Jahreskonzert.View

open System
open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fable.FontAwesome
open Fulma
open global.Data
open Jahreskonzert.Types
open App.Forms

let root model dispatch =
  let canSendForm =
      match model.FormState with
      | NotSent
      | SendError -> true
      | Sending
      | SendSuccess -> false

  let getIcon = function
    | Forms.Kartenreservierung.FirstName -> Fa.Solid.User
    | Forms.Kartenreservierung.LastName -> Fa.Solid.User
    | Forms.Kartenreservierung.PhoneNumber -> Fa.Solid.Phone
    | Forms.Kartenreservierung.Email -> Fa.Solid.Envelope
    | Forms.Kartenreservierung.StandardPriceTickets -> Fa.Solid.Male
    | Forms.Kartenreservierung.ReducedPriceTickets -> Fa.Solid.Child
    | Forms.Kartenreservierung.DataUsageConsent -> Fa.Solid.Gavel

  let formInputs =
    model.Inputs
    |> List.map (fun validatedInput ->
      match validatedInput.Input.Type with
      | Forms.StringInput inputProps as inputType ->
        UI.textInput
          validatedInput.Input.Props.Title
          (Option.defaultValue "" inputProps.Value)
          (getIcon inputProps.Field)
          validatedInput.Error
          (Some >> (fun v -> { inputProps with Value = v }) >> Forms.StringInput >> Update >> dispatch)
          (fun () -> dispatch (Validate inputType))
      | Forms.BoolInput inputProps as inputType ->
        UI.boolInput
          validatedInput.Input.Props.Title
          (Option.defaultValue false inputProps.Value)
          validatedInput.Error
          (Some >> (fun v -> { inputProps with Value = v }) >> Forms.BoolInput >> Update >> dispatch)
          (fun () -> dispatch (Validate inputType))
      | Forms.IntegerInput inputProps as inputType ->
        UI.integerInput
          validatedInput.Input.Props.Title
          (inputProps.Value |> Option.map string |> Option.defaultValue "")
          (getIcon inputProps.Field)
          validatedInput.Error
          ((fun v -> { inputProps with Value = v }) >> Forms.IntegerInput >> Update >> dispatch)
          (fun () -> dispatch (Validate inputType))
    )
  Layout.page
    "concert"
    Images.jahreskonzert_w1000h600
    [
      div [Class "rich-text text"]
        [ span []
            [ str "Jahreskonzert am 23. Februar 2019 um 19.43 Uhr"
              br []
              str "der Werkskapelle Laufen Gmunden-Engelhof" ]
          |> App.Html.modernHeader "Ja, ich werde zum" "kommen."

          div []
            [ yield! formInputs
              yield Field.div []
                [ Control.div []
                    [ yield
                        Button.button
                          [ Button.IsLink
                            Button.IsLoading (model.FormState = Sending)
                            Button.Disabled (not canSendForm)
                            Button.OnClick (fun _ev -> dispatch Submit)
                            Button.Props [ Style [ MarginRight "20px" ] ] ]
                          [ Icon.icon [] [ Fa.i [ Fa.Solid.PaperPlane ] [] ]
                            span [] [ str "Absenden" ] ]
                      match model.FormState with
                      | NotSent
                      | Sending -> ()
                      | SendSuccess ->
                        yield
                          span
                            [ Style
                                [ Display "inline-block"
                                  FontFamily "'Dancing Script', cursive"
                                  FontSize "24px"
                                  Color "darkgoldenrod"
                                  TextShadow "0 0 1px black" ] ]
                            [ str "Danke für Ihre Reservierung! Die Karten sind an der Abendkassa hinterlegt." ]
                      | SendError ->
                        yield
                          span
                            [ Style
                                [ Display "inline-block"
                                  MarginLeft "20px"
                                  FontFamily "'Dancing Script', cursive"
                                  FontSize "24px"
                                  Color "orangered"
                                  TextShadow "0 0 1px black" ] ]
                            [ str "Leider ist ein interner Fehler aufgetreten. Bitte versuchen Sie es später noch einmal oder schreiben Sie uns direkt eine E-Mail an obmann@wk-laufen.at. Vielen Dank." ]
                    ] ] ] ]
    ]