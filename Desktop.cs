using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using Microsoft.Win32;

public class Change {public string Kind{get;set;} public string Path{get;set;} public string Before{get;set;} public string After{get;set;}}
public class Desktop {
 Window window; bool diff; TextBox left,right; string formatInput="",formatOutput="",diffLeft="",diffRight=""; bool switched;
 T Find<T>(string name) where T:class {return window.FindName(name) as T;}
 void Text(string name,string text){Find<TextBlock>(name).Text=text;}
 void Visible(string name,bool show){Find<UIElement>(name).Visibility=show?Visibility.Visible:Visibility.Collapsed;}
 void Hook(string name,Action action){Find<Button>(name).Click+=(s,e)=>{try{action();}catch(Exception ex){Text("Status","操作失败，请检查输入");MessageBox.Show(window,ex.Message,"请检查输入",MessageBoxButton.OK,MessageBoxImage.Warning);}};}
 public Desktop(){using(var stream=typeof(Desktop).Assembly.GetManifestResourceStream("Shell.xaml"))window=(Window)XamlReader.Load(stream);left=Find<TextBox>("LeftEditor");right=Find<TextBox>("RightEditor");
 Hook("NavHome",Home);Hook("NavFormat",()=>Page(false));Hook("OpenFormat",()=>Page(false));Hook("NavDiff",()=>Page(true));Hook("OpenDiff",()=>Page(true));Hook("Run",()=>{if(diff)Compare();else Format(true);});Hook("Compress",()=>Format(false));Hook("Validate",()=>{Json.Parse(left.Text);Text("Status","✓ JSON 校验通过");});Hook("Clear",()=>{left.Clear();right.Clear();ResetResult();Text("Status","已清空");});Hook("Example",Example);Hook("Swap",()=>{string t=left.Text;left.Text=right.Text;right.Text=t;});Hook("LoadLeft",()=>Load(left));Hook("LoadRight",()=>Load(right));Hook("Copy",()=>{if(right.Text.Length>0){Clipboard.SetText(right.Text);Text("Status","已复制结果");}});Hook("Save",Save);
 left.TextChanged+=(s,e)=>{Text("LeftCount",left.Text.Length.ToString("N0")+" 个字符");if(diff)ResetResult();};right.TextChanged+=(s,e)=>{Text("RightCount",right.Text.Length.ToString("N0")+" 个字符");if(diff)ResetResult();};Home();}
 void Persist(){if(!switched)return;if(diff){diffLeft=left.Text;diffRight=right.Text;}else{formatInput=left.Text;formatOutput=right.Text;}}
 void SelectNav(string name){foreach(string n in new[]{"NavHome","NavFormat","NavDiff"}){var b=Find<Button>(n);b.Background=(Brush)new BrushConverter().ConvertFromString(n==name?"#303D64":"Transparent");b.Foreground=(Brush)new BrushConverter().ConvertFromString(n==name?"#FFFFFF":"#ABB7CD");}}
 void Home(){Persist();switched=false;Visible("HomePage",true);Visible("ToolPage",false);SelectNav("NavHome");Text("Breadcrumb","工作空间  /  工具概览");Text("Status","就绪，选择一个工具开始");}
 void Page(bool isDiff){Persist();diff=isDiff;switched=true;Visible("HomePage",false);Visible("ToolPage",true);SelectNav(diff?"NavDiff":"NavFormat");Text("Breadcrumb","工作空间  /  "+(diff?"JSON Diff":"JSON 格式化"));Text("ToolTitle",diff?"JSON Diff":"JSON 格式化");Text("ToolSubtitle",diff?"并排比较两份数据，快速定位字段与结构的变化。":"让数据更易读。格式化、压缩和校验，一步完成。");Find<Button>("Run").Content=diff?"⇄  比较差异":"{ }  格式化";foreach(string n in new[]{"Compress","Validate","Copy","Save"})Visible(n,!diff);Visible("Swap",diff);Visible("LoadRight",diff);Visible("Results",diff);Find<RowDefinition>("ResultSpacer").Height=new GridLength(diff?18:0);Find<RowDefinition>("ResultRow").Height=diff?new GridLength(0.65,GridUnitType.Star):new GridLength(0);right.IsReadOnly=!diff;Text("LeftTitle",diff?"原始 JSON":"输入 JSON");Text("RightTitle",diff?"新 JSON":"处理结果");left.Text=diff?diffLeft:formatInput;right.Text=diff?diffRight:formatOutput;ResetResult();Text("Status",diff?"对象忽略字段顺序 · 数组按索引比较 · 数字保留原始写法":"粘贴 JSON 或打开文件，也可以先试试示例");}
 void ResetResult(){Find<DataGrid>("DiffGrid").ItemsSource=null;Text("Summary","等待比较");Text("EmptyResult","点击「比较差异」，在这里查看变化");Visible("EmptyResult",true);}
 void Load(TextBox box){var d=new OpenFileDialog{Filter="JSON / 文本|*.json;*.txt|所有文件|*.*"};if(d.ShowDialog(window)==true){box.Text=File.ReadAllText(d.FileName,Encoding.UTF8);Text("Status","已打开 "+System.IO.Path.GetFileName(d.FileName));}}
 void Save(){if(right.Text.Length==0)throw new Exception("请先生成处理结果");var d=new SaveFileDialog{Filter="JSON 文件|*.json",FileName="result.json"};if(d.ShowDialog(window)==true){File.WriteAllText(d.FileName,right.Text,new UTF8Encoding(false));Text("Status","已保存 "+System.IO.Path.GetFileName(d.FileName));}}
 void Format(bool pretty){var json=Json.Parse(left.Text);right.Text=json.Print(pretty);Text("Status",pretty?"✓ 格式化完成 · 两空格缩进":"✓ 压缩完成");}
 void Compare(){ResetResult();Json a,b;try{a=Json.Parse(left.Text);}catch(Exception ex){throw new Exception("原始 JSON："+ex.Message);}try{b=Json.Parse(right.Text);}catch(Exception ex){throw new Exception("新 JSON："+ex.Message);}var rows=new List<string[]>();Json.Diff(a,b,"$",rows);var data=new List<Change>();int add=0,remove=0,modify=0;foreach(var row in rows){data.Add(new Change{Kind=row[0],Path=row[1],Before=row[2],After=row[3]});if(row[0]=="新增")add++;else if(row[0]=="删除")remove++;else modify++;}Find<DataGrid>("DiffGrid").ItemsSource=data;Visible("EmptyResult",rows.Count==0);Text("EmptyResult","✓ 两份 JSON 内容一致");Text("Summary","新增 "+add+"   /   删除 "+remove+"   /   修改 "+modify);Text("Status",rows.Count==0?"✓ 比较完成，内容一致":"比较完成，共 "+rows.Count+" 处差异");}
 void Example(){left.Text=Json.Parse("{\"name\":\"MyTool\",\"version\":\"1.0\",\"tools\":[\"JSON Formatter\",\"JSON Diff\"],\"local\":true}").Print(true);if(diff)right.Text=Json.Parse("{\"name\":\"MyTool\",\"version\":\"1.1\",\"tools\":[\"JSON Formatter\",\"JSON Diff\"],\"theme\":\"light\"}").Print(true);else Format(true);Text("Status","示例已载入，可直接编辑和处理");}
 [STAThread]public static int Main(string[] args){if(args.Length>0&&args[0]=="--self-test"){int result=Tests.Run();if(result!=0)return result;try{var test=new Desktop();test.Page(false);test.Example();if(Json.Parse(test.left.Text).Print(false)!=Json.Parse(test.right.Text).Print(false))throw new Exception("Format UI failed");test.Page(true);test.Example();test.Compare();if(((List<Change>)test.Find<DataGrid>("DiffGrid").ItemsSource).Count!=3)throw new Exception("Diff UI failed");test.Page(false);if(test.left.Text.Length==0)throw new Exception("Page persistence failed");test.window.Close();File.AppendAllText("test-results.txt",Environment.NewLine+"PASS: WPF startup, formatter output, diff results, page persistence.");return 0;}catch(Exception ex){File.AppendAllText("test-results.txt",Environment.NewLine+"FAIL: "+ex);return 1;}}var app=new Application();var desktop=new Desktop();app.Run(desktop.window);return 0;}
}

