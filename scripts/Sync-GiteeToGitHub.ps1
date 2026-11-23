<#
.SYNOPSIS
    把 Gitee 上的提交单向同步到 GitHub。

.DESCRIPTION
    默认把 Gitee（origin）的 master 快进推送到 GitHub（github）的 main。
    脚本不会提交工作区改动，也不会切换当前分支；它直接推送刚拉下来的
    Gitee 远端分支。

    如果 GitHub 上有 Gitee 没有的提交，普通 push 会被拒绝。脚本不会自动
    强制覆盖，避免误删 GitHub 上的内容。

.PARAMETER SourceRemote
    Gitee 远端名，默认 origin。

.PARAMETER SourceBranch
    要同步的 Gitee 分支，默认 master。

.PARAMETER TargetRemote
    GitHub 远端名，默认 github。

.PARAMETER TargetBranch
    要推送到的 GitHub 分支，默认 main。

.PARAMETER PushTags
    同时把本地标签推送到 GitHub。默认不推送，避免误带临时标签。

.PARAMETER Proxy
    GitHub 使用的 HTTP 代理，例如 http://127.0.0.1:7892。
    只作用于 GitHub，不影响 Gitee。

.PARAMETER Force
    当 GitHub 分支与 Gitee 分叉时，允许把 GitHub 分支改成 Gitee 的镜像。
    实际使用 --force-with-lease，并锁定刚检查到的 GitHub 提交；如果远端
    在此期间被别人更新，推送仍会被拒绝。

.PARAMETER DryRun
    实际执行 push --dry-run。会访问两个远端，但不会修改 GitHub。

.EXAMPLE
    .\Sync-GiteeToGitHub.ps1

.EXAMPLE
    .\Sync-GiteeToGitHub.ps1 -DryRun

.EXAMPLE
    .\Sync-GiteeToGitHub.ps1 -PushTags

.EXAMPLE
    .\Sync-GiteeToGitHub.ps1 -Proxy http://127.0.0.1:7892

.EXAMPLE
    .\Sync-GiteeToGitHub.ps1 -Proxy http://127.0.0.1:7892 -Force

.EXAMPLE
    .\Sync-GiteeToGitHub.ps1 -SourceBranch dev -TargetBranch dev
#>
[CmdletBinding()]
param(
    [string]$SourceRemote = 'origin',
    [string]$SourceBranch = 'master',
    [string]$TargetRemote = 'github',
    [string]$TargetBranch = 'main',
    [switch]$PushTags,
    [string]$Proxy,
    [switch]$Force,
    [switch]$DryRun
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Write-Step([string]$Text) {
    Write-Host ''
    Write-Host "== $Text ==" -ForegroundColor Cyan
}

function Assert-GitSuccess([string]$Action) {
    if ($LASTEXITCODE -ne 0) {
        throw "$Action 失败（git exit=$LASTEXITCODE）"
    }
}

function Get-RemoteUrl([string]$Name) {
    $url = & git remote get-url $Name 2>$null
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($url)) {
        throw "找不到远端 '$Name'。先检查 git remote -v，或用 -SourceRemote/-TargetRemote 指定名称。"
    }
    return ($url | Select-Object -First 1)
}

