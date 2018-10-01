using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Text.RegularExpressions;
using Vimeo;
using Vimeo.Recorder;

public class VimeoRecorderPlayTest : TestConfig
{
#if UNITY_2017_3_OR_NEWER 
    GameObject cube;
    GameObject light;
    GameObject camObj;
    GameObject recorderObj;

    VimeoRecorder recorder;

    bool uploaded;
    bool error;

    float timeout = 30;
    float elapsed = 0;

    [SetUp]
    public void _Before()
    {
        // Setup cube
        cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.AddComponent<ObjectRotation>();

        // Camera setup
        camObj = new GameObject("Camera");
        camObj.AddComponent<Camera>();
        camObj.tag = "MainCamera";
        camObj.transform.Translate(0, 0, -3);
        camObj.transform.LookAt(cube.transform);

        // Light setup
        light = new GameObject("Light");
        Light l = light.AddComponent<Light>();
        l.type = LightType.Directional;

        // Recorder setup
        recorderObj = new GameObject("Recorder");
        recorder = recorderObj.AddComponent<VimeoRecorder>();
        recorder.encoderType       = EncoderType.MediaEncoder;
        recorder.defaultResolution = Vimeo.Recorder.Resolution.x540p;
        recorder.realTime          = true;
        recorder.recordMode        = RecordMode.Duration;
        recorder.recordDuration    = 5;
        recorder.privacyMode       = VimeoApi.PrivacyModeDisplay.OnlyPeopleWithPrivateLink;
        recorder.openInBrowser     = false;

        System.DateTime dt = System.DateTime.Now;
        recorder.videoName = "(Unity " + Application.unityVersion + ")";

        uploaded = false;
    }

    [UnityTest]
    public IEnumerator Can_Record_Video_From_Screen_With_Valid_Token() 
    {    
        recorder.videoName = "Screen Test " + recorder.videoName;
        recorder.defaultVideoInput = VideoInputType.Screen;
        recorder.SignIn(VALID_RECORDING_TOKEN);
        recorder.BeginRecording();

        recorder.OnUploadComplete += UploadComplete;

        while (!uploaded) {
            yield return new WaitForSeconds(.25f);
            TimeoutCheck();
        }
    }

    [UnityTest]
    public IEnumerator Can_Record_Video_From_MainCamera_With_Valid_Token() 
    {    
        recorder.videoName = "MainCamera Test " + recorder.videoName;
        recorder.defaultVideoInput = VideoInputType.Camera;
        recorder.SignIn(VALID_RECORDING_TOKEN);
        recorder.BeginRecording();

        recorder.OnUploadComplete += UploadComplete;

        while (!uploaded) {
            yield return new WaitForSeconds(.25f);
            TimeoutCheck();
        }
    }    

    private void TimeoutCheck(string msg = "Test timed out")
    {
        elapsed += .25f;
        if (elapsed >= timeout) {
            recorder.CancelRecording();
            Assert.Fail(msg);
        }
    }

    private void UploadComplete()
    {
        uploaded = true;
    }

    [TearDown]
    public void _After()
    {
        uploaded = false;
        UnityEngine.GameObject.DestroyImmediate(camObj);
        UnityEngine.GameObject.DestroyImmediate(light);
        UnityEngine.GameObject.DestroyImmediate(cube);
        UnityEngine.GameObject.DestroyImmediate(recorderObj);
    }
#endif     
}
