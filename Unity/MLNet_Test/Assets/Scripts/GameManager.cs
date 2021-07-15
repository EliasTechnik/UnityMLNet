using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class playerObject{
    private float maxSpeed=0.4f;
    private float gain=0.02f;
    private float friction=0.995f;//0.995f;
    private Vector3 inertia;
    private Vector3 trailingPosition;
    private GameObject real_object;
    private Vector3 targetPosition;
    private GameObject targetArrow;
    private GameObject movementArrow;
    private float targetArrowRotation;
    private float movementArrowRotation;
    private Vector3 currentPosition;
    private WorldManager world;
    private void updateArrows(){
        Vector3 tp=targetPosition;
        tp.y+=0.6f;
        Vector3 direction=(tp-targetArrow.transform.position).normalized;
        Quaternion lookRotation=Quaternion.LookRotation(-direction);
        targetArrow.transform.rotation=Quaternion.Slerp(targetArrow.transform.rotation,lookRotation,1);
        targetArrowRotation=180-lookRotation.eulerAngles.y;

        Vector3 fp=trailingPosition;
        fp.y+=1f;
        direction=(fp-movementArrow.transform.position).normalized;
        lookRotation=Quaternion.LookRotation(direction);
        movementArrow.transform.rotation=Quaternion.Slerp(movementArrow.transform.rotation,lookRotation,1);
        movementArrowRotation=180-lookRotation.eulerAngles.y;
    }
    public Vector3 CurrentPosition{get{return currentPosition;}}
    public float DistanceToTarget{get{return Vector3.Distance(CurrentPosition,targetPosition);}}
    public float ArrowDifference{get{return movementArrowRotation-targetArrowRotation;}}
    public float MovementArrowRotation{get{return MovementArrowRotation;}}
    public float TargetArrowRotation{get{return targetArrowRotation;}}
    public GameObject RealObject{get{return real_object;}}
    public playerObject(GameObject _object, WorldManager _world, GameObject _targetArrow, GameObject _movementArrow){
        real_object=_object;
        world=_world;
        targetArrow=_targetArrow;
        movementArrow=_movementArrow;
        currentPosition=real_object.transform.position;
        inertia=new Vector3(0,0,0.25f);
        trailingPosition=new Vector3(0,0,0.25f);
    }
    public void triggerAction(KeyCode _key){
        if(_key==KeyCode.S){
            if((inertia.z+gain)<maxSpeed){
                inertia=new Vector3(inertia.x,inertia.y,inertia.z+gain);
            }   
        }
        if(_key==KeyCode.D){
            if((inertia.x-gain)>(maxSpeed*-1)){
                inertia=new Vector3(inertia.x-gain,inertia.y,inertia.z);
            }
        }
        if(_key==KeyCode.W){
            if((inertia.z-gain)>(maxSpeed*-1)){
                inertia=new Vector3(inertia.x,inertia.y,inertia.z-gain);
            }
        }
        if(_key==KeyCode.A){
            if((inertia.x+gain)<maxSpeed){
                inertia=new Vector3(inertia.x+gain,inertia.y,inertia.z);
            }
        }
        trailingPosition=currentPosition;
    }
    public void setTargetPosition(Vector3 _targetPosition){
        targetPosition=_targetPosition;
        updateArrows();
    }
    public void UpdateMove(){
        
        //currentPosition=currentPosition+inertia;
        currentPosition=real_object.transform.position+inertia; //for physics to take affect
        //Debug.Log("CurrentPosition: "+currentPosition.x.ToString()+" "+currentPosition.z.ToString());
        //currentPosition=world.removeBorder(currentPosition); //old
        world.updateMap(currentPosition);
        inertia=new Vector3(inertia.x*friction,inertia.y*friction,inertia.z*friction);
        real_object.transform.SetPositionAndRotation(currentPosition,new Quaternion(0,0.5f,0,0));
        updateArrows();
    }
}
public class targetManager{
    private Vector3 targetPosition;
    private GameObject targetObject;
    public Vector3 TargetPosition{get{return targetPosition;}}
    public targetManager(GameObject _target){
        targetObject=_target;
    }
    public bool hasCollided(GameObject _object){
        //get Dimensions and position of _object
        Vector3 objectCenter=_object.transform.position;
        Vector3 objectScale=_object.transform.lossyScale;
        Vector3 targetCenter=targetObject.transform.position;
        Vector3 targetScale=targetObject.transform.lossyScale;
        float distance=Vector3.Distance(_object.transform.position,targetObject.transform.position);
        if(distance<(((targetScale.x/2)+(targetScale.y/2)+(targetScale.z/2))/3)+(((objectScale.x/2)+(objectScale.y/2)+(objectScale.z/2))/3)){
            return true;
        }
        else{
            return false;
        }
    }
    public Vector3 respawn_target(GameObject ground){
        Vector3 center=ground.transform.position;
        Vector3 scale=ground.transform.lossyScale;
        targetPosition=new Vector3(Random.Range((center.x-3)*scale.x,(center.x+3)*scale.x),center.y+0.5f,Random.Range((center.z-3)*scale.z,(center.z+3)*scale.z));
        targetObject.transform.SetPositionAndRotation(targetPosition,targetObject.transform.rotation);
        return targetPosition;
    }
}
public class obstacleManager{
    private float globalHeight;
    private List<Vector3> avoidList;
    private List<GameObject> obstacleList;
    private List<GameObject> obstacleCatalog;
    private GameObject getRandomObsticle(){
        return obstacleCatalog[Mathf.RoundToInt(Random.Range(1,obstacleCatalog.Count))-1];
    }
    private bool isInCollisionZone(Vector3 position1, float requiredSpace){
        foreach(Vector3 no in avoidList){
            if(Vector3.Distance(position1,no)<requiredSpace){
                return true;
            }
        }
        return false;
    }
    public obstacleManager(GameObject[] obstacles, float _globalHeight){
        obstacleCatalog=new List<GameObject>();
        foreach(GameObject o in obstacles){
            obstacleCatalog.Add(o);
        }
        avoidList=new List<Vector3>();
        obstacleList=new List<GameObject>();
        globalHeight=_globalHeight;
    }
    public void avoid(Vector3[] positions){
        foreach(Vector3 p in positions){
            avoid(p);
        }
    } 
    public void avoid(Vector3 position){
            avoidList.Add(position);
    } 
    public void ForgetAvoidance(Vector3 position){
        avoidList.Remove(position);
    }
    public void generateObstacles(Vector3 center,float xWidth, float zWidth,int count){
        float pos_x_boundary=(center.x)+(xWidth/2);
        float neg_x_boundary=(center.x)-(xWidth/2);
        float pos_z_boundary=(center.z)+(zWidth/2);
        float neg_z_boundary=(center.z)-(zWidth/2);
        float x=0;
        float z=0;
        for(int i=0;i<count;i++){
            do{
                x=Mathf.Round(Random.Range(neg_x_boundary,pos_z_boundary));
                z=Mathf.Round(Random.Range(neg_z_boundary,pos_z_boundary));
            }while(isInCollisionZone(new Vector3(x,globalHeight,z),10)); //could fail if to many objects are generated
            Vector3 pos=new Vector3(x,globalHeight,z);
            obstacleList.Add(GameObject.Instantiate(getRandomObsticle(),pos,new Quaternion(0,0,0,0)));
        }
    }
}
public class WorldManager{
    private GameObject ground;
    private Vector3[,] groundGridPositions;
    private GameObject[,] playGround;
    private float[] mapVirtualCenter;
    private float[] mapOldVirtualCenter;
    private obstacleManager ObstacleManager;
    public GameObject Ground{get{return ground;}}
    public Vector3 Center{get{return playGround[1,1].transform.position;}}
    public float xWidth{get{return playGround[1,1].transform.lossyScale.x*10;}}
    public float zWidth{get{return playGround[1,1].transform.lossyScale.z*10;}}
    private void degenerateGround(){
        for(int r=0;r<3;r++){
            for(int c=0;c<3;c++){
                GameObject.Destroy(playGround[r,c]);
            }
        }
    }
    private void generateGround(){
        for(int x=0;x<3;x++){
            for(int z=0;z<3;z++){
                GameObject inst=ground;
                inst.name="Tile: x:"+groundGridPositions[x,z].x.ToString()+" z:"+groundGridPositions[x,z].z.ToString();
                playGround[x,z]=GameObject.Instantiate(inst,groundGridPositions[x,z],new Quaternion(0,0,0,0));
            }
        }  
        //ObstacleManager.generateObstacles(this.Center,this.xWidth,this.zWidth,20);
    }
    private void moveMapCenter(float _x_center, float _z_center){
        mapOldVirtualCenter=mapVirtualCenter;
        mapVirtualCenter=new float[]{_x_center,_z_center};
        degenerateGround();
        generateGridPositions(mapVirtualCenter);
        generateGround();
    }
    private void generateGridPositions(float[] _center){
        float zOffset=_center[1]-1;
        float xOffset=_center[0]-1;
        groundGridPositions=new Vector3[3,3];  
        for(int x=0;x<3;x++){
            for(int z=0;z<3;z++){
                groundGridPositions[x,z]=new Vector3((ground.transform.lossyScale.x*10)*(x+xOffset),0,(ground.transform.lossyScale.z*10)*(z+zOffset));
            }
        }
    }
    public WorldManager(GameObject _ground, obstacleManager _ObstacleManager){
        ground=_ground;
        ObstacleManager=_ObstacleManager;
        mapVirtualCenter=new float[]{0,0};
        mapOldVirtualCenter=new float[]{0,0};
        groundGridPositions=new Vector3[3,3];
        playGround=new GameObject[3,3];
        generateGridPositions(mapVirtualCenter);  
        generateGround();
    }
    public void updateMap(Vector3 playerPosition){
        //determ player position within the map
        float player_x=playerPosition.x;
        float player_z=playerPosition.z;
        float pos_x_boundary=(playGround[1,1].transform.position.x)+(playGround[1,1].transform.lossyScale.x*10)/2;
        float neg_x_boundary=(playGround[1,1].transform.position.x)-(playGround[1,1].transform.lossyScale.x*10)/2;
        float pos_z_boundary=(playGround[1,1].transform.position.z)+(playGround[1,1].transform.lossyScale.z*10)/2;
        float neg_z_boundary=(playGround[1,1].transform.position.z)-(playGround[1,1].transform.lossyScale.z*10)/2;
        if(player_x<neg_x_boundary){
            //row 0
            moveMapCenter(mapVirtualCenter[0]-1,mapVirtualCenter[1]);
            //Debug.Log("neg_x_boundary was crossed");
        }
        if(player_x>pos_x_boundary){
            //row 2
            moveMapCenter(mapVirtualCenter[0]+1,mapVirtualCenter[1]);
            //Debug.Log("pos_x_boundary was crossed");
        }
        if(player_z<neg_z_boundary){
            //col 0
            moveMapCenter(mapVirtualCenter[0],mapVirtualCenter[1]-1);
            //Debug.Log("neg_z_boundary was crossed");
        }
        if(player_z>pos_z_boundary){
            //col 2
            moveMapCenter(mapVirtualCenter[0]-1,mapVirtualCenter[1]+1);
            //Debug.Log("pos_z_boundary was crossed");
        }
    }
    public Vector3 removeBorder(Vector3 target_pos){ //old //do not use
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
}

public class GameManager : MonoBehaviour
{
    public GameObject groundPrefab;
    public GameObject L_obstaclePrefab;
    private playerObject Player;
    private WorldManager World;
    private targetManager TargetManager;
    private int lastAction;
    private QAgent ai;
    private Translator translator;
    private obstacleManager ObstacleManager;
    private RoundStats RStats;
    private CSVTable statistics;
    private Text StatsDisplay;
    // Start is called before the first frame update
    void Start()
    {
        //World=new WorldManager(GameObject.Find("ground_obj"));
        StatsDisplay=GameObject.Find("bottom_textfield").GetComponent<Text>();
        ObstacleManager=new obstacleManager(new GameObject[]{L_obstaclePrefab},0.5f);
        World=new WorldManager(
            groundPrefab,
            ObstacleManager);
        TargetManager=new targetManager(GameObject.Find("target_obj"));
        ObstacleManager.avoid(TargetManager.respawn_target(World.Ground));
        Player=new playerObject(
            GameObject.Find("player_obj"),
            World,
            GameObject.Find("targetArrow"),
            GameObject.Find("movementArrow")
        );
        Player.setTargetPosition(TargetManager.TargetPosition);
        ObstacleManager.avoid(Player.CurrentPosition);
        //ObstacleManager.generateObstacles(World.Center,World.xWidth,World.zWidth,20);
        translator=new Translator(Player.DistanceToTarget,Player.ArrowDifference);
        ai=new QAgent();
        lastAction=4;
        statistics=new CSVTable(';');
        RStats=new RoundStats();
        statistics.addLine(RStats.getCSVHeader(),true);
        RStats.startRound(TargetManager.TargetPosition,Player.CurrentPosition);
    }

