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
    FileInfo testFile;
    int testFileSize = 2879; // hardcoding the file size of the test file for reference

    VimeoUploader uploader;

    [SetUp]
    public void _Before()
    {
        uploaderObj = new GameObject();
        uploader = uploaderObj.AddComponent<VimeoUploader>();
        uploader.Start();
        uploader.Init("xtokenx");
        testFile = new FileInfo(TEST_IMAGE_PATH);
    }

    [Test]
    public void Hides_From_Inspector()
    {
        Assert.AreEqual(uploader.hideFlags, HideFlags.HideInInspector);
    }

    [Test]
    public void Init_Works()
    {
        uploader.Init("xtokenx", 10000);
        
        Assert.IsNotNull(uploader.chunks);
        Assert.AreEqual(uploader.token, "xtokenx");
        Assert.AreEqual(uploader.max_chunk_size, 10000);
    }

    [Test]
    public void Init_Sets_Default_Chunk_Size()
    {
        Assert.AreEqual(uploader.max_chunk_size, 1024 * 1024 * 128);
    }

    [Test]
    public void Upload_Sets_File_Info()
    {
        uploader.Upload(TEST_IMAGE_PATH);
        Assert.AreEqual(uploader.file, TEST_IMAGE_PATH);
        Assert.IsNotNull(uploader.file_info);
    }

    [Test]
    public void CreateChunks_Makes_Multiple_Chunks()
    {
        uploader.Init("xtokenx", 1000);
        uploader.CreateChunks("xxx", testFile, "xxx");
        Assert.AreEqual(uploader.chunks.Count, 3);
    }

    [Test]
    public void CreateChunks_Makes_One_Chunk_For_Small_Files()
    {
        uploader.Init("xtokenx", 100000000);
        uploader.CreateChunks("xxx", testFile, "xxx");
        Assert.AreEqual(uploader.chunks.Count, 1);
        Assert.AreEqual(uploader.chunks.Peek().chunk_size, testFileSize);
    }

    [Test]
    public void CreateChunks_Properly_Sets_Up_Chunk()
    {
        uploader.CreateChunks("video file path", testFile, "upload url");
        Assert.AreEqual(uploader.chunks.Peek().url, "upload url");
        Assert.AreEqual(uploader.chunks.Peek().file_path, "video file path");
    }

    [Test]
    public void CreateChunks_Sets_Size_Of_Each_Chunk()
    {
        uploader.Init("xtokenx", 1234);
        uploader.CreateChunks("xxx", testFile, "xxx");
        Assert.AreEqual(uploader.chunks.Peek().chunk_size, 1234);
    }

    [Test]
    public void CreateChunks_Last_Chunk_Is_Remainder()
    {
        uploader.Init("xtokenx", 1000);
        uploader.CreateChunks("xxx", testFile, "xxx");
        Assert.AreEqual(
            uploader.chunks.ToArray()[uploader.chunks.Count - 1].chunk_size, 879
        );
    }

    [Test]
    public void UploadNextChunk_Dequeues()
    {
        uploader.CreateChunks(testFile.FullName, testFile, "xxx");

        int length = uploader.chunks.Count;
        uploader.UploadNextChunk();
        Assert.AreEqual(uploader.chunks.Count, length - 1);
    }

    // TODO setup way to load mock json file
    // [Test]
    // public void GetTusUploadLink_Works()
    // {
    //     string uploadLink = VimeoUploader.GetTusUploadLink(mockJson);
    //     Assert.AreEqual(uploadLink, "");    
    // }

    // [Test]
    // public void GetVideoPermlink_Works()
    // {
    //     string videoPermalink = VimeoUploader.GetVideoPermlink(mockJson);
    //     Assert.AreEqual(videoPermalink, "");
    // }

    [TearDown]
    public void _After()
    {
        UnityEngine.GameObject.DestroyImmediate(uploaderObj);
    }
}
