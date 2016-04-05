// delete:
//POST /core/inc_cms/hole_ajax_div_up.php?thumbnail_aktiv=1&cmyk_aktiv=&big_aktiv=&bereich=mitglieder_bild&bereich_tabelle=mitglieder_bild_archiv&bereich_verzeichniss=mitglieder_bild&a_id=ua&anzeige_form=&b_session=a_1567829406767d318ca0599791a1bef5d37ade56&rand=0.7974673410457369&del_rec=3930&id=594&sprache=deu HTTP/1.1

#if INTERACTIVE
#I "..\Common"
#load "OOEBV.fsx"
#endif

open System.IO

let credentials = ("<username>", "<password>")

let memberImageDir = @"C:\Users\Johannes\Workspace\wklaufen\design\mitglieder\resized"
let memberImages =
    Directory.GetFiles memberImageDir
    |> Seq.map (fun f -> Path.GetFileNameWithoutExtension f |> int, f)
    |> Map.ofSeq

OOEBV.login credentials
|> Async.bind (Choice.bindAsync OOEBV.loadAndResetMemberOverviewPage)
|> Async.bind (Choice.bindAsync (OOEBV.replaceMemberImages memberImages))
|> Async.RunSynchronously
|> function
| Choice1Of2 () -> printfn "Replaced all images"
| Choice2Of2 error -> printfn "ERROR: %s" error
