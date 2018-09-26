using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System;
using System.Collections;
using System.Text.RegularExpressions;
using Vimeo.Player;

public class VimeoPlayerTest : TestConfig {

    GameObject playerObj;
    VimeoPlayer player;

    bool wait = true;

    float timeout = 5;
    float elapsed = 0;
    

    [SetUp]
    public void _Before()
    {
        playerObj = new GameObject();
        player = playerObj.AddComponent<VimeoPlayer>();
        player.videoScreen = GameObject.CreatePrimitive(PrimitiveType.Cube);
        player.vimeoVideoId = "1234";
        
        elapsed = 0;
        wait = true;
    }

    [Test]
    public void SignIn_Works_In_Editor()
    {
        UnityEngine.TestTools.LogAssert.NoUnexpectedReceived();
        player.SignIn("xxx");
        Assert.AreEqual(player.GetVimeoToken(), "xxx");
    }

    [Test]
    public void SignIn_Updates_Token()
    {
        UnityEngine.TestTools.LogAssert.NoUnexpectedReceived();

        player.autoPlay = false;
        player.Start();
        player.SignIn("xxx");
        
        Assert.IsNotNull(player.GetComponent<Vimeo.VimeoApi>());
        Assert.AreEqual(player.GetComponent<Vimeo.VimeoApi>().token, "xxx");
    }

    [Test]
    public void Play_Automatically_Loads_Video()
    {
        UnityEngine.TestTools.LogAssert.NoUnexpectedReceived();
        player.autoPlay = false;
        player.Start();
        player.SignIn("xxx");
        
        Assert.AreEqual(player.autoPlay, false);
        Assert.AreEqual(player.IsPlayerLoaded(), false);
        Assert.AreEqual(player.IsVideoMetadataLoaded(), false);
        Assert.AreEqual(player.loadingVideoMetadata, false);

        player.Play();
        Assert.AreEqual(player.loadingVideoMetadata, true);
    }

    [Test]
    public void Autoplay_Throws_Error_If_Not_Signed_in()
    {
        UnityEngine.TestTools.LogAssert.Expect(LogType.Error, new Regex("not signed in"));

        player.autoPlay = true;
        player.Start();
        player.SignIn("xxx");
    }

    [TearDown]
    public void _After()
    {
        UnityEngine.GameObject.DestroyImmediate(playerObj);
    }
}
