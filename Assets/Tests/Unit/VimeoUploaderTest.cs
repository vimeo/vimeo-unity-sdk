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
        Assert.AreEqual(uploader.maxChunkSize, 10000);
    }

    [Test]
    public void Init_Sets_Default_Chunk_Size()
    {
        Assert.AreEqual(uploader.maxChunkSize, 1024 * 1024 * 128);
    }

    [Test]
    public void Upload_Sets_File_Info()
    {
        uploader.Upload(TEST_IMAGE_PATH);
        Assert.AreEqual(uploader.file, TEST_IMAGE_PATH);
        Assert.IsNotNull(uploader.fileInfo);
    }

    [Test]
    public void CreateChunks_Makes_Multiple_Chunks()
    {
        uploader.Init("xtokenx", 1000);
        uploader.CreateChunks(testFile, "xxx");
        Assert.AreEqual(uploader.chunks.Count, 3);
    }

    [Test]
    public void CreateChunks_Makes_One_Chunk_For_Small_Files()
    {
        uploader.Init("xtokenx", 100000000);
        uploader.CreateChunks(testFile, "xxx");
        Assert.AreEqual(uploader.chunks.Count, 1);
        Assert.AreEqual(uploader.chunks[0].chunkSize, testFileSize);
    }

    [Test]
    public void CreateChunks_Properly_Sets_Up_Chunk()
    {
        uploader.CreateChunks(testFile, "upload url");
        Assert.AreEqual(uploader.chunks[0].url, "upload url");
        Assert.AreEqual(uploader.chunks[0].filePath, new FileInfo(TEST_IMAGE_PATH).FullName);
    }

    [Test]
    public void CreateChunks_Sets_Size_Of_Each_Chunk()
    {
        uploader.Init("xtokenx", 1234);
        uploader.CreateChunks(testFile, "xxx");
        Assert.AreEqual(uploader.chunks[0].chunkSize, 1234);
    }

    [Test]
    public void CreateChunks_Last_Chunk_Is_Remainder()
    {
        uploader.Init("xtokenx", 1000);
        uploader.CreateChunks(testFile, "xxx");
        Assert.AreEqual(
            uploader.chunks.ToArray()[uploader.chunks.Count - 1].chunkSize, 879
        );
    }

    [Test]
    public void Upload_Sets_Upload_States()
    {
        Assert.IsFalse(uploader.isUploading);
        uploader.Init("xtokenx", 1000);
        uploader.CreateChunks(testFile, "xxx");
        uploader.Upload(testFile.FullName);
        Assert.IsTrue(uploader.isUploading);
    }

    [Test]
    public void GetTotalBytes_Returns_Zero_Even_If_No_Chunks_Are_Created()
    {
        uploader.Init("xtokenx", 1000);
        Assert.AreEqual(uploader.GetTotalBytesUploaded(), 0);
    }

    [Test]
    public void ClearAllChunks_Clears_All_Chunks()
    {
        uploader.Init("xtokenx", 1000);
        uploader.CreateChunks(testFile, "xxx");
        Assert.Greater(uploader.chunks.Count, 0);
        uploader.ClearAllChunks();
        Assert.AreEqual(uploader.chunks.Count, 0);
    }

    [Test]
    public void TotalChunksRemaining_Shows_All_Chunks_Before_Upload_Starts()
    {
        uploader.Init("xtokenx", 1000);
        uploader.CreateChunks(testFile, "xxx");
        Assert.AreEqual(uploader.chunks.Count, uploader.TotalChunksRemaining());
    }

    [Test]
    public void TotalChunksRemaining_Does_Not_Break_Before_Chunks_Are_Created()
    {
        uploader.Init("xtokenx", 1000);
        Assert.AreEqual(uploader.TotalChunksRemaining(), 0);
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
