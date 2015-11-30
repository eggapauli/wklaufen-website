open System

let fromJsonOption =
    function
    | Some x -> x
    | None -> "null"

let fromOption =
    function
    | Some x -> sprintf "\"%O\"" x
    | None -> "null"

let fromAssetId =
    sprintf """{
        "de-AT": {
            "sys": {
                "type": "Link",
                "linkType": "Asset",
                "id": "%s"
            }
        }
    }"""

let fromAssetIdOption assetId =
    assetId
    |> Option.map fromAssetId
    |> fromJsonOption

let fromAssetIds =
    function
    | [] -> None
    | assetIds ->
        assetIds
        |> Seq.map fromAssetId
        |> String.concat ","
        |> sprintf "[%s]"
        |> Some
    >> fromJsonOption

let fromArray items =
    items
    |> Seq.map (sprintf "\"%O\"")
    |> String.concat ", "
    |> sprintf "[%s]"

let fromArrayOption items =
    items
    |> Option.map fromArray
    |> fromJsonOption

let fromDate (date: DateTime) =
    date.ToString "yyyy-MM-dd"
        
let fromDateOption (date: DateTime option) =
    date
    |> Option.map fromDate
    |> fromOption

let fromDateTime (date: DateTime) =
    date.ToString "yyyy-MM-dd HH:mm:ss"

let fromDateTimeOption (date: DateTime option) =
    date
    |> Option.map fromDateTime
    |> fromOption

let fromRichText (text: string) =
    text.Replace(@"\", @"\\")
        .Replace("\"", "\\\"")
        .Replace("\r\n", @"\r\n")
        .Replace("\r", @"\r\n")
        .Replace("\n", @"\r\n")
