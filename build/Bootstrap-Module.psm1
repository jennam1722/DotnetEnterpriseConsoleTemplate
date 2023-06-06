function Get-BuildWorkingDirectory {
    param(
        $Path
    )

    $file = Get-ChildItem -Path $Path -Include *.sln -Depth 0
    if (!$file) {
        $file = Get-ChildItem -Path .. -Include *.sln -Depth 0
    }
    
    if ($file) {
        return $file.Directory
    }

    throw "Unable to find .sln file"    
}

function Invoke-PrepareProject {
    param($Path)
    $process = [BootstrapExecution]::new()
    $process.StepName = "CleanUpFolders"
    $process.StartTime = Get-Date
    $folders = @("packages/", "projectTests/" , "CodeCoverage/", "ArtifactsRaw/", "Artifacts/")
    $cmd = ""
    foreach ($folder in $folders) {
        $cmd += "Remove-Item $folder -Recurse -Force`r`n"
    }
    $process.FullCommand = $cmd.TrimEnd("`r`n")
    foreach ($folder in $folders) {
        $pth = Join-Path -Path $Path -ChildPath $folder
        Write-Verbose "Removing Folder: $folder"
        if ((Test-Path $pth)) {
            Remove-Item $pth -Recurse -Force -ErrorVariable $process.ErrorOutput
        }
    }
    $process.EndTime = Get-Date
    $process.Success = $LastExitCode -eq 0
    return $process
}

function Invoke-DotnetClean {
    param(
        [string]$DotnetPath = "dotnet",
        [string]$ProjectFile,
        [ValidateSet('quiet', 'minimal', 'normal', 'detailed', 'diagnostic')]
        [string]$Verbosity
    )
    $process = [BootstrapExecution]::new()
    $process.StepName = "Dotnet Clean"
    $process.StartTime = Get-Date
    if (!$ProjectFile) {
        $sln = Get-ChildItem -Path $Path -Include *.sln -Recurse
        $process.FullCommand = "$DotnetPath clean ""$($sln.FullName)"" --verbosity $Verbosity --nologo"
        & $DotnetPath clean ""$($sln.FullName)"" --verbosity $Verbosity 2> processError.txt > process.txt 
    }
    else {
        $execPath = " ""$ProjectFile"""
        $process.FullCommand = "$DotnetPath clean$execPath --verbosity $Verbosity --nologo"
        & $DotnetPath clean$execPath --verbosity $Verbosity 2> processError.txt > process.txt 
    }
   
    ThrowOnNativeFailure -ExecutionInformation $process
    $process.Success = $true
    $process.EndTime = Get-Date
    return $process
}


