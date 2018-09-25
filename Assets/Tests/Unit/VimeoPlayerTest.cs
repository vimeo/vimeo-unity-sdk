using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System;
using System.Collections;
using Vimeo.Player;

public class VimeoPlayerTest : TestConfig {

    GameObject playerObj;
    VimeoPlayer player;

    [SetUp]
    public void _Before()
    {
        playerObj = new GameObject();
        player = playerObj.AddComponent<VimeoPlayer>();
        player.videoScreen = GameObject.CreatePrimitive(PrimitiveType.Cube);
        player.vimeoVideoId = "1234";
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
        Assert.IsNull(player.GetComponent<VideoController>().videoScreenObject);

        player.Play();
        Assert.AreEqual(player.autoPlay, true);
        Assert.IsNotNull(player.GetComponent<VideoController>().videoScreenObject);
    }

    [TearDown]
    public void _After()
    {
        UnityEngine.GameObject.DestroyImmediate(playerObj);
    }
}
