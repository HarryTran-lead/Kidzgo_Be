Param(
    [string]$ProjectPath = "C:\Users\Administrator\Desktop\Projects\Kidzgo.Worktrees\dev",
    [string]$PublishPath = "C:\apps\kidzgo-api-dev",
    [string]$ServiceName = "KidzgoAPI-Dev",
    [string]$ApiBindUrl = "http://127.0.0.1:5001",
    [string]$PublicBaseUrl = "https://dev-api.kidzgo.vn",
    [string]$EnvironmentName = "Production"
)

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$deployScript = Join-Path $scriptRoot "deploy-win.ps1"

& $deployScript `
    -Branch "dev" `
    -ProjectPath $ProjectPath `
    -PublishPath $PublishPath `
    -ServiceName $ServiceName `
    -ApiBindUrl $ApiBindUrl `
    -PublicBaseUrl $PublicBaseUrl `
    -EnvironmentName $EnvironmentName
