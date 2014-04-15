call %~dp0\SetupEnv.cmd

msbuild.exe  %AzurePSRoot%\..\build.proj /t:test
