using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System;
using System.Collections;
using System.Text.RegularExpressions;
using Vimeo;
using Vimeo.SimpleJSON;

public class VimeoVideoTest : TestConfig 
{
    VimeoVideo video;

    private string mockProductionAppJson = "{\"uri\":\"/videos/306323307\",\"name\":\"Teste\",\"description\":null,\"duration\":5,\"width\":1280,\"height\":720,\"files\":[{\"quality\":\"sd\",\"type\":\"video/mp4\",\"width\":960,\"height\":540,\"link\":\"https://player.vimeo.com/external/xxx.sd.mp4?s=5c4fdf9d53e13bb94c865d0ba202da37614221&profile_id=165&oauth2_token_id=...\",\"created_time\":\"2018-12-12T19:44:48+00:00\",\"fps\":25,\"size\":1056407,\"md5\":\"5c7488635ca71fa3f6e9142ea106a5ec\"},{\"quality\":\"sd\",\"type\":\"video/mp4\",\"width\":640,\"height\":360,\"link\":\"https://player.vimeo.com/external/xxx.sd.mp4?s=5c4fdf9d53e19c3bb94c865d0ba202da37614221&profile_id=164&oauth2_token_id=...\",\"created_time\":\"2018-12-12T19:44:48+00:00\",\"fps\":25,\"size\":407528,\"md5\":\"4a6cabb95f8e129902941e681ffe8f55\"},{\"quality\":\"hd\",\"type\":\"video/mp4\",\"width\":1280,\"height\":720,\"link\":\"https://player.vimeo.com/external/xxx.hd.mp4?s=53f4ebf2a97f8c974f11f8a1a68241efbbe5a7d1&profile_id=174&oauth2_token_id=...\",\"created_time\":\"2018-12-12T19:44:48+00:00\",\"fps\":25,\"size\":1751580,\"md5\":\"12dace60d0815f55d5769f3551cdf47a\"},{\"quality\":\"hls\",\"type\":\"video/mp4\",\"link\":\"https://player.vimeo.com/external/xxx.m3u8?s=edab7a40157183128871d34b0794feb5f1534501&oauth2_token_id=...\",\"created_time\":\"2018-12-12T19:44:48+00:00\",\"fps\":25,\"size\":1056407,\"md5\":\"5c7488635ca71fa3f6e9142ea106a5ec\"}]}";

