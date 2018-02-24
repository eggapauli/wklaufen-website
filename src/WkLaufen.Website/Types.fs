module App.Types

open Global

type Msg =
  | GoBack
  | NewsMsg of News.Types.Msg

type Model = {
  CurrentPage: Page
  NewsModel: News.Types.Model
}
