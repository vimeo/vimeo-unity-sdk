using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Text.RegularExpressions;
using Vimeo;
using Vimeo.Recorder;
using Vimeo.SimpleJSON;

public class VimeoUploaderPlayTest : TestConfig
{
    VimeoUploader uploader;

    GameObject uploaderObj;

    bool uploaded;

    [SetUp]
    public void _Before()
    {
        // Setup only uploader
        uploaderObj = new GameObject();
        uploader = uploaderObj.AddComponent<VimeoUploader>();
        uploader.Init(VALID_RECORDING_TOKEN);
        uploaded = false;
    }

    [UnityTest]
    [Timeout(500000)]
    public IEnumerator Can_Upload_Very_Big_File()
    {
        UnityEngine.TestTools.LogAssert.NoUnexpectedReceived();

        //Register events
        uploader.OnUploadComplete += UploadComplete;
        uploader.OnUploadProgress += UploadProgress;
        uploader.OnUploadInit += UploadInit;
        uploader.OnError += UploadError;
        uploader.OnNetworkError += UploadError;

        //Upload the big file
        uploader.Upload(VERY_BIG_FILE_PATH);
        yield return new WaitUntil(() => uploaded == true);
        Assert.IsTrue(uploaded);
    }

    public void UploadComplete(string status)
    {
        uploaded = true;
    }

    public void UploadInit(string response)
    {
        VimeoVideo video = new VimeoVideo(JSONNode.Parse(response));
        uploader.SetVideoViewPrivacy(VimeoApi.PrivacyModeDisplay.OnlyPeopleWithPrivateLink);
        uploader.SetVideoName("Large file test (" + Application.platform + " " + Application.unityVersion + ")");
        uploader.SaveVideo(video);

    }

    public void UploadProgress(string status, float progress)
    {
        Debug.Log("[VimeoUploader] Uploading file, progress: " + progress);
    }

    public void UploadError(string response)
    {
        Assert.Fail(response);
    }

    [TearDown]
    public void _After()
    {
        UnityEngine.GameObject.DestroyImmediate(uploaderObj);
        UnityEngine.GameObject.DestroyImmediate(uploader);
    }
}
