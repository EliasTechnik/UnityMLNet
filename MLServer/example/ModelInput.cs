// This file was auto-generated by ML.NET Model Builder. 

using Microsoft.ML.Data;

namespace MLTestML.Model
{
    public class ModelInput
    {
        [ColumnName("frameNr"), LoadColumn(0)]
        public float FrameNr { get; set; }


        [ColumnName("startPosition.x"), LoadColumn(1)]
        public float StartPosition_x { get; set; }


        [ColumnName("startPosition.y"), LoadColumn(2)]
        public float StartPosition_y { get; set; }


        [ColumnName("endPosition.x"), LoadColumn(3)]
        public float EndPosition_x { get; set; }


        [ColumnName("endPosition.y"), LoadColumn(4)]
        public float EndPosition_y { get; set; }


        [ColumnName("KeysUsed"), LoadColumn(5)]
        public string KeysUsed { get; set; }


        [ColumnName("arrowDirection"), LoadColumn(6)]
        public float ArrowDirection { get; set; }


        [ColumnName("rating"), LoadColumn(7)]
        public float Rating { get; set; }


        [ColumnName("col8"), LoadColumn(8)]
        public float Col8 { get; set; }


    }
}
