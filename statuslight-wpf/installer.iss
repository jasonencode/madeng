[Setup]
AppName=Claude Status Light
AppVersion=1.3.0
AppPublisher=ClaudeStatusLight
DefaultDirName={autopf}\ClaudeStatusLight
DefaultGroupName=Claude Status Light
OutputDir=installer
OutputBaseFilename=ClaudeStatusLight_Setup
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin

[Languages]
Name: "chinesesimplified"; MessagesFile: "compiler:Languages\ChineseSimplified.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "创建桌面快捷方式"; GroupDescription: "附加图标:"
Name: "startupicon"; Description: "开机自动启动"; GroupDescription: "附加选项:"

[Files]
Source: "publish\ClaudeStatusLight.exe"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\Claude Status Light"; Filename: "{app}\ClaudeStatusLight.exe"
Name: "{group}\卸载 Claude Status Light"; Filename: "{uninstallexe}"
Name: "{autodesktop}\Claude Status Light"; Filename: "{app}\ClaudeStatusLight.exe"; Tasks: desktopicon
Name: "{userstartup}\ClaudeStatusLight"; Filename: "{app}\ClaudeStatusLight.exe"; Tasks: startupicon

[Run]
Filename: "{app}\ClaudeStatusLight.exe"; Description: "启动 Claude Status Light"; Flags: nowait postinstall skipifsilent
