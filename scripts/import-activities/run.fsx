#load @"..\common\Choice.fsx"
#load @"..\common\CommandLine.fsx"
#load @"..\common\Http.fsx"
#load @"..\common\Json.fsx"
#load @".\DownloadHelper.fsx"
#load @".\Activities.fsx"

open System.IO
open DownloadHelper

match CommandLine.getArg "source" with
| Some sourcePath ->
    let targetPath = Path.Combine(__SOURCE_DIRECTORY__, "..", "..", "data", "activities.json")

    Activities.import sourcePath
    |> List.map Activities.getJson
    |> saveEntries targetPath
    |> fun _ -> printfn "Successfully imported activities."
| None -> failwith "One or more of the following arguments couldn't be found: \"source\""