using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Threading;

public class Movement{
    private Vector3 startPosition;
    private Vector3 endPosition;
    private long frameNr;
    private List<KeyCode> keysUsed; 
    private float travelingDistance;
    private int rating;
    private float arrowDirection;
    public float ArrowDirection{get{return arrowDirection;}set{arrowDirection=value;}}
    public int UsedKeysCount{get{return keysUsed.Count;}}
    public float TravelingDistance{get{return travelingDistance;}}
    public Vector3 StartPosition{get{return startPosition;}}
    public Vector3 EndPosition{get{return endPosition;}}
    public int Rating{get{return rating;}set{rating=value;}}
    public Movement(Vector3 _startPosition,long _frameNr){
        startPosition=_startPosition;
        frameNr=_frameNr;
        keysUsed=new List<KeyCode>();
        travelingDistance=0;
    }
    public void setEndPosition(Vector3 _endPosition,Vector3 _targetPosition){
        endPosition=_endPosition;
        travelingDistance=Vector3.Distance(startPosition,endPosition);
        float firstDistanceToTarget=Vector3.Distance(_targetPosition,startPosition);
        float secondDistanceToTarget=Vector3.Distance(_targetPosition,endPosition);
        if(firstDistanceToTarget>secondDistanceToTarget){
            rating=1;
        }
        else{
            rating=0;
        }
    }
    public void addUsedKeys(KeyCode k){
        keysUsed.Add(k);
    }
    public XMLobject toXML(){
        XMLobject xo=new XMLobject("movement");
        xo.addAttribute("frame",frameNr.ToString());
        xo.addChild(new XMLobject("startPosition",new XMLobject[] {
            new XMLobject("x",startPosition.x.ToString()),
            //new XMLobject("y",startPosition.y.ToString()),
            new XMLobject("z",startPosition.z.ToString())
        }));
        xo.addChild(new XMLobject("endPosition",new XMLobject[] {
            new XMLobject("x",endPosition.x.ToString()),
            //new XMLobject("y",endPosition.y.ToString()),
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
        //xo.addChild(new XMLobject("travelingDistance",travelingDistance.ToString()));
        xo.addChild(new XMLobject("rating",rating.ToString()));
        xo.addChild(new XMLobject("arrowDirection",arrowDirection.ToString()));
        return xo;
    }
    public string toXMLstring(){
        return this.toXML().serialize();
    }
    public CSVLine toCSVLine(){
        CSVLine l=new CSVLine(new string[] {
            frameNr.ToString(),
            startPosition.x.ToString(),
            startPosition.z.ToString(),
            endPosition.x.ToString(),
            endPosition.z.ToString()
        });
        string k="";
        foreach(KeyCode kc in keysUsed){
            k+=kc.ToString();
        }
       // k=""; //provisorisch
        l.addCell(new CSVCell(k));
        l.addCell(new CSVCell(arrowDirection.ToString()));
        l.addCell(new CSVCell(rating.ToString()));
        return l;
    }
}
public class Score{
    static int scoreIndex;
    private float scorePoints;
    private float scoreTime;
    private float timerStartTime;
    private float targetDistance;
    private Vector3 targetPosition;
    private List<Movement> actionlist; //Moves that that happend during the Score
    private int scoreId;
    public int ScoreId{get{return scoreId;}}
    public float ScorePoints{get{return scorePoints;}}
    public float ScoreTime{get{return scoreTime;}}
    public Score(){
        scorePoints=0;
        scoreTime=0;
        targetDistance=0;
        actionlist=new List<Movement>();
        scoreId=scoreIndex;
        scoreIndex++;
    }
    public float startTimer(Vector3 _targetposition, Vector3 _currentPosition){
        timerStartTime=Time.realtimeSinceStartup;
        targetDistance=Vector3.Distance(_targetposition,_currentPosition);
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
        if(Vector3.Distance(m.StartPosition,targetPosition)>Vector3.Distance(m.EndPosition,targetPosition)){
            m.Rating=1;
        }
        else{
            m.Rating=0;
        }
        actionlist.Add(m);
    }
    public XMLobject toXML(){
        XMLobject xo=new XMLobject("Score",new XMLobject[]{
            new XMLobject("scorePoints",scorePoints.ToString()),
            new XMLobject("scoreTime",scoreTime.ToString()),
            new XMLobject("targetDistance",targetDistance.ToString())
        });
        foreach(Movement m in actionlist){
            if(m.UsedKeysCount>0){
               xo.addChild(m.toXML()); 
            }
        }
        xo.addAttribute("index",scoreId.ToString());
        return xo;
    }
    public string toXMLstring(){
        return this.toXML().serialize();
    }
    public CSVTable getCSVTable(){
        CSVLine heading=new CSVLine(new string[]{"frameNr","startPosition_x","startPosition_y","endPosition_x","endPosition_y","KeysUsed","arrowDirection","rating"});
        CSVTable t=new CSVTable();
        t.addLine(heading,true);
        foreach(Movement m in actionlist){
            t.addLine(m.toCSVLine());
        }
        return t;
    }
}
public class ScoreManager{
    private List<Score> scorelist;
    private float averageScore;
    private float averageTime;
    private string savepath;
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
    private void SaveLvlThread(){
        Score s=scorelist[scorelist.Count-1];
        StreamWriter file=new StreamWriter(savepath+"leveldata_"+s.ScoreId.ToString()+".xml");
        file.WriteLine(s.toXMLstring());
        //file.WriteAsync(s.toXMLstring());
        file.Close();
        Debug.Log("Saved current lvl recording to "+savepath+"leveldata_"+s.ScoreId.ToString()+".xml");
    }
    public void AddScore(Score s){
        scorelist.Add(s);
        updateAverage();
    }
    public void saveLastRound(string path){
        //savepath=path;
        //Thread t =new Thread(new ThreadStart(SaveLvlThread));
        //t.Start();
        Score s=scorelist[scorelist.Count-1];
        s.getCSVTable().saveToFile(path,"leveldata_"+s.ScoreId.ToString()+".csv");
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
