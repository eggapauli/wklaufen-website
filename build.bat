set ooebvUsername=
set ooebvPassword=
set facebookAccessToken=
set uploadUrl=
set uploadUsername=
set uploadPassword=
set tempUrl=
set phpExePath=php.exe
set activitiesPath=

.\packages\FAKE\tools\FAKE.exe .\Scripts\Build\run.fsx %* ^
-ev ooebv-username %ooebvUsername% ^
-ev ooebv-password %ooebvPassword% ^
-ev facebook-access-token %facebookAccessToken% ^
-ev upload-url %uploadUrl% ^
-ev temp-url %tempUrl% ^
-ev upload-username %uploadUsername% ^
-ev upload-password %uploadPassword% ^
-ev php-exe-path %phpExePath%

REM pushd .\scripts\build

REM fsi.exe import-activities.fsx ^
REM --source %activitiesPath%

REM popd
