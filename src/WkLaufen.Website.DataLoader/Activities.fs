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

let private toActivityTimestamp (date: IDateTime) =
    if not date.HasTime
    then
        // I think there's a bug in Ical.Net.
        // To reproduce, create an "All day"-event on the day where Daylight Saving Time switches from winter to summer time.
        // `date.Value` of the end time was 1 hour behind
        if date.Value.TimeOfDay > TimeSpan(12, 0, 0)
        then System.DateTime(date.Value.Year, date.Value.Month, date.Value.Day, 0, 0, 0).AddDays(1.)
        else System.DateTime(date.Value.Year, date.Value.Month, date.Value.Day, 0, 0, 0)
        |> Date
    else DateTime date.Value

let fromFile path =
    let calendar =
        use stream = File.OpenRead path
        Calendar.Load stream

    let from = DateTime.Today.AddDays -7.
    let ``to`` = from.AddYears(1).AddDays(-1.)
    calendar.GetOccurrences(from, ``to``)
    |> Seq.map (fun v ->
        let calendarEvent = v.Source :?> CalendarEvent
        let endTime =
            v.Period.EndTime
            |> Option.ofObj
            |> Option.map toActivityTimestamp
        {
            Title = calendarEvent.Summary
            BeginTime = toActivityTimestamp v.Period.StartTime
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
