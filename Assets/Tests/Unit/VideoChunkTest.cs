using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System;
using System.Collections;
using UnityEngine.Networking;
using System.Text.RegularExpressions;
using Vimeo;

public class VideoChunkTest : TestConfig
{
    VideoChunk chunk;

    GameObject chunkObj;

    [SetUp]
    public void _Before()
    {
        chunkObj = new GameObject();
        chunk = chunkObj.AddComponent<VideoChunk>();
    }

    [Test]
    public void Init_Works()
    {
        Assert.IsNull(chunk.bytes);

        chunk.Init(0, "test_tus_url", "test_file_path", 10000);

        Assert.AreEqual("test_file_path", chunk.filePath);
        Assert.AreEqual("test_tus_url", chunk.url);
        Assert.AreEqual(0, chunk.startByte);
        Assert.AreEqual(0, chunk.lastByteUploaded);
        Assert.AreEqual(10000, chunk.totalBytes);
    }

    [Test]
    public void Stores_And_Disposes_Bytes()
    {
        chunk.Init(0, "test_tus_url", TEST_IMAGE_PATH, 10000);
        chunk.ReadBytes();
        chunk.bytes[0] = 5;
        chunk.DisposeBytes();
        Assert.AreNotEqual(chunk.bytes[0], 5);
    }

    [Test]
    public void Reads_Bytes_From_File()
    {
        chunk.Init(0, "test_tus_url", TEST_IMAGE_PATH, 12412);
        Assert.AreEqual(chunk.bytes, null);
        chunk.ReadBytes();
        Assert.AreEqual(chunk.bytes.Length, 12412);
        Assert.AreNotEqual(chunk.bytes[0], 0);
    }

    [Test]
    public void GetBytesUploaded_Returns_Zero_Before_Uploading()
    {
        chunk.Init(0, "test_tus_url", TEST_IMAGE_PATH, 1);
        Assert.AreEqual(chunk.GetBytesUploaded(), 0);
    }

    [Test]
    public void GetBytesUploaded_Returns_Total_Chunk_Size_When_Finished_Uploading()
    {
        chunk.Init(0, "test_tus_url", TEST_IMAGE_PATH, 1441);
        chunk.isFinishedUploading = true;
        Assert.AreEqual(chunk.GetBytesUploaded(), 1441);
    }

    [Test]
    public void GetBytesUploaded_Returns_Uploaded_Bytes_When_Uploading()
    {
        chunk.Init(0, "test_tus_url", TEST_IMAGE_PATH, 1441);
        chunk.isUploading = true;

        Assert.AreEqual(chunk.GetBytesUploaded(), 0);
    }

    [Test]
    public void UploadError_Retries_Upload()
    {
        UnityEngine.TestTools.LogAssert.Expect(LogType.Warning, new Regex("Retrying..."));
        chunk.Init(0, "test_tus_url", TEST_IMAGE_PATH, 1441);
        Assert.AreEqual(chunk.totalRetries, 0);
        chunk.UploadError("my msg");
        Assert.AreEqual(chunk.totalRetries, 1);
    }

    [Test]
    public void UploadError_Retries_Three_Times_And_Errors()
    {
        UnityEngine.TestTools.LogAssert.Expect(LogType.Error, new Regex("it's error time"));
        chunk.Init(0, "test_tus_url", TEST_IMAGE_PATH, 1441);
        Assert.AreEqual(chunk.totalRetries, 0);
        chunk.UploadError("my msg");
        Assert.AreEqual(chunk.totalRetries, 1);
        chunk.UploadError("my msg");
        Assert.AreEqual(chunk.totalRetries, 2);
        chunk.UploadError("my msg");
        Assert.AreEqual(chunk.totalRetries, 3);

        chunk.UploadError("it's error time");
    }

    [Test]
    public void Can_Resume_Upload()
    {
        chunk.Init(0, "test_tus_url", TEST_IMAGE_PATH, 1541);
        chunk.lastByteUploaded = 541;
        chunk.ReadBytes();
        Assert.AreEqual(chunk.bytes.Length, 1541 - 541);
    }


    [TearDown]
    public void _After()
    {
        UnityEngine.GameObject.DestroyImmediate(chunkObj);
    }
}
