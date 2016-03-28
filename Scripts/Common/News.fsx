#if INTERACTIVE
#I @"..\..\"
#r @"packages\FSharp.Data\lib\net40\FSharp.Data.dll"
#load "Choice.fsx"
#load "DataModels.fsx"
#load "Http.fsx"
#load "DownloadHelper.fsx"
#load "Facebook.fsx"
#load "Json.fsx"
#endif


#if COMPILED
module News
#endif

open System
open System.IO
open DownloadHelper

let download accessToken =
    Facebook.getNews accessToken

let getJson (m: DataModels.FacebookNews, images) =
    sprintf """{
        "FacebookId": "%s",
        "Content": "%s",
        "Timestamp": "%s",
        "Images": %s
    }"""
        m.Id
        (Json.fromRichText m.News.Content)
        (m.News.Timestamp |> Json.fromDate)
        (images |> Json.fromArray)

let downloadImages baseDir (m: DataModels.FacebookNews) =
    m.Images
    |> List.mapi (fun idx imageUri ->
        let fileName = sprintf "%s_%d%s" m.Id (idx + 1) (getExtension imageUri)
        let filePath = Path.Combine(baseDir, "news", fileName)
        tryDownload imageUri filePath
        |> Async.map (Choice.map (fun () -> fileName))
    )
    |> Async.ofList
    |> Async.map (
        Choice.ofList
        >> Choice.map (fun images -> m, images)
    )
