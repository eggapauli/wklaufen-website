module Serialize

open System
open DataModels

let date (d: DateTime) =
    sprintf
        "System.DateTime(%d, %d, %d, %d, %d, %d, %d, %s)"
        d.Year
        d.Month
        d.Day
        d.Hour
        d.Minute
        d.Second
        d.Millisecond
        (sprintf "%s.%O" typeof<DateTimeKind>.FullName d.Kind)

let dateOption = function
    | Some d -> date d |> sprintf "%s |> Some"
    | None -> "None"

let timespan (v: TimeSpan) =
    sprintf "System.TimeSpan(%d, %d, %d, %d, %d)" v.Days v.Hours v.Minutes v.Seconds v.Milliseconds

let dateTimeOffset (d: DateTimeOffset) =
    sprintf
        "System.DateTimeOffset(%d, %d, %d, %d, %d, %d, %d, %s)"
        d.Year
        d.Month
        d.Day
        d.Hour
        d.Minute
        d.Second
        d.Millisecond
        (timespan d.Offset)

let stringOption = function
    | Some v -> sprintf "Some \"%s\"" v
    | None -> "None"

let string =
    sprintf "\"\"\"%s\"\"\""

let seq lines =
    [
        yield "["
        yield! lines |> List.map (sprintf "  %s")
        yield "]"
    ]

let stringSeq = List.map string >> seq

let activityTimestamp = function
    | DateTime d -> dateTimeOffset d |> sprintf "%s |> DateTime"
    | Date d -> dateTimeOffset d |> sprintf "%s |> Date"

let activityTimestampOption = function
    | Some v -> activityTimestamp v |> sprintf "%s |> Some"
    | None -> "None"
