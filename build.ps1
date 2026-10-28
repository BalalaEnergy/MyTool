$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
& $compiler /nologo /target:winexe /optimize+ /platform:anycpu /utf8output /out:MyTool.exe /win32manifest:app.manifest /reference:System.Windows.Forms.dll /reference:System.Drawing.dll MyTool.cs
if ($LASTEXITCODE -ne 0) { throw 'Build failed' }
Write-Host 'Built MyTool.exe'
