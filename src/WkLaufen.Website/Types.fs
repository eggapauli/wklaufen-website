module App.Types

open Global

type Msg =
  | GoBack
  | NewsMsg of News.Types.Msg
  | UnterstuetzenMsg of Unterstuetzen.Types.Msg

type Model = {
  CurrentPage: Page
  NewsModel: News.Types.Model
  UnterstuetzenModel: Unterstuetzen.Types.Model
}
