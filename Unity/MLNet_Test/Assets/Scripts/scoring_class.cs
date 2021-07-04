using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Score{
    private float scorePoints;
    private float scoreTime;
    private float timerStartTime;
    private float targetDistance;
    public float ScorePoints{get{return scorePoints;}}
    public float ScoreTime{get{return scoreTime;}}
    public Score(){
        scorePoints=0;
        scoreTime=0;
        targetDistance=0;
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
