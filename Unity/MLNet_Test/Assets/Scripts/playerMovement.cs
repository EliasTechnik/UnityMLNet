using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerMovement : MonoBehaviour
{
    private float movementSpeed=0.02f;
    public int score;
    private void respawn_target(){
        //get borders
        GameObject ground=GameObject.Find("ground_obj");
        GameObject target=GameObject.Find("target_obj");
        Vector3 center=ground.transform.position;
        Vector3 scale=ground.transform.lossyScale;
        Vector3 newpos=new Vector3(Random.Range((center.x-4)*scale.x,(center.x+4)*scale.x),center.y+0.5f,Random.Range((center.z-4)*scale.z,(center.z+4)*scale.z));
        target.transform.SetPositionAndRotation(newpos,target.transform.rotation);
    }
    // Start is called before the first frame update
    void Start()
    {
        score=0;
    }
    // Update is called once per frame
    void Update()
    {
    Vector3 currentPosition = this.transform.position;
    if(Input.GetKey(KeyCode.W)){
        currentPosition+=new Vector3(0,0,movementSpeed);
    }
    if(Input.GetKey(KeyCode.A)){
        currentPosition+=new Vector3(-movementSpeed,0,0);
    }
    if(Input.GetKey(KeyCode.S)){
        currentPosition+=new Vector3(0,0,-movementSpeed);
    }
    if(Input.GetKey(KeyCode.D)){
        currentPosition+=new Vector3(movementSpeed,0,0);
    }   
    this.transform.SetPositionAndRotation(currentPosition,this.transform.rotation); 
    }
    private void OnTriggerEnter(Collider other){
        if(other.name=="target_obj"){
            score++;
            respawn_target();
            Debug.Log("Score: "+score);
        }
        //Debug.Log("Collision detected with "+other.name+" !");
    }
}
