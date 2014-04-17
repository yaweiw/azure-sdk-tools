@echo off

if defined AzurePSRoot exit /b 0

echo Initializing environment...

set teamInternalCommand="\\vwdbuild01\dev\AdxSdk\SetPowerShellEnvVars.cmd"

if defined PRIVATE_SETTING_CMD (
    call "%PRIVATE_SETTING_CMD%"
) else (
    if exist "%teamInternalCommand%" (
        call "%teamInternalCommand%"
    )
)

if not defined PRIVATE_FEED_URL (
    echo Error, please set following environment variables so that build script can download Spec Nuget Packages:
    echo     PRIVATE_FEED_URL, PRIVATE_FEED_USER_NAME and PRIVATE_FEED_PASSWORD
	exit /b 1
)

::PowerShell commands need elevation for dependencies installation and running tests
::Here we invoke an elevation-needing command to test it
net session > NUL 2>&1
if ERRORLEVEL 1 (
    ECHO ERROR: Please launch command under administrator account. It is needed for environment setting up and unit test.
    EXIT /B 1
)

set "AzurePSRoot=%~dp0"
::get rid of the \tools\
set "AzurePSRoot=%AzurePSRoot:~0,-7%" 

if defined ProgramFiles(x86) (
    set "ADXSDKProgramFiles=%ProgramFiles(x86)%"
    set "ADXSDKPlatform=x64"
    set "wow64RegKey=\Wow6432Node"
) else (
    set "ADXSDKProgramFiles=%ProgramFiles%"
    set "ADXSDKPlatform=x86"
)
:: Update policy, so azure powershell can run
reg add "HKLM\SOFTWARE%wow64RegKey%\Microsoft\PowerShell\1\ShellIds\Microsoft.PowerShell" /f /v "ExecutionPolicy" /t REG_SZ /d "Unrestricted"

if exist "%ADXSDKProgramFiles%\Microsoft Visual Studio 12.0" (
    set ADXSDKVSVersion=12.0
) else (
    set ADXSDKVSVersion=11.0
)

call "%ADXSDKProgramFiles%\Microsoft Visual Studio %ADXSDKVSVersion%\VC\vcvarsall.bat" x86