using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
//inspiration from https://developer.mozilla.org/en-US/docs/Web/API/WebSockets_API/Writing_WebSocket_server

public class WSClient{
    private TcpClient tcp_client;
    private NetworkStream stream;
    public TcpClient Tcp_Client{get{return tcp_client;}}
    public NetworkStream Stream{get{return stream;}}
    public WSClient(TcpClient _tcp_client, NetworkStream _stream){
        tcp_client=_tcp_client;
        stream=_stream;
    }
}
public class WSServer{
    private string ip;
    private int port;
    private bool stopserver;
    private TcpListener server;
    private List<WSClient> clients;
    private Thread openEarThread;
    private Thread mainLoopThread;
    private void openEar(){//waits for new connections
        while(stopserver==false){
            TcpClient client = server.AcceptTcpClient();
            NetworkStream stream=client.GetStream();
            clients.Add(new WSClient(client,stream));
            //yield();
        } 
    }
    private void mainLoop(){
        while(stopserver==false){
            if(clients.Count>0){
                foreach(WSClient client in clients){
                    NetworkStream stream=client.Stream;
                    TcpClient tcp_client=client.Tcp_Client;
                    //if(stream.)
                }
            }
        }
    }
    public WSServer(string _ip, int _port){
        ip=_ip;
        port=_port;
        clients=new List<WSClient>();
        stopserver=true;
    }
    public void Start(){ //change to bool
        if(stopserver==true){
            stopserver=false;
            server=new TcpListener(IPAddress.Parse(ip),port);
            openEarThread=new Thread(new ThreadStart(openEar));
            openEarThread.Start();
            mainLoopThread=new Thread(new ThreadStart(mainLoop));
            mainLoopThread.Start();
        }
    }
}