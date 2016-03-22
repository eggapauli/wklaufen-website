#I @"..\..\"
#r @"packages\ImageProcessor\lib\net45\ImageProcessor.dll"

//#load @"..\common\Choice.fsx"
//#load @"..\common\Http.fsx"

open System
open System.IO
open System.Text.RegularExpressions
open ImageProcessor

let private maxImageSize = System.Drawing.Size(1500, 1500)

let saveEntries filePath entries = 
    let content =
        entries
        |> String.concat ", "
        |> sprintf "[%s]"
    File.WriteAllText(filePath, content)

let download filePath uri =
    printfn "Downloading image from %O" uri
    Http.get uri
    |> Choice.map (fun res -> res.Content.ReadAsStreamAsync() |> Async.AwaitTask |> Async.RunSynchronously)
    |> Choice.map (fun content ->
        Path.GetDirectoryName filePath |> Directory.CreateDirectory |> ignore
        use imageFactory = new ImageFactory()
        imageFactory
            .Load(content)
            .Resize(Imaging.ResizeLayer(maxImageSize, Imaging.ResizeMode.Max, Upscale=false))
            .Save(filePath)
    )

let getExtension (uri: Uri) =
    uri.Segments
    |> Array.last
    |> Path.GetExtension
    |> fun x -> x.ToLowerInvariant()

let tryDownload (uri: Uri) filePath =
    match download filePath uri with
    | Choice1Of2 image -> Choice1Of2()
    | Choice2Of2 x -> Choice2Of2 (sprintf "Error while downloading image. %s" x)

let getFileNameFromTitle (title: string) =
    title
        .ToLowerInvariant()
        .Replace("\u00C4", "Ae")
        .Replace("\u00D6", "Oe")
        .Replace("\u00DC", "Ue")
        .Replace("\u00E4", "ae")
        .Replace("\u00F6", "oe")
        .Replace("\u00FC", "ue")
    |> fun t -> Regex.Replace(t, "[^a-zA-Z0-9]+", "-")
