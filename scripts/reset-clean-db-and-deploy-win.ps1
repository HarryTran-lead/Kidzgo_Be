param(
    [string]$Branch = "main",
    [string]$ProjectPath = "C:\Users\Administrator\Desktop\Projects\Kidzgo",
    [string]$PublishPath = "C:\apps\kidzgo-api",
    [string]$ServiceName = "KidzgoAPI",
    [string]$ApiBindUrl = "http://0.0.0.0:5000",
    [string]$PublicBaseUrl = "https://rexengswagger.duckdns.org",
    [string]$EnvironmentName = "Production",
    [ValidateSet("Schema", "Truncate")]
    [string]$ResetMode = "Schema",
    [string]$SeedSqlPath = "",
    [string]$BackupDirectory = "C:\backups\kidzgo",
    [string]$DeployScriptPath = "",
    [string]$PsqlPath = "",
    [string]$PgDumpPath = "",
    [switch]$Force,
    [switch]$InspectOnly,
    [switch]$SkipBackup,
    [int]$MigrationWaitTimeoutSeconds = 180
)

$ErrorActionPreference = "Stop"

function Resolve-RepoRoot {
    $repoRoot = Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..")
    return $repoRoot.Path
}

function Resolve-ExistingPath {
    param(
        [string]$Path,
        [string]$BasePath
    )

    if ([string]::IsNullOrWhiteSpace($Path)) {
        return $null
    }

    if ([System.IO.Path]::IsPathRooted($Path)) {
        return (Resolve-Path -LiteralPath $Path).Path
    }

    $candidate = Join-Path $BasePath $Path
    if (Test-Path -LiteralPath $candidate) {
        return (Resolve-Path -LiteralPath $candidate).Path
    }

    return $candidate
}

function Resolve-ToolPath {
    param(
        [string]$ToolName,
        [string]$ExplicitPath
    )

    if (-not [string]::IsNullOrWhiteSpace($ExplicitPath)) {
        return (Resolve-Path -LiteralPath $ExplicitPath).Path
    }

    $command = Get-Command -Name $ToolName -CommandType Application -ErrorAction SilentlyContinue
    if ($null -ne $command) {
        return $command.Source
    }

    $pgRoot = "C:\Program Files\PostgreSQL"
    if (Test-Path -LiteralPath $pgRoot) {
        $match = Get-ChildItem -Path $pgRoot -Recurse -File -Filter $ToolName -ErrorAction SilentlyContinue |
            Sort-Object FullName |
            Select-Object -First 1

        if ($null -ne $match) {
            return $match.FullName
        }
    }

    throw "Cannot find $ToolName. Install PostgreSQL client tools or pass -${ToolName}Path."
}

function Read-ConnectionString {
    param(
        [string]$PublishPath,
        [string]$EnvironmentName
    )

    $configPath = Join-Path $PublishPath ("appsettings.{0}.local.json" -f $EnvironmentName)
    if (Test-Path -LiteralPath $configPath) {
        $json = Get-Content -LiteralPath $configPath -Raw | ConvertFrom-Json
        $connectionString = $json.ConnectionStrings.Database
        if (-not [string]::IsNullOrWhiteSpace($connectionString)) {
            return [pscustomobject]@{
                Source = $configPath
                Value = $connectionString
            }
        }
    }

    $envValue = [Environment]::GetEnvironmentVariable("ConnectionStrings__Database", "Machine")
    if ([string]::IsNullOrWhiteSpace($envValue)) {
        $envValue = [Environment]::GetEnvironmentVariable("ConnectionStrings__Database", "Process")
    }

    if (-not [string]::IsNullOrWhiteSpace($envValue)) {
        return [pscustomobject]@{
            Source = "ConnectionStrings__Database environment variable"
            Value = $envValue
        }
    }

    throw "Could not resolve ConnectionStrings:Database from $configPath or environment variables."
}

function Parse-ConnectionString {
    param([string]$ConnectionString)

    $map = @{}
    foreach ($segment in $ConnectionString -split ';') {
        if ([string]::IsNullOrWhiteSpace($segment)) {
            continue
        }

        $parts = $segment -split '=', 2
        if ($parts.Count -ne 2) {
            continue
        }

        $key = $parts[0].Trim().ToLowerInvariant()
        $value = $parts[1].Trim()
        if (-not [string]::IsNullOrWhiteSpace($key)) {
            $map[$key] = $value
        }
    }

    return $map
}

