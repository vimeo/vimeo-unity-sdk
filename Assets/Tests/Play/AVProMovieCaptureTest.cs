using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System;
using System.Collections;
using System.Text.RegularExpressions;
using Vimeo;
using Vimeo.Recorder;

#if VIMEO_AVPRO_CAPTURE_SUPPORT
using RenderHeads.Media.AVProMovieCapture;
#endif

public class AVProMovieCaptureTest : TestConfig
{
#if VIMEO_AVPRO_CAPTURE_SUPPORT

    GameObject camObj;
    GameObject light;
    GameObject recorderObj;
    GameObject screenObj;
    VimeoRecorder recorder;

    bool uploaded;

    RenderHeads.Media.AVProMovieCapture.CaptureFromScreen movieCapture;
    
    [SetUp]
    public void _Before()
    {
        // Camera setup
        camObj = new GameObject("Camera");
        camObj.AddComponent<Camera>();
        camObj.transform.Translate(0, 0, -3);

        // Light setup
        light = new GameObject("Light");
        Light l = light.AddComponent<Light>();
        l.type = LightType.Directional;
        
        // Recorder Setup
        recorderObj = new GameObject("Video Recorder");
        recorder = recorderObj.AddComponent<VimeoRecorder>();
        recorder.OnUploadComplete += OnUploadComplete;
        recorder.videoName = "AVPro Movie Capture (" + Application.platform + " " + Application.unityVersion + ")";
        recorder.privacyMode = VimeoApi.PrivacyModeDisplay.OnlyPeopleWithPrivateLink;
        recorder.encoderType = EncoderType.AVProMovieCapture;

        // AVPro setup
        movieCapture = recorderObj.AddComponent<RenderHeads.Media.AVProMovieCapture.CaptureFromScreen>();
        movieCapture._stopMode = StopMode.SecondsEncoded;
        movieCapture._stopSeconds = 10;
        movieCapture._useMediaFoundationH264 = true;

        recorder.avproEncoder = movieCapture;

        // Screen setup
        screenObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        screenObj.AddComponent<ObjectRotation>();

        camObj.transform.LookAt(screenObj.transform);

        uploaded = false;
    }

    [UnityTest]
    [Timeout(30000)]
    public IEnumerator Can_Record_And_Upload() 
    {    
        recorder.SignIn(VALID_RECORDING_TOKEN);
        movieCapture.StartCapture();
        yield return new WaitUntil(()=> uploaded == true);
    }

    public void OnUploadComplete()
    {
        uploaded = true;
        Assert.IsTrue(true);
    }

    [TearDown]
    public void _After()
    {
        UnityEngine.GameObject.DestroyImmediate(camObj);
        UnityEngine.GameObject.DestroyImmediate(light);
        UnityEngine.GameObject.DestroyImmediate(screenObj);
        UnityEngine.GameObject.DestroyImmediate(recorderObj);
    }
#endif 
}
