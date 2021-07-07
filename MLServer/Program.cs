using System;
//using Microsoft.ML;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;


namespace MLServer
{
    class Program
    {
        static void callback(WSMessage msg){
            //Console.WriteLine("Callback got called.");
            Console.WriteLine(msg.getString());
            XMLobject xo=new XMLobject();
            xo.decodeXML(msg.getString());
            string instruction=xo.find("instruction").Payload;
            switch(instruction){
                case "predict":
                    Console.WriteLine("Im asking the AI about: {0}",xo.find("movement_data").Payload);
                    break;
            }
        }
        static void Main(string[] args)
        {
            WSServer server=new WSServer("127.0.0.1",3333);
            server.OnMessageReceive(callback);
            server.Start();

        }
    }
}