function Get-ConnectionValue {
    param(
        [hashtable]$Map,
        [string[]]$Keys
    )

    foreach ($key in $Keys) {
        $normalized = $key.ToLowerInvariant()
        if ($Map.ContainsKey($normalized)) {
            return $Map[$normalized]
        }
    }

    return $null
}

function Write-ConnectionTarget {
    param(
        [string]$Source,
        [hashtable]$Map
    )

    $host = Get-ConnectionValue -Map $Map -Keys @("Host", "Server", "Data Source")
    $port = Get-ConnectionValue -Map $Map -Keys @("Port")
    $database = Get-ConnectionValue -Map $Map -Keys @("Database", "Initial Catalog")
    $username = Get-ConnectionValue -Map $Map -Keys @("Username", "User ID", "Uid", "User")

    Write-Host "Connection source: $Source" -ForegroundColor Cyan
    Write-Host ("Target DB: host={0}; port={1}; database={2}; username={3}" -f $host, $port, $database, $username) -ForegroundColor Green
}

function Invoke-Psql {
    param(
        [string]$PsqlExe,
        [string]$ConnectionString,
        [hashtable]$ConnectionMap,
        [string]$Sql,
        [string]$WorkingDirectory = ""
    )

    $password = Get-ConnectionValue -Map $ConnectionMap -Keys @("Password", "Pwd")
    $previousPassword = $env:PGPASSWORD
    if (-not [string]::IsNullOrWhiteSpace($password)) {
        $env:PGPASSWORD = $password
    }

    try {
        if ([string]::IsNullOrWhiteSpace($WorkingDirectory)) {
            & $PsqlExe --dbname $ConnectionString --no-psqlrc --set ON_ERROR_STOP=1 -c $Sql
        }
        else {
            Push-Location $WorkingDirectory
            try {
                & $PsqlExe --dbname $ConnectionString --no-psqlrc --set ON_ERROR_STOP=1 -c $Sql
            }
            finally {
                Pop-Location
            }
        }

        if ($LASTEXITCODE -ne 0) {
            throw "psql failed with exit code $LASTEXITCODE."
        }
    }
    finally {
        if ($null -ne $previousPassword) {
            $env:PGPASSWORD = $previousPassword
        }
        else {
            Remove-Item Env:PGPASSWORD -ErrorAction SilentlyContinue
        }
    }
}

function Invoke-PsqlFile {
    param(
        [string]$PsqlExe,
        [string]$ConnectionString,
        [hashtable]$ConnectionMap,
        [string]$FilePath
    )

    $password = Get-ConnectionValue -Map $ConnectionMap -Keys @("Password", "Pwd")
    $previousPassword = $env:PGPASSWORD
    if (-not [string]::IsNullOrWhiteSpace($password)) {
        $env:PGPASSWORD = $password
    }

    try {
        & $PsqlExe --dbname $ConnectionString --no-psqlrc --set ON_ERROR_STOP=1 -f $FilePath
        if ($LASTEXITCODE -ne 0) {
            throw "psql failed with exit code $LASTEXITCODE."
        }
    }
    finally {
        if ($null -ne $previousPassword) {
            $env:PGPASSWORD = $previousPassword
        }
        else {
            Remove-Item Env:PGPASSWORD -ErrorAction SilentlyContinue
        }
    }
}

