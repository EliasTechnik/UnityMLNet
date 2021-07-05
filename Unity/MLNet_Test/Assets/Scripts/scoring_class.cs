using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement{
    private Vector3 startPosition;
    private Vector3 endPosition;
    private long frameNr;
    private List<KeyCode> keysUsed; 
    private float travelingDistance;
    public Movement(Vector3 _startPosition,long _frameNr){
        startPosition=_startPosition;
        frameNr=_frameNr;
        keysUsed=new List<KeyCode>();
        travelingDistance=0;
    }
    public void setEndPosition(Vector3 _endPosition){
        endPosition=_endPosition;
        travelingDistance=Vector3.Distance(startPosition,endPosition);
    }
    public void addUsedKeys(KeyCode k){
        keysUsed.Add(k);
    }
    public XMLobject toXML(){
        XMLobject xo=new XMLobject("movement");
        xo.addAttribute("frame",frameNr.ToString());
        xo.addChild(new XMLobject("startPosition",new XMLobject[] {
            new XMLobject("x",startPosition.x.ToString()),
            new XMLobject("y",startPosition.y.ToString()),
            new XMLobject("z",startPosition.z.ToString())
        }));
        xo.addChild(new XMLobject("endPosition",new XMLobject[] {
            new XMLobject("x",endPosition.x.ToString()),
            new XMLobject("y",endPosition.y.ToString()),
            new XMLobject("z",endPosition.z.ToString())
        }));
        XMLobject ku=new XMLobject("keysused");
        int i=0;
        foreach(KeyCode k in keysUsed){
            XMLobject kc=new XMLobject("keycode",k.ToString());
            kc.addAttribute("index",i.ToString());
            ku.addChild(kc);
            i++;
        }
        xo.addChild(ku);
        xo.addChild(new XMLobject("travelingDistance",travelingDistance.ToString()));
        return xo;
    }
    public string toXMLstring(){
        return this.toXML().serialize();
    }
}
public class Score{
    private float scorePoints;
    private float scoreTime;
    private float timerStartTime;
    private float targetDistance;
    private List<Movement> actionlist; //Moves that that happend during the Score
    public float ScorePoints{get{return scorePoints;}}
    public float ScoreTime{get{return scoreTime;}}
    public Score(){
        scorePoints=0;
        scoreTime=0;
        targetDistance=0;
        actionlist=new List<Movement>();
    }
    public float startTimer(float _distanceToTarget){
        timerStartTime=Time.realtimeSinceStartup;
        targetDistance=_distanceToTarget;
        return timerStartTime;
    }
    public float stopTimer(bool _targetReached){
       scoreTime=Time.realtimeSinceStartup-timerStartTime;
       if(_targetReached){
           scorePoints=targetDistance/scoreTime;
       }
       else{
           scorePoints=0;
       } 
       return scorePoints;
    }
    public void addMovement(Movement m){
        actionlist.Add(m);
    }
}
public class ScoreManager{
    private List<Score> scorelist;
    private float averageScore;
    private float averageTime;
    public float AverageScore{get{return averageScore;}}
    public float AverageTime{get{return averageTime;}}
    public ScoreManager(){
        scorelist=new List<Score>();
        averageScore=0;
        averageTime=0;
    }
    private void updateAverage(){
        float scoresum=0;
        float timesum=0;
        foreach(Score s in scorelist){
            scoresum+=s.ScorePoints;
            timesum+=s.ScoreTime;
        }
        averageScore=scoresum/scorelist.Count;
        averageTime=timesum/scorelist.Count;
    }
    public void AddScore(Score s){
        scorelist.Add(s);
        updateAverage();
    }

}
public class scoring_class : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
