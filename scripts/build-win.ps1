<#
.SYNOPSIS
    一键出 Windows 安装包。

.DESCRIPTION
    依次做五件事，任何一步失败就中止（不会拿半成品继续往下走）：
      1) dotnet publish → build\backend\        （.NET 后端，自包含，用户机器不需要装 .NET）
      2) PyInstaller    → pyai\dist\TsukiAi\     （AI 服务，用户机器不需要装 Python）
      3) npm run build  → Face\bot-face\out\     （typecheck + 前端打包）
      4) electron-builder --win --dir
                        → Face\bot-face\dist\win-unpacked\  （Electron + 上面两者塞进 resources\）
      5) ISCC.exe       → installer\output\Tsuki-<版本>-setup.exe  （Inno Setup 压成安装器）

    第 5 步需要先装 Inno Setup 6（https://jrsoftware.org/isdl.php）；
    没装的话脚本会跳过它并打印手动命令，前四步的产物照样可用。

.PARAMETER SkipBackend
    跳过后端发布（后端代码没改时可以省 10 秒）。

.PARAMETER SkipAi
    跳过 AI 服务打包（pyai 没改时可以省 1 分钟）。注意：跳过时用的是上次的 dist\TsukiAi。

.PARAMETER SkipApp
    跳过前端构建与 electron-builder（只重打安装器时用）。

.PARAMETER SkipInstaller
    只做到 win-unpacked 为止，不调 Inno（想自己用 GUI 编译 .iss 时用）。

.PARAMETER Iscc
    ISCC.exe 的完整路径（装在非默认位置时用）。

.EXAMPLE
    .\scripts\build-win.ps1
    .\scripts\build-win.ps1 -SkipBackend -SkipAi     # 只改了前端时
#>
[CmdletBinding()]
param(
    [switch]$SkipBackend,
    [switch]$SkipAi,
    [switch]$SkipApp,
    [switch]$SkipInstaller,
    [string]$Iscc
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$face = Join-Path $root 'Face\bot-face'
$pyai = Join-Path $root 'pyai'
$py = Join-Path $pyai '.venv\Scripts\python.exe'
$backendOut = Join-Path $root 'build\backend'
$unpacked = Join-Path $face 'dist\win-unpacked'
$installerOut = Join-Path $root 'installer\output'

$version = (Get-Content (Join-Path $face 'package.json') -Raw | ConvertFrom-Json).version

function Write-Step([string]$text) {
    Write-Host ''
    Write-Host "════ $text ════" -ForegroundColor Cyan
}

function Assert-Ok([string]$what) {
    if ($LASTEXITCODE -ne 0) { throw "$what 失败（exit=$LASTEXITCODE）" }
}

function Get-DirSizeMb([string]$path) {
    if (-not (Test-Path $path)) { return 0 }
    $bytes = (Get-ChildItem $path -Recurse -File -ErrorAction SilentlyContinue | Measure-Object Length -Sum).Sum
    return [math]::Round($bytes / 1MB, 1)
}

Write-Host "Tsuki 打包脚本 · 版本 $version" -ForegroundColor Green
Write-Host "仓库根: $root"

# ─────────────────────────────────────────────────────────── 1) 后端
if (-not $SkipBackend) {
    Write-Step '1/5 发布 .NET 后端（自包含 win-x64）'
    Remove-Item $backendOut -Recurse -Force -ErrorAction SilentlyContinue
    dotnet publish (Join-Path $root 'cs\cs\cs\cs.csproj') `
        -c Release -r win-x64 --self-contained true -o $backendOut --nologo -v quiet
    Assert-Ok 'dotnet publish'

    # 这条以前踩过：Resource 不是 wwwroot，不显式声明就不会进发布输出，装完头像全 404
    $avatarCount = @(Get-ChildItem (Join-Path $backendOut 'Resource\Avatar') -File -ErrorAction SilentlyContinue).Count
    if ($avatarCount -eq 0) { throw "发布输出里没有 Resource\Avatar（头像会 404），检查 cs.csproj 里的 Content Include" }
    Write-Host "  ✓ $backendOut  ($(Get-DirSizeMb $backendOut) MB, $avatarCount 张头像)"
} else {
    Write-Host '  跳过 1/5' -ForegroundColor DarkGray
}

# ─────────────────────────────────────────────────────────── 2) AI 服务
if (-not $SkipAi) {
    Write-Step '2/5 打包 AI 服务（PyInstaller onedir）'
    if (-not (Test-Path $py)) {
        throw "找不到 pyai 的虚拟环境：$py`n先执行：python -m venv pyai\.venv ; pyai\.venv\Scripts\pip install -r pyai\requirements.txt"
    }
    & $py -m pip show pyinstaller *> $null
    if ($LASTEXITCODE -ne 0) {
        Write-Host '  venv 里没有 PyInstaller，先装一下（构建工具，不进 requirements.txt）'
        & $py -m pip install --quiet pyinstaller
        Assert-Ok 'pip install pyinstaller'
    }

    Push-Location $pyai
    try {
        Remove-Item 'build', 'dist\TsukiAi' -Recurse -Force -ErrorAction SilentlyContinue
        & $py -m PyInstaller TsukiAi.spec --noconfirm --log-level WARN
        Assert-Ok 'PyInstaller'
    } finally {
        Pop-Location
    }
    $aiExe = Join-Path $pyai 'dist\TsukiAi\TsukiAi.exe'
    if (-not (Test-Path $aiExe)) { throw "PyInstaller 没产出 $aiExe" }
    Write-Host "  ✓ $(Join-Path $pyai 'dist\TsukiAi')  ($(Get-DirSizeMb (Join-Path $pyai 'dist\TsukiAi')) MB)"
} else {
    Write-Host '  跳过 2/5（沿用上次的 dist\TsukiAi）' -ForegroundColor DarkGray
}

