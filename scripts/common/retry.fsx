open System

let rec executeChoice (timeouts: TimeSpan list) fn =
    match timeouts with
    | [] -> Choice2Of2 ()
    | timeout :: remainingTimeouts ->
        match fn() with
        | Choice1Of2 x -> Choice1Of2 x
        | Choice2Of2 x ->
            eprintfn "An error occured: %O" x
            eprintfn "Retrying in %O" timeout
            Async.Sleep (int timeout.TotalMilliseconds) |> Async.RunSynchronously
            executeChoice remainingTimeouts fn

let rec executeExn (timeouts: TimeSpan list) fn =
    let wrapped () =
        try
            fn() |> Choice1Of2
        with x -> Choice2Of2 x
    executeChoice timeouts wrapped