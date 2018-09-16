using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System;
using System.Collections;
using Vimeo.Player;

public class VimeoSettingsTest {

    GameObject playerObj;
    VimeoPlayer player;

    [SetUp]
    public void _Before()
    {
        playerObj = new GameObject();
        player = playerObj.AddComponent<VimeoPlayer>();
    }

    [Test]
    public void Doesnt_Save_Token_In_Scene()
    {
        player.SignIn("xxxxxxLOLxxxxxx");
        Assert.AreEqual(player.vimeoToken, null);
        Assert.AreEqual(player.GetVimeoToken(), "xxxxxxLOLxxxxxx");
        Assert.AreEqual(player.vimeoSignIn, true);
    }

    [Test]
    public void Cant_Play_Video_With_Invalid_Token() {
        
        // Assert.AreEqual(0, 0);
    }

    [TearDown]
    public void _After()
    {
        playerObj = null;
        player = null;
    }
}
