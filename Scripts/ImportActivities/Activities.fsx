#if INTERACTIVE
#I @"..\..\"
#I @"..\Common\"
#r @"packages\ExcelDataReader\lib\net45\Excel.dll"
#r @"packages\SharpZipLib\lib\20\ICSharpCode.SharpZipLib.dll"

#load "Json.fsx"
#load "DownloadHelper.fsx"
#endif

#if COMPILED
module Activities
#endif

open System
open System.IO
open System.Text.RegularExpressions
open Excel
open DownloadHelper

module private Seq =
    let zipn seqs =
        let enumerators =
            seqs
            |> Seq.map (fun (seq: 'a seq) -> seq.GetEnumerator())
            |> Seq.toList
        Seq.initInfinite (fun i -> enumerators)
            |> Seq.takeWhile (fun x -> x |> List.map (fun e -> e.MoveNext()) |> List.contains false |> not)
            |> Seq.map (List.map (fun e -> e.Current))

type Activity = {
    Title: string
    BeginTime: DateTime option
    EndTime: DateTime option
    Place: string
}

let parse filePath =
    let parseDate (date: obj) =
        match date with
        | :? DateTime as d -> Some d.Date
        | _ -> None

    let parseTimespan (time: obj) =
        let parse x =
            TimeSpan.ParseExact(x, [| @"hh\:mm"; @"hh\.mm" |], null)

        match time with
        | :? DateTime as d -> Some d.TimeOfDay, None
        | :? string as d ->
            if String.IsNullOrWhiteSpace d || d.Equals("ganzt\u00e4gig", StringComparison.InvariantCultureIgnoreCase)
            then Some TimeSpan.Zero, None
            else
                let matches = Regex.Match(d, @"^\s*(?<begin>\d{2}(:|\.)\d{2})\s*(-\s*(?<end>\d{2}(:|\.)\d{2})\s*)?$")
                match matches.Groups.["begin"], matches.Groups.["end"] with
                | b, e when b.Success && e.Success ->
                    Some (parse b.Value)
                    , Some (parse e.Value)
                | b, e when b.Success && not e.Success ->
                    Some (parse b.Value)
                    , None
                | _ -> failwithf "Unknown time span \"%s\"" d
        | _ -> None, None

    let getTimeSpan (date: DateTime option) (beginTime, endTime) =
        match date, beginTime, endTime with
        | Some date, Some beginTime, Some endTime -> Some (date + beginTime), Some (date + endTime)
        | Some date, Some beginTime, None -> Some (date + beginTime), None
        | Some date, None, None -> Some date, None
        | _ -> None, None

    let stream = File.Open(filePath, FileMode.Open, FileAccess.Read)
    use excelReader = ExcelReaderFactory.CreateOpenXmlReader stream
    let result = excelReader.AsDataSet()
    result.Tables.[0].Rows
    |> Seq.cast<Data.DataRow>
    |> Seq.take 4
    |> Seq.map (fun row -> row.ItemArray |> Seq.skip 4)
    |> Seq.zipn
    |> Seq.map (fun fields ->
        let date = parseDate fields.[1]
        let time = parseTimespan fields.[2]
        let (beginTime, endTime) = getTimeSpan date time
        {
            Title = sprintf "%O" fields.[0]
            BeginTime = beginTime
            EndTime = endTime
            Place = sprintf "%O" fields.[3]
        }
    )
    |> Seq.filter (fun x -> String.IsNullOrWhiteSpace x.Title |> not)
    |> Seq.toList

let getJson (m: Activity) =
    sprintf """{
        "Title": "%s",
        "BeginTime": %s,
        "EndTime": %s,
        "Location": "%s",
        "IsPublic": false
    }"""
        m.Title
        (Json.fromDateTimeOption m.BeginTime)
        (Json.fromDateTimeOption m.EndTime)
        m.Place

let import sourcePath targetPath =
    parse sourcePath
    |> List.map getJson
    |> saveEntries targetPath
