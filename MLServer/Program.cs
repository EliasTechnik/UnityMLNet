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
        static AIWrapper ai; 
        static WSServer server;
        static void callback(WSMessage msg){
            //Console.WriteLine("Callback got called.");
            Console.WriteLine(msg.getString());
            XMLobject xo=new XMLobject();
            xo.decodeXML(msg.getString());
            msg.Connection.emptyInbox();//maybe rethink that complete inbox thing xD
            XMLobject erg =xo.find("instruction");
            if(erg!=null){
                string instruction=erg.Payload;
                //Console.WriteLine("instruction: "+instruction);
                switch(instruction){
                    case "predict":
                        //Console.WriteLine("Im asking the AI about: {0}",xo.find("movement_data").Payload);
                        CSVLine l=new CSVLine(xo.find("movement_data").Payload,',');
                        ModelInput data=new ModelInput(l);
                        ModelOutput prediction = ai.predict(data);
                        XMLobject xp=new XMLobject("ai_input");
                        if(prediction.Score[0]==1){
                            //Console.WriteLine("Keykode: D");
                            xp.addPayload("S");
                        }
                        if(prediction.Score[1]==1){
                            //Console.WriteLine("Keykode: W");
                            xp.addPayload("A");
                        }
                        if(prediction.Score[2]==1){
                            //Console.WriteLine("Keykode: A");
                            xp.addPayload("W");
                        }
                        if(prediction.Score[3]==1){
                            //Console.WriteLine("Keykode: S");
                            xp.addPayload("D");
                        }
                        msg.Connection.addMessageToOutbox(new WSMessage(xp.serialize(),msg.Connection,true));
                        //server.sendText(msg.Connection,xp.serialize());
                        break;
                }
            }
            else{
                msg.Connection.addMessageToOutbox(new WSMessage(msg.getString(),msg.Connection,true));
            }
        }
        static void Main(string[] args)
        {
            server=new WSServer("127.0.0.1",3333);
            server.OnMessageReceive(callback);
            server.Start();
            ai=new AIWrapper();
            ai.loadModel("model/MLModel.zip");

        }
    }
}
