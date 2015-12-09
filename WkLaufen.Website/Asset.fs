module Asset

open System
open System.Drawing
open System.IO
open WebSharper.Html.Server

let private computeSize (imageWidth, imageHeight) desiredSize =
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

let resizeDefinitionFile = Path.Combine("assets", "resize.txt")
let private registerForResize container name desiredSize =
    let sourcePath = Path.Combine("assets", "images", container, name)
    use image = Image.FromFile sourcePath
    let (width, height) = computeSize (image.Width, image.Height) desiredSize
    let json = sprintf """{ "Path": "%s", "Width": %d, "Height": %d }""" (sourcePath.Replace(@"\", @"\\")) width height
    File.AppendAllLines(Path.Combine("assets", "resize.txt"), [ json ])
    let fileName = sprintf "%s_%dx%d%s" (Path.GetFileNameWithoutExtension sourcePath) width height (Path.GetExtension sourcePath)
    fileName, (width, height)

let resize container name size =
    registerForResize container name size
    |> fst
    |> (sprintf "assets/images/%s/%s" container)

let htmlImage container name size =
    registerForResize container name size
    |> fun (fileName, (width, height)) ->
        Img [ Src (sprintf "assets/images/%s/%s" container fileName); Width (string width ); Height (string height) ]
