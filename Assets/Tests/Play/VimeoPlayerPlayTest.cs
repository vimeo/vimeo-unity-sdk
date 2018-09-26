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
        player.PlayVideo(276918964);

        UnityEngine.TestTools.LogAssert.NoUnexpectedReceived();

        while (!triggered) {
            yield return new WaitForSeconds(.25f);
            elapsed += .25f;

            if (elapsed >= timeout) {
                Assert.Fail("Failed to play video");
            }
        }
    }

    [UnityTest]
    public IEnumerator Cant_Stream_Video_With_Invalid_Token() 
    {    
        player.OnLoadError += EventTriggered;
        player.SignIn("xxxxxxxxxxxxxxx");
        player.PlayVideo(286281950);
        
        UnityEngine.TestTools.LogAssert.Expect(LogType.Error, new Regex("401"));
        UnityEngine.TestTools.LogAssert.Expect(LogType.Error, new Regex("Something strange occurred"));
        
        while (!triggered) {
            yield return new WaitForSeconds(.25f);
            elapsed += .25f;

            if (elapsed >= timeout) {
                Assert.Fail("Failed to receive error event");
            }
        }
    }

    [UnityTest]
    public IEnumerator OnVideoMetadataLoad_Event_Triggered()
    {
        player.OnVideoMetadataLoad += EventTriggered;
        player.SignIn(VALID_STREAMING_TOKEN);
        player.PlayVideo(286281950);
        
        Assert.IsNull(player.vimeoVideo);

        while (!triggered) {
            yield return new WaitForSeconds(.25f);
            elapsed += .25f;

            if (elapsed >= timeout) {
                Assert.Fail("Failed to receive metadata event");
            }
        }

        Assert.IsNotNull(player.vimeoVideo);
    }

    public void EventTriggered()
    {
        triggered = true;
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
