module WkLaufen.Website.DataLoader.Test.Program

open Expecto

let tests =
    testList "All" [
        BMV.tests
    ]

[<EntryPoint>]
let main argv =
    runTestsWithCLIArgs [] [||] tests
