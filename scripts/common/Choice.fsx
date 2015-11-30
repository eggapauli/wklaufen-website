let bind fn =
    function
    | Choice1Of2 x -> fn x
    | Choice2Of2 x -> Choice2Of2 x

let bindError fn =
    function
    | Choice1Of2 x -> Choice1Of2 x
    | Choice2Of2 x -> fn x

let map fn =
    bind (fn >> Choice1Of2)

let mapError fn =
    bindError (fn >> Choice2Of2)

let ofOption error =
    function
    | Some x -> Choice1Of2 x
    | None -> Choice2Of2 error

let partitionList list =
    list |> List.choose (function | Choice1Of2 x -> Some x | Choice2Of2 _ -> None)
    , list |> List.choose (function | Choice1Of2 _ -> None | Choice2Of2 x -> Some x)

let apply fn x =
    match fn, x with
    | Choice1Of2 fn, Choice1Of2 x -> fn x |> Choice1Of2
    | Choice2Of2 fn, _ -> Choice2Of2 fn
    | _, Choice2Of2 x -> Choice2Of2 x

let rec ofList list =
    let (<*>) = apply
    let cons head tail = head :: tail
    match list with
    | [] -> Choice1Of2 []
    | head :: tail ->
        (Choice1Of2 cons) <*> head <*> (ofList tail)
