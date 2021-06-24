
[<AutoOpen>]
module Operators

open System.IO

#if !FABLE_COMPILER
let (@@) a b = Path.Combine(a, b)
#endif

let flip fn a b = fn b a
