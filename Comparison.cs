using System;
using System.Collections.Generic;

public class CompareLine {
    public string Left, Right, Kind, Path;
    public CompareLine(string left,string right,string kind,string path){Left=left;Right=right;Kind=kind;Path=path;}
}

public static class Comparison {
    public static List<CompareLine> JsonLines(Json left,Json right){var rows=new List<CompareLine>();JsonPair(left,right,"","",false,false,0,"$",rows);return rows;}
    static string Indent(int depth){return new string(' ',depth*2);}
    static void Single(Json value,string prefix,bool comma,int depth,List<string> lines){
        bool obj=value.Kind=="object",arr=value.Kind=="array";
        if(!obj&&!arr){lines.Add(Indent(depth)+prefix+value.Print(false)+(comma?",":""));return;}
        int count=obj?value.Members.Count:value.Items.Count;
        if(count==0){lines.Add(Indent(depth)+prefix+(obj?"{}":"[]")+(comma?",":""));return;}
        lines.Add(Indent(depth)+prefix+(obj?"{":"["));
        for(int i=0;i<count;i++)Single(obj?value.Members[i].Value:value.Items[i],obj?Json.Quote(value.Members[i].Key)+": ":"",i<count-1,depth+1,lines);
        lines.Add(Indent(depth)+(obj?"}":"]")+(comma?",":""));
    }
    static void JsonPair(Json a,Json b,string ap,string bp,bool ac,bool bc,int depth,string path,List<CompareLine> rows){
        if(a==null||b==null||a.Kind!=b.Kind){
            var al=new List<string>();var bl=new List<string>();if(a!=null)Single(a,ap,ac,depth,al);if(b!=null)Single(b,bp,bc,depth,bl);
            for(int i=0;i<Math.Max(al.Count,bl.Count);i++)rows.Add(new CompareLine(i<al.Count?al[i]:null,i<bl.Count?bl[i]:null,a==null?"新增":b==null?"删除":"修改",path));return;
        }
        if(a.Kind!="object"&&a.Kind!="array"){
            rows.Add(new CompareLine(Indent(depth)+ap+a.Print(false)+(ac?",":""),Indent(depth)+bp+b.Print(false)+(bc?",":""),a.Value==b.Value?"相同":"修改",path));return;
        }
        bool obj=a.Kind=="object";int an=obj?a.Members.Count:a.Items.Count,bn=obj?b.Members.Count:b.Items.Count;
        if(an==0&&bn==0){rows.Add(new CompareLine(Indent(depth)+ap+(obj?"{}":"[]")+(ac?",":""),Indent(depth)+bp+(obj?"{}":"[]")+(bc?",":""),"相同",path));return;}
        rows.Add(new CompareLine(Indent(depth)+ap+(obj?"{":"["),Indent(depth)+bp+(obj?"{":"["),"相同",path));
        if(obj){
            var aa=new Dictionary<string,Json>(StringComparer.Ordinal);var bb=new Dictionary<string,Json>(StringComparer.Ordinal);var keys=new List<string>();
            foreach(var m in a.Members){aa.Add(m.Key,m.Value);keys.Add(m.Key);}foreach(var m in b.Members){bb.Add(m.Key,m.Value);if(!aa.ContainsKey(m.Key))keys.Add(m.Key);}
            int ai=0,bi=0;foreach(string key in keys){Json av,bv;aa.TryGetValue(key,out av);bb.TryGetValue(key,out bv);if(av!=null)ai++;if(bv!=null)bi++;string p=Json.Quote(key)+": ";JsonPair(av,bv,p,p,ai<an,bi<bn,depth+1,path+"["+Json.Quote(key)+"]",rows);}
        }else for(int i=0;i<Math.Max(an,bn);i++)JsonPair(i<an?a.Items[i]:null,i<bn?b.Items[i]:null,"","",i<an-1,i<bn-1,depth+1,path+"["+i+"]",rows);
        rows.Add(new CompareLine(Indent(depth)+(obj?"}":"]")+(ac?",":""),Indent(depth)+(obj?"}":"]")+(bc?",":""),"相同",path));
    }
    static string[] Lines(string s){return s.Replace("\r\n","\n").Replace('\r','\n').Split('\n');}
    public static List<CompareLine> TextLines(string a,string b,out bool coarse){
        string[] aa=Lines(a),bb=Lines(b);var rows=new List<CompareLine>();coarse=false;
        int start=0;while(start<aa.Length&&start<bb.Length&&aa[start]==bb[start]){rows.Add(new CompareLine(aa[start],bb[start],"相同",""));start++;}
        int endA=aa.Length,endB=bb.Length;while(endA>start&&endB>start&&aa[endA-1]==bb[endB-1]){endA--;endB--;}
        int n=endA-start,m=endB-start;var deleted=new List<string>();var added=new List<string>();
        if((long)(n+1)*(m+1)>2000000){coarse=true;for(int i=start;i<endA;i++)deleted.Add(aa[i]);for(int j=start;j<endB;j++)added.Add(bb[j]);Flush(deleted,added,rows);}
        else{
            var dp=new int[n+1,m+1];for(int i=n-1;i>=0;i--)for(int j=m-1;j>=0;j--)dp[i,j]=aa[start+i]==bb[start+j]?dp[i+1,j+1]+1:Math.Max(dp[i+1,j],dp[i,j+1]);
            int x=0,y=0;while(x<n||y<m){if(x<n&&y<m&&aa[start+x]==bb[start+y]){Flush(deleted,added,rows);rows.Add(new CompareLine(aa[start+x],bb[start+y],"相同",""));x++;y++;}else if(x<n&&(y==m||dp[x+1,y]>=dp[x,y+1]))deleted.Add(aa[start+x++]);else added.Add(bb[start+y++]);}Flush(deleted,added,rows);
        }
        while(endA<aa.Length)rows.Add(new CompareLine(aa[endA++],bb[endB++],"相同",""));return rows;
    }
    static void Flush(List<string> deleted,List<string> added,List<CompareLine> rows){for(int i=0;i<Math.Max(deleted.Count,added.Count);i++)rows.Add(new CompareLine(i<deleted.Count?deleted[i]:null,i<added.Count?added[i]:null,i<deleted.Count&&i<added.Count?"修改":i<deleted.Count?"删除":"新增",""));deleted.Clear();added.Clear();}
}
