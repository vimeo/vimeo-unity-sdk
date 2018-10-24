using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System;
using System.Collections;
using System.Text.RegularExpressions;
using Vimeo;
using Vimeo.Recorder;

public class VimeoRecorderTest : TestConfig 
{
    GameObject recorderObj;

    VimeoRecorder recorder;

    [SetUp]
    public void _Before()
    {
        // Recorder setup
        recorderObj = new GameObject("Recorder");
        recorder = recorderObj.AddComponent<VimeoRecorder>();
        recorder.encoderType       = EncoderType.MediaEncoder;
        recorder.defaultResolution = Vimeo.Recorder.Resolution.x540p;
        recorder.realTime          = true;
        recorder.recordMode        = RecordMode.Duration;
        recorder.recordDuration    = 5;

        System.DateTime dt = System.DateTime.Now;
        recorder.videoName = "(Unity " + Application.unityVersion + ")";

#if !UNITY_2017_3_OR_NEWER        
        UnityEngine.TestTools.LogAssert.Expect(LogType.Error, new Regex("Recording is only avaialabe in 2017.2 or higher"));
#endif

        recorder.Start();
    }

    [Test]

    public void Test_Auto_Upload_Default_Value()
    {
        Assert.IsTrue(recorder.autoUpload, "Auto upload default value is set to true");
    }

    [Test]
    public void Test_Open_In_Browser_Default_Value()
    {
        Assert.IsTrue(recorder.openInBrowser, "Open in browser default value is set to true");
    }

    [Test]
    public void Test_Enable_Downloads_Default_Value()
    {
        Assert.IsTrue(recorder.enableDownloads, "Enable downloads default value is set to true");
    }

    [Test]
    public void Test_Enable_Review_Page_Default_Value()
    {
        Assert.IsTrue(recorder.enableReviewPage, "Enable reivew page default value is set to true");
    }

    [Test]
    public void Test_Enable_Comment_Mode_Default_Value()
    {
        Assert.IsTrue(recorder.commentMode == VimeoApi.CommentMode.Anyone, "Enable reivew page default value is set to true");
    }

    [Test]
    public void Test_Enable_Privacy_Mode_Default_Value()
    {
        Assert.IsTrue(recorder.privacyMode == VimeoApi.PrivacyModeDisplay.Anyone, "Enable reivew page default value is set to true");
    }

    [Test]
    public void Can_Record_Video_Without_Uploading()
    {
        recorder.SignIn(VALID_RECORDING_TOKEN);
        recorder.autoUpload = false;
        recorder.BeginRecording();
        Assert.IsTrue(recorder.isRecording, "Recording state set to true while recording");     
        recorder.EndRecording();
        Assert.IsFalse(recorder.isUploading, "Recording state set to true while recording");
        Assert.IsTrue(recorder.encoder.GetVideoFilePath() != "", "A video file path exists");

    }

    [Test]
    public void Recording_State_True_When_Recording()
    {
        recorder.SignIn(VALID_RECORDING_TOKEN);
        recorder.autoUpload = false;
        Assert.IsFalse(recorder.isRecording, "Recording state set to false before recording");
        recorder.BeginRecording();
        Assert.IsTrue(recorder.isRecording, "Recording state set to true while recording");
        recorder.EndRecording();
        Assert.IsFalse(recorder.isRecording, "Recording state set to false after recording");
    }

    [TearDown]
    public void _After()
    {
        UnityEngine.GameObject.DestroyImmediate(recorderObj);
    }

}
