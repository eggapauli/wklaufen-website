module Asset

open System
open System.Drawing
open System.IO
open WebSharper.Html.Server

let private computeSize (imageWidth, imageHeight) (desiredWidthOpt, desiredHeightOpt) =
    let desiredWidth =
        match desiredWidthOpt with
        | Some x -> x
        | None -> imageWidth
    let desiredHeight =
        match desiredHeightOpt with
        | Some x -> x
        | None -> imageHeight

    let widthRatio = (float imageWidth) / (float desiredWidth)
    let heightRatio = (float imageHeight) / (float desiredHeight)
    let ratio = Math.Max(Math.Max(widthRatio, heightRatio), 1.)
    let width = Math.Round((float imageWidth) / ratio) |> int
    let height = Math.Round((float imageHeight) / ratio) |> int
    width, height

let resizeDefinitionFile = Path.Combine("assets", "resize.txt")
let private registerForResize container name desiredSize =
    let sourcePath = Path.Combine("assets", "images", container, name)
    use image = Image.FromFile sourcePath
    let (width, height) = computeSize (image.Width, image.Height) desiredSize
    let json = sprintf """{ "Path": "%s", "Width": %d, "Height": %d }""" (sourcePath.Replace(@"\", @"\\")) width height
    File.AppendAllLines(Path.Combine("assets", "resize.txt"), [ json ])
    sourcePath, (width, height)

let resize container name size =
    registerForResize container name size
    |> fst
    |> (sprintf "assets/images/%s/%s" container)

let htmlImage container name size =
    registerForResize container name size
    |> fun (fileName, (width, height)) ->
        Img [ Src (sprintf "assets/images/%s/%s" container fileName); Width (string width ); Height (string height) ]
