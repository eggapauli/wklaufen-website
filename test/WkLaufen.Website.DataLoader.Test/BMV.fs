module WkLaufen.Website.DataLoader.Test.BMV

open Expecto
open System

let userName = Environment.GetEnvironmentVariable("BMV_USERNAME") |> Option.ofObj |> Option.defaultWith (fun () -> failwith "BMV_USERNAME not set")
let password = Environment.GetEnvironmentVariable("BMV_PASSWORD") |> Option.ofObj |> Option.defaultWith (fun () -> failwith "BMV_PASSWORD not set")

let tests =
    testList "BMV" [
        testCase "Can log in" <| fun _ ->
            let sessionCookie = BMV.login (userName, password)
            Expect.isNotEmpty sessionCookie "Session cookie should not be empty"
        ftestCaseAsync "Can get members" <| async {
            let sessionCookie = BMV.login (userName, password)
            use httpClient = BMV.createLoggedInHttpClient sessionCookie
            let! members = BMV.getMembers httpClient
            Expect.isNonEmpty members "Members should not be empty"
        }
    ]
