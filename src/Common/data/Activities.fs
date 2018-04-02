namespace Data

open System

type RecurrenceGroup = Probe

type RecurrenceType =
    | Weekly

type ActivityData = {
    Title: string
    BeginTime: DateTime
    EndTime: DateTime option
    Location: string
}

type InternalActivityOverrides =
    | BeginTime of DateTime
    | EndTime of DateTime option
    | Location of string

type ActivityImportance = Important | Normal

type PublicActivityData = {
    Data: ActivityData
    InternalOverrides: InternalActivityOverrides list
    Importance: ActivityImportance
}

type Activity =
    | Public of PublicActivityData
    | Internal of ActivityData

type ActivityRecurrence =
    | NotRecurring of Activity
    | Recurring of (RecurrenceGroup * RecurrenceType) * Activity
    | OverwriteRecurrenceOnce of (RecurrenceGroup * DateTime option) * Activity
    | DeleteRecurrenceOnce of RecurrenceGroup * DateTime

module Activities =
    let private withoutRecurrence = NotRecurring

    let private withRecurrence settings activity =
        Recurring (settings, activity)

    let private overwriteRecurrenceOnceNoDateChange group activity =
        OverwriteRecurrenceOnce ((group, None), activity)

    let private overwriteRecurrenceOnce (group, date) activity =
        OverwriteRecurrenceOnce ((group, Some date), activity)

    let private deleteRecurringActivityOnce settings date =
        DeleteRecurrenceOnce (settings, date)

    let private makePublic importance internalOverrides data =
        {
            Data = data
            InternalOverrides = internalOverrides
            Importance = importance
        }
        |> Public

    let private makeInternal = Internal

    let private createActivity title beginTime location =
        {
            Title = title
            BeginTime = beginTime
            EndTime = None
            Location = location
        }

    let private withEndTime date activity =
        { activity with EndTime = Some date }

    let items =
        [
            createActivity "Palmsonntag" (DateTime(2018, 03, 25, 09, 00, 00)) "Gmunden"
            |> makePublic Normal []
            |> withoutRecurrence

            createActivity "Gesamtprobe" (DateTime(2018, 03, 28, 19, 30, 00)) "Musizimmer"
            |> withEndTime (DateTime(2018, 03, 28, 21, 30, 00))
            |> makeInternal
            |> withRecurrence (Probe, Weekly)

            createActivity "Gesamt- oder Marschprobe" (DateTime(2018, 04, 04, 19, 30, 00)) "Musizimmer"
            |> makeInternal
            |> overwriteRecurrenceOnceNoDateChange Probe

            createActivity "Geburtstagsfeier Schobi" (DateTime(2018, 04, 07, 18, 30, 00)) "Ohlsdorf"
            |> makeInternal
            |> withoutRecurrence

            createActivity "Festzug 110 Jahre Gamundia" (DateTime(2018, 04, 21, 18, 00, 00)) "Stadtpfarrkirche"
            |> makePublic Normal []
            |> withoutRecurrence

            createActivity "Maibaum aufstellen" (DateTime(2018, 04, 30, 16, 30, 00)) "Schloss Cumberland / Rathausplatz"
            |> makePublic Normal []
            |> withoutRecurrence

            createActivity "Weckruf" (DateTime(2018, 05, 01, 00, 00, 00)) "Gmunden"
            |> makePublic Normal [
                BeginTime (DateTime(2018, 05, 01, 05, 00, 00))
                EndTime (Some (DateTime(2018, 05, 01, 16, 00, 00)))
                Location "Musizimmer"
            ]
            |> withoutRecurrence

            createActivity "Marschprobe" (DateTime(2018, 05, 02, 19, 30, 00)) "Musizimmer"
            |> makeInternal
            |> overwriteRecurrenceOnceNoDateChange Probe

            createActivity "Marschprobe" (DateTime(2018, 05, 09, 19, 30, 00)) "Musizimmer"
            |> makeInternal
            |> overwriteRecurrenceOnceNoDateChange Probe

            createActivity "Gesamt- oder Marschprobe" (DateTime(2018, 05, 16, 19, 30, 00)) "Musizimmer"
            |> makeInternal
            |> overwriteRecurrenceOnceNoDateChange Probe

            createActivity "Gesamt- oder Marschprobe" (DateTime(2018, 05, 23, 19, 30, 00)) "Musizimmer"
            |> makeInternal
            |> overwriteRecurrenceOnceNoDateChange Probe

            createActivity "Erstkommunion" (DateTime(2018, 05, 27, 09, 00, 00)) "Stadtpfarrkirche"
            |> makePublic Normal []
            |> withoutRecurrence

            createActivity "Gesamt- oder Marschprobe" (DateTime(2018, 05, 30, 19, 30, 00)) "Musizimmer"
            |> makeInternal
            |> overwriteRecurrenceOnceNoDateChange Probe

            createActivity "Fronleichnam" (DateTime(2018, 05, 31, 08, 00, 00)) "Stadtpfarrkirche"
            |> makePublic Normal []
            |> withoutRecurrence

            createActivity "Gesamt- oder Marschprobe" (DateTime(2018, 06, 06, 19, 30, 00)) "Musizimmer"
            |> makeInternal
            |> overwriteRecurrenceOnceNoDateChange Probe

            createActivity "BMF MV Roitham am Traunfall" (DateTime(2018, 06, 09, 16, 00, 00)) "Roitham"
            |> makePublic Normal []
            |> withoutRecurrence

            createActivity "Generalprobe Schlosskonzert" (DateTime(2018, 06, 18, 19, 30, 00)) "Musizimmer"
            |> makeInternal
            |> overwriteRecurrenceOnce (Probe, DateTime(2018, 06, 20, 19, 30, 00))

            createActivity "Schlosskonzert" (DateTime(2018, 06, 20, 19, 30, 00)) "Schloss Ort"
            |> withEndTime (DateTime(2018, 06, 20, 21, 30, 00))
            |> makePublic Important [
                BeginTime (DateTime(2018, 06, 20, 17, 45, 00))
            ]
            |> withoutRecurrence

            createActivity "BMF MV Hofkirchen an der Trattnach" (DateTime(2018, 06, 30, 14, 00, 00)) "Hofkirchen"
            |> makePublic Normal []
            |> withoutRecurrence

            deleteRecurringActivityOnce Probe (DateTime(2018, 07, 04, 19, 30, 00))
            deleteRecurringActivityOnce Probe (DateTime(2018, 07, 11, 19, 30, 00))
            deleteRecurringActivityOnce Probe (DateTime(2018, 07, 18, 19, 30, 00))
            deleteRecurringActivityOnce Probe (DateTime(2018, 07, 25, 19, 30, 00))

            createActivity "Festzug zum Rathausplatz" (DateTime(2018, 08, 15, 17, 30, 00)) "Yachtclub Gmunden"
            |> makePublic Normal []
            |> withoutRecurrence

            createActivity "Töpfermarkt" (DateTime(2018, 08, 24, 17, 00, 00)) "Stadtplatz"
            |> makePublic Normal []
            |> withoutRecurrence

            createActivity "Tag der Tracht" (DateTime(2018, 09, 09, 10, 00, 00)) ""
            |> makePublic Normal []
            |> withoutRecurrence

            createActivity "Musiausflug" (DateTime(2018, 09, 14, 13, 00, 00)) "Brixen"
            |> withEndTime (DateTime(2018, 09, 16, 00, 00, 00))
            |> makeInternal
            |> withoutRecurrence

            createActivity "Konzertwertung" (DateTime(2018, 11, 03, 00, 00, 00)) ""
            |> makePublic Important []
            |> withoutRecurrence

            createActivity "Adventkonzert" (DateTime(2018, 12, 16, 18, 00, 00)) ""
            |> withEndTime (DateTime(2018, 12, 16, 19, 00, 00))
            |> makePublic Important [
                BeginTime (DateTime(2018, 12, 16, 16, 00, 00))
            ]
            |> withoutRecurrence
      ]
