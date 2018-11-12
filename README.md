<h1 align="center">Vimeo Unity SDK</h1>

This plugin has two major features:

* **[Record video](#recording)** (including 4K, 360, stereo 360) and upload to your Vimeo account. 
  * [Features](#features)
  * [Requirements](#requirements)
  * [Quickstart](#getting-started)

* **[Stream video](#streaming)** (4K, 360, stereo 360) from your own Vimeo account into Unity.
  * [Features](#streaming-features)
  * [Requirements](#streaming-requirements)
  * [Quickstart](#setting-up)

* **[Vimeo & AVPro](#vimeo--avpro)**
  * [Streaming with AVPro Video and Vimeo](#streaming-with-avpro-video-and-vimeo)
  * [Recording with AVPro Movie Capture and Vimeo](#recording-with-avpro-movie-capture-and-vimeo)

# Installation
Download the most recent `.unitypackage` from the [latest releases](https://github.com/vimeo/vimeo-unity-sdk/releases) and open it up.

# Recording

<img src="https://i.imgur.com/tnP8Rl7.gif" height="250" /> <img src="https://i.imgur.com/blyIiks.gif" height="250" />

## Features
The Vimeo Recorder is built on top of Unity's native `MediaEncoder` to make it simple to capture your Unity scene and share to Vimeo. Features include:
* Easy Vimeo uploading: Customize your video title, privacy levels, downloads, and which project to add it to.
* Record up to 4K video
* Record 360 and stereo 360 video and publish to Vimeo's 360 platform
* Record from the Unity Editor on Windows and Mac. 
* Capture from Screen, Camera, or Render Texture
* Adjustable capture frame rate
* Record in real-time or render offline
* Control recording length manually, by duration, or via a custom script
* _\***NEW**\*_ The Vimeo Recorder is now compatiable with [AVPro Movie Capture](http://renderheads.com/product/av-pro-movie-capture/). You should go check out their full feature list, but here are some of the most notable improvements:
  * Extremely robust & higher quality video recording 
  * Works in the editor and also in standalone builds
  
## Requirements
* Requires a basic [Vimeo account](https://vimeo.com) or higher. 
* Unity `2017.2` or higher.

## Getting started

### 1. Setup

We provide a simple demo scene in the `Vimeo/Scenes` folder called `Recorder`. The `Recorder` scene contains the prefab, `[VimeoRecorder]`. Select it and in the component inspector you'll see the recording settings.

In order to add the Vimeo recorder in your own Unity scene simply go to the menu, `GameObject > Video > Vimeo Recorder`. You can also right-click in your Game Hierarchy and go to `Video > Vimeo Recorder`.

<img src="https://i.imgur.com/cgagVAh.gif" width="300" />

Sign into your Vimeo account by clicking `Get Token`. After you authorize Unity with Vimeo, copy the token, paste it into the `Vimeo Token` field, and click `Sign into Vimeo`.

### 2. Recording
Once you're signed in, all you have to do is click the `Start Recording` button. When you're done recording, click `Finish & Upload`.  

<img src="https://i.imgur.com/iEyhwBD.gif" />

## Control recording via a script
Probably the most powerful part about this plugin is that you can control recording via a script. Below is a simple code snippet on how you would do that. 

```csharp
using UnityEngine;
using Vimeo.Recorder;

public class CustomRecorder : MonoBehaviour {
  VimeoRecorder recorder;

  void Start() {
    recorder = gameObject.GetComponent<VimeoRecorder>();
    recorder.defaultVideoInput = Vimeo.Recorder.VideoInputType.Camera;		
    recorder.defaultResolution = Vimeo.Recorder.Resolution.x2160p_4K;
    recorder.defaultAspectRatio = Vimeo.Recorder.AspectRatio.x16_9;
    recorder.frameRate = 60;
    recorder.recordMode = Vimeo.Recorder.RecordMode.Duration;
    recorder.recordDuration = 5; // in seconds
    recorder.autoUpload = true;

    recorder.videoName = "My Custom Video";
    recorder.privacyMode = Vimeo.Services.VimeoApi.PrivacyModeDisplay.HideThisFromVimeo;
    recorder.autoPostToChannel = false; 

    recorder.OnUploadComplete += UploadComplete;

    recorder.BeginRecording();
  }
  
  void UploadComplete() 
  {
    Debug.Log("Uploaded to Vimeo!");
    // Now you could change the scene and then immediately queue up another recording.
    // ...
    // recorder.BeginRecording();
  }
}

```

In this sample, `CustomRecorder` is assuming that it is a part of the same GameObject as the `VimeoRecorder`. All the settings that are avaiable in the editor GUI can be controlled via code.

# Streaming
The SDK comes with the following example Unity scenes in the `Vimeo/Scenes` folder. Please note these demos don't work out of the box. You will need to authorize your Vimeo account and select your own video test. 

<table>
  <tr>
    <td style="width:33%">
      <img src="https://i.imgur.com/bulVmhe.gif" height="200" /><br>
      <b>Player</b> - Demos simple video streaming inside of a 3D environment.
    </td>
    <td style="width:33%">
      <img src="https://i.imgur.com/GJsNC64.gif" height="200" /><br>
      <b>360Player</b> - Demos how to stream 360 videos from Vimeo (supports 3D / stereoscopic video).
    </td>
    <td style="width:33%">
      <img src="https://i.imgur.com/gNGweB0.gif" height="200" /><br>
      <b>CanvasPlayer</b> - Demos how to build a canvas-based player with basic playback controls.
    </td>
  </tr>
</table>

## Streaming Features
* Vimeo Streaming leverages Unity's native `VideoPlayer` behind the scenes.
* 4K video (on supported hardware)
* 360 and stereo 360 video (equirectangular)
* Select your preferred streaming video quality
* Search for your Vimeo video within the Unity Editor, or manually specify via URL
* Easy to use GUI and drag & drop components
* _\***NEW**\*_ We are now compatible with [AVPro Video](http://renderheads.com/product/avpro-video/). You should go check out their full feature list, but here are some of the most notable improvements:
  * Versions for iOS, tvOS, macOS, Android, WebGL, Windows, Windows Phone and UWP
  * Adaptive video support. Vimeo supports HLS & DASH streaming. And with [Vimeo Live](https://vimeo.com/live) you can even livestream into Unity.
  * 8K video (on supported hardware)
  * VR support (mono, stereo, equirectangular and cubemap)
  

## Streaming Requirements
* Unity `5.6.0` or higher.
* Streaming Vimeo videos requires video file access via the Vimeo API. Accessing video files is limited to [Vimeo Pro and Business](https://vimeo.com/upgrade) customers. 
* You can only stream videos from your own Vimeo account. Access to all videos is limited to partnership-level integrations. If you are interested in a partnership, reach out to labs@vimeo.com

## Setting up
### 1. Choose your playback method
Open up your Unity scene or create a new one. For streaming any Vimeo video, you will be using the `VimeoPlayer` component. There are two ways to setup the Vimeo player in Unity:

#### Prefab setup
The easiest way to get started is to either drag in one of the prefabs from `Vimeo/Resources/Prefabs` or simply right-click on your scene and the Vimeo Player from the `Video` menu:

<img src="https://i.imgur.com/LYO4k95.gif" width="500" />

#### Manual setup 
If you are setting up your own scene and want to specify your video "screen" and audio outputs, you can add the `Vimeo Player` component to your GameObject. 

<img src="https://i.imgur.com/F9aGHYD.gif"  />

### 2. Sign into your Vimeo account
Sign into your [Vimeo](https://vimeo.com) account (or create one if you haven't already) and make sure you have some videos uploaded. 

<img src="https://i.imgur.com/9aLNQJQ.gif"  />

* Click the "Get Token" button which will ask you to authorize with your Vimeo account. 
* After authorizing, copy the token and paste it into the `Vimeo Token` field in Unity. 
* [Find your video](https://vimeo.com/manage/videos) and then copy the full URL or the video ID and paste it into the `Vimeo Video URL` field.
* If you are not using one of the existing prefabs, then you will need to assign a GameObject to the `Video Screen` field. Currently, we only support Mesh and RawImage GameObjects.
* `Audio Output` is optional. If left empty, a new audio object will be attached to the video screen. 
* For security reasons, the Vimeo token is not saved into the scene. But when you build it, the token will be included in the build itself automatically.
 
 You're all set! Press play and you should now see your Vimeo video streaming in your app.

# Vimeo & AVPro
If you're looking for an even more robust streaming and/or recording solution, we have integrated into AVPro's [Video](http://renderheads.com/product/avpro-video/) and [Movie Capture](http://renderheads.com/product/av-pro-movie-capture/) plugins. 

After installing either of AVPro's tools, the Vimeo plugin will automatically detect AVPro and a new option will appear.

### Streaming with AVPro Video and Vimeo
The `Vimeo Player` component will now show a `Video Player` dropdown letting you to switch between Unity's native video player and `AVPro Video`. After selecting AVPro, you will need to assign their `Media Player` to the Vimeo Player component. Now when you run Unity, the Vimeo Player will automatically set the URL in the `Media Player`.

<img src="https://i.imgur.com/hxnYP7L.png" />

### Recording with AVPro Movie Capture and Vimeo
The `Vimeo Recorder` component will now show a `Video Encoder` dropdown letting yo uswitch between Unity's native `Media Encoder` and `AVPro Movie Capture`. Select AVPro Movie Capture and assign the Capture component. Now when you record with AVPro's tools, Vimeo will automatically upload the video after it is finished recording.

<img src="https://i.imgur.com/8L1KIYQ.png" />

# Questions, help, and support
For questions and support, [ask on StackOverflow](https://stackoverflow.com/questions/ask/?tags=vimeo). If you found a bug, please file a [GitHub issue](https://github.com/vimeo/unity-vimeo-player/issues).

Make pull requests, file bug reports, and make feature requests via a [GitHub issue](https://github.com/vimeo/unity-vimeo-player/issues).

# Let's collaborate
Working on a cool video project? Let's talk - labs at vimeo dot com

# Thanks
Big thanks to the Unity teams building [MediaEncoder](https://docs.unity3d.com/2018.1/Documentation/ScriptReference/Media.MediaEncoder.html), [GenericFrameRecorder](https://github.com/Unity-Technologies/GenericFrameRecorder), [SkyboxPanoramicShader](https://github.com/Unity-Technologies/SkyboxPanoramicShader), and [DepthKit](http://www.depthkit.tv/)
