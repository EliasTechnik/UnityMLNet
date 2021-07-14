using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ML;
using MLTestML.Model;
using Microsoft.ML.Data;

//structure definition of traindata
public class ModelInput2{
    private float frameNr;
    private float startPosition_x;
    private float startPosition_y;
    private float endPosition_x;
    private float endPosition_y;
    private string keysUsed;
    private float arrowDirection;
    private float rating;
    
    [ColumnName("frameNr"),LoadColumn(0)]
    public float FrameNr { get{return frameNr;}}
    
    [ColumnName("startPosition_x"),LoadColumn(1)]
    public float StartPosition_x { get{return startPosition_x;}}
    
    [ColumnName("startPosition_y"),LoadColumn(2)]
    public float StartPosition_y { get{return startPosition_y;}}
    
    [ColumnName("endPosition_x"),LoadColumn(3)]
    public float EndPosition_x { get{return endPosition_x;}}
    
    [ColumnName("endPosition_y"),LoadColumn(4)]
    public float EndPosition_y { get{return endPosition_y;}}

    [ColumnName("KeysUsed"),LoadColumn(5)]
    public string KeysUsed { get{return keysUsed;}}

    [ColumnName("arrowDirection"),LoadColumn(6)]
    public float ArrowDirection { get{return arrowDirection;}}

