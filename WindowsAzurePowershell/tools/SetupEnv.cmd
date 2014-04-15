@echo off

if defined AzurePSRoot exit /b 0

echo Initializing environment...

::PowerShell environment needs elevation for dependencies installation and running tests
::Here we invoke an elevation-needing command to test it
net session > NUL 2>&1
IF ERRORLEVEL 1 (
    ECHO ERROR: Please launch command under administrator account. It is needed for environment setting up and unit test.
    EXIT /B 1
)

if exist "%USERPROFILE%\SetNugetFeed.cmd" call "%USERPROFILE%\SetNugetFeed.cmd"

if not defined PRIVATE_FEED_URL (
    echo Error, please set following environment variables so that build script can download Spec Nuget Packages:
    echo     PRIVATE_FEED_URL, PRIVATE_FEED_USER_NAME and PRIVATE_FEED_PASSWORD
	exit /b 1
)

set "AzurePSRoot=%~dp0"
::get rid of the \tools\
set "AzurePSRoot=%AzurePSRoot:~0,-7%" 

if defined ProgramFiles(x86) (
    set "ADXSDKProgramFiles=%ProgramFiles(x86)%"
    set "ADXSDKPlatform=x64"
) else (
    set "ADXSDKProgramFiles=%ProgramFiles%"
    set "ADXSDKPlatform=x86"
)

if exist "%ADXSDKProgramFiles%\Microsoft Visual Studio 12.0" (
    set ADXSDKVSVersion=12.0
) else (
    set ADXSDKVSVersion=11.0
)

call "%ADXSDKProgramFiles%\Microsoft Visual Studio %ADXSDKVSVersion%\VC\vcvarsall.bat" x86

if not exist "%ProgramFiles%\Microsoft SDKs\Windows Azure\.NET SDK\v2.3" (
    ECHO installing Azure Authoring Tools
    %~dp0\emulators\WindowsAzureAuthoringTools-%ADXSDKPlatform%.msi /passive
)

if not exist "%ProgramFiles%\Microsoft SDKs\Windows Azure\Emulator" (
    ECHO installing Azure Compute Emulator
    %~dp0\emulators\WindowsAzureEmulator-%ADXSDKPlatform%.exe /passive
)

if not exist "%ADXSDKProgramFiles%\Microsoft SDKs\Windows Azure\Storage Emulator" (
    ECHO installing Azure Storage Emulator
    %~dp0\emulators\WindowsAzureStorageEmulator.msi /passive
)

if exist "%ADXSDKProgramFiles%\Git\bin" (
    ECHO Adding Git installation folder to the PATH environment variable(Needed for 2 unit tests)
    set "path=%path%;%ADXSDKProgramFiles%\Git\bin"
)
