@echo off
SET SOLUTIONPATH=./src/SyncTool.sln
CALL ./msbuild %SOLUTIONPATH% /t:Restore /p:Configuration=Release
CALL ./msbuild %SOLUTIONPATH% /t:Build /p:Configuration=Release /p:RunCreateSetup=true %*

