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
        player.Start();
    }

    [Test]
    public void SignIn_Doesnt_Save_Token_In_Scene()
    {
        player.SignIn("xxxxxxLOLxxxxxx");
        Assert.AreEqual(player.vimeoToken, null);
        Assert.AreEqual(player.GetVimeoToken(), "xxxxxxLOLxxxxxx");
        Assert.AreEqual(player.vimeoSignIn, true);
    }

    [Test]
    public void SignIn_Fails_With_Invalid_String()
    {
        player.SignIn(null);
        Assert.AreEqual(player.vimeoToken, null);
        Assert.AreEqual(player.GetVimeoToken(), null);
        Assert.AreEqual(player.vimeoSignIn, false);
    }

    [TearDown]
    public void _After()
    {
        playerObj = null;
        player = null;
    }
}
