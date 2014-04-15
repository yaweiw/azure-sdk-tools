call %~dp0\SetupEnv.cmd

msbuild %AzurePSRoot%\..\build.proj /t:builddebug