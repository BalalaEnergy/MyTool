using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

public class YapiProfile {
 public string Id=Guid.NewGuid().ToString("N"),Name="新项目",ScanDirectory="",SourceRoot="",Server="",TokenCipher="",ProjectName="",LastUpload="",LastResult="";
 public int ProjectId,CategoryId; public bool UpdateExisting=true;public List<string> SelectedKeys=new List<string>();
 public override string ToString(){return Name+(LastUpload.Length>0?"  ·  "+LastUpload:"");}
 public string Token(){return TokenCipher.Length==0?"":Encoding.UTF8.GetString(ProtectedData.Unprotect(Convert.FromBase64String(TokenCipher),null,DataProtectionScope.CurrentUser));}
 public void SetToken(string token){TokenCipher=Convert.ToBase64String(ProtectedData.Protect(Encoding.UTF8.GetBytes(token),null,DataProtectionScope.CurrentUser));}
}
public static class YapiCache {
 public static string Folder{get{return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"MyTool");}}
 public static string FilePath{get{return Path.Combine(Folder,"yapi-projects.json");}}
 public static List<YapiProfile> Load(){if(!File.Exists(FilePath))return new List<YapiProfile>();try{return JavaScanner.Serializer().Deserialize<List<YapiProfile>>(File.ReadAllText(FilePath))??new List<YapiProfile>();}catch{throw new Exception("历史配置无法读取，请检查 "+FilePath+"，原文件已保留。");}}
 public static void Save(List<YapiProfile> profiles){Directory.CreateDirectory(Folder);string temp=FilePath+"."+Guid.NewGuid().ToString("N")+".tmp";File.WriteAllText(temp,JavaScanner.Serializer().Serialize(profiles),new UTF8Encoding(false));if(File.Exists(FilePath))File.Replace(temp,FilePath,null);else File.Move(temp,FilePath);}
}
public class YapiCategory {public int Id;public string Name;public override string ToString(){return Name;}}
public class YapiClient {
 readonly string server,token;
 public YapiClient(YapiProfile p){Uri uri;if(!Uri.TryCreate(p.Server.TrimEnd('/'),UriKind.Absolute,out uri)||(uri.Scheme!="http"&&uri.Scheme!="https")||uri.UserInfo.Length>0||uri.Query.Length>0||uri.Fragment.Length>0)throw new Exception("请输入有效的 YApi HTTP / HTTPS 地址");server=p.Server.TrimEnd('/');token=p.Token();if(token.Length==0)throw new Exception("请填写项目 Token");}
 public object Call(string endpoint,Dictionary<string,object> data,bool post=false){data=new Dictionary<string,object>(data);data["token"]=token;string url=server+"/api/"+endpoint;if(!post)url+="?"+string.Join("&",data.Select(x=>Uri.EscapeDataString(x.Key)+"="+Uri.EscapeDataString(Convert.ToString(x.Value))).ToArray());try{var req=(HttpWebRequest)WebRequest.Create(url);req.Timeout=20000;req.ReadWriteTimeout=20000;req.AllowAutoRedirect=false;req.Proxy=null;if(post){req.Method="POST";req.ContentType="application/json; charset=utf-8";byte[] bytes=Encoding.UTF8.GetBytes(JavaScanner.Serializer().Serialize(data));req.ContentLength=bytes.Length;using(var stream=req.GetRequestStream())stream.Write(bytes,0,bytes.Length);}using(var response=(HttpWebResponse)req.GetResponse()){if((int)response.StatusCode>=300)throw new Exception("服务器重定向，请检查地址");using(var reader=new StreamReader(response.GetResponseStream(),Encoding.UTF8)){var body=JavaScanner.Serializer().Deserialize<Dictionary<string,object>>(reader.ReadToEnd());if(Convert.ToInt32(body["errcode"])!=0)throw new Exception("YApi 返回错误码 "+body["errcode"]);return body["data"];}}}catch(WebException){throw new Exception("连接 YApi 失败，请检查网络、地址和 Token");}}
 public Dictionary<string,object> Project(){return (Dictionary<string,object>)Call("project/get",new Dictionary<string,object>());}
 public List<YapiCategory> Categories(int project){var data=(System.Collections.IEnumerable)Call("interface/getCatMenu",new Dictionary<string,object>{{"project_id",project}});var result=new List<YapiCategory>();foreach(Dictionary<string,object> item in data)result.Add(new YapiCategory{Id=Convert.ToInt32(item["_id"]),Name=Convert.ToString(item["name"])});return result;}
 public Dictionary<string,Dictionary<string,object>> Existing(int project){var cats=(System.Collections.IEnumerable)Call("interface/list_menu",new Dictionary<string,object>{{"project_id",project}});var result=new Dictionary<string,Dictionary<string,object>>(StringComparer.Ordinal);foreach(Dictionary<string,object> cat in cats){if(!cat.ContainsKey("list"))continue;foreach(Dictionary<string,object> item in (System.Collections.IEnumerable)cat["list"]){var copy=new Dictionary<string,object>(item);copy["catid"]=cat["_id"];result[Convert.ToString(item["method"]).ToUpperInvariant()+" "+Convert.ToString(item["path"])]=copy;}}return result;}
 public string Upload(ApiEntry entry,YapiProfile profile,Dictionary<string,Dictionary<string,object>> existing){Dictionary<string,object> old;var payload=new Dictionary<string,object>(entry.Payload);payload["project_id"]=profile.ProjectId;payload["catid"]=profile.CategoryId;if(existing.TryGetValue(entry.Key,out old)){if(!profile.UpdateExisting)return "跳过：已存在";var detail=(Dictionary<string,object>)Call("interface/get",new Dictionary<string,object>{{"id",old["_id"]}});foreach(string key in new[]{"title","catid","markdown","desc","status","tag"})if(detail.ContainsKey(key))payload[key]=detail[key];}Call("interface/save",payload,true);return old==null?"新增成功":"更新成功";}
}
