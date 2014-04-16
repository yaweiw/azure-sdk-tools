call %~dp0\SetupEnv.cmd

::Teh command needs elevation for dependencies installation and running tests
::Here we invoke an elevation-needing command to test it
net session > NUL 2>&1
IF ERRORLEVEL 1 (
    ECHO ERROR: Please launch command under administrator account. It is needed for environment setting up and unit test.
    EXIT /B 1
)

if not exist "%ProgramFiles%\Microsoft SDKs\Windows Azure\.NET SDK\v2.3" (
    ECHO installing Azure Authoring Tools
    %~dp0\test\WindowsAzureAuthoringTools-%ADXSDKPlatform%.msi /passive
)

if not exist "%ProgramFiles%\Microsoft SDKs\Windows Azure\Emulator" (
    ECHO installing Azure Compute Emulator
    %~dp0\test\WindowsAzureEmulator-%ADXSDKPlatform%.exe /passive
)

if not exist "%ADXSDKProgramFiles%\Microsoft SDKs\Windows Azure\Storage Emulator" (
    ECHO installing Azure Storage Emulator
    %~dp0\test\WindowsAzureStorageEmulator.msi /passive
)

if exist "%ADXSDKProgramFiles%\Git\bin" (
    ECHO Adding Git installation folder to the PATH environment variable, needed for 2 unit tests
    set "path=%path%;%ADXSDKProgramFiles%\Git\bin"
)

if not exist "%SystemDrive%\Python27" (
    ECHO, install Python27, PIP, and Django 1.5
    msiexec /i %~dp0\test\python-2.7.msi /passive
    %SystemDrive%\Python27\python.exe "%~dp0\test\get-pip.py"
    %SystemDrive%\Python27\scripts\pip.exe install Django==1.5
)

msbuild.exe  %AzurePSRoot%\..\build.proj /t:test
