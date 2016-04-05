#if INTERACTIVE
#I @"..\..\"
#r @"packages\ImageProcessor\lib\net45\ImageProcessor.dll"
#endif

open System.IO
open ImageProcessor

let memberImageDir = @"C:\Users\Johannes\Workspace\wklaufen\design\mitglieder"
let maxImageSize = System.Drawing.Size(1500, 1500)

Directory.GetFiles(memberImageDir)
|> Seq.iter (fun f ->
    let newPath = Path.Combine(Path.GetDirectoryName f, "resized", Path.GetFileName f)
    newPath |> Path.GetDirectoryName |> Directory.CreateDirectory |> ignore
    use imageFactory = new ImageFactory()
    imageFactory
        .Load(f)
        .Resize(Imaging.ResizeLayer(maxImageSize, Imaging.ResizeMode.Min, Upscale=false))
        .Save(newPath)
    |> ignore
)