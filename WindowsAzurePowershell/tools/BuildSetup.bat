call %~dp0\SetupEnv.cmd
msbuild %AzurePSRoot%\..\build.proj /t:buildsetupdebug
if %ERRORLEVEL% EQU 0 (
    ECHO MSI file path: %AzurePSRoot%\setup\build\Debug\x86
)