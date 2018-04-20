module Activities

open System
open System.IO
open Ical.Net
open Ical.Net.CalendarComponents
open DataModels
open Ical.Net.DataTypes

let private getImportance (calendarEvent: CalendarEvent) =
    let isImportant =
        [
            "Jahreskonzert"
            "Schlosskonzert"
            "Konzertwertung"
            "Adventkonzert"
        ]
        |> Seq.exists (fun e -> calendarEvent.Summary.Equals(e, StringComparison.InvariantCultureIgnoreCase))
    if isImportant then Important
    else Normal

let private toActivityTimestamp (timezone: VTimeZone) (date: IDateTime) =
    if date.HasTime then date.ToTimeZone(timezone.TzId).Value |> DateTime
    else Date date.Value

let fromFile path =
    let calendar =
        use stream = File.OpenRead path
        Calendar.Load stream

    let from = DateTime.Today.AddDays -7.
    let ``to`` = from.AddYears(1).AddDays(-1.)
    calendar.GetOccurrences(from, ``to``)
    |> Seq.map (fun v ->
        let calendarEvent = v.Source :?> CalendarEvent
        let timezone = VTimeZone.FromDateTimeZone "Europe/Vienna"
        let endTime =
            v.Period.EndTime
            |> Option.ofObj
            |> Option.map (toActivityTimestamp timezone)
        {
            Title = calendarEvent.Summary
            BeginTime = toActivityTimestamp timezone v.Period.StartTime
            EndTime = endTime
            Location = Option.ofObj calendarEvent.Location
            Importance = getImportance calendarEvent
        })
    |> Seq.sortBy (fun v -> v.BeginTime)
    |> Seq.toList

let serializeActivity activity =
    [
        yield "{"
        yield!
            [
                yield sprintf "Title = %s" (Serialize.string activity.Title)
                yield sprintf "BeginTime = %s" (Serialize.activityTimestamp activity.BeginTime)
                yield sprintf "EndTime = %s" (Serialize.activityTimestampOption activity.EndTime)
                yield sprintf "Location = %s" (Serialize.stringOption activity.Location)
                yield sprintf "Importance = %A" activity.Importance
            ]
            |> List.map (sprintf "  %s")
        yield "}"
    ]

let serialize activities =
    activities
    |> Seq.map (
        serializeActivity
        >> List.map (sprintf "    %s")
        >> String.concat Environment.NewLine
    )
    |> String.concat Environment.NewLine
    |> (sprintf """module Data.Activities

open DataModels

let items =
  [
%s
  ]""")
