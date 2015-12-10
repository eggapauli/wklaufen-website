module Asset

open System
open System.Drawing
open System.IO
open WebSharper.Html.Server

let private computeSize desiredSize (imageWidth, imageHeight) =
    match desiredSize with
    | Some width, Some height ->
        width, height
    | Some width, None ->
        let ratio = (float imageWidth) / (float width)
        let height = Math.Round((float imageHeight) / ratio) |> int
        width, height
    | None, Some height ->
        let ratio = (float imageHeight) / (float height)
        let width = Math.Round((float imageWidth) / ratio) |> int
        width, height
    | None, None ->
        imageWidth, imageHeight

let private computeSizeNoCrop (desiredWidthOpt, desiredHeightOpt) (imageWidth, imageHeight) =
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
let private registerForResize container name sizeComputeFn =
    let sourcePath = Path.Combine("assets", "images", container, name)
    use image = Image.FromFile sourcePath
    let (width, height) = sizeComputeFn (image.Width, image.Height)
    let json = sprintf """{ "Path": "%s", "Width": %d, "Height": %d }""" (sourcePath.Replace(@"\", @"\\")) width height
    File.AppendAllLines(Path.Combine("assets", "resize.txt"), [ json ])
    let fileName = sprintf "%s_%dx%d%s" (Path.GetFileNameWithoutExtension sourcePath) width height (Path.GetExtension sourcePath)
    fileName, (width, height)

let private resizeImpl container name computeSizeFn =
    registerForResize container name computeSizeFn
    |> fst
    |> (sprintf "assets/images/%s/%s" container)

let resize container name size =
    resizeImpl container name (computeSize size)

let resizeNoCrop container name size =
    resizeImpl container name (computeSizeNoCrop size)

let private htmlImageImpl container name computeSizeFn =
    registerForResize container name computeSizeFn
    |> fun (fileName, (width, height)) ->
        Img [
            Src (sprintf "assets/images/%s/%s" container fileName)
            Width (string width)
            Height (string height)
        ]

let htmlImage container name size =
    htmlImageImpl container name (computeSize size)

let htmlImageNoCrop container name size =
    htmlImageImpl container name (computeSizeNoCrop size)
