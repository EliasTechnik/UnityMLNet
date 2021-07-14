using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerMovement : MonoBehaviour
{
    private WSWrapper api;
    private float maxSpeed=0.4f;
    private float gain=0.002f;//0.05f;
    private float friction=0.995f;//0.995f;
    private Vector3 inertia;
    private Vector3 targetPosition;
    private float lastRoundTime;
    private float startTime;
    private ScoreManager mainScore;
    private Movement currentMovement;
    private Score currentScore;
    private Vector3 currentPosition;
    private bool trainingStarted;
    private void respawn_target(){
        //get borders
        GameObject ground=GameObject.Find("ground_obj");
        GameObject target=GameObject.Find("target_obj");
        Vector3 center=ground.transform.position;
        Vector3 scale=ground.transform.lossyScale;
        targetPosition=new Vector3(Random.Range((center.x-3)*scale.x,(center.x+3)*scale.x),center.y+0.5f,Random.Range((center.z-3)*scale.z,(center.z+3)*scale.z));
        target.transform.SetPositionAndRotation(targetPosition,target.transform.rotation);
    }
    private Vector3 remove_border(Vector3 target_pos){
        GameObject ground=GameObject.Find("ground_obj");
        Vector3 center=ground.transform.position;
        Vector3 scale=ground.transform.lossyScale;
        float x_neg_edge=(center.x-5)*scale.x+30;
        float z_neg_edge=(center.z-5)*scale.z+30;
        float x_pos_edge=(center.x+5)*scale.x-30;
        float z_pos_edge=(center.z+5)*scale.z-30;
        if(target_pos.x<x_neg_edge){
            target_pos=new Vector3(x_pos_edge,target_pos.y,target_pos.z);
            Debug.Log("Neg X Border crossed!");
        }
        if(target_pos.x>x_pos_edge){
            target_pos=new Vector3(x_neg_edge,target_pos.y,target_pos.z);
            Debug.Log("Pos X Border crossed!");
        }
        if(target_pos.z<z_neg_edge){
            target_pos=new Vector3(target_pos.x,target_pos.y,z_pos_edge);
            Debug.Log("Neg Z Border crossed!");
        }
        if(target_pos.z>z_pos_edge){
            target_pos=new Vector3(target_pos.x,target_pos.y,z_neg_edge);
            Debug.Log("Pos Z Border crossed!");
        }
        return target_pos;
    }
    private float updateCompass(){ //returns value between 0-359
        GameObject compass=GameObject.Find("arrow");
        Vector3 tp=targetPosition;
        tp.y+=0.6f;
        Vector3 direction=(tp-compass.transform.position).normalized;
        Quaternion lookRotation=Quaternion.LookRotation(-direction);
        compass.transform.rotation=Quaternion.Slerp(compass.transform.rotation,lookRotation,1);
        //Debug.Log("Angle: "+lookRotation.eulerAngles.y.ToString());
        return lookRotation.eulerAngles.y;
    }
    private void trigger_input(KeyCode input){//gets called every time a input happens
        if(input==KeyCode.W){
            if((inertia.z+gain)<maxSpeed){
                inertia=new Vector3(inertia.x,inertia.y,inertia.z+gain);
            }   
            currentMovement.addUsedKeys(KeyCode.W);
        }
        if(input==KeyCode.A){
            if((inertia.x-gain)>(maxSpeed*-1)){
                inertia=new Vector3(inertia.x-gain,inertia.y,inertia.z);
            }
            currentMovement.addUsedKeys(KeyCode.A);
        }
        if(input==KeyCode.S){
            if((inertia.z-gain)>(maxSpeed*-1)){
                inertia=new Vector3(inertia.x,inertia.y,inertia.z-gain);
            }
            currentMovement.addUsedKeys(KeyCode.S);
        }
        if(input==KeyCode.D){
            if((inertia.x+gain)<maxSpeed){
                inertia=new Vector3(inertia.x+gain,inertia.y,inertia.z);
            }
            currentMovement.addUsedKeys(KeyCode.D);
        }   
    }
    private Movement get_output(){//returns information about the current movement
        return currentMovement;
    }
    
    // Start is called before the first frame update
    void Start()
    {
        inertia=new Vector3(0,0,0);
        respawn_target();
        startTime=Time.realtimeSinceStartup;
        mainScore=new ScoreManager();
        currentScore=new Score();
        currentPosition = this.transform.position;
        currentScore.startTimer(targetPosition,currentPosition);
        api=new WSWrapper();
        api.Connect("ws://127.0.0.1:3333");
        trainingStarted=false;
    }
    // Update is called once per frame
    void Update()
    {
        api.Refresh();
        string answer=api.getRecentMessage();
        currentPosition = this.transform.position;
        currentMovement = new Movement(currentPosition,Time.frameCount);
        if(answer!=null){
            Debug.Log(answer);
            XMLobject ai=new XMLobject();
            ai.decodeXML(answer);
            XMLobject ai_input=ai.find("ai_input");
            if(ai_input!=null){
                if(ai_input.Payload=="W"){trigger_input(KeyCode.W);}
                if(ai_input.Payload=="A"){trigger_input(KeyCode.A);}
                if(ai_input.Payload=="S"){trigger_input(KeyCode.S);}
                if(ai_input.Payload=="D"){trigger_input(KeyCode.D);}
            }
        }
        if(Input.GetKey(KeyCode.W)){
            trigger_input(KeyCode.W);
        }
        if(Input.GetKey(KeyCode.A)){
            trigger_input(KeyCode.A);
        }
        if(Input.GetKey(KeyCode.S)){
            trigger_input(KeyCode.S);
        }
        if(Input.GetKey(KeyCode.D)){
            trigger_input(KeyCode.D);
        }   
        currentPosition=currentPosition+inertia;
        currentPosition=remove_border(currentPosition);
        currentMovement.setEndPosition(currentPosition,targetPosition);
        inertia=new Vector3(inertia.x*friction,inertia.y*friction,inertia.z*friction);
        this.transform.SetPositionAndRotation(currentPosition,this.transform.rotation); 
        currentMovement.ArrowDirection=updateCompass();
        XMLobject xo;
        if(currentScore.ScoreId==-1){
            if(trainingStarted==false){
                //advice server to collect data
                xo=new XMLobject("instruction","learnFromUser_start");
                api.SendString(xo.serialize());
                trainingStarted=true;
            }
            else{
                xo=new XMLobject("instruction","learnFromUser_data");
            }
        }
        else{
             xo=new XMLobject("instruction","predict2");
             //xo=new XMLobject("instruction","");
        }
        
        xo.addChild(new XMLobject("movement_data",currentMovement.toCSVLine().getCSV(',')));
        api.SendString(xo.serialize());
        currentScore.addMovement(currentMovement);
    }
    private void OnTriggerEnter(Collider other){
        if(other.name=="target_obj"){
            currentScore.stopTimer(true);
            respawn_target();
            Debug.Log("Score: "+currentScore.ScorePoints+" Time: "+currentScore.ScoreTime);
            mainScore.AddScore(currentScore);
            if(currentScore.ScoreId==-1){
                XMLobject xo=new XMLobject("instruction","createModel");
                api.SendString(xo.serialize());
            }
            //mainScore.saveLastRound("Assets/eduData/");
            currentScore=new Score();
            currentScore.startTimer(targetPosition,currentPosition);
        }
        //Debug.Log("Collision detected with "+other.name+" !");
    }
}
