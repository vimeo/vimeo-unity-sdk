using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Vimeo;
using Vimeo.Recorder;

public class VimeoPublisherTest : TestConfig
{
    GameObject publisherObj;

    VimeoPublisher publisher;

    VimeoRecorder recorder;

    VimeoUploader uploader;

    [SetUp]
    public void _Before()
    {
        publisherObj = new GameObject();

        recorder = publisherObj.AddComponent<VimeoRecorder>();
        publisher = publisherObj.AddComponent<VimeoPublisher>();
        publisher.Init(recorder);
    }

    [Test]
    public void Init_Works()
    {
        Assert.IsNotNull(publisher.vimeoUploader);
        Assert.IsNotNull(publisher.recorder);
    }

    [Test]
    public void GetVimeoPermalink_Should_Break_Without_VimeoVideo()
    {
        UnityEngine.TestTools.LogAssert.Expect(LogType.Error, new Regex("No vimeo video link found, try recording again"));
        Assert.IsNull(publisher.GetVimeoPermalink());
    }

    [Test]
    public void PublishVideo_Breaks_When_Video_File_Doesnt_Exist()
    {
        UnityEngine.TestTools.LogAssert.Expect(LogType.Error, new Regex("File doesn't exist, try recording it again"));
        publisher.PublishVideo("xxx");
    }

    [TearDown]
    public void _After()
    {
        UnityEngine.GameObject.DestroyImmediate(publisherObj);
    }
}
