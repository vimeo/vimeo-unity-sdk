using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System;
using System.Collections;
using System.Text.RegularExpressions;
using Vimeo.Player;
using UnityEngine.Video;

public class DepthkitPlayTest : TestConfig
{
#if VIMEO_DEPTHKIT_SUPPORT
    GameObject camObj;
    GameObject depthkitObj;
    GameObject playerObj;
    VimeoPlayer player;

    Depthkit.Depthkit_Clip depthkitClip;

    bool triggered;
    bool errored;


    float timeout = 10;
    float elapsed = 0;
    
    [SetUp]
    public void _Before()
    {
        // Camera setup
        camObj = new GameObject("Camera");
        camObj.AddComponent<Camera>();
        camObj.transform.Translate(0, 0, -3);

        // Player Setup
        playerObj = new GameObject("Video Player");
        player = playerObj.AddComponent<VimeoPlayer>();
        player.selectedResolution = StreamingResolution.x360p;
        player.videoPlayerType = VideoPlayerType.Depthkit;
        player.autoPlay = false;

        //Depthkit setup
        depthkitObj = new GameObject("Depthkit Clip");
        depthkitClip = depthkitObj.AddComponent<Depthkit.Depthkit_Clip>();
        depthkitClip.Setup(Depthkit.AvailablePlayerType.UnityVideoPlayer, Depthkit.RenderType.Photo, new TextAsset());
        player.depthKitClip = depthkitClip;  
        triggered = false;
        elapsed = 0;
    }

    [UnityTest]
    public IEnumerator Can_Playback_Volumetric_Video_With_Valid_Token_And_Unity_VideoPlayer() 
    {


        player.OnVideoStart += EventTriggered;
        
        player.SignIn(VALID_STREAMING_TOKEN);
        player.PlayVideo(VALID_VIMEO_VOLUMETRIC_VIDEO_ID);

        while (!triggered) {
            if (!String.IsNullOrEmpty(depthkitObj.GetComponent<VideoPlayer>().url)) {
                triggered = true;
            }
            yield return new WaitForSeconds(3.25f);
            TimeoutCheck();
        }
    }

#if VIMEO_AVPRO_VIDEO_SUPPORT
    [UnityTest]
    public IEnumerator Can_Playback_Volumetric_Video_With_Valid_Token_And_AVPro_Video() 
    {
        depthkitClip.Setup(Depthkit.AvailablePlayerType.AVProVideo, Depthkit.RenderType.Photo, new TextAsset());
        player.depthKitClip = depthkitClip;

        //Set these so AVPro doesn't log errors about not having a file path
        depthkitClip.GetComponent<RenderHeads.Media.AVProVideo.MediaPlayer>().m_AutoOpen = false;
        depthkitClip.GetComponent<RenderHeads.Media.AVProVideo.MediaPlayer>().m_AutoStart = false;
        
        depthkitClip._needToResetPlayerType = true;

        player.OnVideoStart += EventTriggered;

        player.SignIn(VALID_STREAMING_TOKEN);
        player.PlayVideo(VALID_VIMEO_VOLUMETRIC_VIDEO_ID);

        while (!triggered) {
            if (!String.IsNullOrEmpty(depthkitClip.GetComponent<RenderHeads.Media.AVProVideo.MediaPlayer>().m_VideoPath)) {
                triggered = true;
            }
            yield return new WaitForSeconds(3.25f);
            TimeoutCheck();
        }
    }

    [UnityTest]
    public IEnumerator Can_Playback_Adaptive_Volumetric_Video_With_Valid_Token_And_AVPro_Video() 
    {
        depthkitClip.Setup(Depthkit.AvailablePlayerType.AVProVideo, Depthkit.RenderType.Photo, new TextAsset());
        player.depthKitClip = depthkitClip;

        player.selectedResolution = StreamingResolution.Adaptive;

        //Set these so AVPro doesn't log errors about not having a file path
        depthkitClip.GetComponent<RenderHeads.Media.AVProVideo.MediaPlayer>().m_AutoOpen = false;
        depthkitClip.GetComponent<RenderHeads.Media.AVProVideo.MediaPlayer>().m_AutoStart = false;
        
        depthkitClip._needToResetPlayerType = true;

        player.OnVideoStart += EventTriggered;

        player.SignIn(VALID_STREAMING_TOKEN);
        player.PlayVideo(VALID_VIMEO_VOLUMETRIC_VIDEO_ID);

        while (!triggered) {
            if (!String.IsNullOrEmpty(depthkitClip.GetComponent<RenderHeads.Media.AVProVideo.MediaPlayer>().m_VideoPath)) {
                triggered = true;
            }
            yield return new WaitForSeconds(3.25f);
            TimeoutCheck();
        }
    }
#endif

    [UnityTest]
    public IEnumerator Vimeo_Player_Sets_Volumetric_Metadata() 
    {    
        player.OnVideoStart += EventTriggered;
        
        player.SignIn(VALID_STREAMING_TOKEN);
        player.PlayVideo(VALID_VIMEO_VOLUMETRIC_VIDEO_ID);
        TextAsset emptyTextAsset = new TextAsset(" ");

        while (!triggered) {
            if (depthkitClip._metaDataFile.text != emptyTextAsset.text) {
                triggered = true;
            }
            yield return new WaitForSeconds(.25f);
            TimeoutCheck();
        }
    }

    [UnityTest]
    public IEnumerator Vimeo_Player_Gracefully_Errors_When_Metadata_Is_Not_Valid()
    {
        UnityEngine.TestTools.LogAssert.Expect(LogType.Error, "[Vimeo] Your volumetric video metadata is either missing or not supported");
        player.OnLoadError += ErrorTriggered;
        
        player.SignIn(VALID_STREAMING_TOKEN);
        player.PlayVideo(VALID_VIMEO_VIDEO_ID);

        yield return new WaitUntil(()=> errored);
    }

    private void EventTriggered()
    {
        triggered = true;
    }

    private void ErrorTriggered()
    {
        errored = true;
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
        depthkitClip = null;
        UnityEngine.GameObject.DestroyImmediate(camObj);
        UnityEngine.GameObject.DestroyImmediate(depthkitObj);
        UnityEngine.GameObject.DestroyImmediate(playerObj);
    }
#endif 
}