    // Update is called once per frame
    void Update()
    {
        int state=0;
        if(TargetManager.hasCollided(Player.RealObject)){
            Debug.Log("Target was hit!");
            //state=translator.determState(Player.DistanceToTarget,Player.ArrowDifference,true);
            state=translator.determState2(Player.TargetArrowRotation,true);
            ai.RewardAction(lastAction,state);
            RStats.stopRound();
            statistics.addLine(RStats.getCSVLine());
            Player.setTargetPosition(TargetManager.respawn_target(World.Ground));
            RStats.startRound(TargetManager.TargetPosition,Player.CurrentPosition);
        }
        else{
            //state=translator.determState(Player.DistanceToTarget,Player.ArrowDifference,false);
            state=translator.determState2(Player.TargetArrowRotation,false);
        }
        if(Input.GetKey(KeyCode.W)){
            Player.triggerAction(KeyCode.W);
        }
        if(Input.GetKey(KeyCode.A)){
            Player.triggerAction(KeyCode.A);
        }
        if(Input.GetKey(KeyCode.S)){
            Player.triggerAction(KeyCode.S);
        }
        if(Input.GetKey(KeyCode.D)){
            Player.triggerAction(KeyCode.D);
        }   
        double rew=ai.RewardAction(lastAction,state);
        Debug.Log("Reward: "+rew.ToString()+"State: "+state.ToString());
        //Debug.Log("Target Angle : "+Player.TargetArrowRotation.ToString());
        StatsDisplay.text="Round: "+RStats.RoundNumber.ToString();
        lastAction=ai.TrainAndPredict(state);

        float targetAngle=Player.TargetArrowRotation;
        if(targetAngle>-45 && targetAngle<45){
            Player.triggerAction(KeyCode.S);
            Debug.Log("Input: S");
        }
        if(targetAngle>-135 && targetAngle<-45){
            Player.triggerAction(KeyCode.A);
            Debug.Log("Input: A");
        }
        if((targetAngle>-180 && targetAngle<-135) || (targetAngle>135 && targetAngle<180)){
            Player.triggerAction(KeyCode.W);
            Debug.Log("Input: W");
        }
        if(targetAngle>45 && targetAngle<135){
            Player.triggerAction(KeyCode.D);
            Debug.Log("Input: D");
        }
        
        //Player.triggerAction(translator.ActionToKeycode(lastAction));
        Player.UpdateMove();
        RStats.addWayPoint(Player.CurrentPosition);
        //ai.printQTable();
    }
    void OnApplicationQuit(){
        statistics.saveToFile("Assets/stats/","levelStatistics_.csv");
    }
}

