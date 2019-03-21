using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System;
using System.Collections;
using System.Text.RegularExpressions;
using Vimeo;

public class UUIDGeneratorTest : TestConfig
{
    UUIDGenerator uuid;

    [SetUp]
    public void _Before()
    {
        // Generate some fake request uuids for session and video
        uuid = new UUIDGenerator();
    }

    [Test]
    public void Generate_Generates_A_UUID_At_Specific_Length()
    {
        string test = uuid.Generate(500);
        Assert.AreEqual(test.Length, 500);
    }
}
