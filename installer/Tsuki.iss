; ─────────────────────────────────────────────────────────────────────────────
; Tsuki · Inno Setup 安装脚本
;
; 编译（二选一）：
;   1) 直接跑仓库根的脚本（推荐，会自动带上版本号）：
;        .\scripts\build-win.ps1
;   2) 手动：
;        "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" /DMyAppVersion=1.0.0 installer\Tsuki.iss
;
; 前置条件：跑这个脚本之前，dist\win-unpacked\ 必须已经存在
;   （由 scripts\build-win.ps1 里的 electron-builder --win --dir 生成）
;
; 设计取舍：
;   · PrivilegesRequired=lowest → 装到 %LOCALAPPDATA%\Programs\Tsuki，全程不弹 UAC，
;     普通用户双击就能装（这比装到 Program Files 少一堆权限问题）。
;   · 用户数据在 %APPDATA%\Tsuki（KeKe.db / 日志 / 提示词），卸载时【故意不删】，
;     避免"卸了重装聊天记录没了"。要彻底清干净，让用户自己删那个目录。
;   · 一次装完，不做组件勾选（个人项目没必要）。
; ─────────────────────────────────────────────────────────────────────────────

#ifndef MyAppVersion
  ; 兜底值：从 Inno 的 IDE 里直接按 F9 编译时用（命令行 ISCC 会用 /D 覆盖）。
  ; 注意和 Face\bot-face\package.json 的 version 保持一致，改版本时两处一起改。
  #define MyAppVersion "0.0.1"
#endif

#define MyAppName "Tsuki"
#define MyAppPublisher "Muecheack"
#define MyAppExeName "Tsuki.exe"
; AppId 一旦发布就不能改：Inno 靠它识别"这是同一个软件的升级"，改了会变成并存安装
#define MyAppId "{{6C1B7E2A-9F4D-4C3E-8A51-2B7D9E4F1C33}"
; win-unpacked 的位置，相对本文件（installer\）所在目录
#define AppSourceDir "..\Face\bot-face\dist\win-unpacked"

[Setup]
AppId={#MyAppId}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={localappdata}\Programs\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
OutputDir=output
OutputBaseFilename={#MyAppName}-{#MyAppVersion}-setup
SetupIconFile=..\Face\bot-face\build\tsuki_ico.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern
; Inno 6.3+ 的写法；如果 ISCC 报 "Unknown architecture"，改成 x64
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
; 装/升时如果 Tsuki 还开着，提示用户关闭（我们的 sidecar 会随 Electron 一起退）
CloseApplications=yes
RestartApplications=no

[Languages]
Name: "en"; MessagesFile: "compiler:Default.isl"
; 中文界面：Inno 官方包里不带中文，需要先把 ChineseSimplified.isl 放到
; Inno 安装目录的 Languages\ 下（下载：https://jrsoftware.org/files/istrans/ ），
; 然后打开下面这行、并把上面的 en 那行注释掉。
; Name: "zh"; MessagesFile: "compiler:Languages\ChineseSimplified.isl"

[Tasks]
; checkedonce：默认勾上，但用户取消过就不再强塞
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: checkedonce

[InstallDelete]
; 覆盖安装时先清掉上次的程序文件，避免升级后残留旧 dll/exe。
; 安全前提：用户数据不在安装目录（在 %APPDATA%\Tsuki），所以删这里不会碰数据。
Type: filesandordirs; Name: "{app}\resources"
Type: files; Name: "{app}\{#MyAppExeName}"

[Files]
; recursesubdirs：resources\backend、resources\ai 这些子目录一起装
Source: "{#AppSourceDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
; postinstall + skipifsilent：装完的最后一页给一个"启动 Tsuki"的勾选；静默安装时不启动
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent
