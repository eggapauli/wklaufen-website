module Activities

open DataModels
open Ical.Net
open Ical.Net.CalendarComponents
open Ical.Net.DataTypes
open Ical.Net.Serialization
open System
open System.Globalization
open System.IO
open System.Text.RegularExpressions

let private getImportance (title: string) =
    let importantEvents =
        [
            "jahreskonzert"
            "schlosskonzert"
            "konzertwertung"
        ]
    if List.contains (title.ToLower()) importantEvents then Important
    else Normal

let private timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById "Central European Standard Time"

let private toActivityTimestamp (date: IDateTime) =
    let dateTime =
        if date.Value.Kind = DateTimeKind.Utc
        then TimeZoneInfo.ConvertTimeFromUtc(date.Value, timeZoneInfo)
        else date.Value
        |> fun dt -> DateTimeOffset(dt, timeZoneInfo.GetUtcOffset dt)
    if date.HasTime
    then DataModels.DateTime dateTime
    else Date dateTime

let fromFile path =
    let calendar =
        use stream = File.OpenRead path
        Calendar.Load stream

    let from = DateTime.Today.AddDays -7.
    let ``to`` = from.AddYears(1).AddDays(-1.)
    calendar.GetOccurrences(from, ``to``)
    |> Seq.map (fun v ->
        let calendarEvent = v.Source :?> CalendarEvent
        
        // Ical.Net calculates a wrong period on a day where Daylight Saving Time changes
        if v.Period.Duration <> TimeSpan.Zero && not v.Period.StartTime.HasTime && not v.Period.EndTime.HasTime
        then
            let fix = timeZoneInfo.GetUtcOffset v.Period.EndTime.Value -
                      timeZoneInfo.GetUtcOffset v.Period.StartTime.Value
            v.Period.EndTime <- v.Period.StartTime.Add(v.Period.Duration + fix)

        let endTime =
            v.Period.EndTime
            |> Option.ofObj
            |> Option.map toActivityTimestamp
        {
            Title = calendarEvent.Summary
            BeginTime = toActivityTimestamp v.Period.StartTime
            EndTime = endTime
            Location = Option.ofObj calendarEvent.Location
            Importance = getImportance calendarEvent.Summary
        })
    |> Seq.sortBy (fun v -> v.BeginTime)
    |> Seq.toList

let fixKonzertmeisterCalendar (content: string) =
    let calendar = Calendar.Load content
    calendar.Events
    |> Seq.toList
    |> List.iter (fun evt ->
        let fns = [
            fun (evt: CalendarEvent) ->
                evt.Summary <- Regex.Replace(evt.Summary, @"\s*\([^)]+\)$", "") // Remove Konzertmeister association name
                Some evt

            fun (evt: CalendarEvent) ->
                if Regex.IsMatch(evt.Description, "^(auftritt|performance)(:|$)", RegexOptions.IgnoreCase)
                then
                    evt.Description <- Regex.Replace(evt.Description, @"^(auftritt|performance)(:\s*)?", "", RegexOptions.IgnoreCase)
                    Some evt
                else None

            fun (evt: CalendarEvent) ->
                let descriptionLines =
                    evt.Description.Split([| "\r\n"; "\r"; "\n" |], StringSplitOptions.None)
                    |> Array.toList
                let metadataLines =
                    match descriptionLines |> List.skipWhile (fun line -> not <| line.Trim().StartsWith("---")) with
                    | [] -> descriptionLines
                    | x -> List.skip 1 descriptionLines
                let metadata =
                    metadataLines
                    |> List.choose (fun line ->
                        match line.IndexOf(':') with
                        | -1 -> None
                        | idx -> Some (line.Substring(0, idx).Trim().ToLowerInvariant(), line.Substring(idx + 1).Trim())
                    )
                    |> Map.ofList
                let doTry fn arg =
                    match fn arg with
                    | (true, value) -> Some value
                    | (false, _) -> None
                let parseAndUpdateTime (originalTimestamp: IDateTime) v =
                    let timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById "W. Europe Standard Time"
                    let setTime (t: TimeSpan) =
                        let d = originalTimestamp.Value
                        if t = TimeSpan.Zero then
                            let result = CalDateTime(DateTime(d.Year, d.Month, d.Day)) :> IDateTime
                            printfn "%b" result.HasTime
                            result
                        else
                            let dt = DateTime(d.Year, d.Month, d.Day, t.Hours, t.Minutes, t.Seconds, t.Milliseconds)
                            TimeZoneInfo.ConvertTimeToUtc(dt, timeZoneInfo)
                            |> CalDateTime
                            :> IDateTime
                    let culture = CultureInfo.GetCultureInfo "de-AT"
                    doTry (fun () -> TimeSpan.TryParse(v, culture)) ()
                    |> Option.map setTime
                    |> Option.orElseWith (fun () ->
                        doTry (fun () -> DateTime.TryParse(v, culture, DateTimeStyles.None)) ()
                        |> Option.map (fun d ->
                            if d.TimeOfDay = TimeSpan.Zero then CalDateTime(DateTime(d.Year, d.Month, d.Day)) :> IDateTime
                            else
                                TimeZoneInfo.ConvertTimeToUtc(d, timeZoneInfo)
                                |> CalDateTime
                                :> IDateTime
                        )
                    )
                    |> Option.defaultWith (fun () -> failwithf "Can't parse time: \"%s\" (Event title = %s)" v evt.Summary)
                evt.Summary <- Map.tryFind "titel" metadata |> Option.defaultValue evt.Summary
                evt.Description <- Map.tryFind "details" metadata |> Option.defaultValue ""
                evt.Start <-
                    Map.tryFind "beginn" metadata
                    |> Option.map (parseAndUpdateTime evt.Start)
                    |> Option.defaultValue evt.Start
                evt.End <-
                    Map.tryFind "ende" metadata
                    |> Option.map (parseAndUpdateTime evt.End)
                    |> Option.defaultValue evt.End
                evt.Location <-
                    Map.tryFind "ort" metadata
                    |> Option.defaultValue evt.Location
                Some evt
        ]
        match List.fold (flip Option.bind) (Some evt) fns with
        | Some evt -> ()
        | None -> calendar.Events.Remove evt |> ignore
    )
    CalendarSerializer().SerializeToString calendar

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
