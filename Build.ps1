
$nugetVersion = "v3.4.4"         # version of the nuget client to download. To use the latest version, set version to "latest"
$nugetExeDir = "./build/tools"         # the (relative) path of the directory to store nuget.exe
$msbuildVersion = "14.0"
$solutionPath = "./src/SyncTool.sln"
$platform = "Any CPU"
$configuration = "Release"
$msbuildLogPath = "./build/build.log"

function Get-NugetPath()
{
    #TODO: Use nuget from path if it is there

    $url = "https://dist.nuget.org/win-x86-commandline/$($nugetVersion)/NuGet.exe"
    $nugetExePath = Join-Path $nugetExeDir "nuget.exe"

    # make sure the target directory exists
    if((Test-Path $nugetExeDir) -eq $false)
    {
        New-Item -ItemType Directory $nugetExeDir 1> $null
    }

    # is no nuget.exe is found in the directory, download it from nuget.org
    if((Test-Path $nugetExePath) -eq $false)
    {
        Invoke-WebRequest $url -OutFile $nugetExePath
    }

    return $nugetExePath 
}

function Get-MsBuildPath()
{
    $command = Get-Command "msbuild" -ErrorAction SilentlyContinue

    if($command -ne $null)
    {
        return $command.Source
    }

    $registry = Get-ItemProperty "HKLM:\Software\Microsoft\MSBuild\ToolsVersion\$($msbuildVersion)" -Name MSBuildToolsPath
    return Join-Path $registry.MSBuildToolsPath "msbuild.exe"    
}


function Get-MsbuildLogPath()
{
    $logDir = Split-Path $msbuildLogPath
    if((Test-Path $logDir) -eq $false)
    {
        New-Item -Itemtype Directory $logDir 1> $null
    }
    return $msbuildLogPath
}


# restore
Write-Host "Restoring Nuget Packages"
Invoke-Expression ("& `"$(Get-NugetPath)`" restore `"$($solutionPath)`"")
if($LASTEXITCODE -ne 0)
{
    Write-Error "Package restore failed"
    exit 1
}


#build
Write-Host "Building $($solutionPath)"

$msBuildExpression =`
 "& `"$(Get-MsBuildPath)`"" + `
 " `"/m`"" + ` 
 " `"$($solutionPath)`" " + ` 
 " `"/p:Platform=$($platform)`" " + ` 
 " `"/p:Configuration=$($configuration)`""+ ` 
 " `"/l:FileLogger,Microsoft.Build.Engine;logfile=$(Get-MsbuildLogPath)`"" + `
 " `"/consoleloggerparameters:verbosity=minimal`" "
 
Invoke-Expression $msBuildExpression
if($LASTEXITCODE -ne 0)
{
    Write-Error "Build failed"
    exit 1
}

