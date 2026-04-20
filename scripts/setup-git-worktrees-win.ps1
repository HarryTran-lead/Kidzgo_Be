Param(
    [string]$RepositoryPath = "C:\Users\Administrator\Desktop\Projects\Kidzgo",
    [string]$WorktreeRoot = "C:\Users\Administrator\Desktop\Projects\Kidzgo.Worktrees",
    [string]$MainRemoteBranch = "main",
    [string]$DevRemoteBranch = "dev",
    [string]$DevSeedRemoteBranch = "main",
    [string]$MainLocalBranch = "vps-main",
    [string]$DevLocalBranch = "vps-dev",
    [string]$MainFolderName = "main",
    [string]$DevFolderName = "dev",
    [switch]$CreateMissingRemoteBranches
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

function Test-RemoteBranchExists {
    param(
        [string]$RepoPath,
        [string]$RemoteBranch
    )

    Push-Location $RepoPath
    try {
        $result = & git ls-remote --heads origin $RemoteBranch
        return -not [string]::IsNullOrWhiteSpace(($result | Out-String))
    }
    finally {
        Pop-Location
    }
}

function Ensure-RemoteBranch {
    param(
        [string]$RepoPath,
        [string]$RemoteBranch,
        [string]$SeedRemoteBranch,
        [switch]$AllowCreate
    )

    if (Test-RemoteBranchExists -RepoPath $RepoPath -RemoteBranch $RemoteBranch) {
        return
    }

    if (-not (Test-RemoteBranchExists -RepoPath $RepoPath -RemoteBranch $SeedRemoteBranch)) {
        throw "Remote branch 'origin/$RemoteBranch' does not exist, and seed branch 'origin/$SeedRemoteBranch' was also not found."
    }

    if (-not $AllowCreate) {
        throw @"
Remote branch 'origin/$RemoteBranch' does not exist.

Choose one of these options:
1. Create branch '$RemoteBranch' on GitHub, then rerun this script.
2. Rerun this script with -CreateMissingRemoteBranches to create '$RemoteBranch' from 'origin/$SeedRemoteBranch'.

Example:
.\scripts\setup-git-worktrees-win.ps1 -CreateMissingRemoteBranches
"@
    }

    Push-Location $RepoPath
    try {
        Write-Host "Creating remote branch 'origin/$RemoteBranch' from 'origin/$SeedRemoteBranch'..." -ForegroundColor Yellow
        & git push origin "refs/remotes/origin/$($SeedRemoteBranch):refs/heads/$($RemoteBranch)"
        if ($LASTEXITCODE -ne 0) {
            throw "Failed to create remote branch '$RemoteBranch' from '$SeedRemoteBranch'."
        }
    }
    finally {
        Pop-Location
    }
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

Ensure-RemoteBranch `
    -RepoPath $RepositoryPath `
    -RemoteBranch $MainRemoteBranch `
    -SeedRemoteBranch $MainRemoteBranch `
    -AllowCreate:$false

Ensure-RemoteBranch `
    -RepoPath $RepositoryPath `
    -RemoteBranch $DevRemoteBranch `
    -SeedRemoteBranch $DevSeedRemoteBranch `
    -AllowCreate:$CreateMissingRemoteBranches

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
