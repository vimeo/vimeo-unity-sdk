using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System;
using System.Collections;
using System.Text.RegularExpressions;
using Vimeo.Player;

public class AVProVideoPlayTest : TestConfig
{
#if VIMEO_AVPRO_VIDEO_SUPPORT
    GameObject camObj;
    GameObject light;
    GameObject playerObj;
    GameObject screenObj;
    VimeoPlayer player;

    bool triggered;

    float timeout = 10;
    float elapsed = 0;

    RenderHeads.Media.AVProVideo.MediaPlayer mediaPlayer;
    
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
        
        // Player Setup
        playerObj = new GameObject("Video Player");
        player = playerObj.AddComponent<VimeoPlayer>();
        player.selectedResolution = StreamingResolution.x360p;
        player.autoPlay = false;

        // AVPro setup
        mediaPlayer = playerObj.AddComponent<RenderHeads.Media.AVProVideo.MediaPlayer>();
        mediaPlayer.m_AutoStart = false;
        mediaPlayer.m_AutoOpen = false;
        player.videoPlayerType = VideoPlayerType.AVProVideo;
        player.mediaPlayer = mediaPlayer;

        // Screen setup
        screenObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        player.videoScreen = screenObj;
        camObj.transform.LookAt(playerObj.transform);

        // Attach avpro to screen
        RenderHeads.Media.AVProVideo.ApplyToMesh applyToMesh = playerObj.AddComponent<RenderHeads.Media.AVProVideo.ApplyToMesh>();
        applyToMesh.Player = mediaPlayer;
        applyToMesh.MeshRenderer = screenObj.GetComponent<MeshRenderer>();

        triggered = false;
        elapsed = 0;
    }

    [UnityTest]
    public IEnumerator Can_Stream_Video_With_Valid_Token() 
    {    
        player.OnVideoStart += EventTriggered;

        player.SignIn(VALID_STREAMING_TOKEN);
        player.PlayVideo(VALID_VIMEO_VIDEO_ID);

        while (!triggered) {
            if (!String.IsNullOrEmpty(mediaPlayer.m_VideoPath)) {
                triggered = true;
            }
            yield return new WaitForSeconds(.25f);
            TimeoutCheck();
        }
    }

    [UnityTest]
    public IEnumerator Logs_Warning_If_No_MediaPlayer_Set() 
    {    
        UnityEngine.TestTools.LogAssert.Expect(LogType.Warning, new Regex("MediaPlayer has not been assigned"));

        player.OnVideoStart += EventTriggered;
        player.mediaPlayer = null;
        player.SignIn(VALID_STREAMING_TOKEN);
        player.PlayVideo(VALID_VIMEO_VIDEO_ID);

        yield return null;
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
        UnityEngine.GameObject.DestroyImmediate(light);
        UnityEngine.GameObject.DestroyImmediate(screenObj);
        UnityEngine.GameObject.DestroyImmediate(playerObj);
    }
#endif 
}
