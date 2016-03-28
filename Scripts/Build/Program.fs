module Program

open Fake

[<EntryPoint>]
let main argv =
    setBuildParam "ooebv-username" ""
    setBuildParam "ooebv-password" ""
    setBuildParam "facebook-access-token" ""
    setBuildParam "upload-url" ""
    setBuildParam "upload-username" ""
    setBuildParam "upload-password" ""

    RunTargetOrDefault "Default"
    0