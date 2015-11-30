#I @"..\..\"
#r "System.Net.Http"
#r @"packages\FSharp.Data\lib\net40\FSharp.Data.dll"

//#load @"..\common\DataModels.fsx"
//#load @"..\common\Choice.fsx"
//#load @"..\common\Http.fsx"

open System
open System.Net.Http
open FSharp.Data
open DataModels

let private baseUrl = Uri "https://graph.facebook.com/v2.5/"

[<Literal>]
let private SamplePostListResponse = __SOURCE_DIRECTORY__ + @"\FacebookPostList.json"
type private PostList = JsonProvider<SamplePostListResponse>

[<Literal>]
let private SamplePostSingleAttachmentResponse = __SOURCE_DIRECTORY__ + @"\FacebookPostSingleAttachment.json"
type private PostSingleAttachment = JsonProvider<SamplePostSingleAttachmentResponse>

[<Literal>]
let private SamplePostMultipleAttachmentsResponse = __SOURCE_DIRECTORY__ + @"\FacebookPostMultipleAttachments.json"
type private PostMultipleAttachments = JsonProvider<SamplePostMultipleAttachmentsResponse>

[<Literal>]
let private SampleAttachmentImagesResponse = __SOURCE_DIRECTORY__ + @"\FacebookAttachmentImages.json"
type private PostAttachmentImages = JsonProvider<SampleAttachmentImagesResponse>

[<Literal>]
let private SamplePageCoverResponse = __SOURCE_DIRECTORY__ + @"\FacebookPageCover.json"
type private PageCover = JsonProvider<SamplePageCoverResponse>

let private httpGet accessToken (relUrl: string) =
    let url = Uri(baseUrl, relUrl)
    use request = new HttpRequestMessage(System.Net.Http.HttpMethod.Get, url)
    request.Headers.Authorization <- Headers.AuthenticationHeaderValue("Bearer", accessToken)
    Http.sendRequest request
    |> Choice.map Http.getContentString

let getNews accessToken =
    httpGet accessToken "werkskapellelaufen/posts"
    |> Choice.map PostList.Parse
    |> Choice.bind (fun x ->
        x.Data
        |> Seq.choose (fun post ->
            post.Message
            |> Option.map (fun (postMessage) ->
                httpGet accessToken (post.Id + "/attachments")
                |> Choice.map (fun attachments ->
                    try
                        PostMultipleAttachments.Parse(attachments).Data
                        |> Seq.collect (fun d -> d.Subattachments.Data)
                        |> Seq.choose (fun d -> d.Target.Id)
                        |> Seq.toList
                    with e ->
                        PostSingleAttachment.Parse(attachments).Data
                        |> Seq.choose (fun d ->
                            match d.Type, d.Target.Id with
                            | "photo", targetId -> targetId
                            | "cover_photo", Some targetId ->
                                httpGet accessToken (sprintf "%d?fields=cover" d.Target.Id.Value)
                                |> Choice.map PageCover.Parse
                                |> Choice.map (fun cover -> cover.Cover.CoverId)
                                |> function
                                | Choice1Of2 x -> Some x
                                | Choice2Of2 x -> None
                            | x, _ ->
                                printfn "Unknown attachment type in post %s: \"%s\"" post.Id x
                                None
                        )
                        |> Seq.toList
                )
                |> Choice.bind(fun photoIds ->
                    photoIds
                    |> List.map (fun photoId ->
                        httpGet accessToken (sprintf "%d?fields=images" photoId)
                    )
                    |> Choice.ofList
                )
                |> Choice.map(fun imagesResponse ->
                    imagesResponse
                    |> Seq.map PostAttachmentImages.Parse
                    |> Seq.map (fun attachmentImages ->
                        attachmentImages.Images
                        |> Seq.maxBy (fun image -> image.Width)
                        |> fun image -> image.Source
                        |> Uri
                    )
                    |> Seq.toList
                )
                |> Choice.map (fun images ->
                    {
                        Id = post.Id
                        News =
                            {
                                Timestamp = post.CreatedTime
                                Content = postMessage
                            }
                        Images = images
                    }
                )
            )
        )
        |> Seq.toList
        |> Choice.ofList
    )
    |> Choice.mapError (sprintf "Error while retrieving Facebook posts. %s")