    [SetUp]
    public void _Before()
    {
        string json = "{\"uri\":\"/videos/279300468\",\"name\":\"Video Test\",\"description\":\"Original 360 version https://vimeo.com/266943310\",\"duration\":306,\"width\":3456,\"height\":2160,\"play\":{\"progressive\":[{\"type\":\"video/mp4\",\"width\":2304,\"height\":1440,\"link_expiration_time\":\"2018-09-27T22:58:59+00:00\",\"link\":\"https://fpdl.vimeocdn.com/xxx/01/860/11/279300468/1046140246.mp4?token=1538089139-0x9a526defb049337f2f4a2bf35f7b27b2cf3c8219\",\"created_time\":\"2018-07-10T16:17:06+00:00\",\"fps\":30,\"size\":432584522,\"md5\":\"af068c26beebc314e5a0074e144d299b\",\"log\":\"https://test.vimeo.com/videos/279300468:5ad7cb7854/log/1046140246/1538078339/dc9caec747a0eb1db906c280b50d9867b9f773c0\"},{\"type\":\"video/mp4\",\"width\":3456,\"height\":2160,\"link_expiration_time\":\"2018-09-27T22:58:59+00:00\",\"link\":\"https://fpdl.vimeocdn.com/xxx/01/860/11/279300468/1046140239.mp4?token=1538089139-0x51b793ee8a26df3e5441dc2c98d107501cd354f1\",\"created_time\":\"2018-07-10T16:17:06+00:00\",\"fps\":30,\"size\":839989600,\"md5\":\"a58de4bbdab6e16794228d2907438eaf\",\"log\":\"https://test.vimeo.com/videos/279300468:5ad7cb7854/log/1046140239/1538078339/6f9a254433115037f0c748a8ab7ca7675b1bdb7f\"},{\"type\":\"video/mp4\",\"width\":864,\"height\":540,\"link_expiration_time\":\"2018-09-27T22:58:59+00:00\",\"link\":\"https://fpdl.vimeocdn.com/xxx/01/860/11/279300468/1046140237.mp4?token=1538089139-0x6a94f2a69755cd7f3ba48a8ec047b1f8e05cafaf\",\"created_time\":\"2018-07-10T16:17:06+00:00\",\"fps\":30,\"size\":55626502,\"md5\":\"bdf87cbc8509efd44fd61979c5554813\",\"log\":\"https://test.vimeo.com/videos/279300468:5ad7cb7854/log/1046140237/1538078339/4994c74d313423eece5bf6aa018fa33b83da1c88\"},{\"type\":\"video/mp4\",\"width\":1728,\"height\":1080,\"link_expiration_time\":\"2018-09-27T22:58:59+00:00\",\"link\":\"https://fpdl.vimeocdn.com/xxx/01/860/11/279300468/1046140236.mp4?token=1538089139-0x08bafb7ae0a43c6b7cc76c9f6693140908a81ec6\",\"created_time\":\"2018-07-10T16:17:06+00:00\",\"fps\":30,\"size\":190693968,\"md5\":\"392a27653f3c8f9849be84bb9269cb48\",\"log\":\"https://test.vimeo.com/videos/279300468:5ad7cb7854/log/1046140236/1538078339/606b1d862a599281112a61db03a1fe309366f80c\"},{\"type\":\"video/mp4\",\"width\":576,\"height\":400,\"link_expiration_time\":\"2018-09-27T22:58:59+00:00\",\"link\":\"https://fpdl.vimeocdn.com/xxx/01/860/11/279300468/1046140235.mp4?token=1538089139-0x15438ac28abac2cd7c8636d21de7640c639d6cd9\",\"created_time\":\"2018-07-10T16:17:06+00:00\",\"fps\":30,\"size\":19100125,\"md5\":\"d3a0f86dcaa6f2d7b7ee79277dbbb29b\",\"log\":\"https://test.vimeo.com/videos/279300468:5ad7cb7854/log/1046140235/1538078339/7713097c65f01ab1ca7971625d132395e44e9a73\"},{\"type\":\"video/mp4\",\"width\":1152,\"height\":720,\"link_expiration_time\":\"2018-09-27T22:58:59+00:00\",\"link\":\"https://fpdl.vimeocdn.com/xxx/01/860/11/279300468/1046140233.mp4?token=1538089139-0x1f1a8e206bdbe112e5ac1bc61b1cf716dd799816\",\"created_time\":\"2018-07-10T16:17:06+00:00\",\"fps\":30,\"size\":95464913,\"md5\":\"628bacb9124df721b0b0332fbf8f2b74\",\"log\":\"https://test.vimeo.com/videos/279300468:5ad7cb7854/log/1046140233/1538078339/74f68f53f41f291edc76087cad0f41c5e0d6a79f\"}],\"hls\":{\"link_expiration_time\":\"2018-09-27T21:58:59+00:00\",\"link\":\"https://player.vimeo.com/play/104610246,1046140239,1d140237,10461d0236,1046140235,1046140233/hls?s=279300468_1538085539_2e78157d9c236cc773328a5685b4dc65&context=Vimeo%5CController%5CApi%5CResources%5CVideoController.&logging=false\",\"log\":\"https://test.vimeo.com/videos/279300468:5ad7cb7854/log/hls/1538078339/ed60f756f0982358980263f34b3c0ed4acd85697\"},\"dash\":{\"link_expiration_time\":\"2018-09-27T21:58:59+00:00\",\"link\":\"https://player.vimeo.com/play/1046140246,1046140239,1046140237,1046140236,1046140235,1046140233/dash?s=279300468_1538085539_2e78157d9c236cc773328a5685b4dc65&context=Vimeo%5CController%5CApi%5CResources%5CVideoController.&logging=false\",\"log\":\"https://test.vimeo.com/videos/279300468:5ad7cb7854/log/dash/1538078339/df1645640bf16272a3f4c386b9eb8dcde3c31c95\"},\"status\":\"playable\"}}";
        video = new VimeoVideo(JSON.Parse(json));
    }

    [Test]
    public void Name_Is_Set_With_Id()
    {
        Assert.AreEqual(video.name, "Video Test (279300468)");
    }


    [Test]
    public void GetMetadata_Returns_Null_When_No_Metadata_Present_In_Description()
    {
        JSONNode sampleJson = video.GetMetadata();
        Assert.IsNull(sampleJson);
    }

    [Test]
    public void GetMetadata_Works()
    {
        video.description = " { \"test\": 1 } ";
        JSONNode json = video.GetMetadata();
        Assert.AreEqual(json["test"].Value, "1");
    }

    [Test]
    public void Can_Parse_Json_From_Description()
    {
        UnityEngine.TestTools.LogAssert.NoUnexpectedReceived();
        JSONNode json = video.GetJsonFromString("Test !!!!! \n {\"_versionMajor\": 0,\"_versionMinor\": 2,\"boundsCenter\": \n{\"x\": 0,\"y\": 0,\"z\": 1.03093326091766},\"boundsSize\": \n{\"x\": 3.14853119850159,\"y\": 1.76878845691681,\"z\": 1.08638906478882},\"crop\": \n{\"w\": 1.02883338928223,\"x\": 0.186250150203705,\"y\": -0.0672345161437988,\"z\": 0.522190392017365},\"depthFocalLength\": \n{\"x\": 1919.83203125,\"y\": 1922.28527832031},\"depthImageSize\": \n{\"x\": 3840.0,\"y\": 2160.0},\"depthPrincipalPoint\": \n{\"x\": 1875.52282714844,\"y\": 1030.56298828125},\"extrinsics\": \n{\"e00\": 1,\"e01\": 0,\"e02\": 0,\"e03\": 0,\"e10\": 0,\"e11\": 1,\"e12\": 0,\"e13\": 0,\"e20\": 0,\"e21\": 0,\"e22\": 1,\"e23\": 0,\"e30\": 0,\"e31\": 0,\"e32\": 0,\"e33\": 1},\"farClip\": 1.57412779331207,\"format\": \"perpixel\",\"nearClip\": 0.487738698720932,\"numAngles\": 1,\"textureHeight\": 4096,\"textureWidth\": 2048} What a nice test");
        Assert.AreEqual(json["boundsCenter"]["x"].Value, "0");
    }

