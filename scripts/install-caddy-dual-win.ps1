Param(
    [string]$Version = "2.10.2",
    [string]$InstallRoot = "C:\Caddy",
    [string]$MainDomain = "api.kidzgo.vn",
    [string]$DevDomain = "dev-api.kidzgo.vn",
    [string]$MainApiUpstream = "127.0.0.1:5000",
    [string]$DevApiUpstream = "127.0.0.1:5001"
)

$ErrorActionPreference = "Stop"

Write-Host "Installing Caddy $Version to $InstallRoot" -ForegroundColor Cyan

function Remove-ServiceIfExists {
    param([string]$Name)

    $service = Get-Service -Name $Name -ErrorAction SilentlyContinue
    if ($null -eq $service) {
        return
    }

    if ($service.Status -eq "Running") {
        Stop-Service -Name $Name -Force -ErrorAction Stop
        $service.WaitForStatus("Stopped", "00:00:20")
    }

    sc.exe delete $Name | Out-Null
    Start-Sleep -Seconds 2
}

New-Item -ItemType Directory -Force -Path $InstallRoot | Out-Null

$zipPath = Join-Path $env:TEMP "caddy_${Version}_windows_amd64.zip"
$downloadUrl = "https://github.com/caddyserver/caddy/releases/download/v$Version/caddy_${Version}_windows_amd64.zip"

Invoke-WebRequest -Uri $downloadUrl -OutFile $zipPath
Expand-Archive -Path $zipPath -DestinationPath $InstallRoot -Force

$caddyFilePath = Join-Path $InstallRoot "Caddyfile"
$caddyFile = @"
$MainDomain {
    @root path /
    redir @root /swagger/index.html 308

    encode zstd gzip

    header {
        Strict-Transport-Security "max-age=31536000; includeSubDomains; preload"
        X-Content-Type-Options "nosniff"
        X-Frame-Options "SAMEORIGIN"
        Referrer-Policy "strict-origin-when-cross-origin"
    }

    reverse_proxy $MainApiUpstream
}

$DevDomain {
    @root path /
    redir @root /swagger/index.html 308

    encode zstd gzip

    header {
        Strict-Transport-Security "max-age=31536000; includeSubDomains; preload"
        X-Content-Type-Options "nosniff"
        X-Frame-Options "SAMEORIGIN"
        Referrer-Policy "strict-origin-when-cross-origin"
    }

    reverse_proxy $DevApiUpstream
}
"@

Set-Content -Path $caddyFilePath -Value $caddyFile -Encoding ASCII

Push-Location $InstallRoot
try {
    & .\caddy.exe validate --config $caddyFilePath --adapter caddyfile

    Remove-ServiceIfExists -Name "caddy"

    $binPath = "`"$InstallRoot\caddy.exe`" run --environ --config `"$caddyFilePath`" --adapter caddyfile"
    sc.exe create caddy binPath= $binPath start= auto DisplayName= "Caddy" | Out-Null
    sc.exe description caddy "Caddy reverse proxy for Kidzgo API main/dev" | Out-Null

    Start-Service caddy
    Get-Service caddy
}
finally {
    Pop-Location
}

Write-Host "Caddy installed." -ForegroundColor Green
Write-Host "  Main URL: https://$MainDomain/swagger/index.html" -ForegroundColor Green
Write-Host "  Dev URL : https://$DevDomain/swagger/index.html" -ForegroundColor Green
