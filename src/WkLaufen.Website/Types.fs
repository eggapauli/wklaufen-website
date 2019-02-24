module App.Types

open Global

type Msg =
  | GoBack
  | NewsMsg of News.Types.Msg
  | UnterstuetzenMsg of Unterstuetzen.Types.Msg
  | WirUeberUnsMsg of WirUeberUns.Types.Msg

type Model = {
  CurrentPage: Page
  NewsModel: News.Types.Model
  UnterstuetzenModel: Unterstuetzen.Types.Model
  WirUeberUnsModel: WirUeberUns.Types.Model
}
