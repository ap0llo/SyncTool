@echo off

for /f "usebackq tokens=*" %%i in (`"%programfiles(x86)%\Microsoft Visual Studio\Installer\vswhere" -latest -products * -requires Microsoft.Component.MSBuild -property installationPath`) do (
  set InstallDir=%%i
)

set MSBuildPath="%InstallDir%\MSBuild\15.0\Bin\MSBuild.exe"

%MSBuildPath% /clp:verbosity=minimal %*

