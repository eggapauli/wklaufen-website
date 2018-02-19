module Contests

open System

let download credentials =
    OOEBV.login credentials
    |> Async.bind (Choice.bindAsync OOEBV.Contests.loadAndResetContestOverviewPage)
    |> Async.map (Choice.bind OOEBV.Contests.loadContests)

let serializeContest (m: DataModels.Contest) =
    [
        yield "{"
        yield!
            [
                yield sprintf "Year = %d" m.Year
                yield sprintf "Type = %A" m.Type
                yield sprintf "Category = \"%s\"" m.Category
                yield sprintf "Points = %f" m.Points
                yield sprintf "Result = \"%s\"" m.Result
                yield sprintf "Location = \"%s\"" m.Location
            ]
            |> List.map (sprintf "  %s")
        yield "}"
    ]

let serialize contests =
    contests
    |> Seq.map (
        serializeContest
        >> List.map (sprintf "    %s")
        >> String.concat Environment.NewLine
    )
    |> String.concat Environment.NewLine
    |> (sprintf """module Generated.Contests

open DataModels

let items =
  [
%s
  ]""")