    [Test]
    public void Returns_Null_When_JSON_Is_Invalid()
    {
        UnityEngine.TestTools.LogAssert.Expect(LogType.Error, new Regex("problem parsing"));
        JSONNode json = video.GetJsonFromString("{ hello: 1, \n computer: truth }");
        Assert.AreEqual(json, null);
    }

    [Test]
    public void Returns_Null_When_JSON_Is_Not_Found()
    {
        UnityEngine.TestTools.LogAssert.Expect(LogType.Warning, new Regex("No JSON"));
        JSONNode json = video.GetJsonFromString("No JSON found here!");
        Assert.AreEqual(json, null);
    }

#region Progress Video Tests
    [Test]
    public void GetVideoFileByResolution_Uses_Lowest_Resolution_If_Not_Found()
    {
        UnityEngine.TestTools.LogAssert.Expect(LogType.Log, new Regex("Defaulting to 400p"));
        JSONNode file = video.GetVideoFileByResolution(Vimeo.Player.StreamingResolution.x360p);
        Assert.AreEqual(file["height"].Value, "400");
    }

    [Test]
    public void GetVideoFileByResolution_Uses_Selected_Resolution()
    {
        UnityEngine.TestTools.LogAssert.NoUnexpectedReceived();
        JSONNode file = video.GetVideoFileByResolution(Vimeo.Player.StreamingResolution.x1080p_FHD);
        Assert.AreEqual(file["height"].Value, "1080");
    }

    [Test]
    public void GetVideoFileByResolution_Uses_Selected_Resolution_For_Files_Response()
    {
        UnityEngine.TestTools.LogAssert.NoUnexpectedReceived();
        video = new VimeoVideo(JSON.Parse(mockProductionAppJson));
        JSONNode file = video.GetVideoFileByResolution(Vimeo.Player.StreamingResolution.x720p_HD);
        Assert.AreEqual(file["height"].Value, "720");
    }

#endregion


#region Adaptive Tests
    [Test]
    public void GetHlsUrl_Works_For_Play_Response()
    {
        UnityEngine.TestTools.LogAssert.NoUnexpectedReceived();
        Assert.AreEqual(video.getHlsUrl(), "https://player.vimeo.com/play/104610246,1046140239,1d140237,10461d0236,1046140235,1046140233/hls?s=279300468_1538085539_2e78157d9c236cc773328a5685b4dc65&context=Vimeo%5CController%5CApi%5CResources%5CVideoController.&logging=false");
    }

    [Test]
    public void GetDashUrl_Works_For_Play_Response()
    {
        UnityEngine.TestTools.LogAssert.NoUnexpectedReceived();
        Assert.AreEqual(video.getDashUrl(), "https://player.vimeo.com/play/1046140246,1046140239,1046140237,1046140236,1046140235,1046140233/dash?s=279300468_1538085539_2e78157d9c236cc773328a5685b4dc65&context=Vimeo%5CController%5CApi%5CResources%5CVideoController.&logging=false");
    }
    
    [Test]
    // TODO somehow test different application platforms
    public void GetAdaptiveVideoFileURL_Returns_Dash_By_Default()
    {
        Assert.AreEqual(video.GetAdaptiveVideoFileURL(), video.getDashUrl());
    }

    [Test]
    public void GetHlsUrl_Works_For_Files_Response()
    {
        UnityEngine.TestTools.LogAssert.NoUnexpectedReceived();
        video = new VimeoVideo(JSON.Parse(mockProductionAppJson));
        Assert.AreEqual(video.getHlsUrl(), "https://player.vimeo.com/external/xxx.m3u8?s=edab7a40157183128871d34b0794feb5f1534501&oauth2_token_id=...");
    }

    [Test]
    public void GetDashUrl_Defaults_To_Hls_For_Files_Response()
    {
        UnityEngine.TestTools.LogAssert.Expect(LogType.Warning, "[Vimeo] No DASH manfiest found. Defaulting to HLS.");
        video = new VimeoVideo(JSON.Parse(mockProductionAppJson));
        Assert.AreEqual(video.getDashUrl(), "https://player.vimeo.com/external/xxx.m3u8?s=edab7a40157183128871d34b0794feb5f1534501&oauth2_token_id=...");
    }
#endregion

    [TearDown]
    public void _After()
    {

    }
}
