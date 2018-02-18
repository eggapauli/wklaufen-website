module Global

type Page =
  | Home
  | Kontakte
  | News
  | NewsDetails of string
  | Termine
  | Musiker
  | MusikerRegister of string
  | BMF2017
  | WirUeberUns
  | Vision2020
  | Wertungen
  | Jugend
  | Floetenkids

let toHash = function
  | Home -> "#home"
  | Kontakte -> "#kontakte"
  | News -> "#news"
  | NewsDetails newsId -> sprintf "#news/%s" newsId
  | Termine -> "#termine"
  | Musiker -> "#musiker"
  | MusikerRegister registerId -> sprintf "#musiker/%s" registerId
  | BMF2017 -> "#bmf-2017"
  | WirUeberUns -> "#wir-ueber-uns"
  | Vision2020 -> "#vision-2020"
  | Wertungen -> "#wertungen"
  | Jugend -> "#jugend"
  | Floetenkids -> "#floetenkids"
