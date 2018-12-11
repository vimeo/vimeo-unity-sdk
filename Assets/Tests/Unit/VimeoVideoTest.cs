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

    [SetUp]
    public void _Before()
    {
        string json = "{\"uri\":\"/videos/279300468\",\"name\":\"Video Test\",\"description\":\"Original 360 version https://vimeo.com/266948610\",\"duration\":306,\"width\":3456,\"height\":2160,\"play\":{\"progressive\":[{\"type\":\"video/mp4\",\"width\":2304,\"height\":1440,\"link_expiration_time\":\"2018-09-27T22:58:59+00:00\",\"link\":\"https://fpdl.vimeocdn.com/vimeo-prod-skyfire-std-us/01/860/11/279300468/1046140246.mp4?token=1538089139-0x9a526defb049337f2f4a2bf35f7b27b2cf3c8219\",\"created_time\":\"2018-07-10T16:17:06+00:00\",\"fps\":30,\"size\":432584522,\"md5\":\"af068c26beebc314e5a0074e144d299b\",\"log\":\"https://api.vimeo.com/videos/279300468:5ad7cb7854/log/1046140246/1538078339/dc9caec747a0eb1db906c280b50d9867b9f773c0\"},{\"type\":\"video/mp4\",\"width\":3456,\"height\":2160,\"link_expiration_time\":\"2018-09-27T22:58:59+00:00\",\"link\":\"https://fpdl.vimeocdn.com/vimeo-prod-skyfire-std-us/01/860/11/279300468/1046140239.mp4?token=1538089139-0x51b793ee8a26df3e5441dc2c98d107501cd354f1\",\"created_time\":\"2018-07-10T16:17:06+00:00\",\"fps\":30,\"size\":839989600,\"md5\":\"a58de4bbdab6e16794228d2907438eaf\",\"log\":\"https://api.vimeo.com/videos/279300468:5ad7cb7854/log/1046140239/1538078339/6f9a254433115037f0c748a8ab7ca7675b1bdb7f\"},{\"type\":\"video/mp4\",\"width\":864,\"height\":540,\"link_expiration_time\":\"2018-09-27T22:58:59+00:00\",\"link\":\"https://fpdl.vimeocdn.com/vimeo-prod-skyfire-std-us/01/860/11/279300468/1046140237.mp4?token=1538089139-0x6a94f2a69755cd7f3ba48a8ec047b1f8e05cafaf\",\"created_time\":\"2018-07-10T16:17:06+00:00\",\"fps\":30,\"size\":55626502,\"md5\":\"bdf87cbc8509efd44fd61979c5554813\",\"log\":\"https://api.vimeo.com/videos/279300468:5ad7cb7854/log/1046140237/1538078339/4994c74d313423eece5bf6aa018fa33b83da1c88\"},{\"type\":\"video/mp4\",\"width\":1728,\"height\":1080,\"link_expiration_time\":\"2018-09-27T22:58:59+00:00\",\"link\":\"https://fpdl.vimeocdn.com/vimeo-prod-skyfire-std-us/01/860/11/279300468/1046140236.mp4?token=1538089139-0x08bafb7ae0a43c6b7cc76c9f6693140908a81ec6\",\"created_time\":\"2018-07-10T16:17:06+00:00\",\"fps\":30,\"size\":190693968,\"md5\":\"392a27653f3c8f9849be84bb9269cb48\",\"log\":\"https://api.vimeo.com/videos/279300468:5ad7cb7854/log/1046140236/1538078339/606b1d862a599281112a61db03a1fe309366f80c\"},{\"type\":\"video/mp4\",\"width\":576,\"height\":400,\"link_expiration_time\":\"2018-09-27T22:58:59+00:00\",\"link\":\"https://fpdl.vimeocdn.com/vimeo-prod-skyfire-std-us/01/860/11/279300468/1046140235.mp4?token=1538089139-0x15438ac28abac2cd7c8636d21de7640c639d6cd9\",\"created_time\":\"2018-07-10T16:17:06+00:00\",\"fps\":30,\"size\":19100125,\"md5\":\"d3a0f86dcaa6f2d7b7ee79277dbbb29b\",\"log\":\"https://api.vimeo.com/videos/279300468:5ad7cb7854/log/1046140235/1538078339/7713097c65f01ab1ca7971625d132395e44e9a73\"},{\"type\":\"video/mp4\",\"width\":1152,\"height\":720,\"link_expiration_time\":\"2018-09-27T22:58:59+00:00\",\"link\":\"https://fpdl.vimeocdn.com/vimeo-prod-skyfire-std-us/01/860/11/279300468/1046140233.mp4?token=1538089139-0x1f1a8e206bdbe112e5ac1bc61b1cf716dd799816\",\"created_time\":\"2018-07-10T16:17:06+00:00\",\"fps\":30,\"size\":95464913,\"md5\":\"628bacb9124df721b0b0332fbf8f2b74\",\"log\":\"https://api.vimeo.com/videos/279300468:5ad7cb7854/log/1046140233/1538078339/74f68f53f41f291edc76087cad0f41c5e0d6a79f\"}],\"hls\":{\"link_expiration_time\":\"2018-09-27T21:58:59+00:00\",\"link\":\"https://player.vimeo.com/play/1046140246,1046140239,1046140237,1046140236,1046140235,1046140233/hls?s=279300468_1538085539_2e78157d9c236cc773328a5685b4dc65&context=Vimeo%5CController%5CApi%5CResources%5CVideoController.&logging=false\",\"log\":\"https://api.vimeo.com/videos/279300468:5ad7cb7854/log/hls/1538078339/ed60f756f0982358980263f34b3c0ed4acd85697\"},\"dash\":{\"link_expiration_time\":\"2018-09-27T21:58:59+00:00\",\"link\":\"https://player.vimeo.com/play/1046140246,1046140239,1046140237,1046140236,1046140235,1046140233/dash?s=279300468_1538085539_2e78157d9c236cc773328a5685b4dc65&context=Vimeo%5CController%5CApi%5CResources%5CVideoController.&logging=false\",\"log\":\"https://api.vimeo.com/videos/279300468:5ad7cb7854/log/dash/1538078339/df1645640bf16272a3f4c386b9eb8dcde3c31c95\"},\"status\":\"playable\"}}";
        video = new VimeoVideo(JSON.Parse(json));
    }

    [Test]
    public void Name_Is_Set_With_Id()
    {
        Assert.AreEqual(video.name, "Video Test (279300468)");
    }

    [Test]
    public void GetVideoFileByResolution_Uses_Lowest_Resolution_If_Not_Found()
    {
        UnityEngine.TestTools.LogAssert.Expect(LogType.Log, new Regex("Defaulting to 400p"));
        video.GetVideoFileUrlByResolution(Vimeo.Player.StreamingResolution.x360p);
    }

    [Test]
    public void GetVideoFileByResolution_Uses_Selected_Resolution()
    {
        UnityEngine.TestTools.LogAssert.NoUnexpectedReceived();
        video.GetVideoFileUrlByResolution(Vimeo.Player.StreamingResolution.x1080p_FHD);
    }

    [Test]
    public void GetMetadata_Returns_Null_When_No_Metadata_Present_In_Description()
    {
        JSONNode sampleJson = video.GetMetadata();
        Assert.IsNull(sampleJson);
    }

    [Test]
    public void GetStringBetween_Splits_The_String_From_Description_Properly()
    {
        string sampleString = video.GetStringBetween("Test { hello: 1, computer: 2 } Test", "{", "}");
        Assert.AreEqual("{ hello: 1, computer: 2 }", sampleString);
    }

    [TearDown]
    public void _After()
    {

    }
}
