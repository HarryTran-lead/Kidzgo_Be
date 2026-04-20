Param(
    [string]$Branch = "main",
    [string]$ProjectPath = "C:\Users\Administrator\Desktop\Projects\Kidzgo",
    [string]$PublishPath = "C:\apps\kidzgo-api",
    [string]$ServiceName = "KidzgoAPI",
    [string]$ApiBindUrl = "http://127.0.0.1:5000",
    [string]$PublicBaseUrl = "https://rexengswagger.duckdns.org",
    [string]$EnvironmentName = "Production"
)

Write-Host "==== Kidzgo deploy script (Windows) ====" -ForegroundColor Cyan

function Assert-AdminSession {
    $identity = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = New-Object Security.Principal.WindowsPrincipal($identity)

    if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
        throw "Run this script in an elevated PowerShell session."
    }
}

function Stop-ServiceSafe {
    param([string]$Name)

    try {
        $svc = Get-Service -Name $Name -ErrorAction Stop
        if ($svc.Status -eq "Running") {
            Write-Host "Stopping service $Name..." -ForegroundColor Yellow
            Stop-Service -Name $Name -Force -ErrorAction Stop
            $svc.WaitForStatus("Stopped", "00:00:20")
        }
    }
    catch {
        Write-Host "Service $Name not found or cannot be stopped (may be first deploy)." -ForegroundColor DarkYellow
    }
}

function Start-ServiceSafe {
    param([string]$Name)

    try {
        Write-Host "Starting service $Name..." -ForegroundColor Yellow
        Start-Service -Name $Name -ErrorAction Stop
        $svc = Get-Service -Name $Name
        $svc.WaitForStatus("Running", "00:00:20")
        Write-Host "Service $Name is running." -ForegroundColor Green
    }
    catch {
        Write-Host "ERROR: Could not start service $Name" -ForegroundColor Red
        throw
    }
}

function Set-MachineEnvironmentVariable {
    param(
        [string]$Name,
        [string]$Value
    )

    $currentValue = [Environment]::GetEnvironmentVariable($Name, "Machine")

    if ($currentValue -eq $Value) {
        Write-Host "$Name is already set to $Value" -ForegroundColor DarkGreen
        return
    }

    Write-Host "Setting machine environment variable $Name=$Value" -ForegroundColor Yellow
    [Environment]::SetEnvironmentVariable($Name, $Value, "Machine")
}

function Get-EnvironmentLocalConfigFileName {
    param([string]$Name)

    return "appsettings.$Name.local.json"
}

function ConvertTo-NormalizedObject {
    param([object]$Value)

    if ($null -eq $Value) {
        return $null
    }

    if ($Value -is [System.Collections.IDictionary]) {
        $result = @{}
        foreach ($key in $Value.Keys) {
            $result[$key] = ConvertTo-NormalizedObject -Value $Value[$key]
        }

        return $result
    }

    if ($Value -is [System.Management.Automation.PSCustomObject]) {
        $result = @{}
        foreach ($property in $Value.PSObject.Properties) {
            $result[$property.Name] = ConvertTo-NormalizedObject -Value $property.Value
        }

        return $result
    }

    if ($Value -is [System.Collections.IEnumerable] -and -not ($Value -is [string])) {
        $items = @()
        foreach ($item in $Value) {
            $items += ,(ConvertTo-NormalizedObject -Value $item)
        }

        return $items
    }

    return $Value
}

function Load-JsonConfig {
    param([string]$Path)

    if (-not (Test-Path $Path)) {
        return @{}
    }

    $content = Get-Content -Path $Path -Raw -ErrorAction SilentlyContinue
    if ([string]::IsNullOrWhiteSpace($content)) {
        return @{}
    }

    $parsed = $content | ConvertFrom-Json -Depth 50
    return ConvertTo-NormalizedObject -Value $parsed
}

function Merge-Config {
    param(
        [hashtable]$Base,
        [hashtable]$Overrides
    )

    foreach ($key in $Overrides.Keys) {
        $overrideValue = $Overrides[$key]

        if ($Base.ContainsKey($key) -and
            $Base[$key] -is [System.Collections.IDictionary] -and
            $overrideValue -is [System.Collections.IDictionary]) {
            $Base[$key] = Merge-Config -Base ([hashtable]$Base[$key]) -Overrides ([hashtable]$overrideValue)
            continue
        }

        $Base[$key] = $overrideValue
    }

    return $Base
}

function Save-JsonConfig {
    param(
        [string]$Path,
        [hashtable]$Config
    )

    $directory = Split-Path -Parent $Path
    if (-not [string]::IsNullOrWhiteSpace($directory)) {
        New-Item -ItemType Directory -Path $directory -Force | Out-Null
    }

    $json = $Config | ConvertTo-Json -Depth 50
    Set-Content -Path $Path -Value $json -Encoding ASCII
}

