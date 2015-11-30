#I @"..\..\"
#r @"packages\FSharp.Data\lib\net40\FSharp.Data.dll"

//#load @"..\common\DataModels.fsx"
//#load @"..\common\Json.fsx"
//#load @"..\facebook\Facebook.fsx"
//#load @".\DownloadHelper.fsx"

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
        |> Choice.map (fun () -> fileName)
    )
    |> Choice.ofList
    |> Choice.map (fun images -> m, images)
