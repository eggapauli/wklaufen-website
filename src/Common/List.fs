module List

let intersperse sep list =
    List.foldBack (fun x -> function
        | [] -> [x]
        | xs -> x::sep::xs) list []