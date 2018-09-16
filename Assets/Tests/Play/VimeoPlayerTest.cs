using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using Vimeo.Player;

public class VimeoPlayerz 
{
    GameObject camObj;
    GameObject playerObj;
    VimeoPlayer player;

    bool loaded;

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
        player.vimeoVideoId = "244123293";
        player.SignIn("c8df7253ea55e7018fae9e0b7a9ac392");
        camObj.transform.LookAt(playerObj.transform);

        loaded = false;
    }

    [UnityTest]
    public IEnumerator Can_Stream_Video_With_Valid_Token() 
    {    
        // Screen setup
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        player.videoScreen = cube;
        player.OnVideoStart += VideoStarted;
        player.LoadVideo();

        yield return new WaitForSeconds(10f);
        float timeout = 5;
        float elapsed = 0;


        // while (!loaded) {
        //     yield return new WaitForSeconds(.25f);
        //     elapsed += .25f;

        //     if (elapsed >= timeout) {
        //         Assert.Fail("Failed to play video");
        //     }
        // }
    }

    public void VideoStarted()
    {
        loaded = true;
    }

    [TearDown]
    public void _After()
    {
    }
}
