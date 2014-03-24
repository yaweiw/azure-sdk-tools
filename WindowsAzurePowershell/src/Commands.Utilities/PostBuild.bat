@echo off

set outdir=%1
set source=%outdir%Resources\Scaffolding
set destination=%outdir%Scaffolding

:: This block helps getting the scaffolding folder outside resources folder which is important for the unit tests.
if exist %destination% (
    echo Removing stale scaffolding %destination%
    rmdir /S /Q %destination%
)

echo Moving scaffolding to %destination%
move /Y %source% %destination%