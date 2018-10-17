using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Vimeo;

public class VimeoUploaderTest : TestConfig 
{
    GameObject uploaderObj;

    VimeoUploader uploader;

    [SetUp]
    public void _Before()
    {
        uploaderObj = new GameObject();
        uploader = uploaderObj.AddComponent<VimeoUploader>();
    }

    [Test]
    public void Init_Works()
    {
        uploader.Init(VALID_RECORDING_TOKEN, 10000);
        
        Assert.AreEqual(uploader.token, VALID_RECORDING_TOKEN);
        Assert.AreEqual(uploader.max_chunk_size, 10000);
    }

    [TearDown]
    public void _After()
    {
        UnityEngine.GameObject.DestroyImmediate(uploaderObj);
    }
}
