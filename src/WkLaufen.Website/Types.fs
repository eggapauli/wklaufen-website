module App.Types

open Global

type Msg =
  | GoBack
  | UnterstuetzenMsg of Unterstuetzen.Types.Msg
  | WirUeberUnsMsg of WirUeberUns.Types.Msg
  | JahreskonzertMsg of Jahreskonzert.Types.Msg

type Model = {
  CurrentPage: Page
  UnterstuetzenModel: Unterstuetzen.Types.Model
  WirUeberUnsModel: WirUeberUns.Types.Model
  JahreskonzertModel: Jahreskonzert.Types.Model
}
