module Global

type Page =
  | Home
  | Kontakte
  | News
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
  | Instrumentenfindung

let toLink = function
  | Home -> "#home"
  | Kontakte -> "#kontakte"
  | News -> "https://www.facebook.com/werkskapellelaufen"
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
  | Instrumentenfindung -> "#finde-dein-instrument"

let toUrl = toLink >> fun s -> s.Replace("#", "/")
