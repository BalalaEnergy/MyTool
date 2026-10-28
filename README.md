# MyTool · 个人工具箱

Windows 桌面工具箱，首页提供 JSON 格式化和 JSON Diff 入口。所有 JSON 处理均在本机完成，不上传数据。

## 下载与运行

从 [Releases](https://github.com/BalalaEnergy/MyTool/releases) 下载 `MyTool.exe`，直接双击运行，无需安装。适用于安装了 .NET Framework 4.x 的 Windows（Windows 10/11 通常自带）。

窗口默认最大化到当前屏幕可用区域，保留任务栏，支持调整窗口大小及 DPI 缩放。程序不会修改显示器分辨率。

## 功能

- **JSON 格式化**：校验、两空格缩进、压缩、打开文件、复制和保存结果。
- **JSON Diff**：按 JSON 路径显示新增、删除、修改及类型变化；支持打开文件和交换两侧。
- 对象字段顺序不影响比较；数组按索引比较。
- 数字保留原始文本，避免大整数精度丢失；`1` 和 `1.0` 会被视为不同。
- 拒绝重复字段、非法 JSON 和超过 200 层的嵌套。

## 本地构建

使用 Windows PowerShell，在项目目录运行：

```powershell
.\build.ps1
```

输出为项目目录下的 `MyTool.exe`。使用 Windows 自带 .NET Framework C# 编译器，无需第三方依赖。

运行核心功能测试：

```powershell
$p = Start-Process .\MyTool.exe -ArgumentList '--self-test' -Wait -PassThru
Get-Content .\test-results.txt
if ($p.ExitCode -ne 0) { throw 'Tests failed' }
```

测试覆盖非法输入、Unicode、无损格式化、对象字段顺序和结构差异。`test-results.txt` 是测试生成文件。

## 发布

推送 `v*` 标签会触发 GitHub Actions，构建、测试并将 `MyTool.exe` 和 SHA-256 校验文件上传到对应 Release。标签关联该版本源码，Release 附带可执行程序。

## 实现

C# / Windows Forms，使用自定义 JSON 解析器保留数字原文。主入口和界面位于 `MyTool.cs`。

当前版本在界面线程处理 JSON，适合日常文本；非常大的文件可能暂时阻塞界面。
