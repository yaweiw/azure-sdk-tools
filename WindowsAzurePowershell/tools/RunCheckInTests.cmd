call %~dp0\SetupEnv.cmd

::Get WebPI CMD
SET WebPi=%~dp0\WebpiCmd.exe
for /F "tokens=1,2*" %%i in ('reg query "HKLM\SOFTWARE\Microsoft\WebPlatformInstaller" /s') DO (
    if "%%i"=="InstallPath" (
        SET WebPi="%%k\WebpiCmd.exe"
    )
)
echo webpi: %webpi%

if not exist "%ProgramFiles%\Microsoft SDKs\Windows Azure\.NET SDK\v2.3" (
    echo installing Azure Authoring Tools
    %WebPi% /Install /products:WindowsAzureSDK_Only_2_3 /accepteula
)

if not exist "%ProgramFiles%\Microsoft SDKs\Windows Azure\Emulator" (
    echo installing Azure Compute Emulator
    %WebPi% /Install /products:WindowsAzureEmulator_Only_2_3 /accepteula
)

if not exist "%ADXSDKProgramFiles%\Microsoft SDKs\Windows Azure\Storage Emulator" (
    echo installing Azure Storage Emulator
    %WebPi% /Install /products:WindowsAzureStorageEmulator /accepteula
)

git.exe > NUL 2>&1
if %ERRORLEVEL% GEQ 1 (
    if exist "%ADXSDKProgramFiles%\Git\bin" (
        echo Adding Git installation folder to the PATH environment variable, needed for 2 unit tests
        set "path=%path%;%ADXSDKProgramFiles%\Git\bin"
    )
)

::The detecting logic for django is not decent, but the best we can do so far.
if not exist "%SystemDrive%\Python27" (
    echo Install Python27, PIP, and Django 1.5
    msiexec /i %~dp0\test\python-2.7.msi /passive
    %SystemDrive%\Python27\python.exe "%~dp0\test\get-pip.py"
    %SystemDrive%\Python27\scripts\pip.exe install Django==1.5
)

msbuild.exe  %AzurePSRoot%\..\build.proj /t:test
