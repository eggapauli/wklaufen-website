module Facebook

open System
open System.Net.Http
open FSharp.Data
open DataModels

let private baseUrl = Uri "https://graph.facebook.com/v2.5/"

[<Literal>]
let private JsonBaseDir = __SOURCE_DIRECTORY__ + @"\facebook\"

[<Literal>]
let private SamplePostListResponse = JsonBaseDir + @"FacebookPostList.json"
type private PostList = JsonProvider<SamplePostListResponse>

[<Literal>]
let private SamplePostSingleAttachmentResponse = JsonBaseDir + @"FacebookPostSingleAttachment.json"
type private PostSingleAttachment = JsonProvider<SamplePostSingleAttachmentResponse>

[<Literal>]
let private SamplePostMultipleAttachmentsResponse = JsonBaseDir + @"FacebookPostMultipleAttachments.json"
type private PostMultipleAttachments = JsonProvider<SamplePostMultipleAttachmentsResponse>

[<Literal>]
let private SampleAttachmentImagesResponse = JsonBaseDir + @"FacebookAttachmentImages.json"
type private PostAttachmentImages = JsonProvider<SampleAttachmentImagesResponse>

[<Literal>]
let private SamplePageCoverResponse = JsonBaseDir + @"FacebookPageCover.json"
type private PageCover = JsonProvider<SamplePageCoverResponse>

let private httpGet accessToken (relUrl: string) = async {
    let url = Uri(baseUrl, relUrl)
    use request = new HttpRequestMessage(System.Net.Http.HttpMethod.Get, url)
    request.Headers.Authorization <- Headers.AuthenticationHeaderValue("Bearer", accessToken)
    return!
        Http.sendRequest request
        |> Async.bind (Choice.mapAsync Http.getContentString)
}

let getNews accessToken =
    httpGet accessToken "werkskapellelaufen/posts"
    |> Async.map (Choice.map PostList.Parse)
    |> Async.bind (Choice.bindAsync (fun x ->
        x.Data
        |> Seq.choose (fun post ->
            match post.Story with
            | Some story when story.EndsWith "updated their profile picture." || story.EndsWith "updated their cover photo." -> None
            | _ -> post.Message
            |> Option.map (fun (postMessage) ->
                httpGet accessToken (post.Id + "/attachments")
                |> Async.bind (Choice.mapAsync (fun attachments ->
                    try
                        PostMultipleAttachments.Parse(attachments).Data
                        |> Seq.collect (fun d -> d.Subattachments.Data)
                        |> Seq.choose (fun d -> d.Target.Id)
                        |> Seq.toList
                        |> Async.unit
                    with e ->
                        PostSingleAttachment.Parse(attachments).Data
                        |> Seq.map (fun d ->
                            match d.Type, d.Target.Id with
                            | "photo", targetId -> async { return targetId }
                            | "cover_photo", Some targetId ->
                                httpGet accessToken (sprintf "%d?fields=cover" d.Target.Id.Value)
                                |> Async.map (
                                    Choice.map PageCover.Parse
                                    >> Choice.map (fun cover -> cover.Cover.CoverId)
                                    >> Choice.toOption
                                )
                            | x, _ ->
                                printfn "Unknown attachment type in post %s: \"%s\"" post.Id x
                                async { return None }
                        )
                        |> Async.ofList
                        |> Async.map (List.choose id)
                ))
                |> Async.bind (Choice.bindAsync(fun photoIds ->
                    photoIds
                    |> List.map (fun photoId ->
                        httpGet accessToken (sprintf "%d?fields=images" photoId)
                    )
                    |> Async.ofList
                    |> Async.map Choice.ofList
                ))
                |> Async.map (Choice.map(fun imagesResponse ->
                    imagesResponse
                    |> Seq.map PostAttachmentImages.Parse
                    |> Seq.map (fun attachmentImages ->
                        attachmentImages.Images
                        |> Seq.maxBy (fun image -> image.Width)
                        |> fun image -> image.Source
                        |> Uri
                    )
                    |> Seq.toList
                    |> fun images ->
                        {
                            Id = post.Id
                            News =
                                {
                                    Timestamp = post.CreatedTime
                                    Content = postMessage
                                }
                            Images = images
                        }
                ))
            )
        )
        |> Seq.toList
        |> Async.ofList
        |> Async.map Choice.ofList
    ))
    |> Async.map (Choice.mapError (sprintf "Error while retrieving Facebook posts. %s"))
