using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ModelManager
{
    public static int modelId;

    public static string modelFilepath;

    public static string modelFilename;

    public static string modelExtension;
    
    public static void InitModelData(int _modelId) 
    { 
        modelId = _modelId;
    }
}