# ─────────────────────────────────────────────────────────── 3) 前端 + 4) 解包目录
if (-not $SkipApp) {
    Write-Step '3/5 构建前端（typecheck + electron-vite）'
    Push-Location $face
    try {
        npm run build
        Assert-Ok 'npm run build'

        Write-Step '4/5 生成解包目录（electron-builder --win --dir）'
        $builder = Join-Path $face 'node_modules\.bin\electron-builder.cmd'
        if (Test-Path $builder) {
            & $builder --win --dir
        } else {
            npx electron-builder --win --dir
        }
        Assert-Ok 'electron-builder --win --dir'
    } finally {
        Pop-Location
    }

    if (-not (Test-Path (Join-Path $unpacked 'Tsuki.exe'))) { throw "没生成 $unpacked\Tsuki.exe" }
    foreach ($need in @('resources\backend\cs.exe', 'resources\ai\TsukiAi.exe')) {
        $p = Join-Path $unpacked $need
        if (-not (Test-Path $p)) { throw "解包目录里缺 $need —— 检查 electron-builder.yml 的 extraResources" }
    }
    Write-Host "  ✓ $unpacked  ($(Get-DirSizeMb $unpacked) MB)"
} else {
    Write-Host '  跳过 3–4/5（沿用上次的 win-unpacked）' -ForegroundColor DarkGray
}

# ─────────────────────────────────────────────────────────── 5) 安装器
if (-not $SkipInstaller) {
    Write-Step '5/5 生成安装器（Inno Setup）'

    $isccPath = $Iscc
    if (-not $isccPath) {
        foreach ($candidate in @(
                "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
                "$env:ProgramFiles\Inno Setup 6\ISCC.exe",
                "$env:LOCALAPPDATA\Programs\Inno Setup 6\ISCC.exe"
            )) {
            if (Test-Path $candidate) { $isccPath = $candidate; break }
        }
    }
    if (-not $isccPath) {
        $cmd = Get-Command 'ISCC.exe' -ErrorAction SilentlyContinue
        if ($cmd) { $isccPath = $cmd.Source }
    }

    if ($isccPath) {
        & $isccPath "/DMyAppVersion=$version" (Join-Path $root 'installer\Tsuki.iss')
        Assert-Ok 'ISCC'
        Write-Host "  ✓ $installerOut  ($(Get-DirSizeMb $installerOut) MB)"
    } else {
        Write-Host '  ⚠ 没找到 ISCC.exe，跳过安装器。' -ForegroundColor Yellow
        Write-Host '    装好 Inno Setup 6 之后重跑本脚本，或手动执行：' -ForegroundColor Yellow
        Write-Host "      ISCC.exe /DMyAppVersion=$version `"$root\installer\Tsuki.iss`"" -ForegroundColor Yellow
    }
} else {
    Write-Host '  跳过 5/5' -ForegroundColor DarkGray
}

# ─────────────────────────────────────────────────────────── 汇总
Write-Step '完成'
Write-Host "  后端      : $backendOut"
Write-Host "  AI 服务   : $(Join-Path $pyai 'dist\TsukiAi')"
Write-Host "  解包目录  : $unpacked"
if (Test-Path $installerOut) { Write-Host "  安装包    : $installerOut" }
Write-Host ''
Write-Host '  装之前的自测：直接双击 dist\win-unpacked\Tsuki.exe' -ForegroundColor Green
Write-Host '  （它就是"装完之后的样子"，用户数据会写进 %APPDATA%\Tsuki）' -ForegroundColor DarkGray
