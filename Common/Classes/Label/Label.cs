using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Label
{
    public string level;
    public string subLevel;
    public int labelIdParent;
    public int labelIdChild;
    public int modelId;
    public int lessonId;
    public string labelName;
    public string audioLabel;
    public string videoLabel;
    public Coordinate coordinates;
    public string labelIndex;
    public string parentId;
}

