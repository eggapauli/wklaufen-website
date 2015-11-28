module Asset

open System
open System.Drawing
open System.Drawing.Drawing2D
open System.Drawing.Imaging
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

let private resizeImpl sourcePath (desiredWidth, desiredHeight) =
    use image = Image.FromFile sourcePath
    
    let (width, height) = computeSize (image.Width, image.Height) (desiredWidth, desiredHeight)

    let destRect = new Rectangle(0, 0, width, height)
    let destImage = new Bitmap(width, height)

    destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution)

    use graphics = Graphics.FromImage destImage
    graphics.CompositingMode <- CompositingMode.SourceCopy
    graphics.CompositingQuality <- CompositingQuality.HighQuality
    graphics.InterpolationMode <- InterpolationMode.HighQualityBicubic
    graphics.SmoothingMode <- SmoothingMode.HighQuality
    graphics.PixelOffsetMode <- PixelOffsetMode.HighQuality

    use wrapMode = new ImageAttributes()
    wrapMode.SetWrapMode(WrapMode.TileFlipXY)
    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode)

    let fileName = sprintf "%s_%dx%d%s" (Path.GetFileNameWithoutExtension sourcePath) width height (Path.GetExtension sourcePath)
    let targetPath = Path.Combine(Path.GetDirectoryName sourcePath, fileName)
    destImage.Save targetPath
    fileName, (width, height)

let resize container name size =
    let sourcePath = Path.Combine("assets", "images", container, name)
    resizeImpl sourcePath size
    |> fst
    |> (sprintf "assets/images/%s/%s" container)

let htmlImage container name size =
    let sourcePath = Path.Combine("assets", "images", container, name)
    resizeImpl sourcePath size
    |> fun (fileName, (width, height)) ->
        Img [ Src (sprintf "assets/images/%s/%s" container fileName); Width (string width ); Height (string height) ]
