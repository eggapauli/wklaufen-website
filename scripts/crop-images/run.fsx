open System.Drawing
open System.IO

let sourceDir = Path.Combine(Path.GetDirectoryName __SOURCE_DIRECTORY__, @"design\Musikerfotos  zu den einzelnen Registern")
let targetDir = Path.Combine(sourceDir, "cropped")
Directory.CreateDirectory targetDir |> ignore

let getMusicianPhotoArea (bmp: Bitmap) =
    let zipIndexes = [0..3]
    let getBlackBorderIndexes (pixels: Color list) =
        pixels
        |> List.mapi (fun idx _ ->
            zipIndexes
            |> List.map ((+) idx)
            |> List.map (fun index ->
                pixels
                |> List.tryItem index
            )
            |> function
            | l when l |> List.forall (fun item -> item.IsSome) -> Some (idx, l |> List.map (fun item -> item.Value))
            | _ -> None
        )
        |> List.choose id
        |> List.filter (fun (startIndex, pixels) ->
            pixels
            |> List.forall (fun p -> p.GetBrightness() < 0.1f)
        )
        |> List.map fst

    let vPixels =
        let startX = 200
        List.init bmp.Height (fun i -> bmp.GetPixel(startX, i))
        |> getBlackBorderIndexes
        |> fun l ->
            match List.tryHead l, List.tryLast l with
            | Some topIndex, Some bottomIndex -> Some (topIndex + zipIndexes.Length, bottomIndex)
            | _ -> None

    let hPixels startY =
        List.init bmp.Width (fun i -> bmp.GetPixel(i, startY))
        |> getBlackBorderIndexes
        |> fun l ->
            match List.tryHead l, List.tryLast l with
            | Some leftIndex, Some rightIndex -> Some (leftIndex + zipIndexes.Length, rightIndex)
            | _ -> None

    vPixels
    |> Option.bind(fun y ->
        hPixels ((snd y) - 5)
        |> Option.map (fun x -> x, y)
    )
    |> Option.map (fun ((x1, x2), (y1, y2)) -> Rectangle(Point(x1, y1), Size(x2 - x1, y2 - y1)))

Directory.GetFiles(sourceDir, "*.jpg")
|> Seq.iter (fun f ->
    use bmpImage = Image.FromFile f :?> Bitmap
    match getMusicianPhotoArea bmpImage with
    | Some cropArea ->
        printfn "%s %O" (Path.GetFileName f) cropArea
        use croppedImage = bmpImage.Clone(cropArea, bmpImage.PixelFormat)
        let targetFileName = Path.Combine(targetDir, Path.GetFileName f)
        croppedImage.Save targetFileName
    | None -> eprintfn "Couldn't find crop area of %s" f
)