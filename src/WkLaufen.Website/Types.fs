module App.Types

open Global

type Msg =
  | GoBack

type Model = {
  CurrentPage: Page
}
