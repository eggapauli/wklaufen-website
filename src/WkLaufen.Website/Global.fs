module Global

type Page =
  | Home
  | Kontakte
  | News
  | NewsDetails of string
  | Termine
  | Musiker
  | MusikerRegister of string
  | Unterstuetzen
  | WirUeberUns
  | Vision2020
  | Wertungen
  | Jugend
  | Floetenkids
  | Impressum

let toHash = function
  | Home -> "#home"
  | Kontakte -> "#kontakte"
  | News -> "#news"
  | NewsDetails newsId -> sprintf "#news/%s" newsId
  | Termine -> "#termine"
  | Musiker -> "#musiker"
  | MusikerRegister registerId -> sprintf "#musiker/%s" registerId
  | Unterstuetzen -> "#unterstuetzen"
  | WirUeberUns -> "#wir-ueber-uns"
  | Vision2020 -> "#vision-2020"
  | Wertungen -> "#wertungen"
  | Jugend -> "#jugend"
  | Floetenkids -> "#floetenkids"
  | Impressum -> "#impressum"