# 不论从哪个目录调用脚本，都在仓库根目录执行。
Push-Location $PSScriptRoot
try {
    $repoRoot = & git rev-parse --show-toplevel 2>$null
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($repoRoot)) {
        throw "当前目录不是 Git 仓库：$PSScriptRoot"
    }

    $sourceUrl = Get-RemoteUrl $SourceRemote
    $targetUrl = Get-RemoteUrl $TargetRemote
    $targetGitConfig = @()
    if (-not [string]::IsNullOrWhiteSpace($Proxy)) {
        $targetGitConfig = @('-c', "http.proxy=$Proxy")
    }

    Write-Host 'Tsuki · Gitee -> GitHub 单向同步' -ForegroundColor Green
    Write-Host "  仓库   : $repoRoot"
    Write-Host "  Gitee  : $SourceRemote/$SourceBranch  ($sourceUrl)"
    Write-Host "  GitHub : $TargetRemote/$TargetBranch  ($targetUrl)"
    if (-not [string]::IsNullOrWhiteSpace($Proxy)) {
        Write-Host "  GitHub代理: $Proxy"
    }
    if ($DryRun) {
        Write-Host '  模式   : DryRun（不会修改 GitHub）' -ForegroundColor Yellow
    }

    Write-Step '1/4 拉取 Gitee 最新提交'
    & git fetch --prune $SourceRemote $SourceBranch
    Assert-GitSuccess "git fetch $SourceRemote $SourceBranch"

    $sourceRef = "refs/remotes/$SourceRemote/$SourceBranch"
    $sourceSha = & git rev-parse --verify "$sourceRef^{commit}" 2>$null
    Assert-GitSuccess "读取 $sourceRef"
    $sourceSha = ($sourceSha | Select-Object -First 1).Trim()

    $sourceSubject = & git log -1 --format='%s' $sourceSha 2>$null
    Assert-GitSuccess '读取 Gitee 最新提交说明'
    $sourceSubject = ($sourceSubject | Select-Object -First 1)

    Write-Host "  最新提交: $($sourceSha.Substring(0, 7))  $sourceSubject"

    Write-Step '2/4 检查 GitHub 当前提交'
    $targetRef = "refs/heads/$TargetBranch"
    $remoteLine = @(& git @targetGitConfig ls-remote --heads $TargetRemote $targetRef 2>$null)
    Assert-GitSuccess "git ls-remote $TargetRemote $targetRef"

    $targetSha = $null
    if ($remoteLine.Count -gt 0 -and -not [string]::IsNullOrWhiteSpace($remoteLine[0])) {
        $targetSha = ($remoteLine[0] -split '\s+')[0]
    }

    if ($targetSha) {
        Write-Host "  GitHub 原提交: $($targetSha.Substring(0, 7))"
    } else {
        Write-Host "  GitHub 上还没有 $TargetBranch，将创建该分支"
    }

    if ($targetSha -eq $sourceSha) {
        Write-Host "  已经同步，无需推送：$TargetRemote/$TargetBranch" -ForegroundColor Green
    } else {
        Write-Step '3/4 拉取 GitHub 并检查是否可快进'
        if ($targetSha) {
            & git @targetGitConfig fetch --prune $TargetRemote $TargetBranch
            Assert-GitSuccess "git fetch $TargetRemote $TargetBranch"

            & git merge-base --is-ancestor $targetSha $sourceSha
            $isFastForward = ($LASTEXITCODE -eq 0)

            if (-not $isFastForward) {
                Write-Host '  GitHub 与 Gitee 已分叉，不能直接快进。' -ForegroundColor Yellow
                Write-Host '  仅存在于 GitHub 的提交：' -ForegroundColor Yellow
                $githubOnly = @(& git log --oneline --reverse "$sourceSha..$targetSha")
                if ($githubOnly.Count -eq 0) {
                    Write-Host '    （无法列出，请检查本地 Git 对象）' -ForegroundColor Yellow
                } else {
                    $githubOnly | ForEach-Object { Write-Host "    $_" -ForegroundColor Yellow }
                }

                if (-not $Force) {
                    throw @'

同步已停止，没有覆盖 GitHub。
如果确认 Gitee 是唯一事实来源，可以加 -Force 重新执行：
  .\Sync-GiteeToGitHub.ps1 -Force

如果 GitHub 那条提交还需要保留，请先把它合并或摘取到 Gitee，再重新同步。
'@
                }

                Write-Host '  已指定 -Force：将使用 --force-with-lease 覆盖 GitHub 分支。' -ForegroundColor Yellow
            }
        }

        Write-Step '4/4 推送 Gitee 提交到 GitHub'
        # 冒号左边是来源，右边是目标：Gitee master -> GitHub main。
        $refspec = '{0}:{1}' -f $sourceRef, $targetRef
        $pushArgs = @('push')
        if ($DryRun) {
            $pushArgs += '--dry-run'
        }
        if ($targetSha -and -not $isFastForward) {
            if (-not $Force) {
                throw '内部错误：分叉状态没有 Force 授权。'
            }
            $pushArgs += "--force-with-lease=$($targetRef):$targetSha"
        }
        $pushArgs += @($TargetRemote, $refspec)

        & git @targetGitConfig @pushArgs
        if ($LASTEXITCODE -ne 0) {
            throw @'

推送被拒绝。通常说明 GitHub 上有 Gitee 没有的提交。
不要直接强制覆盖；先比较两边：
  git fetch github
  git log --oneline --graph github/main..origin/master
  git log --oneline --graph origin/master..github/main
'@
        }
    }

    if ($PushTags) {
        Write-Step '附加：同步标签'
        $tagArgs = @('push')
        if ($DryRun) {
            $tagArgs += '--dry-run'
        }
        $tagArgs += @($TargetRemote, '--tags')
        & git @targetGitConfig @tagArgs
        Assert-GitSuccess "git push $TargetRemote --tags"
    }

    Write-Host ''
    if ($DryRun) {
        Write-Host "DryRun 完成：同步命令有效，Gitee/$SourceBranch -> GitHub/$TargetBranch 未实际推送。" -ForegroundColor Yellow
    } else {
        Write-Host "同步完成：Gitee/$SourceBranch -> GitHub/$TargetBranch" -ForegroundColor Green
    }
}
finally {
    Pop-Location
}
