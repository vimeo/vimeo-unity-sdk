using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System;
using System.Collections;
using System.Text.RegularExpressions;
using Vimeo;

public class PlayLoggingTest : TestConfig
{
    UUIDGenerator uuid;
    string vuid;
    string sessionId;
    string testConcatString;
    PlayLogging log;

    [SetUp]
    public void _Before()
    {
        // Generate some fake request uuids for session and video
        uuid = new UUIDGenerator();
        sessionId = uuid.Generate(40);
        vuid = uuid.Generate(255);

        testConcatString = "{\"session_id\":\"" + sessionId +  "\",\"furthest_watched_time_code\":15.0,\"exit_watched_time_code\":15.0,\"vuid\":\"" + vuid + "\",\"locale\":\"en_US\"}";

        log = new PlayLogging(sessionId, 15.0f, 15.0f, vuid);
    }

    [Test]
    public void Can_Create_JSON_String_From_Serialized_Class()
    {
        Assert.AreEqual(JsonUtility.ToJson(log) ,testConcatString);
    }

    [Test]
    public void Can_Create_Serialized_Class_From_JSON_String()
    {
        PlayLogging stringToClass = JsonUtility.FromJson<PlayLogging>(testConcatString);
        Assert.AreEqual(stringToClass.vuid, vuid);
        Assert.AreEqual(stringToClass.session_id, sessionId);
        Assert.AreEqual(stringToClass.exit_watched_time_code, 15.0f);
        Assert.AreEqual(stringToClass.furthest_watched_time_code, 15.0f);
        Assert.AreEqual(stringToClass.locale, "en_US");
    }

}
