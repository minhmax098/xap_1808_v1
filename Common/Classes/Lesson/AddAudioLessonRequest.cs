using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio; 
using UnityEngine.Video;
using UnityEngine.UI;
using UnityEngine.Networking;

[Serializable]
public class AddAudioLessonRequest
{
    public int lessonId;
    public byte[] audio;
}
