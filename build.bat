set ooebvUsername=
set ooebvPassword=
set facebookAccessToken=
set uploadUrl=
set uploadUsername=
set uploadPassword=
set activitiesPath=

.\packages\FAKE\tools\FAKE.exe .\Scripts\Build\run.fsx %* ^
-ev ooebv-username %ooebvUsername% ^
-ev ooebv-password %ooebvPassword% ^
-ev facebook-access-token %facebookAccessToken% ^
-ev upload-url %uploadUrl% ^
-ev upload-username %uploadUsername% ^
-ev upload-password %uploadPassword%

REM pushd .\scripts\build

REM fsi.exe import-activities.fsx ^
REM --source %activitiesPath%

REM popd