function Invoke-DotnetRestore {
    param(
        $Path,
        $DotnetPath = "dotnet",
        [Parameter(Mandatory = $true)]
        [ValidateSet('quiet', 'minimal', 'normal', 'detailed', 'diagnostic')]
        [string]$Verbosity,
        [Uri]$NugetSource = "https://api.nuget.org/v3/index.json",
        [string]$Folder = "packages"
    )
    $process = [BootstrapExecution]::new()
    $process.StepName = "Dotnet Restore"
    $process.StartTime = Get-Date
    $sln = Get-ChildItem -Path $Path -Include *.sln -Recurse
    $packagesFolder = Join-Path -Path $Path -ChildPath $Folder
    $packagesFolder = $packagesFolder.TrimEnd('/').TrimEnd('\')
    $process.FullCommand = $DotnetPath + " restore ""$($sln.FullName)"" --verbosity $Verbosity --source $($NugetSource.AbsoluteUri) --packages $packagesFolder --force"
    & $DotnetPath restore ""$($sln.FullName)"" --verbosity $Verbosity --source $($NugetSource.AbsoluteUri) --packages $packagesFolder --force 2> processError.txt > process.txt    
    ThrowOnNativeFailure -ExecutionInformation $process
    $process.Success = $true
    $process.EndTime = Get-Date
    return $process
}

function Invoke-DotnetTest {
    param(
        $Path,
        $DotnetPath = "dotnet",
        [Parameter(Mandatory = $true)]
        [ValidateSet('quiet', 'minimal', 'normal', 'detailed', 'diagnostic')]
        [string]$Verbosity,
        [string]$Folder = "packages"
    )
    $process = [BootstrapExecution]::new()
    $process.StepName = "Dotnet Test"
    $process.StartTime = Get-Date
    $TestProjects = Get-ChildItem -Path $Path -Include *.csproj -Recurse
    foreach ($TestProject in $TestProjects) {
        $cmd += "$DotnetPath test ""$TestProject""  --logger ""trx;v=d"" -c Debug --results-directory projectTests --collect:""XPlat Code Coverage"" -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.ExcludeByAttribute=""CodeCoverageIgnore"" --verbosity $Verbosity"
    }
    $process.FullCommand = $cmd
    foreach ($TestProject in $TestProjects) {
        & $DotnetPath test ""$TestProject"" --logger """trx;v=d""" -c Debug --results-directory projectTests --collect:"""XPlat Code Coverage""" -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.ExcludeByAttribute=""CodeCoverageIgnore"" --verbosity $Verbosity 2> processError.txt > process.txt  
        ThrowOnNativeFailure -ExecutionInformation $process
    }   
    $process.Success = $true
    $process.EndTime = Get-Date
    return $process
}

function Invoke-ReportGeneratorCoverage {
    param(
        $Path,
        $DotnetPath = "dotnet",
        [string]$NetVersion = "net7.0",
        [Parameter(Mandatory = $true)]
        [ValidateSet('quiet', 'minimal', 'normal', 'detailed', 'diagnostic')]
        [string]$Verbosity,
        [string]$Folder = "packages"
    )
    $process = [BootstrapExecution]::new()
    $process.StepName = "Generate Coverage"
    $process.StartTime = Get-Date
    $rgdll = Find-ReportGeneratorDll -NetVersion $NetVersion
    $TestResults = Get-ChildItem -Path projectTests -Include coverage.opencover.xml -Recurse
    foreach ($TestResult in $TestResults) {
        $filePth = $TestResult.FullName
        $cmd += "$rgdll ""-reports:$filePth"" ""-targetdir:CodeCoverage"" ""-reporttypes:HtmlInline;Badges;MarkdownSummaryGithub"" ""-verbosity=$Verbosity"""
    }
    $process.FullCommand = $cmd
    foreach ($TestResult in $TestResults) {
        $filePth = $TestResult.FullName
        & $DotnetPath $rgdll """-reports:$filePth""" """-targetdir:CodeCoverage""" """-reporttypes:HtmlInline;Badges;MarkdownSummaryGithub""" """-verbosity=$Verbosity""" --verbosity $Verbosity 2> processError.txt > process.txt  
        ThrowOnNativeFailure -ExecutionInformation $process
    }   
    $process.Success = $true
    $process.EndTime = Get-Date
    return $process
}

function Invoke-DotnetBuild {
    param(
        $Path,
        $DotnetPath = "dotnet",
        [Parameter(Mandatory = $true)]
        [ValidateSet('quiet', 'minimal', 'normal', 'detailed', 'diagnostic')]
        [string]$Verbosity,
        [string]$Folder = "ArtifactsRaw",
        [Uri]$NugetSource = "https://api.nuget.org/v3/index.json"
    )
    $process = [BootstrapExecution]::new()
    $process.StepName = "Dotnet Build"
    $process.StartTime = Get-Date
    $Projects = Get-ChildItem -Path $Path -Include *.csproj -Recurse
    foreach ($Project in $Projects) {
        $cmd += "$DotnetPath build ""$Project"" --output ArtifactsRaw -c Release --no-incremental --source $NugetSource --verbosity $Verbosity"
    }
    $process.FullCommand = $cmd
    foreach ($Project in $Projects) {
        & $DotnetPath build """$Project""" --output ArtifactsRaw -c Release --no-incremental --source """$NugetSource""" --verbosity $Verbosity 2> processError.txt > process.txt  
        ThrowOnNativeFailure -ExecutionInformation $process
    }   
    $process.Success = $true
    $process.EndTime = Get-Date
    return $process
}

function Invoke-DotnetPublish {
    param(
        $DotnetPath = "dotnet",
        [string]$ProjectFile,
        [Parameter(Mandatory = $true)]
        [ValidateSet('quiet', 'minimal', 'normal', 'detailed', 'diagnostic')]
        [string]$Verbosity,
        [string]$Folder = "ArtifactsRaw",
        [string]$Framework = "net7.0",
        [Uri]$NugetSource = "https://api.nuget.org/v3/index.json"
    )
    $process = [BootstrapExecution]::new()
    $process.StepName = "Dotnet Publish"
    $process.StartTime = Get-Date
    $process.FullCommand = "$DotnetPath publish $($ProjectFile) --output ArtifactsRaw -c Release --self-contained --framework $Framework --source $NugetSource --verbosity $Verbosity"
    & $DotnetPath publish $($ProjectFile)  --output ArtifactsRaw -c Release --self-contained --framework $Framework --source """$NugetSource""" --verbosity $Verbosity 2> processError.txt > process.txt  
    ThrowOnNativeFailure -ExecutionInformation $process
    $process.Success = $true
    $process.EndTime = Get-Date
    return $process
}

function Invoke-CompressArtifact {
    param(
        [string]$Source = "ArtifactsRaw",
        [string]$Destination = "Artifacts",
        [string]$ArchiveName = "Application.zip"
    )

    if (!(Test-Path $Destination)) {
        New-Item -Path $Destination -ItemType Directory > $null
    }

    $process = [BootstrapExecution]::new()
    $process.StepName = "Compress Artifact"
    $process.StartTime = Get-Date
    $process.FullCommand = """Compress-Archive -Path $Source/* -DestinationPath $Destination/$ArchiveName"""
    Compress-Archive -Path $Source/* -DestinationPath $Destination/$ArchiveName -Force 2> processError.txt > process.txt 
    ThrowOnNativeFailure -ExecutionInformation $process
    $process.Success = $true
    $process.EndTime = Get-Date
    return $process
}


function Invoke-BootstrapSummary {
    param(
        $ExecutionItems
    )

    [BootstrapTable[]] $castedItems = @()
    $ExecutionItems | ForEach-Object { $castedItems += ConvertToBootstrapTable($_) } 
    $castedItems | Format-Table
}


function Find-ReportGeneratorDll {
    param(
        [string]$NetVersion = "net7.0"
    )

    $DllPath = Get-ChildItem -Path "packages\reportgenerator\**\tools" -Include ReportGenerator.dll -Recurse
    return $DllPath[0].FullName
}

function Invoke-ProcessEcho {
    param (
        [bool]$DotnetErrorsOnly,
        [BootstrapExecution]$ExecutionInformation
    )
    if (!$DotnetErrorsOnly -and ![string]::IsNullOrEmpty($ExecutionInformation.ExecutionOutput)) {
        Write-Host $ExecutionInformation.ExecutionOutput
    }
    if (!$ExecutionInformation.Success) {
        Write-Host $ExecutionInformation.ErrorOutput
    }
}

function RemoveLogs {
    param(
        [BootstrapExecution]$ExecutionInformation
    )

    if (Test-Path processError.txt) {
        $process.ErrorOutput = Get-Content processError.txt -Raw
        Remove-Item processError.txt  2>&1 > $null
    }

    if (Test-Path process.txt) {
        $process.ExecutionOutput = Get-Content process.txt -Raw    
        Remove-Item process.txt  2>&1 > $null
    }
    
}

function ThrowOnNativeFailure {
    param(
        [BootstrapExecution]$ExecutionInformation
    )
    
    if (-not $?) {        
        RemoveLogs -ExecutionInformation  $ExecutionInformation
        Write-Host -Message $("Error in step: " + $ExecutionInformation.StepName) -ForegroundColor Red
        Write-Host -Message "Error Executing command:" -ForegroundColor Red
        Write-Host -Message $ExecutionInformation.FullCommand-ForegroundColor Red
        Write-Host -Message "Error Output:" -ForegroundColor Red
        if ($ExecutionInformation.ExecutionOutput) {
            Write-Host -Message $ExecutionInformation.ExecutionOutput  -ForegroundColor Red
        }
        if ($ExecutionInformation.ErrorOutput) {
            Write-Host -Message $ExecutionInformation.ErrorOutput   -ForegroundColor Red
        }
        throw "Bootstrap Failure"
    }
    else {
        RemoveLogs -ExecutionInformation  $ExecutionInformation
    }
}

function ConvertToBootstrapTable {
    param(
        [BootstrapExecution]$ExecutionInformation
    )  

    $success = switch ($ExecutionInformation.Success) {
        $true { "Success" }
        $false { "Failure" }
    }
    $process = [BootstrapTable]::new()
    $process.StepName = $ExecutionInformation.StepName
    $process.Status = $success
    $process.Duration = New-TimeSpan -Start $ExecutionInformation.StartTime -End $ExecutionInformation.EndTime
    return $process
}


class BootstrapTable {
    [string] $StepName;
    [string] $Status;
    [TIMESPAN]$Duration;
}

class BootstrapExecution {
    [datetime] $StartTime;
    [datetime] $EndTime;
    [string] $StepName;
    [string] $FullCommand;
    [string] $ExecutionOutput;
    [string] $ErrorOutput;
    [bool] $Success;
}