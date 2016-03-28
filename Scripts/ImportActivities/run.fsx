#if INTERACTIVE
#I @"..\Common\"
#load "Choice.fsx"
#load "Http.fsx"
#load "Json.fsx"
#load "DownloadHelper.fsx"
#load "Activities.fsx"
#endif

open System.IO
open DownloadHelper

let sourcePath = @"C:\Path\to\Activities.xls"
let targetPath = Path.Combine(__SOURCE_DIRECTORY__, "..", "..", "WkLaufen.Website", "data", "activities.json")

Activities.import sourcePath targetPath
printfn "Successfully imported activities."
