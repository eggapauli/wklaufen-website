module ImageResize

open System
open System.IO
open SixLabors.ImageSharp
open SixLabors.ImageSharp.Processing

type ResizeOptions = {
    Width: int option
    Height: int option
    Crop: bool
}

type CodeGeneration =
    | IncludeSizeInName
    | Indexed of string
    | IndexedList of string

let resizeImages dataDir sourceDir deployBaseDir deployDir =
    let resize (sourcePath: string) resizeOptions (targetPath: string) =
        let computeSize desiredSize (imageWidth, imageHeight) =
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

        let computeSizeNoCrop (desiredWidthOpt, desiredHeightOpt) (imageWidth, imageHeight) =
            let desiredWidth = desiredWidthOpt |> Option.defaultValue imageWidth
            let desiredHeight = desiredHeightOpt |> Option.defaultValue imageHeight

            let widthRatio = (float imageWidth) / (float desiredWidth)
            let heightRatio = (float imageHeight) / (float desiredHeight)
            let ratio = Math.Max(Math.Max(widthRatio, heightRatio), 1.)
            let width = Math.Round((float imageWidth) / ratio) |> int
            let height = Math.Round((float imageHeight) / ratio) |> int
            width, height

        use image = Image.Load sourcePath

        let width, height =
            let actualSize = (image.Width, image.Height)
            let desiredSize = (resizeOptions.Width, resizeOptions.Height)
            if resizeOptions.Crop
            then computeSize desiredSize actualSize
            else computeSizeNoCrop desiredSize actualSize

        printfn "Resizing %s to Width = %d, Height = %d" sourcePath width height

        image.Mutate(fun x ->
            let width', height' =
                let widthRatio = (float image.Width) / (float width)
                let heightRatio = (float image.Height) / (float height)
                if widthRatio > heightRatio
                then 0, height
                else width, 0
            x.Resize(width', height') |> ignore

            let cropRect =
                let srcX = (image.Width - width) / 2
                let srcY = (image.Height - height) / 2
                SixLabors.Primitives.Rectangle(srcX, srcY, width, height)
            x.Crop cropRect |> ignore
        )

        Path.GetDirectoryName targetPath |> Directory.CreateDirectory |> ignore
        image.Save targetPath |> ignore

    let defaultResizeOptions = {
        Width = None
        Height = None
        Crop = true
    }

    let fileHasExtension extensions file =
        let fileExt = Path.GetExtension file
        extensions
        |> Seq.exists (fun ext -> String.equalsIgnoreCase ext fileExt)

    let getImages dir =
        Directory.EnumerateFiles(sourceDir @@ dir)
        |> Seq.filter (fileHasExtension [ ".png"; ".jpg"; ".jpeg" ])
        |> Seq.toList

    let images =
        [
            yield getImages "menu-items", { defaultResizeOptions with Width = Some 150; Height = Some 100 }, IncludeSizeInName
            yield getImages "member-groups", { defaultResizeOptions with Width = Some 200; Height = Some 130 }, Indexed "memberGroups_w200h130"
            yield getImages "members", { defaultResizeOptions with Width = Some 200; Height = Some 270 }, Indexed "members_w200h270"
            yield
                [ "87181.jpg"; "31180.jpg"; "31145.jpg"; "39627.jpg" ]
                |> List.map (fun f -> "members" @@ f),
                { defaultResizeOptions with Width = Some 110; Height = Some 160 }, Indexed "contacts"
            yield getImages "pages", { defaultResizeOptions with Width = Some 1000; Height = Some 600 }, IncludeSizeInName
            yield getImages "news", { defaultResizeOptions with Width = Some 940; Height = Some 480; Crop = false }, IndexedList "news"
            yield [ "schlosskonzert.jpg" ], { defaultResizeOptions with Width = Some 400 }, IncludeSizeInName
        ]
        |> List.map (fun (paths, resizeOptions, codeGeneration) ->
            let size =
                match resizeOptions.Width, resizeOptions.Height with
                | Some width, Some height -> sprintf "_w%dh%d" width height
                | Some width, None -> sprintf "_w%d" width
                | None, Some height -> sprintf "_h%d" height
                | None, None -> ""

            paths
            |> List.map (fun path ->
                let fileName = sprintf "%s%s%s" (Path.GetFileNameWithoutExtension path) size (Path.GetExtension path)
                let targetPath = deployBaseDir @@ deployDir @@ (Path.GetDirectoryName path |> Path.GetFileName) @@ fileName
                let sourcePath = sourceDir @@ path
                sourcePath, targetPath
            ),
            resizeOptions,
            codeGeneration
        )

    images
    |> List.iter (fun (paths, resizeOptions, _) ->
        paths
        |> List.iter (fun (sourcePath, targetPath) ->
            resize sourcePath resizeOptions targetPath
        )
    )

    images
    |> List.collect (fun (paths, _, codeGeneration) ->
        let getRelativeTargetPath (path: string) =
            path.Substring(deployBaseDir.Length).TrimStart('\\').Replace("\\", "/")

        match codeGeneration with
        | IncludeSizeInName ->
            paths
            |> List.map (fun (_sourcePath, targetPath) ->
                let name =
                    Path.GetFileNameWithoutExtension targetPath
                    |> fun n -> n.Replace("-", "_")
                let value = getRelativeTargetPath targetPath
                sprintf "let %s = \"%s\"" name value
            )
        | Indexed name ->
            [
                yield sprintf "let %s =" name
                yield "  ["
                yield!
                    paths
                    |> List.map (fun (sourcePath, targetPath) ->
                        sprintf "    \"%s\", \"%s\""
                            (Path.GetFileNameWithoutExtension sourcePath)
                            (getRelativeTargetPath targetPath)
                    )
                yield "  ]"
                yield "  |> Map.ofList"
            ]
        | IndexedList name ->
            [
                yield sprintf "let %s =" name
                yield "  ["
                yield!
                    paths
                    |> List.groupBy (fst >> Path.GetFileNameWithoutExtension >> fun sourcePath ->
                        let markerPosition = sourcePath.LastIndexOf "_"
                        sourcePath.Substring(0, markerPosition)
                    )
                    |> List.collect (fun (group, paths) ->
                        [
                            yield sprintf "    \"%s\", [" group
                            yield!
                                paths
                                |> List.map (snd >> getRelativeTargetPath >> sprintf "      \"%s\"")
                            yield "    ]"
                        ]
                    )
                yield "  ]"
                yield "  |> Map.ofList"
            ]
    )
    |> List.append ["module Data.Images"; ""]
    |> fun l -> File.WriteAllLines(dataDir @@ "Images.generated.fs", l)
