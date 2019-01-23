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
  | MitgliedWerden
  | Wertungen
  | Jugend
  | Floetenkids
  | Impressum
  | Jahreskonzert

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
  | MitgliedWerden -> "#mitglied-werden"
  | Wertungen -> "#wertungen"
  | Jugend -> "#jugend"
  | Floetenkids -> "#floetenkids"
  | Impressum -> "#impressum"
  | Jahreskonzert -> "#jahreskonzert"

let toUrl = toHash >> fun s -> s.Replace("#", "/")
