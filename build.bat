set ooebvUsername=
set ooebvPassword=
set facebookAccessToken=
set activitiesPath=

.\packages\FAKE\tools\FAKE.exe .\scripts\build\run.fsx ^
-ev ooebv-username %ooebvUsername% ^
-ev ooebv-password %ooebvPassword% ^
-ev facebook-access-token %facebookAccessToken%

REM pushd .\scripts\build

REM fsi.exe import-activities.fsx ^
REM --source %activitiesPath%

REM popd
