#if INTERACTIVE
#load "Async.fsx"
#endif

#if COMPILED
module Choice
#endif

let unit x = Choice1Of2 x

let bind fn =
    function
    | Choice1Of2 x -> fn x
    | Choice2Of2 x -> Choice2Of2 x

let bindError fn =
    function
    | Choice1Of2 x -> Choice1Of2 x
    | Choice2Of2 x -> fn x

let map fn =
    bind (fn >> unit)

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
    | Choice1Of2 fn, Choice1Of2 x -> fn x |> unit
    | Choice2Of2 fn, _ -> Choice2Of2 fn
    | _, Choice2Of2 x -> Choice2Of2 x

let rec ofList list =
    let (<*>) = apply
    let cons head tail = head :: tail
    match list with
    | [] -> unit []
    | head :: tail ->
        (unit cons) <*> head <*> (ofList tail)

let bindAsync fn =
    function
    | Choice1Of2 x -> async {
        let! y = fn x
        return y
        }
    | Choice2Of2 x -> async { return Choice2Of2 x }

let mapAsync fn x =
    bindAsync (fn >> Async.map unit) x

let toOption =
    function
    | Choice1Of2 x -> Some x
    | _ -> None
