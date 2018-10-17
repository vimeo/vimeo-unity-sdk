using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System;
using System.Collections;
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
        chunk.Init(0, "test_tus_url", "test_file_path", 10000);

        Assert.AreEqual("test_file_path", chunk.file_path);
        Assert.AreEqual("test_tus_url", chunk.url);
        Assert.AreEqual(0, chunk.index_byte);
        Assert.AreEqual(10000, chunk.chunk_size);   
    }

    [Test]
    public void Stores_And_Disposes_Bytes()
    {
        chunk.Init(0, "test_tus_url", "test_file_path", 10000);
        chunk.bytes = new byte[chunk.chunk_size];
        chunk.bytes[0] = 5;   

        Assert.AreEqual(chunk.bytes.Length, chunk.chunk_size);
        
        chunk.DisposeBytes();
        
        Assert.AreNotEqual(chunk.bytes[0], 5);
    }

    [Test]
    public void Reads_Bytes_From_File()
    {
        chunk.Init(0, "test_tus_url", TEST_IMAGE_PATH, 1);
        chunk.ReadBytes();

        Assert.IsNotEmpty(chunk.bytes);
    }

    [TearDown]
    public void _After()
    {

    }
}
