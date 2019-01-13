module WirUeberUns.Types

type Model = {
  SlideNumber: int
}

type Msg =
  | SlideTo of int

let init =
  { SlideNumber = 0 }

let update msg model =
  match msg with
  | SlideTo idx -> { model with SlideNumber = idx }
