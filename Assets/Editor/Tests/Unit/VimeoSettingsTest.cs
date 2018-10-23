using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System;
using System.Collections;
using Vimeo.Player;

public class VimeoSettingsTest : TestConfig {

    GameObject playerObj;
    VimeoPlayer player;

    [SetUp]
    public void _Before()
    {
        playerObj = new GameObject();
        player = playerObj.AddComponent<VimeoPlayer>();
        player.autoPlay = false;
        player.Start();
    }

    [Test]
    public void GameObject_Has_A_Unique_ID()
    {
        Assert.IsNotNull(player.uid);
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
    
    [Test]
    public void GetVimeoToken_Returns_Null()
    {
        Assert.AreEqual(player.GetVimeoToken(), null);
    }

    [Test]
    public void SetVimeoToken_Works()
    {
        player.SetVimeoToken("xxxxxxxxxx");
        Assert.AreEqual(player.vimeoToken, null);
        Assert.AreEqual(player.GetVimeoToken(), "xxxxxxxxxx");
    }

    [Test]
    public void BuildMode_Saves_Token_To_Scene()
    {
        player.SignIn("buildmode");
        player.EnableBuildMode();
        Assert.AreEqual(player.vimeoToken, "buildmode");
    }

    [Test]
    public void Deletes_PlayerPrefs_When_Null()
    {
        player.SetKey("mykey", "cool");
        Assert.AreEqual(PlayerPrefs.GetString("mykey"), "cool");
        player.SetKey("mykey", null);
        Assert.AreEqual(PlayerPrefs.GetString("mykey"), "");
    }

    [Test]
    public void GetCurrentFolderIndex_Works()
    {
        player.currentFolder = new Vimeo.VimeoFolder("Folder 2", "/2234");

        player.vimeoFolders.Add(new Vimeo.VimeoFolder("Folder 1", "/1234"));
        player.vimeoFolders.Add(player.currentFolder);
        player.vimeoFolders.Add(new Vimeo.VimeoFolder("Folder 3", "/3234"));

        Assert.AreEqual(player.GetCurrentFolderIndex(), 1);
    }

    [TearDown]
    public void _After()
    {
        UnityEngine.GameObject.DestroyImmediate(playerObj);
    }
}
