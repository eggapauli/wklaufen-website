#if INTERACTIVE
#load "Choice.fsx"
#load "DataModels.fsx"
#load "Http.fsx"
#load "DownloadHelper.fsx"
#load "Facebook.fsx"
#load "Serialize.fsx"
#endif


#if COMPILED
module News
#endif

open System
open System.IO
open DownloadHelper

let download accessToken =
    Facebook.getNews accessToken

let serializeNews (n: DataModels.NewsEntry) =
    [
        yield "{"
        yield!
            [
                yield sprintf "Timestamp = %s" (Serialize.date n.Timestamp)
                yield sprintf "Content = %s" (Serialize.string n.Content)
            ]
            |> List.map (sprintf "  %s")
        yield "}"
    ]

let serializeLocalNews (n: DataModels.LocalNews) =
    [
        yield "{"
        yield sprintf "  Id = %s" (Serialize.string n.Id)
        yield "  News ="
        yield!
            serializeNews n.News
            |> List.map (sprintf "    %s")
        yield sprintf "  Images ="
        yield! Serialize.stringSeq n.Images |> List.map (sprintf "    %s")
        yield sprintf "  SourceUri = %s" (Serialize.string n.SourceUri)
        yield "}"
    ]

let serialize news =
    news
    |> Seq.map (
        serializeLocalNews
        >> List.map (sprintf "    %s")
        >> String.concat Environment.NewLine
    )
    |> String.concat Environment.NewLine
    |> (sprintf """module Generated.News

open DataModels

let items =
  [
%s
  ]""")

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
        >> Choice.map (fun images ->
        {
            DataModels.LocalNews.Id = m.Id
            DataModels.LocalNews.News = m.News
            DataModels.LocalNews.Images = images
            DataModels.LocalNews.SourceUri = sprintf "https://facebook.com/%s" m.Id
        })
    )
