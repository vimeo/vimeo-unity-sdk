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
        Assert.AreEqual(0, chunk.indexByte);
        Assert.AreEqual(10000, chunk.chunkSize);  
        Assert.AreEqual(chunk.bytes.Length, 10000);
    }

    [Test]
    public void Stores_And_Disposes_Bytes()
    {
        chunk.Init(0, "test_tus_url", "test_file_path", 10000);
        chunk.bytes[0] = 5;   
        chunk.DisposeBytes();
        Assert.AreNotEqual(chunk.bytes[0], 5);
    }

    [Test]
    public void Reads_Bytes_From_File()
    {
        chunk.Init(0, "test_tus_url", TEST_IMAGE_PATH, 1);
        Assert.AreEqual(chunk.bytes[0], 0);
        chunk.ReadBytes();
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

    [TearDown]
    public void _After()
    {
        UnityEngine.GameObject.DestroyImmediate(chunkObj);
    }
}
