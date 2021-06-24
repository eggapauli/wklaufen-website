module Unterstuetzen.View

open Fable.FontAwesome
open Fable.React
open Fable.React.Props
open Fulma
open global.Data
open Unterstuetzen.Types
open App.Forms


let root model dispatch =
  let canSendForm =
      match model.FormState with
      | NotSent
      | SendError -> true
      | Sending
      | SendSuccess -> false

  let getIcon = function
    | Forms.Unterstuetzen.FirstName -> Fa.Solid.User
    | Forms.Unterstuetzen.LastName -> Fa.Solid.User
    | Forms.Unterstuetzen.Street -> Fa.Solid.Building
    | Forms.Unterstuetzen.City -> Fa.Solid.MapPin
    | Forms.Unterstuetzen.Email -> Fa.Solid.Envelope
    | Forms.Unterstuetzen.DataUsageConsent -> Fa.Solid.Gavel

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
    "support"
    Images.unterstuetzen_w1000h600
    [
      div [Class "rich-text text"]
        [ span []
            [ str "Unterstützendes Mitglied"
              br []
              str "der Werkskapelle Laufen Gmunden-Engelhof" ]
          |> App.Html.modernHeader "Ja, ich will" "werden."

          Content.content []
            [ p []
                [ str "Ein altes Sprichwort sagt "
                  em [] [ str "\"Tradition ist nicht die Anbetung der Asche, sondern die Weitergabe des Feuers!\"" ]
                  str "."
                  br []
                  str "Dass alte und auch jüngere Traditionen verbunden mit modernen Aspekten einen echten musikalischen Hochgenuss ergeben können, beweist die Werkskapelle Laufen Gmunden-Engelhof mit ihren vielfältigen Ensembles regelmäßig bei Konzerten, Konzertwertungen und –reisen, Marschwertungen und diversen Ausrückungen. Unser Auftrag ist es nicht nur diese Traditionen hoch zu halten und Ihnen mit erstklassiger Blasmusik eine Freude zu bereiten, sondern sie an die nächsten Generationen weiterzugeben. Als unterstützendes Mitglied unseres Vereins helfen Sie uns mit 15 € jährlich diese Aufgabe in die Tat umzusetzen. Unsere unterstützenden Mitglieder erwarten außerdem ein Newsletter mit Informationen zu allen Terminen und Vereinsaktivitäten und eine persönliche Einladung zu Veranstaltungshighlights." ] ]

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
                                [ Display DisplayOptions.InlineBlock
                                  FontFamily "'Dancing Script', cursive"
                                  FontSize "24px"
                                  Color "darkgoldenrod"
                                  TextShadow "0 0 1px black" ] ]
                            [ str "Danke für Ihre Unterstützung!" ]
                      | SendError ->
                        yield
                          span
                            [ Style
                                [ Display DisplayOptions.InlineBlock
                                  MarginLeft "20px"
                                  FontFamily "'Dancing Script', cursive"
                                  FontSize "24px"
                                  Color "orangered"
                                  TextShadow "0 0 1px black" ] ]
                            [ str "Leider ist ein interner Fehler aufgetreten. Bitte versuchen Sie es später noch einmal oder schreiben Sie uns direkt eine E-Mail an marketing@wk-laufen.at. Vielen Dank." ]
                    ] ] ] ]
    ]
