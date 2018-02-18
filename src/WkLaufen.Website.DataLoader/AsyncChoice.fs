module AsyncChoice

let fromAsync v = Async.map Choice.success v
let fromChoice (v: Choice<'a, 'b>) = Async.unit v

let bind fn =
    Async.bind (
        function
        | Choice1Of2 v -> fn v
        | Choice2Of2 p -> Async.unit (Choice.error p)
    )

let bindAsync fn = bind (fn >> Async.map Choice.success)

let bindChoice fn =
    Async.map (Choice.bind fn)

let mapError fn =
    Async.map (Choice.mapError fn)

let success v = Async.unit (Choice.success v)

let map fn v = Async.map (Choice.map fn) v