    [ColumnName("rating"),LoadColumn(7)]
    public float Rating { get{return rating;}}
    public ModelInput2(CSVLine csv_data){
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
public class ModelOutput2{
    [ColumnName("Action")]
    public string Action;
}
public class AIWrapper2{
    private MLContext mlContext2;
    private PredictionEngine<ModelInput2, ModelOutput2> predictionEngine2;
    private ITransformer trainedModel2;
    private IDataView trainingDataView2;
    private ITransformer predictionPipeline2;
     private DataViewSchema predictionPipelineSchema2;
    private IEstimator<ITransformer> ProcessData(){
        //"frameNr","startPosition_x","startPosition_y","endPosition_x","endPosition_y","KeysUsed","arrowDirection","rating"
        var pipeline = mlContext2.Transforms.Conversion.MapValueToKey(inputColumnName: "KeysUsed", outputColumnName: "Action")
        //.Append(mlContext2.Transforms.Text.FeaturizeText(inputColumnName: "frameNr", outputColumnName: "frameNrFeaturized"))
        //.Append(mlContext2.Transforms.Text.FeaturizeText(inputColumnName: "startPosition_x", outputColumnName: "startPosition_xFeaturized"))
        //.Append(mlContext2.Transforms.Text.FeaturizeText(inputColumnName: "startPosition_y", outputColumnName: "startPosition_yFeaturized"))
        //.Append(mlContext2.Transforms.Text.FeaturizeText(inputColumnName: "endPosition_x", outputColumnName: "endPosition_xFeaturized"))
        //.Append(mlContext2.Transforms.Text.FeaturizeText(inputColumnName: "endPosition_y", outputColumnName: "endPosition_yFeaturized"))
        //.Append(mlContext2.Transforms.Text.FeaturizeText(inputColumnName: "arrowDirection", outputColumnName: "arrowDirectionFeaturized"))
        //.Append(mlContext2.Transforms.Text.FeaturizeText(inputColumnName: "rating", outputColumnName: "ratingFeaturized"))
        /*.Append(mlContext2.Transforms.Concatenate("Features",new[]{
            "frameNrFeaturized",
            "startPosition_xFeaturized",
            "startPosition_yFeaturized",
            "endPosition_xFeaturized",
            "endPosition_yFeaturized",
            "arrowDirectionFeaturized",
            "ratingFeaturized"
        }));*/
        .Append(mlContext2.Transforms.Concatenate("Features",new[]{
            "frameNr",
            "startPosition_x",
            "startPosition_y",
            "endPosition_x",
            "endPosition_y",
            "arrowDirection",
            "rating"
        }));
        //pipeline.AppendCacheCheckpoint(mlContext2); //Verbessert Leistung bei mittleren Datensets //Nicht verwenden bei großen Datensets
        return pipeline;
    }
    private IEstimator<ITransformer> BuildAndTrainModel(IDataView trainingDataView, IEstimator<ITransformer> pipeline){
        var trainingPipeline = pipeline.Append(mlContext2.MulticlassClassification.Trainers.SdcaMaximumEntropy(labelColumnName:"Action",featureColumnName: "Features")).Append(mlContext2.Transforms.Conversion.MapKeyToValue("Action"));
        trainedModel2 = trainingPipeline.Fit(trainingDataView);    
        predictionEngine2 = mlContext2.Model.CreatePredictionEngine<ModelInput2, ModelOutput2>(trainedModel2);
        return trainingPipeline;
    }
    public void Evaluate(DataViewSchema trainingDataViewSchema,string _testPath){
        var testDataView = mlContext2.Data.LoadFromTextFile<ModelInput2>(_testPath,hasHeader: true);
        var testMetrics = mlContext2.MulticlassClassification.Evaluate(trainedModel2.Transform(testDataView),labelColumnName:"Action");
        /*Mikrogenauigkeit: Jedes Beispiel/Klasse-Paar trägt zu gleichen Teilen zur Genauigkeitsmetrik bei. Die Mikrogenauigkeit sollte so nahe wie möglich bei 1 liegen.

        Makrogenauigkeit: Jede Klasse trägt zu gleichen Teilen zur Genauigkeitsmetrik bei. Minderheitsklassen werden gleich wie größere Klassen gewichtet. Die Makrogenauigkeit sollte so nahe wie möglich bei 1 liegen.

        Protokollverlust: Siehe Protokollverlust. Der Protokollverlust sollte so nahe wie möglich bei 0 liegen.

        Verringerung des Protokollverlusts: Dieser liegt zwischen -inf und 1.00, wobei „1.00“ perfekte Vorhersagen und „0“ durchschnittliche Vorhersagen bedeutet. Die Verringerung des Protokollverlusts sollte so nahe wie möglich bei 0 liegen.*/
        Console.WriteLine($"*************************************************************************************************************");
        Console.WriteLine($"*       Metrics for Multi-class Classification model - Test Data     ");
        Console.WriteLine($"*------------------------------------------------------------------------------------------------------------");
        Console.WriteLine($"*       MicroAccuracy: (should be near 1)   {testMetrics.MicroAccuracy:0.###}");
        Console.WriteLine($"*       MacroAccuracy: (should be near 1)   {testMetrics.MacroAccuracy:0.###}");
        Console.WriteLine($"*       LogLoss:       (should be near 0)   {testMetrics.LogLoss:#.###}");
        Console.WriteLine($"*       LogLossReduction: (should be near 0) {testMetrics.LogLossReduction:#.###}");
        Console.WriteLine($"*************************************************************************************************************");

    }
    public AIWrapper2(){
        mlContext2 = new MLContext(seed: 1);//Create MLContext2 //seed:0
    }
    public void loadModel(string path){
        predictionPipeline2 = mlContext2.Model.Load(path, out predictionPipelineSchema2);
        predictionEngine2 = mlContext2.Model.CreatePredictionEngine<ModelInput2, ModelOutput2>(predictionPipeline2);
    }
    public ModelOutput2 predict(ModelInput2 data){
        if(predictionEngine2!=null){
            return predictionEngine2.Predict(data);
        }
        else{
            return null;
        }
    }
    public string trainNewModel(string _dataPath,string saveTo){
        Console.WriteLine("AI2: prepairing for training.");
        trainingDataView2=mlContext2.Data.LoadFromTextFile<ModelInput2>(_dataPath,',',hasHeader: true);
        Console.WriteLine("AI2: Data loaded.");
        var pipeline=ProcessData();
        Console.WriteLine("AI2: Pipeline created.");
        var trainingPipeline = BuildAndTrainModel(trainingDataView2, pipeline);
        Console.WriteLine("AI2: Model trained.");
        //Evaluate(trainingDataView2.Schema,_dataPath);//Evaluate(trainingDataView.Schema,_testPath); //nicht unbedingt notwendig
        mlContext2.Model.Save(trainedModel2, predictionPipelineSchema2, saveTo);
        Console.WriteLine("AI2: model saved.");
        return saveTo;
    }
}