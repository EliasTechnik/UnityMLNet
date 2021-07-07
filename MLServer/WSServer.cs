using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
//inspiration from https://developer.mozilla.org/en-US/docs/Web/API/WebSockets_API/Writing_WebSocket_server

public enum messageState{WSMreceived,WSMdecoded,WSMplain,WSMencoded}
public enum messageType{WSMincome,WSMtoBeSend,WSMnone,WSMConnectonClosed}

public delegate void WSMessageCallback(WSMessage message);
public class WSMessage{
    private byte[] raw;
    private byte[] decoded;
    private WSClient connection;
    private bool fin;
    private bool mask;
    private int opcode;
    private int msglen;
    private int offset;
    private messageState state;
    private messageType type;
    public messageState State => state;
    public messageType Type => type; 
    public WSMessage(byte[] _raw, WSClient _connection){
        raw=_raw;
        connection=_connection;
        state=messageState.WSMreceived;
    }
    public string getString(){
        if(state==messageState.WSMreceived){
            this.decodeRaw();
        }
        string result="";
        try{
            if(decoded.Length>0){
            result=Encoding.UTF8.GetString(decoded);
            }
        }
        catch(System.NullReferenceException){
            connection.Disconnect();
            Console.WriteLine("Undefined Message. The Client was disconnected.");
        }
        return result;
    }
    public void decodeRaw(){
        fin=(raw[0] & 0b10000000) != 0;
        mask=(raw[1] & 0b10000000) != 0;
        opcode=raw[0] & 0b00001111;
        msglen=raw[1] - 128;
        offset = 2;
        if (msglen == 126) {
            msglen = BitConverter.ToUInt16(new byte[] { raw[3], raw[2] }, 0);
            offset = 4;
        }
        else if (msglen == 127) {
            Console.WriteLine("TODO: msglen == 127, needs qword to store msglen");
            // i don't really know the byte order, please edit this
            // msglen = BitConverter.ToUInt64(new byte[] { bytes[5], bytes[4], bytes[3], bytes[2], bytes[9], bytes[8], bytes[7], bytes[6] }, 0);
            // offset = 10;
        }
        if(msglen == 0){
            //no message
        }
        else if(mask){
            decoded = new byte[msglen];
            byte[] masks = new byte[4] { raw[offset], raw[offset + 1], raw[offset + 2], raw[offset + 3] };
            offset += 4;
            for (int i = 0; i < msglen; ++i){
                decoded[i] = (byte)(raw[offset + i] ^ masks[i % 4]);
            }
        }
        state=messageState.WSMdecoded;
    }
}
public class WSClient{
    private bool offline;
    private TcpClient tcp_client;
    private NetworkStream stream;
    private List<WSMessage> inbox;
    public bool Offline{get{return offline;}}
    public WSMessage this[int index]{
        get{
            return inbox[index];
        }
    }
    public int msgCount{get{return inbox.Count;}}
    public TcpClient Tcp_Client{get{return tcp_client;}}
    public NetworkStream Stream{get{return stream;}}
    public WSClient(TcpClient _tcp_client, NetworkStream _stream){
        tcp_client=_tcp_client;
        stream=_stream;
        inbox=new List<WSMessage>();
        offline=false;
        Console.WriteLine("Client {0} connected.",stream.Socket.LocalEndPoint.ToString());
    }
    public int addMessage(WSMessage message){
        inbox.Add(message);
        return inbox.Count-1;
    }
    public void Disconnect(){
        stream.Dispose();
        tcp_client.Dispose();
        offline=true;
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
    private WSMessageCallback onMessageReceive;
    private void handshake(NetworkStream stream, string message){
        // 1. Obtain the value of the "Sec-WebSocket-Key" request header without any leading or trailing whitespace
        // 2. Concatenate it with "258EAFA5-E914-47DA-95CA-C5AB0DC85B11" (a special GUID specified by RFC 6455)
        // 3. Compute SHA-1 and Base64 hash of the new value
        // 4. Write the hash back as the value of "Sec-WebSocket-Accept" response header in an HTTP response
        string swk = Regex.Match(message, "Sec-WebSocket-Key: (.*)").Groups[1].Value.Trim();
        string swka = swk + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
        byte[] swkaSha1 = System.Security.Cryptography.SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(swka));
        string swkaSha1Base64 = Convert.ToBase64String(swkaSha1);
        // HTTP/1.1 defines the sequence CR LF as the end-of-line marker
        byte[] response = Encoding.UTF8.GetBytes(
            "HTTP/1.1 101 Switching Protocols\r\n" +
            "Connection: Upgrade\r\n" +
            "Upgrade: websocket\r\n" +
            "Sec-WebSocket-Accept: " + swkaSha1Base64 + "\r\n\r\n");

        stream.Write(response, 0, response.Length);
    }
    private bool ishandshake(NetworkStream stream, string s){
       if(Regex.IsMatch(s, "^GET", RegexOptions.IgnoreCase)){
           return true;
       } 
       return false;
    }
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
                    if(client.Tcp_Client.Connected==false){
                        //handle disconnect
                        client.Disconnect();
                        Console.WriteLine("Client unexpected disconnected.");
                    }
                    else{
                        if(stream.DataAvailable){
                            while(tcp_client.Available<3);//blocks (not good)
                            byte[] bytes = new byte[tcp_client.Available];
                            stream.Read(bytes, 0, tcp_client.Available);
                            string s = Encoding.UTF8.GetString(bytes);
                            if(ishandshake(stream,s)){
                                handshake(stream,s);
                            }
                            else{
                                WSMessage msg=new WSMessage(bytes,client);
                                msg.decodeRaw();
                                if(msg.getString()!=""){
                                    onMessageReceive(msg); //call callback
                                }
                                //Console.WriteLine("{0}",msg.getString());
                            }
                        }
                    }
                }
                //remove offline clients //maybe change later
                int max_clients=clients.Count;
                for(int i=0;i<max_clients;i++){
                    WSClient client=clients[i];
                    if(client.Offline){
                        clients.Remove(client);
                        max_clients=clients.Count;
                    }
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
            server.Start(); //starts the TcpListener
            openEarThread=new Thread(new ThreadStart(openEar));
            openEarThread.Start();
            mainLoopThread=new Thread(new ThreadStart(mainLoop));
            mainLoopThread.Start();
        }
    }
    public bool Stop(){
        if(stopserver==false){
            stopserver=true;
            while(mainLoopThread.IsAlive);
            while(openEarThread.IsAlive);
            return true;
        }
        return true;
    }
    public void OnMessageReceive(WSMessageCallback callback){
        onMessageReceive=new WSMessageCallback(callback);
    }
}