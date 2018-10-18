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

    [SetUp]
    public void _Before()
    {
        playerObj = new GameObject();
        player = playerObj.AddComponent<VimeoPlayer>();
        player.videoScreen = GameObject.CreatePrimitive(PrimitiveType.Cube);
        player.vimeoVideoId = INVALID_VIMEO_VIDEO_ID;
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
        Assert.AreEqual(player.IsPlayerSetup(), false);

        player.autoPlay = false;
        player.Start();
        player.SignIn("xxx");
        
        Assert.AreEqual(player.autoPlay, false);
        Assert.AreEqual(player.IsVideoMetadataLoaded(), false);
        Assert.AreEqual(player.loadingVideoMetadata, false);

        player.Play();
        Assert.AreEqual(player.loadingVideoMetadata, true);
    }

    [Test]
    public void Play_Fails_If_No_Video_Id_Set()
    {
        UnityEngine.TestTools.LogAssert.Expect(LogType.Error, new Regex("Can't load video"));

        player.autoPlay = false;
        player.vimeoVideoId = null;

        player.Start();
        player.SignIn("xxx");
        player.Play();
    }

    [Test]
    public void Play_Fails_If_Video_Id_Is_Empty_String()
    {
        UnityEngine.TestTools.LogAssert.Expect(LogType.Error, new Regex("Can't load video"));

        player.autoPlay = false;
        player.vimeoVideoId = "";

        player.Start();
        player.SignIn("xxx");
        player.Play();
    }    

    [Test]
    public void PlayVideo_Sets_vimeoVideoId_With_String()
    {
        player.SignIn("xxx");
        player.Start();

        player.vimeoVideoId = null;
        player.PlayVideo("1234");
        Assert.AreEqual(player.vimeoVideoId, "1234");
    }

    [Test]
    public void PlayVideo_Sets_vimeoVideoId_With_Int()
    {
        player.SignIn("xxx");
        player.Start();

        player.vimeoVideoId = null;
        player.PlayVideo(1234);
        Assert.AreEqual(player.vimeoVideoId, "1234");
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

    
    ////////////////////////////////////////////////////////
    // Regex pattern matching tests
    // See https://regexr.com/3prh6
    public class RegexTests : TestConfig
    {
        GameObject playerObj;
        VimeoPlayer player;

        [SetUp]
        public void _Before()
        {
            playerObj = new GameObject();
            player = playerObj.AddComponent<VimeoPlayer>();
            player.videoScreen = GameObject.CreatePrimitive(PrimitiveType.Cube);
            player.vimeoVideoId = INVALID_VIMEO_VIDEO_ID;
            player.SignIn("xxx");
            player.Start();
        }

        [Test]
        public void Load_Video_Parses_Video_Id()
        {
            player.LoadVideo("1234");
            Assert.AreEqual(player.vimeoVideoId, "1234");
        }

        [Test]
        public void Load_Video_Parses_Channel_Video()
        {
            player.LoadVideo("vimeo.com/channels/360vr/3252329");
            Assert.AreEqual(player.vimeoVideoId, "3252329");
        }

        [Test]
        public void Load_Video_Parses_Channel_Video_With_Http()
        {
            player.LoadVideo("https://vimeo.com/channels/staffpicks/249752354");
            Debug.Log(">>> " + player.vimeoVideoId);
            Assert.AreEqual(player.vimeoVideoId, "249752354");
        }

        [Test]
        public void Load_Video_Parses_Private_Video()
        {
            player.LoadVideo("vimeo.com/2304923/4434k3k3j3k3");
            Assert.AreEqual(player.vimeoVideoId, "2304923");
        }

        [Test]
        public void Load_Video_Fails_If_Bad_Vimeo_Url()
        {
            UnityEngine.TestTools.LogAssert.Expect(LogType.Error, new Regex("Invalid Vimeo URL"));
            player.LoadVideo("vimeo.com/casey");
        }

        [TearDown]
        public void _After()
        {
            UnityEngine.GameObject.DestroyImmediate(playerObj);
        }
    }

}
