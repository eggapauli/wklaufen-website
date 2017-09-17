#if INTERACTIVE
#I @"..\..\"
#r @"packages\FSharp.Data\lib\net40\FSharp.Data.dll"
#load "Async.fsx"
#load "Choice.fsx"
#load "DataModels.fsx"
#load "Http.fsx"
#load "Json.fsx"
#load "OOEBV.fsx"
#load "DownloadHelper.fsx"
#endif

#if COMPILED
module Members
#endif

open System
open System.IO
open System.Text.RegularExpressions
open DownloadHelper

let download credentials =
    OOEBV.login credentials
    |> Async.bind (Choice.bindAsync OOEBV.Contests.loadAndResetContestOverviewPage)
    |> Async.map (Choice.bind OOEBV.Contests.loadContests)

let getJson (contest: DataModels.Contest) =
    sprintf """{
        "Year": %d,
        "Type": "%s",
        "Category": "%s",
        "Points": %f,
        "Result": "%s"
    }"""
        contest.Year
        (DataModels.ContestType.toString contest.Type)
        contest.Category
        contest.Points
        contest.Result
