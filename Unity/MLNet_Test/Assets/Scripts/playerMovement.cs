using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerMovement : MonoBehaviour
{
    private float maxSpeed=0.4f;
    private float gain=0.002f;//0.05f;
    private float friction=0.995f;
    private Vector3 inertia;
    private Vector3 targetPosition;
    private float lastRoundTime;
    private float startTime;
    private ScoreManager mainScore;
    private Score currentScore;
    private Vector3 currentPosition;
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
    private void updateCompass(){
        GameObject compass=GameObject.Find("arrow");
        Vector3 direction=(targetPosition-compass.transform.position).normalized;
        Quaternion lookRotation=Quaternion.LookRotation(-direction);
        compass.transform.rotation=Quaternion.Slerp(compass.transform.rotation,lookRotation,1);
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
        currentScore.startTimer(Vector3.Distance(targetPosition,currentPosition));
    }
    // Update is called once per frame
    void Update()
    {
        currentPosition = this.transform.position;
        Movement m = new Movement(currentPosition,Time.frameCount);

        if(Input.GetKey(KeyCode.W)){
            if((inertia.z+gain)<maxSpeed){
                inertia=new Vector3(inertia.x,inertia.y,inertia.z+gain);
            }   
            m.addUsedKeys(KeyCode.W);
        }
        if(Input.GetKey(KeyCode.A)){
            if((inertia.x-gain)>(maxSpeed*-1)){
                inertia=new Vector3(inertia.x-gain,inertia.y,inertia.z);
            }
            m.addUsedKeys(KeyCode.A);
        }
        if(Input.GetKey(KeyCode.S)){
            if((inertia.z-gain)>(maxSpeed*-1)){
                inertia=new Vector3(inertia.x,inertia.y,inertia.z-gain);
            }
            m.addUsedKeys(KeyCode.S);
        }
        if(Input.GetKey(KeyCode.D)){
            if((inertia.x+gain)<maxSpeed){
                inertia=new Vector3(inertia.x+gain,inertia.y,inertia.z);
            }
            m.addUsedKeys(KeyCode.D);
        }   
        currentPosition=currentPosition+inertia;
        currentPosition=remove_border(currentPosition);
        m.setEndPosition(currentPosition);
        inertia=new Vector3(inertia.x*friction,inertia.y*friction,inertia.z*friction);
        this.transform.SetPositionAndRotation(currentPosition,this.transform.rotation); 
        updateCompass();
        currentScore.addMovement(m);
    }
    private void OnTriggerEnter(Collider other){
        if(other.name=="target_obj"){
            currentScore.stopTimer(true);
            respawn_target();
            Debug.Log("Score: "+currentScore.ScorePoints+" Time: "+currentScore.ScoreTime);
            mainScore.AddScore(currentScore);
            currentScore=new Score();
            currentScore.startTimer(Vector3.Distance(targetPosition,currentPosition));
        }
        if(mainScore.)
        //Debug.Log("Collision detected with "+other.name+" !");
    }
}
