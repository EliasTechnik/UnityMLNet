using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XMLobject{
    private string identifier;
    private List<XMLobject> childs;
    private string payload;
    private List<List<string>> attributes; 
    public string Identifier{get{return identifier;}}
    public string Payload{get{return payload;}}
    public XMLobject(){
        attributes=new List<List<string>>();
        childs=new List<XMLobject>();
        payload="";
    }
    public XMLobject(string _identifier){
        identifier=_identifier;
        attributes=new List<List<string>>();
        childs=new List<XMLobject>();
        payload="";
    }
    public XMLobject(string _identifier,string _payload){
        identifier=_identifier;
        attributes=new List<List<string>>();
        childs=new List<XMLobject>();
        payload=_payload;
    }
    public XMLobject(string _identifier, params XMLobject[] child){
        identifier=_identifier;
        attributes=new List<List<string>>();
        childs=new List<XMLobject>();
        foreach (XMLobject xo in child){
            childs.Add(xo);
        }
    }
    public string decodeXML(string _xml){
        attributes=new List<List<string>>();
        if(this.isXML(_xml)){
            int start=_xml.IndexOf('<');
            _xml.Remove(0,start);
            int end=_xml.IndexOf(' ');
            identifier=_xml.Substring(0,end-1);
            _xml.Trim();
            List<string> pair = new List<string>();
            while(_xml[0]!='>'){
                end=_xml.IndexOf('=');
                pair.Add(_xml.Substring(0,end-1));
                start=_xml.IndexOf("'");
                _xml.Remove(0,start);
                end=_xml.IndexOf("'");
                pair.Add(_xml.Substring(0,end-1));
                _xml.Remove(0,end);
                _xml.Trim();
            }
            end=_xml.IndexOf("<"+identifier+"/>");
            payload=_xml.Substring(1,end-1);
            _xml.Remove(0,end+identifier.Length+1);
            while(this.isXML(payload)){
                XMLobject xo = new XMLobject();
                payload=xo.decodeXML(payload);
                childs.Add(xo);
            }
        }   
        return _xml;
    }
    public void addAttribute(string _identifier, string _value){
        List<string> a=new List<string>();
        a.Add(_identifier);
        a.Add(_value);
        attributes.Add(a);
    }
    public void addPayload(string _payload){
        payload+=_payload;
    }
    public void addChild(XMLobject child){
        childs.Add(child);
    }
    public XMLobject this[int index]
    {
        get{  
            if(index<childs.Count){
                return childs[index];
            }
            else{
                return null;
            }
        }
    }
    public XMLobject find(string _identifier){
        XMLobject erg;
        if(childs.Count>0){
            foreach(XMLobject xo in childs){
                if(xo.Identifier==_identifier){
                    return xo;
                }
                else{
                    erg=xo.find(_identifier);
                    if(erg!=null){
                        return erg;
                    }
                }
            }
            return null;
        }
        else{
            return null;
        }
    }
    public string serialize(){
        string output="<"+identifier;
        foreach(List<string> pair in attributes){
            output+=" "+pair[0]+"='"+pair[1]+"'";
        }
        output+=">";
        foreach(XMLobject xo in childs){
            output+=xo.toXML();
        }
        output+=payload+"<"+identifier+">";
        return output;
    }
    public bool isXML(string _xml){
        return (_xml.IndexOf('<')<_xml.IndexOf('>'));
    }

}