Param(
    [string]$Branch = "main",
    [string]$ProjectPath = "C:\Users\Administrator\Desktop\Projects\Kidzgo",
    [string]$PublishPath = "C:\apps\kidzgo-api",
    [string]$ServiceName = "KidzgoAPI",
    [string]$ApiBindUrl = "http://0.0.0.0:5000",
    [string]$PublicBaseUrl = "https://rexengswagger.duckdns.org"
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

Assert-AdminSession

Write-Host "`nStep 1/5: Go to project directory" -ForegroundColor Cyan
Set-Location $ProjectPath

Write-Host "`nStep 2/5: Pull latest code from branch '$Branch'" -ForegroundColor Cyan
git fetch origin
git checkout $Branch
git pull origin $Branch

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

Write-Host "Replacing old files with new ones..." -ForegroundColor Yellow
if (Test-Path $PublishPath) {
    Remove-Item -Path $PublishPath -Recurse -Force -ErrorAction SilentlyContinue
}
Move-Item -Path $TempPublishPath -Destination $PublishPath -Force

Write-Host "`nStep 4/5: Configure environment for dual access" -ForegroundColor Cyan
Set-MachineEnvironmentVariable -Name "ASPNETCORE_URLS" -Value $ApiBindUrl
Set-MachineEnvironmentVariable -Name "ASPNETCORE_FORWARDEDHEADERS_ENABLED" -Value "true"

Write-Host "`nStep 5/5: Start Windows service '$ServiceName'" -ForegroundColor Cyan
Start-ServiceSafe -Name $ServiceName

Write-Host "`nDeploy completed successfully." -ForegroundColor Green
Write-Host "Kidzgo.API is bound to $ApiBindUrl." -ForegroundColor Green
Write-Host "HTTPS public traffic is available through $PublicBaseUrl" -ForegroundColor Green
Write-Host "Legacy IP access on port 5000 can remain available if the VPS firewall still allows it." -ForegroundColor Green