function Invoke-PgDump {
    param(
        [string]$PgDumpExe,
        [string]$ConnectionString,
        [hashtable]$ConnectionMap,
        [string]$BackupFile
    )

    $password = Get-ConnectionValue -Map $ConnectionMap -Keys @("Password", "Pwd")
    $previousPassword = $env:PGPASSWORD
    if (-not [string]::IsNullOrWhiteSpace($password)) {
        $env:PGPASSWORD = $password
    }

    try {
        & $PgDumpExe --format=custom --file $BackupFile --dbname $ConnectionString --no-owner --no-privileges
        if ($LASTEXITCODE -ne 0) {
            throw "pg_dump failed with exit code $LASTEXITCODE."
        }
    }
    finally {
        if ($null -ne $previousPassword) {
            $env:PGPASSWORD = $previousPassword
        }
        else {
            Remove-Item Env:PGPASSWORD -ErrorAction SilentlyContinue
        }
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

function Get-LatestAppLogFile {
    param([string]$LogsPath)

    if (-not (Test-Path -LiteralPath $LogsPath)) {
        return $null
    }

    $latest = Get-ChildItem -Path $LogsPath -File -Filter "*.log" |
        Where-Object { $_.Name -notmatch "error" } |
        Sort-Object LastWriteTime -Descending |
        Select-Object -First 1

    return $latest
}

function Wait-ForMigrationLogs {
    param(
        [string]$LogsPath,
        [int]$TimeoutSeconds
    )

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    $pattern = "Migrations applied successfully|No pending migrations"

    while ((Get-Date) -lt $deadline) {
        $latestLog = Get-LatestAppLogFile -LogsPath $LogsPath
        if ($null -ne $latestLog) {
            $content = Get-Content -LiteralPath $latestLog.FullName -Tail 80 -ErrorAction SilentlyContinue
            if ($content -match $pattern) {
                Write-Host "Migration log detected in $($latestLog.FullName)" -ForegroundColor Green
                return
            }
        }

        Start-Sleep -Seconds 5
    }

    throw "Timed out waiting for migration logs in $LogsPath."
}

function Write-TargetDatabaseDiagnostics {
    param(
        [string]$PsqlExe,
        [string]$ConnectionString,
        [hashtable]$ConnectionMap
    )

    $password = Get-ConnectionValue -Map $ConnectionMap -Keys @("Password", "Pwd")
    $previousPassword = $env:PGPASSWORD
    if (-not [string]::IsNullOrWhiteSpace($password)) {
        $env:PGPASSWORD = $password
    }

    try {
        $query = "select current_database() || '|' || current_user || '|' || coalesce(inet_server_addr()::text, '') || '|' || coalesce(inet_server_port()::text, '')"
        $result = & $PsqlExe --dbname $ConnectionString --no-psqlrc --tuples-only --no-align -c $query
        if ($LASTEXITCODE -ne 0) {
            throw "Unable to query current database."
        }

        $line = ($result | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | Select-Object -First 1)
        if (-not [string]::IsNullOrWhiteSpace($line)) {
            $parts = $line.Trim() -split '\|', 4
            Write-Host ("Server reports: database={0}; user={1}; host={2}; port={3}" -f $parts[0], $parts[1], $parts[2], $parts[3]) -ForegroundColor Green
        }
    }
    finally {
        if ($null -ne $previousPassword) {
            $env:PGPASSWORD = $previousPassword
        }
        else {
            Remove-Item Env:PGPASSWORD -ErrorAction SilentlyContinue
        }
    }
}

function Invoke-Deploy {
    param(
        [string]$DeployScript,
        [string]$Branch,
        [string]$ProjectPath,
        [string]$PublishPath,
        [string]$ServiceName,
        [string]$ApiBindUrl,
        [string]$PublicBaseUrl,
        [string]$EnvironmentName
    )

    & $DeployScript `
        -Branch $Branch `
        -ProjectPath $ProjectPath `
        -PublishPath $PublishPath `
        -ServiceName $ServiceName `
        -ApiBindUrl $ApiBindUrl `
        -PublicBaseUrl $PublicBaseUrl `
        -EnvironmentName $EnvironmentName
}

function Write-InspectionReport {
    param(
        [string]$LogsPath,
        [string]$ConnectionSource,
        [hashtable]$ConnectionMap
    )

    Write-ConnectionTarget -Source $ConnectionSource -Map $ConnectionMap

    $latestLog = Get-LatestAppLogFile -LogsPath $LogsPath
    if ($null -ne $latestLog) {
        Write-Host "Latest app log: $($latestLog.FullName)" -ForegroundColor Cyan
        Write-Host ("Latest log time: {0}" -f $latestLog.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss")) -ForegroundColor Cyan
        $tail = Get-Content -LiteralPath $latestLog.FullName -Tail 40 -ErrorAction SilentlyContinue
        if ($tail) {
            Write-Host "Recent migration-related lines:" -ForegroundColor Cyan
            $tail | Where-Object { $_ -match "Migration|migrat|database|DB|Connection" } | ForEach-Object { $_ }
        }
    }
}

$repoRoot = Resolve-RepoRoot
$deployScriptResolved = Resolve-ExistingPath -Path ($(if ([string]::IsNullOrWhiteSpace($DeployScriptPath)) { Join-Path $repoRoot "deploy-win.ps1" } else { $DeployScriptPath })) -BasePath $repoRoot
$logsPath = Join-Path $PublishPath "logs"
$resetSqlResolved = Resolve-ExistingPath -Path (Join-Path $repoRoot "scripts\db\clear-data-except-core-tables.sql") -BasePath $repoRoot
$seedSqlResolved = Resolve-ExistingPath -Path $SeedSqlPath -BasePath $repoRoot
$seedProvided = -not [string]::IsNullOrWhiteSpace($seedSqlResolved)
$backupDirectoryResolved = if ([System.IO.Path]::IsPathRooted($BackupDirectory)) {
    $BackupDirectory
}
else {
    Join-Path $repoRoot $BackupDirectory
}
$connectionDetails = Read-ConnectionString -PublishPath $PublishPath -EnvironmentName $EnvironmentName
$connectionMap = Parse-ConnectionString -ConnectionString $connectionDetails.Value

Write-Host "==== Kidzgo clean reset + deploy ====" -ForegroundColor Cyan
Write-ConnectionTarget -Source $connectionDetails.Source -Map $connectionMap

if ($InspectOnly) {
    Write-InspectionReport -LogsPath $logsPath -ConnectionSource $connectionDetails.Source -ConnectionMap $connectionMap
    return
}

if (-not $Force) {
    throw "This script is destructive. Re-run with -Force after you have confirmed the target database."
}

$psqlExe = Resolve-ToolPath -ToolName "psql.exe" -ExplicitPath $PsqlPath
$pgDumpExe = Resolve-ToolPath -ToolName "pg_dump.exe" -ExplicitPath $PgDumpPath

Write-TargetDatabaseDiagnostics -PsqlExe $psqlExe -ConnectionString $connectionDetails.Value -ConnectionMap $connectionMap

if (-not $SkipBackup) {
    if (-not (Test-Path -LiteralPath $backupDirectoryResolved)) {
        New-Item -ItemType Directory -Path $backupDirectoryResolved | Out-Null
    }

    $databaseName = Get-ConnectionValue -Map $connectionMap -Keys @("Database", "Initial Catalog")
    $timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
    $backupFile = Join-Path $backupDirectoryResolved ("{0}-{1}.backup" -f $databaseName, $timestamp)

    Write-Host "Creating backup: $backupFile" -ForegroundColor Yellow
    Invoke-PgDump -PgDumpExe $pgDumpExe -ConnectionString $connectionDetails.Value -ConnectionMap $connectionMap -BackupFile $backupFile
}
else {
    Write-Host "Backup skipped by request." -ForegroundColor DarkYellow
}

Stop-ServiceSafe -Name $ServiceName

switch ($ResetMode) {
    "Schema" {
        Write-Host "Reset mode: dropping and recreating schema public" -ForegroundColor Yellow
        Invoke-Psql -PsqlExe $psqlExe -ConnectionString $connectionDetails.Value -ConnectionMap $connectionMap -Sql "DROP SCHEMA IF EXISTS public CASCADE; CREATE SCHEMA public;"
    }
    "Truncate" {
        Write-Host "Reset mode: truncating data via $resetSqlResolved" -ForegroundColor Yellow
        Invoke-PsqlFile -PsqlExe $psqlExe -ConnectionString $connectionDetails.Value -ConnectionMap $connectionMap -FilePath $resetSqlResolved
    }
}

if ($ResetMode -eq "Truncate" -and $seedProvided) {
    Write-Host "Applying seed script before deploy: $seedSqlResolved" -ForegroundColor Yellow
    Invoke-PsqlFile -PsqlExe $psqlExe -ConnectionString $connectionDetails.Value -ConnectionMap $connectionMap -FilePath $seedSqlResolved
}

Write-Host "Publishing and deploying latest branch '$Branch'..." -ForegroundColor Cyan
Invoke-Deploy -DeployScript $deployScriptResolved -Branch $Branch -ProjectPath $ProjectPath -PublishPath $PublishPath -ServiceName $ServiceName -ApiBindUrl $ApiBindUrl -PublicBaseUrl $PublicBaseUrl -EnvironmentName $EnvironmentName

$logsPathResolved = Join-Path $PublishPath "logs"
Wait-ForMigrationLogs -LogsPath $logsPathResolved -TimeoutSeconds $MigrationWaitTimeoutSeconds

if ($ResetMode -eq "Schema" -and $seedProvided) {
    Write-Host "Applying seed script: $seedSqlResolved" -ForegroundColor Yellow
    Invoke-PsqlFile -PsqlExe $psqlExe -ConnectionString $connectionDetails.Value -ConnectionMap $connectionMap -FilePath $seedSqlResolved
}
elseif ($ResetMode -eq "Truncate" -and $seedProvided) {
    Write-Host "Seed script already applied before deploy." -ForegroundColor DarkGreen
}
else {
    Write-Host "No seed script was provided." -ForegroundColor DarkYellow
}

Write-Host "Reset + deploy completed successfully." -ForegroundColor Green
