open global.Data

type CalendarType =
    | PublicCalendar
    | InternalCalendar

module File =
    open System.IO

    let writeAllText path content =
        File.WriteAllText(path, content)

module Event =
    open System
    open Ical.Net
    open Ical.Net.CalendarComponents
    open Ical.Net.DataTypes

    let private toUid = function
        | Probe -> "e6fc42af-8df0-4657-81fb-a23362d25218"

    let private toRecurrencePattern recurrenceType (beginTime: DateTime) =
        match recurrenceType with
        | Weekly ->
            let pattern = RecurrencePattern(FrequencyType.Weekly)
            pattern.ByDay <- [ WeekDay(DayOfWeek = beginTime.DayOfWeek) ] |> System.Collections.Generic.List<_>
            pattern

    let fromActivityData (timezone: VTimeZone) data =
        let event =
            CalendarEvent(
                Start = CalDateTime(data.BeginTime, timezone.TzId),
                Summary = data.Title,
                Location = data.Location
            )

        event.End <-
            data.EndTime
            |> Option.map (fun date -> CalDateTime(date, timezone.TzId) :> IDateTime)
            |> Option.defaultValue (event.Start.Add (TimeSpan.FromHours 2.))

        event

    let applyInternalOverrides (timezone: VTimeZone) overrides (event: CalendarEvent) =
        event.Start <- event.Start.Subtract (TimeSpan.FromMinutes 15.)
        let apply = function
            | BeginTime date -> event.Start <- CalDateTime(date, timezone.TzId)
            | EndTime None -> event.End <- null
            | EndTime (Some date) -> event.End <- CalDateTime(date, timezone.TzId)
            | Location location -> event.Location <- location
        List.fold (fun () -> apply) () overrides
        event

    let fromActivity timezone calendarType activity =
        match calendarType, activity with
        | PublicCalendar, Internal _ -> None
        | InternalCalendar, Internal activity ->
            fromActivityData timezone activity
            |> Some
        | PublicCalendar, Public activity ->
            fromActivityData timezone activity.Data
            |> Some
        | InternalCalendar, Public activity ->
            fromActivityData timezone activity.Data
            |> applyInternalOverrides timezone activity.InternalOverrides
            |> Some

    let fromRecurringActivity timezone calendarType recurringActivities = function
        | NotRecurring activity ->
            fromActivity timezone calendarType activity
        | Recurring ((group, ``type``), activity) ->
            fromActivity timezone calendarType activity
            |> Option.map (fun event ->
                let recurrence = toRecurrencePattern ``type`` event.Start.Value
                event.RecurrenceRules.Add recurrence
                event.Uid <- toUid group

                let exceptionDates = PeriodList(TzId = timezone.TzId)
                recurringActivities
                |> Map.find group
                |> snd
                |> List.map (fun date -> CalDateTime(date, timezone.TzId))
                |> List.iter exceptionDates.Add
                event.ExceptionDates.Add exceptionDates

                event
            )
        | OverwriteRecurrenceOnce ((group, date), activity) ->
            fromActivity timezone calendarType activity
            |> Option.map (fun event ->
                event.Uid <- toUid group

                match date with
                | Some d ->
                    event.RecurrenceId <- CalDateTime(d, timezone.TzId)
                    event.Sequence <- 1
                | None ->
                    event.RecurrenceId <- event.Start

                recurringActivities
                |> Map.find group
                |> fst
                |> snd
                |> fromActivity timezone calendarType
                |> Option.iter (fun mainEvent ->
                    match Option.ofObj mainEvent.End, Option.ofObj event.End with
                    | Some mainEventEnd, None -> event.End <- event.Start.Add (mainEventEnd.Subtract mainEvent.Start)
                    | _ -> ()
                )

                event
            )
        | DeleteRecurrenceOnce _ -> None

module Calendar =
    open Ical.Net
    open Ical.Net.Serialization

    let create createEvents =
        let calendar = Calendar()
        let timezone = calendar.AddTimeZone "Europe/Vienna"
        calendar.Events.AddRange (createEvents timezone)
        calendar

    let serialize (calendar: Calendar) =
        let serializer = CalendarSerializer()
        serializer.SerializeToString calendar

let groupRecurringActivities activities =
    let folder map = function
        | NotRecurring _ -> map
        | OverwriteRecurrenceOnce _ -> map
        | Recurring ((group, ``type``), activity) ->
            Map.add group ((``type``, activity), []) map
        | DeleteRecurrenceOnce (group, date) ->
            let (activity, list) = Map.find group map
            Map.add group (activity, list @ [ date ]) map
    List.fold folder Map.empty activities

let createCalendar calendarType path =
    Calendar.create
        (fun timezone ->
            let recurringActivities = groupRecurringActivities Activities.items
            Activities.items
            |> List.choose (Event.fromRecurringActivity timezone calendarType recurringActivities)
        )
    |> Calendar.serialize
    |> File.writeAllText path

[<EntryPoint>]
let main argv =
    createCalendar InternalCalendar "internal.ics"
    createCalendar PublicCalendar "public.ics"
    0
