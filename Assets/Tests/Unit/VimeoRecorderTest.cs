using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System;
using System.Collections;
using System.Text.RegularExpressions;
using Vimeo.Recorder;

public class VimeoRecorderTest : TestConfig 
{
#if !UNITY_2017_3_OR_NEWER
    [Test]
    public void Throw_Error_If_Old_Unity_Version()
    {
        UnityEngine.TestTools.LogAssert.Expect(LogType.Error, new Regex(" Recording is only avaialabe in 2017.2 or higher"));
        GameObject recorderObj = new GameObject();
        VimeoRecorder r = recorderObj.AddComponent<VimeoRecorder>();
        r.Start();
        UnityEngine.GameObject.DestroyImmediate(recorderObj);
    }
#endif 

}
