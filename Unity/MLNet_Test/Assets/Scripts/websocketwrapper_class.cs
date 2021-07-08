using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NativeWebSocket;

public class WSWrapper{
    private WebSocket ws;
    private bool connected;
    private string recent_message;
    public WSWrapper(){
        connected=false;
        recent_message="";
    }
    public async void Connect(string url){
        ws=new WebSocket(url);
        connected=true;
        ws.OnOpen += () =>{
            Debug.Log("Connection open!");
        };
        ws.OnError += (e) =>{
            Debug.Log("Error! " + e);
        };
        ws.OnClose += (e) =>{
            Debug.Log("Connection closed!");
        };
        ws.OnMessage += (bytes) =>{
            //Debug.Log("OnMessage!");
            //Debug.Log(bytes);
            recent_message = System.Text.Encoding.UTF8.GetString(bytes);
            //Debug.Log("Got Message:"+recent_message);
        };
        await ws.Connect();
    }
    public void Refresh(){
        #if !UNITY_WEBGL || UNITY_EDITOR
            ws.DispatchMessageQueue();
        #endif
    }
    public string getRecentMessage(){
        string result="";
        if(recent_message!=""){
            result=recent_message;
            recent_message="";
            return result;
        }
        else{
            return null;
        }
    }
    public async void SendString(string msg){
        if(ws.State == WebSocketState.Open){
            await ws.SendText(msg);   
        }
    }
    public async void Close(){
        await ws.Close(); //close websocket on closing the app
        connected=false;
    }
}
    