function Copy-IfExists {
    param(
        [string]$SourcePath,
        [string]$DestinationPath
    )

    if (Test-Path $SourcePath) {
        Copy-Item -Path $SourcePath -Destination $DestinationPath -Force
    }
}

function Sync-InstanceConfigFiles {
    param(
        [string]$CurrentPublishPath,
        [string]$TempPublishPath,
        [string]$TargetEnvironmentName,
        [string]$TargetApiBindUrl,
        [string]$TargetPublicBaseUrl,
        [string]$TargetServiceName
    )

    $sharedLocalConfig = "appsettings.Local.json"
    $environmentLocalConfig = Get-EnvironmentLocalConfigFileName -Name $TargetEnvironmentName

    $currentSharedLocalPath = Join-Path $CurrentPublishPath $sharedLocalConfig
    $tempSharedLocalPath = Join-Path $TempPublishPath $sharedLocalConfig
    Copy-IfExists -SourcePath $currentSharedLocalPath -DestinationPath $tempSharedLocalPath

    $currentEnvironmentLocalPath = Join-Path $CurrentPublishPath $environmentLocalConfig
    $existingEnvironmentConfig = Load-JsonConfig -Path $currentEnvironmentLocalPath

    $overrides = @{
        Kestrel = @{
            Endpoints = @{
                Http = @{
                    Url = $TargetApiBindUrl
                }
            }
        }
        ClientSettings = @{
            ApiUrl = $TargetPublicBaseUrl
        }
        Serilog = @{
            EventLog = @{
                Source = $TargetServiceName
            }
        }
    }

    $mergedEnvironmentConfig = Merge-Config -Base $existingEnvironmentConfig -Overrides $overrides
    $tempEnvironmentLocalPath = Join-Path $TempPublishPath $environmentLocalConfig
    Save-JsonConfig -Path $tempEnvironmentLocalPath -Config $mergedEnvironmentConfig
}

Assert-AdminSession

Write-Host "`nStep 1/5: Go to project directory" -ForegroundColor Cyan
Set-Location $ProjectPath

Write-Host "`nStep 2/5: Pull latest code from branch '$Branch'" -ForegroundColor Cyan
git fetch origin
git checkout $Branch
git pull

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: git pull failed, aborting deploy." -ForegroundColor Red
    exit 1
}

Write-Host "`nStep 3/5: Stop service and publish Kidzgo.API (Release) to $PublishPath" -ForegroundColor Cyan
Stop-ServiceSafe -Name $ServiceName

Write-Host "Waiting 2 seconds for file locks to be released..." -ForegroundColor Yellow
Start-Sleep -Seconds 2

$TempPublishPath = "$PublishPath-temp"
if (Test-Path $TempPublishPath) {
    Remove-Item -Path $TempPublishPath -Recurse -Force -ErrorAction SilentlyContinue
}

if (-not (Test-Path $TempPublishPath)) {
    New-Item -ItemType Directory -Path $TempPublishPath | Out-Null
}

Write-Host "Publishing to temporary directory: $TempPublishPath" -ForegroundColor Yellow
dotnet publish ".\Kidzgo.API\Kidzgo.API.csproj" -c Release -o $TempPublishPath

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: dotnet publish failed, aborting deploy." -ForegroundColor Red
    Start-ServiceSafe -Name $ServiceName
    exit 1
}

Write-Host "`nStep 4/5: Preserve local config and apply instance overrides" -ForegroundColor Cyan
Sync-InstanceConfigFiles `
    -CurrentPublishPath $PublishPath `
    -TempPublishPath $TempPublishPath `
    -TargetEnvironmentName $EnvironmentName `
    -TargetApiBindUrl $ApiBindUrl `
    -TargetPublicBaseUrl $PublicBaseUrl `
    -TargetServiceName $ServiceName

Write-Host "Replacing old files with new ones..." -ForegroundColor Yellow
if (Test-Path $PublishPath) {
    Remove-Item -Path $PublishPath -Recurse -Force -ErrorAction SilentlyContinue
}
Move-Item -Path $TempPublishPath -Destination $PublishPath -Force

Write-Host "`nStep 5/5: Start Windows service '$ServiceName'" -ForegroundColor Cyan
Set-MachineEnvironmentVariable -Name "ASPNETCORE_FORWARDEDHEADERS_ENABLED" -Value "true"
Start-ServiceSafe -Name $ServiceName

Write-Host "`nDeploy completed successfully." -ForegroundColor Green
Write-Host "Kidzgo.API instance '$ServiceName' is configured for $ApiBindUrl." -ForegroundColor Green
Write-Host "HTTPS public traffic is available through $PublicBaseUrl" -ForegroundColor Green
Write-Host "Local instance overrides are stored in $PublishPath\$(Get-EnvironmentLocalConfigFileName -Name $EnvironmentName)" -ForegroundColor Green
