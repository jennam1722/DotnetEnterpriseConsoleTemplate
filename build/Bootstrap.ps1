Param(
	[Parameter()]
	[ValidateSet('quiet','minimal','normal','detailed','diagnostic')]
	[string]$Verbosity = 'quiet',
	$DotnetPath = "dotnet",
	[Uri]$NugetSource = "https://api.nuget.org/v3/index.json",
	[bool]$DotnetErrorsOnly = $True
)

$currDir = Get-Location
Import-Module $PSScriptRoot/Bootstrap-Module.psd1 -Force

Write-Verbose "Bootstrap Verbosity Set: $Verbosity"
$buildDir = Get-BuildWorkingDirectory -Path '.'
Write-Verbose "Bootstrap Working Directory: $buildDir"
Set-Location -Path $buildDir.FullName

Write-Host "Bootstrap: Preparing Folders.."
$prepareFolders = Invoke-PrepareProject -Path $buildDir
Invoke-ProcessEcho $DotnetErrorsOnly $prepareFolders

Write-Host "Bootstrap: Dotnet Clean.."
$clean = Invoke-DotnetClean -DotnetPath $DotnetPath -Verbosity $Verbosity
Invoke-ProcessEcho $DotnetErrorsOnly $clean

Write-Host "Bootstrap: Dotnet Restore.."
$restore = Invoke-DotnetRestore -Path $buildDir -DotnetPath $DotnetPath -Verbosity $Verbosity  -NugetSource $NugetSource
Invoke-ProcessEcho $DotnetErrorsOnly $restore

Write-Host "Bootstrap: Dotnet Test.."
$test = Invoke-DotnetTest -DotnetPath $DotnetPath -Verbosity $Verbosity -Path $(Join-Path -Path '.' -ChildPath 'tests')
Invoke-ProcessEcho $DotnetErrorsOnly $test

Write-Host "Bootstrap: Generate Code Coverage.."
$coverage = Invoke-ReportGeneratorCoverage -DotnetPath $DotnetPath -NetVersion "net7.0" -Verbosity $Verbosity -Path $(Join-Path -Path '.' -ChildPath 'tests')
Invoke-ProcessEcho $DotnetErrorsOnly $coverage

Write-Host "Bootstrap: Dotnet Publish.."
$publish = Invoke-DotnetPublish -DotnetPath $DotnetPath -ProjectFile "src/EnterpriseTemplate/EnterpriseTemplate.csproj" -Verbosity $Verbosity
Invoke-ProcessEcho $DotnetErrorsOnly $publish

Write-Host "Bootstrap: Compress Artifact.."
$compress = Invoke-CompressArtifact -ArchiveName EnterpriseTemplate.zip -Verbosity $Verbosity
Invoke-ProcessEcho $DotnetErrorsOnly $compress

Invoke-BootstrapSummary -ExecutionItems @($prepareFolders, $clean, $restore, $test, $coverage, $publish, $compress)


Remove-Module Bootstrap-Module
Set-Location -Path $currDir.Path