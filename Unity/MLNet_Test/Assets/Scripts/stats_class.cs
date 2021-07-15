using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoundStats{
    static int roundCount;
    private int roundNumber;
    public int RoundNumber{get{return roundNumber;}}
    private float RoundTime;
    private float shortestPath;
    public float ShortestPath{get{return shortestPath;}}
    private Vector3 lastPosition;
    private float pathLength;
    public float PathLength{get{return pathLength;}}
    private float score;
    public float Score{get{return score;}}
    private float timerStartTime;
    private float averageSpeed;
    public float AverageSpeed{get{return averageSpeed;}}
    public RoundStats(){}
    public float startRound(Vector3 _targetposition, Vector3 _currentPosition){
        roundNumber++;
        score=0;
        RoundTime=0;
        averageSpeed=0;
        timerStartTime=Time.realtimeSinceStartup;
        shortestPath=Vector3.Distance(_targetposition,_currentPosition);
        lastPosition=_currentPosition;
        pathLength=0;
        return timerStartTime;
    }
    public float stopRound(){
       RoundTime=Time.realtimeSinceStartup-timerStartTime;
       score=shortestPath/RoundTime;
       averageSpeed=pathLength/RoundTime;
       return score;
    }
    public void addWayPoint(Vector3 _currentPosition){ //maybe add current speed feature
        pathLength=pathLength+Vector3.Distance(_currentPosition,lastPosition);
        lastPosition=_currentPosition;
    }
    public CSVLine getCSVHeader(){
        return new CSVLine(new string[]{
            "roundNumber",
            "score",
            "shortestPath",
            "travelingDistance",
            "averageSpeed"});
    }
    public CSVLine getCSVLine(){
        return new CSVLine(new string[]{
            roundNumber.ToString(),
            score.ToString(),
            shortestPath.ToString(),
            pathLength.ToString(),
            averageSpeed.ToString()});
    }
}