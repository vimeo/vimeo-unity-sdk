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
    }

    [UnityTest]
    public IEnumerator Can_Play_Volumetric_Video_With_Valid_Token_And_Unity_VideoPlayer() 
    {
        player.OnVideoStart += EventTriggered;
        
        player.SignIn(VALID_STREAMING_TOKEN);
        player.PlayVideo(VALID_VIMEO_VOLUMETRIC_VIDEO_ID);

        yield return new WaitUntil(()=> triggered);
        Assert.IsFalse(String.IsNullOrEmpty(depthkitObj.GetComponent<VideoPlayer>().url));
    }

#if VIMEO_AVPRO_VIDEO_SUPPORT
    [UnityTest]
    public IEnumerator Can_Play_Volumetric_Video_With_Valid_Token_And_AVPro_Video() 
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

        yield return new WaitUntil(()=> triggered);
        Assert.IsFalse(String.IsNullOrEmpty(depthkitClip.GetComponent<RenderHeads.Media.AVProVideo.MediaPlayer>().m_VideoPath));
    }

    [UnityTest]
    public IEnumerator Can_Play_Adaptive_Volumetric_Video_With_Valid_Token_And_AVPro_Video() 
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
        
        yield return new WaitUntil(()=> triggered);
        yield return new WaitForSeconds(1.0f);
        Assert.IsFalse(String.IsNullOrEmpty(depthkitClip.GetComponent<RenderHeads.Media.AVProVideo.MediaPlayer>().m_VideoPath));
    }
#endif

    [UnityTest]
    public IEnumerator Vimeo_Player_Sets_Volumetric_Metadata() 
    {    
        player.OnVideoStart += EventTriggered;
        
        player.SignIn(VALID_STREAMING_TOKEN);
        player.PlayVideo(VALID_VIMEO_VOLUMETRIC_VIDEO_ID);

        yield return new WaitUntil(()=> triggered);
        Assert.IsNotNull(depthkitClip._metaDataFile);
    }

    private void EventTriggered()
    {
        triggered = true;
    }

    private void ErrorTriggered()
    {
        errored = true;
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
