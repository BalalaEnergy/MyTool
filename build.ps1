$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$wpf = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\WPF'
& $compiler /nologo /target:winexe /optimize+ /platform:anycpu /utf8output /out:MyTool.exe /win32manifest:app.manifest /resource:Shell.xaml,Shell.xaml /resource:Yapi.xaml,Yapi.xaml /reference:System.Windows.Forms.dll /reference:System.Drawing.dll /reference:"$wpf\PresentationFramework.dll" /reference:"$wpf\PresentationCore.dll" /reference:"$wpf\WindowsBase.dll" /reference:System.Xaml.dll /reference:System.Web.Extensions.dll /reference:System.Security.dll MyTool.cs Desktop.cs Comparison.cs JavaSource.cs YapiService.cs YapiPane.cs ImporterTests.cs
if ($LASTEXITCODE -ne 0) { throw 'Build failed' }
Write-Host 'Built MyTool.exe'





