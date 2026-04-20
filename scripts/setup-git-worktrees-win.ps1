Param(
    [string]$RepositoryPath = "C:\Users\Administrator\Desktop\Projects\Kidzgo",
    [string]$WorktreeRoot = "C:\Users\Administrator\Desktop\Projects\Kidzgo.Worktrees",
    [string]$MainRemoteBranch = "main",
    [string]$DevRemoteBranch = "dev",
    [string]$MainLocalBranch = "vps-main",
    [string]$DevLocalBranch = "vps-dev",
    [string]$MainFolderName = "main",
    [string]$DevFolderName = "dev"
)

$ErrorActionPreference = "Stop"

function Assert-GitRepository {
    param([string]$Path)

    if (-not (Test-Path $Path)) {
        throw "Repository path '$Path' does not exist."
    }

    Push-Location $Path
    try {
        & git rev-parse --is-inside-work-tree | Out-Null
        if ($LASTEXITCODE -ne 0) {
            throw "Path '$Path' is not a git repository."
        }
    }
    finally {
        Pop-Location
    }
}

function Ensure-Worktree {
    param(
        [string]$RepoPath,
        [string]$TargetPath,
        [string]$LocalBranch,
        [string]$RemoteBranch
    )

    if (Test-Path $TargetPath) {
        Write-Host "Worktree already exists at $TargetPath" -ForegroundColor DarkGreen
        return
    }

    Push-Location $RepoPath
    try {
        & git show-ref --verify --quiet "refs/heads/$LocalBranch"
        $localBranchExists = $LASTEXITCODE -eq 0

        if ($localBranchExists) {
            Write-Host "Creating worktree '$TargetPath' from existing local branch '$LocalBranch'..." -ForegroundColor Yellow
            & git worktree add $TargetPath $LocalBranch
        }
        else {
            Write-Host "Creating local tracking branch '$LocalBranch' from origin/$RemoteBranch..." -ForegroundColor Yellow
            & git worktree add -b $LocalBranch $TargetPath "origin/$RemoteBranch"
        }

        if ($LASTEXITCODE -ne 0) {
            throw "git worktree add failed for '$TargetPath'."
        }
    }
    finally {
        Pop-Location
    }

    Push-Location $TargetPath
    try {
        & git branch --set-upstream-to="origin/$RemoteBranch" $LocalBranch | Out-Null
        if ($LASTEXITCODE -ne 0) {
            throw "Failed to set upstream for branch '$LocalBranch'."
        }
    }
    finally {
        Pop-Location
    }

    Write-Host "Worktree ready: $TargetPath ($LocalBranch -> origin/$RemoteBranch)" -ForegroundColor Green
}

Assert-GitRepository -Path $RepositoryPath

New-Item -ItemType Directory -Path $WorktreeRoot -Force | Out-Null

Push-Location $RepositoryPath
try {
    Write-Host "Fetching latest refs from origin..." -ForegroundColor Cyan
    & git fetch origin --prune
    if ($LASTEXITCODE -ne 0) {
        throw "git fetch origin --prune failed."
    }
}
finally {
    Pop-Location
}

$mainPath = Join-Path $WorktreeRoot $MainFolderName
$devPath = Join-Path $WorktreeRoot $DevFolderName

Ensure-Worktree `
    -RepoPath $RepositoryPath `
    -TargetPath $mainPath `
    -LocalBranch $MainLocalBranch `
    -RemoteBranch $MainRemoteBranch

Ensure-Worktree `
    -RepoPath $RepositoryPath `
    -TargetPath $devPath `
    -LocalBranch $DevLocalBranch `
    -RemoteBranch $DevRemoteBranch

Write-Host ""
Write-Host "Next paths:" -ForegroundColor Cyan
Write-Host "  Main worktree: $mainPath" -ForegroundColor Green
Write-Host "  Dev worktree : $devPath" -ForegroundColor Green
Write-Host ""
Write-Host "Recommended deploy commands:" -ForegroundColor Cyan
Write-Host "  .\deploy-main-win.ps1 -ProjectPath `"$mainPath`"" -ForegroundColor Green
Write-Host "  .\deploy-dev-win.ps1 -ProjectPath `"$devPath`"" -ForegroundColor Green
