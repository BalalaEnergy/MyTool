# MyTool · 个人工具箱

Windows 桌面工具箱，首页提供 JSON 格式化、JSON Diff、文本比对和接口导入 YApi 入口。JSON 和文本比较均在本机完成；接口导入会将勾选的文档发送到配置的 YApi。

## 下载与运行

从 [Releases](https://github.com/BalalaEnergy/MyTool/releases) 下载 `MyTool.exe`，直接双击运行，无需安装。适用于安装了 .NET Framework 4.8 的 Windows（Windows 10/11 通常自带）。

窗口默认最大化到当前屏幕可用区域，保留任务栏，支持调整窗口大小及 DPI 缩放。程序不会修改显示器分辨率。

## 功能

- **JSON 格式化**：校验、两空格缩进、压缩、打开文件、复制和保存结果。
- **JSON Diff**：按结构对齐后左右高亮新增、删除和修改，支持行内高亮、行号、同步滚动、悬停查看 JSON 路径及返回编辑。
- **文本比对**：逐行对齐和行内高亮；区分空格、大小写，忽略 CRLF / LF 换行类型差异。大范围变化会采用区块对齐，并在状态栏说明。
- **接口导入 YApi**：选择 Java 项目目录和源码根目录，扫描 Spring Controller、预览请求/响应模型、勾选上传。支持保存多套项目配置。
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

C# / WPF，使用自定义 JSON 解析器保留数字原文。JSON 解析与测试位于 `MyTool.cs`，界面布局位于 `Shell.xaml`，交互逻辑位于 `Desktop.cs`。

当前版本在界面线程处理 JSON，适合日常文本；非常大的文件可能暂时阻塞界面。


## YApi 配置与缓存

1. 打开接口导入 YApi，填写配置名称、扫描目录、类型解析根目录、YApi 地址和项目 Token。
2. 点击连接验证，选择新接口分类，扫描后勾选要上传的接口并上传。
3. 下次启动选择历史项目，点击重新扫描并上传历史选择，直接同步上次勾选的接口。

配置保存在当前用户的 %LOCALAPPDATA%\MyTool\yapi-projects.json。Token 使用 Windows DPAPI 当前用户加密，其他 Windows 用户无法直接解密；配置不随 EXE 或源码发布。缓存包含目录、项目 ID、分类、更新选项、上传选择、上次上传时间和结果。

更新已有接口时按 HTTP 方法和路径识别，保留原来的名称、分类、描述和状态；不删除服务器接口。新接口使用配置所选分类。上传逐条记录成功、跳过和失败，下次可重试。

## 源码解析范围

当前为基于词法分析和声明结构的首版解析器，不启动业务项目。支持常见 Spring 映射、请求参数、JSON 请求体、表单/文件字段、Swagger 注解、嵌套类、常见集合、泛型包装和基础字段类型。依赖只在指定源码根目录内查找。

缺失依赖、动态类型、自定义序列化、金额类型、递归模型、文件流及未限定 HTTP 方法会列出警告，默认不勾选；快捷上传会排除出现警告的历史接口。它不是完整 Java 编译器，自定义组合注解、复杂常量路由和外部库类型仍需适配。上传前请预览。

测试还覆盖配置加密序列化、Controller/DTO 提取、泛型、文件参数及历史配置页面加载。

