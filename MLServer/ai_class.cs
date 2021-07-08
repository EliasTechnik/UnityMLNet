using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ML;
using MLTestML.Model;
using Microsoft.ML.Data;

//structure definition of traindata
public class ModelInput{
    private float frameNr;
    private float startPosition_x;
    private float startPosition_y;
    private float endPosition_x;
    private float endPosition_y;
    private string keysUsed;
    private float arrowDirection;
    private float rating;
    
    [ColumnName("frameNr")]
    public float FrameNr { get{return frameNr;}}
    
    [ColumnName("startPosition.x")]
    public float StartPosition_x { get{return startPosition_x;}}
    
    [ColumnName("startPosition.y")]
    public float StartPosition_y { get{return startPosition_y;}}
    
    [ColumnName("endPosition.x")]
    public float EndPosition_x { get{return endPosition_x;}}
    
    [ColumnName("endPosition.y")]
    public float EndPosition_y { get{return endPosition_y;}}

    [ColumnName("KeysUsed")]
    public string KeysUsed { get{return keysUsed;}}

    [ColumnName("arrowDirection")]
    public float ArrowDirection { get{return arrowDirection;}}

    [ColumnName("rating")]
    public float Rating { get{return rating;}}

    [ColumnName("col8")]
    public float Col8 { get; set; }
    public ModelInput(CSVLine csv_data){
        frameNr=float.Parse(csv_data[0].Content);
        startPosition_x=float.Parse(csv_data[1].Content);
        startPosition_y=float.Parse(csv_data[2].Content);
        endPosition_x=float.Parse(csv_data[3].Content);
        endPosition_y=float.Parse(csv_data[4].Content);
        keysUsed=csv_data[5].Content;
        arrowDirection=float.Parse(csv_data[6].Content);
        rating=float.Parse(csv_data[7].Content);
    }
}
public class ModelOutput{
    private string prediction;
    private float[] score;
    public string Prediction{get{return prediction;}set{prediction=value;}}
    public float[] Score { get{return score;} set{score=value;}}
    public ModelOutput(){}
}
public class AIWrapper{
    private MLContext mlContext;
    private DataViewSchema predictionPipelineSchema;
    private ITransformer predictionPipeline;
    private PredictionEngine<ModelInput, ModelOutput> predictionEngine;
    public AIWrapper(){
        mlContext = new MLContext();//Create MLContext
    }
    public void loadModel(string path){
        //Load trained model
        predictionPipeline = mlContext.Model.Load(path, out predictionPipelineSchema);
        // Create PredictionEngines
        predictionEngine = mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(predictionPipeline);
    }
    public ModelOutput predict(ModelInput data){
        return predictionEngine.Predict(data);
    }

}