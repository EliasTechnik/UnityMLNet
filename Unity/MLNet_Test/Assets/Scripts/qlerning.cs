using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using static System.Environment;


public class QLearningStats{
  public int InitialState { get; set; }
  public int EndState { get; set; }
  public int Steps { get; set; }
  public int[] Actions { get; set; }

  public override string ToString()
  {
    StringBuilder sb = new StringBuilder();
    sb.AppendLine($"Agent needed {Steps} steps to find the solution");
    sb.AppendLine($"Agent Initial State: {InitialState}");
    foreach (var action in Actions)
       sb.AppendLine($"Action: {action}");
    sb.AppendLine($"Agent arrived at the goal state: {EndState}");
    return sb.ToString();
  }
}
public class Translator{//determs state and translate action
    private float lastDistance;
    private float lastAngleDiff;
    public KeyCode ActionToKeycode(int action){
        switch(action){
            case 0: Debug.Log("AI: W"); return KeyCode.W; 
            case 1: Debug.Log("AI: A"); return KeyCode.A; 
            case 2: Debug.Log("AI: S"); return KeyCode.S; 
            case 3: Debug.Log("AI: D"); return KeyCode.D; 
            case 4: Debug.Log("AI: SPACE"); return KeyCode.Space; //wait    
            default: return KeyCode.Space;
        }
    }
    public int determState(float currentDistance,float currentAngleDiff,bool targetReaced){
        int state=4;
        if(currentDistance<lastDistance && currentAngleDiff<lastAngleDiff){
            state=1;
        }
        if(currentDistance<lastDistance && currentAngleDiff>lastAngleDiff){
            state=2;
        }
        if(currentDistance>lastDistance && currentAngleDiff<lastAngleDiff){
            state=3;
        }
        if(currentDistance>lastDistance && currentAngleDiff>lastAngleDiff){
            state=4;
        }
        if(targetReaced){
            state=0;
        }
        lastDistance=currentDistance;
        lastAngleDiff=currentAngleDiff;
        return state;
    }
    public int determState2(float targetAngel, bool targetReaced){
        int state=4;
        if(targetAngel>-45 && targetAngel<45){
            state=3;
        }
        if(targetAngel>-135 && targetAngel<-45){
            state=2;
        }
        if((targetAngel>-180 && targetAngel<-135) || (targetAngel>135 && targetAngel<180)){
            state=1;
        }
        if(targetAngel>45 && targetAngel<135){
            state=4;
        }
        if(targetReaced){
            state=0;
        }
        return state;
    }
    public Translator(float currentDistance,float currentAngleDiff){
        lastDistance=currentDistance;
        lastAngleDiff=currentAngleDiff;
    }

}
public class QAgent{
    //https://code-ai.mk/how-to-implement-q-learning-algorithm-in-c/
    private float explorationThreshold=0.5f; //how explorative should the agent be
    private double gamma=0.89; //MaxRewardDiscount
    private double[][] rewards = new double[5][]{
        //Actions      W-z     A+x    S+z     D-x   Wait
        new double[]{  100,   100,    100,    100,  100},//0 target reached 100
        new double[]{    1,   0.5,    0.5,       0,   0.1},//1 W: -45..+45 // target gets closer and angle diverence gets closer
        new double[]{  0.5,   1,    0.5,    0,  0.1},//2 A: -135..-45 //target gets closer but angle diverence gets greater
        new double[]{  0,   0.5,    1,    0.5,  0.1},//3 S: S: -180..-135 && 135..180 // target gets further away but angle diverence gets closer
        new double[]{  0.5,    0,    0.5,    1,  -1} //4 D: 45..135 //target gets further away and angle diverence gets greater
    };
    //Angles:
    /*
    W: -45..+45
    A: -135..-45
    S: -180..-135 && 135..180
    D: 45..135

    */
    private double[][] QTable;
   public double GetReward(int currentState, int action){
        return rewards[currentState][action];
   }  
   public int[] GetValidActions(int currentState){
        List<int> validActions = new List<int>();
        for (int i = 0; i < rewards[currentState].Length; i++)
        {
            if (rewards[currentState][i] != -1)
            validActions.Add(i); //nur valide Aktionen zurückgeben
        }
        return validActions.ToArray();
    }
    public bool GoalStateIsReached(int currentState){
        return currentState == 0;
    }
    public int TrainAndPredict(int state){
        int[] actions=GetValidActions(state);
        //find best value
        double bestValue=-1; //change
        int bestAction=-100;
        foreach(int action in actions){
            if(GetReward(state,action)>bestValue){
                bestValue=GetReward(state,action);
                bestAction=action;
            }
        }
        if(bestAction==-100){
            //no best action found
            bestAction=actions[Mathf.RoundToInt(Random.Range(0,actions.Length))]; //get a random action
        }
        if(Random.Range(0,1)>explorationThreshold){
            //explore
            bestAction=actions[Mathf.RoundToInt(Random.Range(0,actions.Length))]; //get a random action
        }
        return bestAction;
    }
    public double RewardAction(int action,int currentState){
        double saReward = GetReward(currentState, action);
        double nsReward=0; //Highest future revard
        foreach(double rew in QTable[action]){
            if(rew>nsReward){
                nsReward=rew;
            }
        }        
        double qCurrentState = saReward + (gamma * nsReward);
        QTable[currentState][action] = qCurrentState;
        return saReward;
    }
    public QAgent(){
        //init Q
        QTable=new double[5][];
        for(int i=0;i<5;i++){
            double[] row=new double[5];
            for(int j=0;j<5;j++){
                row[j]=0;
            }
            QTable[i]=row;
        } 
    }
    public void printQTable(){
        string output="";
        foreach(double[] rew in QTable){
            foreach(double cell in rew){
                output+=cell.ToString()+" ";
            }
            output+=NewLine;
        }   
        Debug.Log(output);
    }
} 