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

    bool loaded;
    bool error;

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
        
        // Screen setup
        screenObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        player.videoScreen = screenObj;
        player.OnVideoStart += VideoStarted;
        camObj.transform.LookAt(playerObj.transform);

        loaded = false;
        error = false;
        elapsed = 0;
    }

    [UnityTest]
    public IEnumerator Can_Stream_Video_With_Valid_Token() 
    {    
        player.SignIn(VALID_STREAMING_TOKEN);
        player.LoadVimeoVideoById(276918964);

        UnityEngine.TestTools.LogAssert.NoUnexpectedReceived();

        while (!loaded) {
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
        player.OnLoadError += VideoLoadError;
        player.SignIn("xxxxxxxxxxxxxxx");
        player.LoadVimeoVideoById(286281950);
        
        UnityEngine.TestTools.LogAssert.Expect(LogType.Error, new Regex("401"));
        UnityEngine.TestTools.LogAssert.Expect(LogType.Error, new Regex("Something strange occurred"));
        
        while (!error) {
            yield return new WaitForSeconds(.25f);
            elapsed += .25f;

            if (elapsed >= timeout) {
                Assert.Fail("Failed to receive error event");
            }
        }
    }

    public void VideoStarted()
    {
        loaded = true;
    }

    public void VideoLoadError()
    {
        error = true;
    }

    [TearDown]
    public void _After()
    {
        loaded = false;
        player = null;
        UnityEngine.GameObject.DestroyImmediate(camObj);
        UnityEngine.GameObject.DestroyImmediate(screenObj);
        UnityEngine.GameObject.DestroyImmediate(playerObj);
    }
}
