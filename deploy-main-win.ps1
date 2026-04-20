Param(
    [string]$ProjectPath = "C:\Users\Administrator\Desktop\Projects\Kidzgo.Worktrees\main",
    [string]$PublishPath = "C:\apps\kidzgo-api-main",
    [string]$ServiceName = "KidzgoAPI-Main",
    [string]$ApiBindUrl = "http://127.0.0.1:5000",
    [string]$PublicBaseUrl = "https://api.kidzgo.vn",
    [string]$EnvironmentName = "Production"
)

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$deployScript = Join-Path $scriptRoot "deploy-win.ps1"

& $deployScript `
    -Branch "vps-main" `
    -ProjectPath $ProjectPath `
    -PublishPath $PublishPath `
    -ServiceName $ServiceName `
    -ApiBindUrl $ApiBindUrl `
    -PublicBaseUrl $PublicBaseUrl `
    -EnvironmentName $EnvironmentName
