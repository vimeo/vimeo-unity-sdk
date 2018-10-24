using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System;
using System.Collections;
using System.Text.RegularExpressions;
using Vimeo.Recorder;

public class EncoderManagerTest : TestConfig 
{
    GameObject obj;
    EncoderManager encoder;
    VimeoRecorder recorder;
    GameObject asset;

    [SetUp]
    public void _Before()
    {
        obj = new GameObject();
        encoder = obj.AddComponent<EncoderManager>();
        recorder = obj.AddComponent<VimeoRecorder>();
    }

#if VIMEO_LOOKING_GLASS_SUPPORT && UNITY_2017_3_OR_NEWER
    [Test]
    public void Init_LookingGlass_Render_Texture_Doesnt_Overide()
    {
        var holoplay = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/HoloPlay/HoloPlay Capture.prefab", typeof(GameObject));
        asset = GameObject.Instantiate(holoplay, Vector3.zero, Quaternion.identity);

        recorder.captureLookingGlassRT = false;
        Assert.AreEqual(recorder.renderTextureTarget, null);
        encoder.Init(recorder);
        Assert.AreEqual(recorder.renderTextureTarget, null);
    }

    [Test]
    public void Init_LookingGlass_Render_Texture_Is_Overidden()
    {
        var holoplay = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/HoloPlay/HoloPlay Capture.prefab", typeof(GameObject));
        asset = GameObject.Instantiate(holoplay, Vector3.zero, Quaternion.identity);

        recorder.captureLookingGlassRT = true;
        Assert.AreEqual(recorder.renderTextureTarget, null);
        encoder.Init(recorder);
        Assert.AreNotEqual(recorder.renderTextureTarget, null);
    }

    [Test]
    public void Init_LookingGlass_Throws_Error_With_Missing_Quilt()
    {
        UnityEngine.TestTools.LogAssert.Expect(LogType.Error, new Regex("HoloPlay SDK was not found."));
        recorder.captureLookingGlassRT = true;
        encoder.Init(recorder);
    }

    [Test]
    public void Init_LookingGlass_Doesnt_Throw_Error_With_Missing_Quilt()
    {
        UnityEngine.TestTools.LogAssert.NoUnexpectedReceived();
        recorder.captureLookingGlassRT = false;
        encoder.Init(recorder);
    }

#if VIMEO_AVPRO_CAPTURE_SUPPORT
    [Test]
    public void Init_LookingGlass_AVPro_Movie_Capture_Works()
    {
        var holoplay = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/HoloPlay/HoloPlay Capture.prefab", typeof(GameObject));
        asset = GameObject.Instantiate(holoplay, Vector3.zero, Quaternion.identity);

        var avpro = obj.AddComponent<RenderHeads.Media.AVProMovieCapture.CaptureFromTexture>();
        recorder.encoderType = EncoderType.AVProMovieCapture;
        recorder.avproEncoder = avpro;
        recorder.captureLookingGlassRT = true;

        Assert.AreEqual(recorder.renderTextureTarget, null);
        encoder.Init(recorder);
        Assert.AreNotEqual(recorder.renderTextureTarget, null);
    }

    [Test]
    public void Init_LookingGlass_AVPro_Movie_Capture_Fails_With_Wrong_Compnent()
    {
        UnityEngine.TestTools.LogAssert.Expect(LogType.Error, new Regex("you need to use the CaptureFromTexture"));
        var holoplay = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/HoloPlay/HoloPlay Capture.prefab", typeof(GameObject));
        asset = GameObject.Instantiate(holoplay, Vector3.zero, Quaternion.identity);

        var avpro = obj.AddComponent<RenderHeads.Media.AVProMovieCapture.CaptureFromScreen>();
        recorder.encoderType = EncoderType.AVProMovieCapture;
        recorder.avproEncoder = avpro;
        recorder.captureLookingGlassRT = true;

        encoder.Init(recorder);
    }
#endif

    
#endif


    [TearDown]
    public void _After()
    {
        UnityEngine.GameObject.DestroyImmediate(asset);
    }
}
