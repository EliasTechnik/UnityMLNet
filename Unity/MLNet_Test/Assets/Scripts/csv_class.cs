using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static System.Environment;
using System.IO;
using System.Threading;

public class CSVCell{
    private string content;
    public string Content{
        get{
            return content;
        }
        set{
            content=value.Replace(',','.');
        }
    }
    public CSVCell(string _content){
        this.Content=_content;
    }
}
public class CSVLine{
    private List<CSVCell> items;
    public int Length{get{return items.Count;}}
    public void addCell(CSVCell cell){
        items.Add(cell);
    }
public CSVLine(){
        items=new List<CSVCell>();
    }
    public CSVLine(CSVCell[] _cells){
        items=new List<CSVCell>();
        foreach(CSVCell c in _cells){
            this.addCell(c);
        }
    }
    public CSVLine(string[] _cells){
        items=new List<CSVCell>();
        foreach(string s in _cells){
            this.addCell(new CSVCell(s));
        }
    }
    public CSVCell this[int index]{
        get{
            return items[index];
        }
        set{
            items[index]=value;
        }
    }
    public string getCSV(char seperator){
        string s="";
        foreach(CSVCell c in items){
            s+=c.Content+seperator;
        }
         //remove last seperator
        return s.Substring(0,s.LastIndexOf(seperator));
    }
}
public class CSVTable{
    private List<CSVLine> lines;
    private CSVLine header;
    private char seperator=',';
    private string savepath;
    public char Seperator{get{return seperator;}set{seperator=value;}}
    public CSVTable(){
        lines=new List<CSVLine>();
        header=null;
    }
    public CSVTable(char _seperator){
        lines=new List<CSVLine>();
        seperator=_seperator;
        header=null;
    }
    public void addLine(CSVLine _line){
        lines.Add(_line);
    }
    public void addLine(CSVLine _line,bool _isheader){
        if(_isheader){
            header=_line;
        }
        else{
            lines.Add(_line);
        }
    }
    public string toCSV(){
        string s="";
        if(header!=null){
            s+=header.getCSV(seperator)+NewLine;
        };
        foreach(CSVLine l in lines){
            s+=l.getCSV(seperator)+NewLine;
        }
        return s;
    }
    private void saveThread(){
        StreamWriter file=new StreamWriter(savepath);
        file.WriteLine(this.toCSV());
        file.Close();
    }
    public string saveToFile(string path,string filename){
        savepath=path+filename;
        Thread t =new Thread(new ThreadStart(saveThread));
        t.Start();
        return savepath;
    }
}