module Choice

let success x = Choice1Of2 x
let error x = Choice2Of2 x

let bind fn =
    function
    | Choice1Of2 x -> fn x
    | Choice2Of2 x -> error x

let bindError fn =
    function
    | Choice1Of2 x -> success x
    | Choice2Of2 x -> fn x

let map fn =
    bind (fn >> success)

let mapError fn =
    bindError (fn >> error)

let ofOption e =
    function
    | Some x -> success x
    | None -> error e

let partitionList list =
    list |> List.choose (function | Choice1Of2 x -> Some x | Choice2Of2 _ -> None)
    , list |> List.choose (function | Choice1Of2 _ -> None | Choice2Of2 x -> Some x)

let apply fn x =
    match fn, x with
    | Choice1Of2 fn, Choice1Of2 x -> fn x |> success
    | Choice2Of2 fn, _ -> error fn
    | _, Choice2Of2 x -> error x

let rec ofList list =
    let (<*>) = apply
    let cons head tail = head :: tail
    match list with
    | [] -> success []
    | head :: tail ->
        (success cons) <*> head <*> (ofList tail)

let bindAsync fn =
    function
    | Choice1Of2 x -> async {
        let! y = fn x
        return y
        }
    | Choice2Of2 x -> async { return error x }

let mapAsync fn x =
    bindAsync (fn >> Async.map success) x

let toOption =
    function
    | Choice1Of2 x -> Some x
    | _ -> None
