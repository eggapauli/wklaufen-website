#if COMPILED
module Async
#endif

let bind fn x = async {
    let! y = x
    return! fn y
}

let unit x = async { return x }

let map fn x = bind (fn >> unit) x

let ofList x = x |> Async.Parallel |> (map Array.toList)

let perform fn x = map (fun x -> fn x; x) x
