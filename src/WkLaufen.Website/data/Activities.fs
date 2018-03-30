module Data

open System

type RecurrenceGroup = Probe

type RecurrenceType =
    | Weekly

type AdditionalActivityData =
    | EndTime of DateTime
    | CssClass of string
    | MeetingTime of DateTime
    | MeetingLocation of string
    | Recurrence of RecurrenceGroup * RecurrenceType
    | OverwriteRecurrenceOnce of RecurrenceGroup

type ActivityData = {
  Title: string
  BeginTime: DateTime
  Location: string
  More: AdditionalActivityData list
}

type Activity =
    | Public of ActivityData
    | Internal of ActivityData

module Activities =
    let items =
      [
        Public
            {
                Title = "Palmsonntag"
                BeginTime = DateTime(2018, 03, 25, 09, 00, 00)
                Location = "Gmunden"
                More = []
            }
        Internal
            {
                Title = "Gesamtprobe"
                BeginTime = DateTime(2018, 03, 28, 19, 30, 00)
                Location = "Musizimmer"
                More = [ Recurrence (Probe, Weekly) ]
            }
        Internal
            {
                Title = "Gesamt- oder Marschprobe"
                BeginTime = DateTime(2018, 04, 04, 19, 30, 00)
                Location = "Musizimmer"
                More = [ OverwriteRecurrenceOnce Probe ]
            }
        Internal
            {
                Title = "Geburtstagsfeier Schobi"
                BeginTime = DateTime(2018, 04, 07, 18, 30, 00)
                Location = "Ohlsdorf"
                More = []
            }
        Public
            {
                Title = "Festzug 110 Jahre Gamundia"
                BeginTime = DateTime(2018, 04, 21, 18, 00, 00)
                Location = "Stadtpfarrkirche"
                More = []
            }
        Public
            {
                Title = "Maibaum aufstellen"
                BeginTime = DateTime(2018, 04, 30, 16, 30, 00)
                Location = "Schloss Cumberland / Rathausplatz"
                More = []
            }
        Public
            {
                Title = "Weckruf"
                BeginTime = DateTime(2018, 05, 01, 00, 00, 00)
                Location = "Gmunden"
                More =
                [
                    MeetingTime (DateTime(2018, 05, 01, 05, 00, 00))
                    MeetingLocation "Musizimmer"
                ]
            }
        Internal
            {
                Title = "Marschprobe"
                BeginTime = DateTime(2018, 05, 02, 19, 30, 00)
                Location = "Musizimmer"
                More = [ OverwriteRecurrenceOnce Probe ]
            }
        Internal
            {
                Title = "Marschprobe"
                BeginTime = DateTime(2018, 05, 09, 19, 30, 00)
                Location = "Musizimmer"
                More = [ OverwriteRecurrenceOnce Probe ]
            }
        Internal
            {
                Title = "Gesamt- oder Marschprobe"
                BeginTime = DateTime(2018, 05, 06, 19, 30, 00)
                Location = "Musizimmer"
                More = [ OverwriteRecurrenceOnce Probe ]
            }
        Internal
            {
                Title = "Gesamt- oder Marschprobe"
                BeginTime = DateTime(2018, 05, 23, 19, 30, 00)
                Location = "Musizimmer"
                More = [ OverwriteRecurrenceOnce Probe ]
            }
        Public
            {
                Title = "Erstkommunion"
                BeginTime = DateTime(2018, 05, 27, 09, 00, 00)
                Location = "Stadtpfarrkirche"
                More = []
            }
        Internal
            {
                Title = "Gesamt- oder Marschprobe"
                BeginTime = DateTime(2018, 05, 30, 19, 30, 00)
                Location = "Musizimmer"
                More = [ OverwriteRecurrenceOnce Probe ]
            }
        Public
            {
                Title = "Fronleichnam"
                BeginTime = DateTime(2018, 05, 31, 08, 00, 00)
                Location = "Stadtpfarrkirche"
                More = []
            }
        Internal
            {
                Title = "Gesamt- oder Marschprobe"
                BeginTime = DateTime(2018, 06, 06, 19, 30, 00)
                Location = "Musizimmer"
                More = [ OverwriteRecurrenceOnce Probe ]
            }
        Public
            {
                Title = "BMF MV Roitham am Traunfall"
                BeginTime = DateTime(2018, 06, 09, 16, 00, 00)
                Location = "Roitham"
                More = []
            }
        Internal
            {
                Title = "Generalprobe Schlosskonzert"
                BeginTime = DateTime(2018, 06, 18, 19, 30, 00)
                Location = "Musizimmer"
                More = []
            }
        Public
            {
                Title = "Schlosskonzert"
                BeginTime = DateTime(2018, 06, 20, 19, 30, 00)
                Location = "Schloss Ort"
                More = [ MeetingTime (DateTime(2018, 06, 20, 17, 45, 00)); CssClass "highlight" ]
            }
        Public
            {
                Title = "BMF MV Hofkirchen an der Trattnach"
                BeginTime = DateTime(2018, 06, 30, 14, 00, 00)
                Location = "Hofkirchen"
                More = []
            }
        Public
            {
                Title = "Festzug zum Rathausplatz"
                BeginTime = DateTime(2018, 08, 15, 17, 30, 00)
                Location = "Yachtclub Gmunden"
                More = []
            }
        Public
            {
                Title = "Töpfermarkt"
                BeginTime = DateTime(2018, 08, 24, 17, 00, 00)
                Location = "Stadtplatz"
                More = []
            }
        Public
            {
                Title = "Tag der Tracht"
                BeginTime = DateTime(2018, 09, 09, 10, 00, 00)
                Location = ""
                More = []
            }
        Internal
            {
                Title = "Musiausflug"
                BeginTime = DateTime(2018, 09, 14, 13, 00, 00)
                Location = "Brixen"
                More =
                [
                    EndTime (DateTime(2018, 09, 16, 00, 00, 00))
                    MeetingLocation "Busparkplatz Laufen"
                ]
            }
        Public
            {
                Title = "Konzertwertung"
                BeginTime = DateTime(2018, 11, 03, 00, 00, 00)
                Location = ""
                More = [ CssClass "highlight" ]
            }
        Public
            {
                Title = "Adventkonzert"
                BeginTime = DateTime(2018, 12, 16, 18, 00, 00)
                Location = "Kapuzinerkloster"
                More = [ MeetingTime (DateTime(2018, 12, 16, 16, 00, 00)); CssClass "highlight" ]
            }
      ]
