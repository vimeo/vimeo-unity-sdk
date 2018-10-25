using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Text.RegularExpressions;
using Vimeo;
using Vimeo.Recorder;
using Vimeo.SimpleJSON;

public class VimeoRecorderPlayTest : TestConfig
{
#if UNITY_2017_3_OR_NEWER 
    GameObject cube;
    GameObject light;
    GameObject camObj;
    GameObject recorderObj;

    VimeoRecorder recorder;

    bool uploaded;
    bool finished;
    bool error;

    string version;

    [SetUp]
    public void _Before()
    {
        version = "(" + Application.platform + " " + Application.unityVersion + ")";
        
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
        recorder.videoName = version;

        uploaded = false;
        finished = false;
    }

    [UnityTest]
    [Timeout(30000)]
    public IEnumerator Can_Record_Video_From_Screen_With_Valid_Token() 
    {    
        UnityEngine.TestTools.LogAssert.NoUnexpectedReceived();
        
        recorder.videoName = "Screen Test " + recorder.videoName;
        recorder.defaultVideoInput = VideoInputType.Screen;
        recorder.SignIn(VALID_RECORDING_TOKEN);
        recorder.BeginRecording();

        recorder.OnUploadComplete += UploadComplete;

        yield return new WaitUntil(()=> uploaded == true);
        Assert.IsTrue(uploaded);
    }

    [UnityTest]
    [Timeout(30000)]
    public IEnumerator Can_Record_Video_From_MainCamera_With_Valid_Token() 
    {    
        UnityEngine.TestTools.LogAssert.NoUnexpectedReceived();

        recorder.videoName = "MainCamera Test " + recorder.videoName;
        recorder.defaultVideoInput = VideoInputType.Camera;
        recorder.SignIn(VALID_RECORDING_TOKEN);
        recorder.BeginRecording();

        recorder.OnUploadComplete += UploadComplete;

        yield return new WaitUntil(()=> uploaded == true);
        Assert.IsTrue(uploaded);
    }

    [UnityTest]
    [Timeout(100000)]
    public IEnumerator Can_Record_And_Upload_Multiple_Chunks()
    {
        UnityEngine.TestTools.LogAssert.NoUnexpectedReceived();

        recorder.videoName = "Multi Chunk Test " + recorder.videoName;
        recorder.defaultVideoInput = VideoInputType.Camera;
        recorder.byteChunkSize = 50000;
        recorder.SignIn(VALID_RECORDING_TOKEN);
        recorder.BeginRecording();

        recorder.OnUploadComplete += UploadComplete;

        yield return new WaitUntil(()=> uploaded == true);
        Assert.IsTrue(uploaded);
    }

    private void UploadComplete()
    {
        uploaded = true;
    }

    [UnityTest]
    [Timeout(30000)]
    public IEnumerator Uploads_Video_And_Adds_Video_To_Project() 
    {    
        UnityEngine.TestTools.LogAssert.NoUnexpectedReceived();

        recorder.currentFolder = new VimeoFolder("Unity Tests", TEST_PROJECT_FOLDER);
        recorder.videoName = "Folder Test " + recorder.videoName;
        recorder.defaultVideoInput = VideoInputType.Camera;
        recorder.SignIn(VALID_RECORDING_TOKEN);
        recorder.BeginRecording();

        recorder.OnUploadComplete += FolderCheckUploadComplete;

        yield return new WaitUntil(()=> finished == true);
    }    

    private void FolderCheckUploadComplete()
    {
        VimeoApi api = recorderObj.AddComponent<VimeoApi>();
        api.token = VALID_RECORDING_TOKEN;
        api.OnRequestComplete += GetFoldersComplete;
        api.GetVideosInFolder(new VimeoFolder("Unity Tests", TEST_PROJECT_FOLDER));
    }

    private void GetFoldersComplete(string resp)
    {
        JSONNode json = JSON.Parse(resp);
        Assert.AreEqual(recorder.publisher.video.uri, json["data"][0]["uri"].Value);
        finished = true;
    }

    [UnityTest]
    [Timeout(30000)]
    public IEnumerator Multiple_Uploads_Work()
    {
        UnityEngine.TestTools.LogAssert.NoUnexpectedReceived();
        recorder.videoName = "Multi Upload Test #1 " + version;
        recorder.OnUploadComplete += FirstUploadComplete;
        recorder.SignIn(VALID_RECORDING_TOKEN);
        recorder.BeginRecording();

        yield return new WaitUntil(()=> finished == true);
    }

    private void FirstUploadComplete()
    {
        recorder.OnUploadComplete -= FirstUploadComplete;
        Debug.Log("[TEST] FirstUploadComplete");
        UnityEngine.GameObject.DestroyImmediate(cube);
        
        cube = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        cube.AddComponent<ObjectRotation>();

        recorder.videoName = "Multi Upload Test #2 " + version;
        recorder.BeginRecording();

        recorder.OnUploadComplete += SecondUploadComplete;
    }

    private void SecondUploadComplete()
    {
        Debug.Log("[TEST] SecondUploadComplete");
        VimeoApi api = recorderObj.AddComponent<VimeoApi>();
        api.token = VALID_RECORDING_TOKEN;
        api.OnRequestComplete += CheckRecentVideos;
        api.GetRecentUserVideos("name", 2);
    }

    private void CheckRecentVideos(string resp)
    {
        Debug.Log("[TEST] CheckRecentVideos " + resp);
        JSONNode json = JSON.Parse(resp);
        
        Assert.AreEqual(json["data"][0]["name"].Value, "Multi Upload Test #2 " + version);
        Assert.AreEqual(json["data"][1]["name"].Value, "Multi Upload Test #1 " + version);

        finished = true;
    }

    [TearDown]
    public void _After()
    {
        UnityEngine.GameObject.DestroyImmediate(camObj);
        UnityEngine.GameObject.DestroyImmediate(light);
        UnityEngine.GameObject.DestroyImmediate(cube);
        UnityEngine.GameObject.DestroyImmediate(recorderObj);
    }
#endif     
}
