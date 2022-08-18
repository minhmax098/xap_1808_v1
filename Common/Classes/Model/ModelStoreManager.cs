using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public static class ModelStoreManager
{
    public static int modelId; 
    // public static string filePath;
    public static string modelFile;
    public static string modelName;
    public static void InitModelStore(int _modelId, string _modelName, string _modelFile = null)
    {
        modelId = _modelId;
        modelName = _modelName;
        // if (_filePath != null)
        // {
        //     filePath = _filePath;
        // }
        if (_modelFile != null)
        {
            modelFile = _modelFile;
        }
    }
}
