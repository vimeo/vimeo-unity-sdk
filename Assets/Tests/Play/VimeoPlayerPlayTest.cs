using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Text.RegularExpressions;
using Vimeo.Player;

public class VimeoPlayerPlayTest : TestConfig
{
    GameObject camObj;
    GameObject playerObj;
    GameObject screenObj;
    VimeoPlayer player;

    bool triggered;

    float timeout = 5;
    float elapsed = 0;

    [SetUp]
    public void _Before()
    {
        // Camera setup
        camObj = new GameObject();
        camObj.AddComponent<Camera>();
        camObj.transform.Translate(0, 0, 3);
        camObj.AddComponent<AudioListener>();

        // Player Setup
        playerObj = new GameObject();
        player = playerObj.AddComponent<VimeoPlayer>();
        player.selectedResolution = StreamingResolution.x360p;
        player.autoPlay = false;

        // Screen setup
        screenObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        player.videoScreen = screenObj;
        camObj.transform.LookAt(playerObj.transform);

        triggered = false;
        elapsed = 0;
    }

    [UnityTest]
    public IEnumerator Can_Stream_Video_With_Valid_Token()
    {
        player.OnVideoStart += EventTriggered;

        player.SignIn(VALID_STREAMING_TOKEN);
        player.PlayVideo(VALID_VIMEO_VIDEO_ID);

        UnityEngine.TestTools.LogAssert.NoUnexpectedReceived();

        while (!triggered) {
            yield return new WaitForSeconds(.25f);
            TimeoutCheck();
        }
    }

    [UnityTest]
    [Timeout(5000)]
    public IEnumerator Can_Stream_Video_With_Valid_Production_Token()
    {
        player.OnVideoStart += EventTriggered;

        player.SignIn(VALID_PRODUCTION_STREAMING_TOKEN);
        player.PlayVideo(VALID_PRODUCTION_VIMEO_VIDEO_ID);

        UnityEngine.TestTools.LogAssert.NoUnexpectedReceived();

        yield return new WaitUntil(() => triggered == true);
    }

    [UnityTest]
    public IEnumerator Cant_Stream_Video_With_Invalid_Token()
    {
        player.OnLoadError += EventTriggered;
        player.SignIn("xxxxxxxxxxxxxxx");
        player.PlayVideo(VALID_VIMEO_VIDEO_ID);

        UnityEngine.TestTools.LogAssert.Expect(LogType.Error, new Regex("401"));

        while (!triggered) {
            yield return new WaitForSeconds(.25f);
            TimeoutCheck();
        }
    }

    [UnityTest]
    public IEnumerator OnVideoMetadataLoad_Event_Triggered()
    {
        player.OnVideoMetadataLoad += EventTriggered;
        player.SignIn(VALID_STREAMING_TOKEN);
        player.PlayVideo(VALID_VIMEO_VIDEO_ID);

        Assert.IsNull(player.vimeoVideo);

        while (!triggered) {
            yield return new WaitForSeconds(.25f);
            TimeoutCheck();
        }

        Assert.IsNotNull(player.vimeoVideo);
        Assert.IsNotEmpty(player.controller.videoPlayer.url);
    }

    [UnityTest]
    public IEnumerator LoadVideo_Doesnt_Play_Video()
    {
        player.OnVideoMetadataLoad += EventTriggered;
        player.autoPlay = false;

        player.SignIn(VALID_STREAMING_TOKEN);
        player.LoadVideo(VALID_VIMEO_VIDEO_ID);

        while (!triggered) {
            yield return new WaitForSeconds(.25f);
            TimeoutCheck();
        }

        Assert.IsNotNull(player.vimeoVideo);
        Assert.IsEmpty(player.controller.videoPlayer.url);
    }

    [UnityTest]
    public IEnumerator Unfurl_Makes_A_Request_To_Vimeo_And_Redirects_Sucssesfully()
    {
        player.OnVideoMetadataLoad += EventTriggered;

        player.SignIn(VALID_STREAMING_TOKEN);
        player.LoadVideo(VALID_VIMEO_VIDEO_ID);

        yield return new WaitUntil(()=> triggered);

        string linkToUnfurl = player.vimeoVideo.GetAdaptiveVideoFileURL();
        yield return player.Unfurl(linkToUnfurl);

        Assert.AreNotEqual(player.m_file_url, linkToUnfurl);
    }

    [UnityTest]
    public IEnumerator Unfurl_Does_Not_Redirect_If_Provided_A_Link_That_Does_Not_Redirect()
    {
        player.OnVideoMetadataLoad += EventTriggered;

        player.SignIn(VALID_STREAMING_TOKEN);
        player.LoadVideo(VALID_VIMEO_VIDEO_ID);

        yield return new WaitUntil(()=> triggered);

        string linkToUnfurl = player.vimeoVideo.GetAdaptiveVideoFileURL();
        yield return player.Unfurl(linkToUnfurl);

        string unfurledLink = player.m_file_url;

        yield return player.Unfurl(unfurledLink);

        Assert.AreEqual(player.m_file_url, unfurledLink);
    }

    private void EventTriggered()
    {
        triggered = true;
    }

    private void TimeoutCheck(string msg = "Test timed out")
    {
        elapsed += .25f;
        if (elapsed >= timeout) {
            Assert.Fail(msg);
        }
    }

    [TearDown]
    public void _After()
    {
        player = null;
        UnityEngine.GameObject.DestroyImmediate(camObj);
        UnityEngine.GameObject.DestroyImmediate(screenObj);
        UnityEngine.GameObject.DestroyImmediate(playerObj);
    }
}
