<h1 align="center">Vimeo Unity SDK</h1>

This plugin has two major features:

* **[Record video](#recording)** (including 4K, 360, stereo 360) and publish to your Vimeo account and Slack. 
* **[Stream 2D and 360 videos](#streaming)** from your own Vimeo account into your Unity app.


# Recording

<img src="https://i.imgur.com/tnP8Rl7.gif" height="300" /> <img src="https://i.imgur.com/blyIiks.gif" height="300" />

## Features

🧠 **Your new creative workflow.** Getting creative feedback from your team, friends, or community can be difficult and usually involves using 3rd party software to capture your experience. Leveraging this plugin and [Vimeo's review tools](https://vimeo.com/blog/post/collaboration-meet-efficiency-a-new-way-to-review), you can easily get feedback on your project.

💬 **Slack!** Automatically post your video to a Slack channel of your choice after it's done recording.

❤️ **It's simple.** There are a lot of overly complicated recording plugins. We strived to make it as minimalist as possible. With a single click, your recording will publish to Vimeo and Slack!

🌐 **It's cross-platform.** Recording works on both Windows & Mac.

⬆️ **Publish to your social channels.** Once your video is on Vimeo, you can easily publish to both Facebook and YouTube.

## Requirements
* Requires Unity `2018.1` or higher.
* Requires a [Vimeo account](https://vimeo.com). 

## Getting started

### 1. Installation
Download the most recent `.unitypackage` from the [latest releases](https://github.com/vimeo/vimeo-unity-sdk/releases) and open it up.

### 2. Setup

We provide a simple demo scene in the `Vimeo/Scenes` folder called `Recorder`. The `Recorder` scene contains the prefab, `[VimeoRecorder]`. Select it and in the component inspector you'll see the recording settings.

> Note that in order to record you will need to authorize your Vimeo account. [Learn how to sign in.](#3-sign-into-your-vimeo-account)

In order to add the Vimeo recorder in your own Unity scene simply go to the menu, `GameObject > Video > Vimeo Recorder`. You can also right-click in your Game Hiearchy and go to `Video > Vimeo Recorder`.

<img src="https://i.imgur.com/cgagVAh.gif" width="300" />

### 3. Recording
Once you are signed in, all you have to do is click the button, `Start Recording`. When you're done recording, click `Finish & Upload`.  

<img src="https://i.imgur.com/iEyhwBD.gif" />

## Recording & Uploading Info
Here are some more details about all the recording & publishing features.

#### Recording settings
* **Input** - Select where you want to record from.
  * `Screen` - Whatever is being displayed to the game screen
  * `MainCamera` - Records from the MainCamera GameObject
  * `MainCamera (360)` - Records from the MainCamera and converts into a 360, equirectangular format. Supports both mono and stereo.
* **Resolution** - Select `Window` to default to whatever the resolution of your game window is already. Otherwise, you can select from various resolutions, up to 4K.
* **Frame Rate** - The number of frames per second that will be recorded. Defaults to `30`. Vimeo supports up to `60` FPS.
* **Real Time** - If you'd like to record in real-time, without slowing down game playback, then turn this on. Note: Depending on your app and your hardware, frames might be dropped. The lower the resolution, the easier it is to record in real-time.
* **Record Audio** - You have the option to disable audio recording.
* **Record Mode** - There are two ways to control recording:
  * `Manual` - You determine when you'd like to stop recording by manually clicking "Finish & Done"
  * `Seconds` - Choose how many seconds you'd like to record for. This makes it simple to do a one-click record & upload to Vimeo.

#### Vimeo settings
* **Video Name** - Specify the title of the video you want your Vimeo video to have
* **Privacy Mode** - Choose what privacy level you'd like to set your Vimeo video to. [Learn more.](https://help.vimeo.com/hc/en-us/articles/224817847-Privacy-settings-overview) Note: Currently supports a limited number of Vimeo's privacy levels.
* **Open In Browser** - After uploading to Vimeo, this will automatically open up the video in your default browser.

#### Slack settings
Before you can automatically post to a Slack channel, you'll need to authorize Vimeo with your Slack team. Click the `Get Token` button, authorize Slack, and then copy & paste the token into the `Slack Token` field.
* **Slack Channel** - The name of the channel you want to post to. e.g. `#general`
* **Share Link** - Choose whether you want to share the standard Vimeo video page, or a [review link](https://vimeo.com/blog/post/collaboration-meet-efficiency-a-new-way-to-review) to get feedback from your team.
* **Post to Channel** - A simple toggle to disable posting to Slack.


# Streaming

## Getting started

The SDK comes with the following example Unity scenes in the `Vimeo/Scenes` folder. Please note these demos don't work out of the box. You will need to authorize your Vimeo account and select your own video test. [Skip to step #3](#3-sign-into-your-vimeo-account) to learn how to sign in.

<table>
  <tr>
    <td style="width:33%">
      <img src="https://i.imgur.com/bulVmhe.gif" /><br>
      <b>Player</b> - Demos simple video streaming inside of a 3D environment.
    </td>
    <td style="width:33%">
      <img src="https://i.imgur.com/xAdsGvz.gif" /><br>
      <b>360Player</b> - Demos how to stream 360 videos from Vimeo (supports 3D / stereoscopic video).
    </td>
    <td style="width:33%">
      <img src="https://i.imgur.com/gNGweB0.gif" /><br>
      <b>CanvasPlayer</b> - Demos how to build a canvas-based player with basic playback controls.
    </td>
  </tr>
</table>

> Streaming Vimeo videos requires video file access via the Vimeo API. Accessing video files is limited to [Vimeo Pro and Business](https://vimeo.com/upgrade) customers. 

> You can only stream videos from your own Vimeo account. Access to all videos is limited to partnership-level integrations. If you are interested in a partnership, reach out to casey@vimeo.com

### 1. Installation
Download the most recent `.unitypackage` from the [latest releases](https://github.com/vimeo/vimeo-unity-sdk/releases) and open it up.

### 2. Choose your playback method
Open up your Unity scene or create a new one. For streaming any Vimeo video, you will be using the `VimeoPlayer` component. There are two ways to setup the Vimeo player in Unity:

#### Prefab setup
The easiest way to get started is to either drag in one of the prefabs from `Vimeo/Resources/Prefabs` or simply right-click on your scene and the Vimeo Player from the `Video` menu:

<img src="https://i.imgur.com/LYO4k95.gif" width="500" />

#### Manual setup 
If you are setting up your own scene and want to specify your video "screen" and audio outputs, you can add the `Vimeo Player` component to your GameObject. 

<img src="https://i.imgur.com/F9aGHYD.gif"  />

### 3. Sign into your Vimeo account
Sign into your [Vimeo](https://vimeo.com) account (or create one if you haven't already) and make sure you have some videos uploaded. Please read the following two requirements to streaming Vimeo videos into Unity:

<img src="https://i.imgur.com/P8F3A6y.gif"  />

* Click "Sign into Vimeo" which will ask you to authorize with your Vimeo account. 
* After authorizing, copy the token and paste it into the `Vimeo Token` field in Unity. 
* [Find your video](https://vimeo.com/manage/videos) and then copy the full URL or the video ID and paste it into the `Vimeo Video URL` field.
* If you are not using one of the existing prefabs, then you will need to assign a GameObject to the `Video Screen` field. Currently, we only support Mesh and RawImage GameObjects.
* `Audio Output` is optional. If left empty, a new audio object will be attached to the video screen. 
* For security reasons, the Vimeo token is not saved into the scene. But, if you plan on building this app so others can watch video, then be sure to check `Save token with scene`
 
 You're all set! Press play and you should now see your Vimeo video streaming in your app.

# Future support
We do not support the following features yet, but we hope to in the near future:
  
#### Streaming
* Adaptive video playback (HLS/DASH). The `VimeoPlayer` component does allow you to set your streaming quality preference.
* Vimeo Live streaming playback & publishing

#### Recording

# Questions
For questions and support, [ask on StackOverflow](https://stackoverflow.com/questions/ask/?tags=vimeo).

# Help & Contributing
Questions, comments, concerns? Make pull requestss or report bugs and make feature requests via a [GitHub issue](https://github.com/vimeo/unity-vimeo-player/issues).

# Thanks
Big thanks to the Unity teams building [MediaEncoder](https://docs.unity3d.com/2018.1/Documentation/ScriptReference/Media.MediaEncoder.html), [GenericFrameRecorder](https://github.com/Unity-Technologies/GenericFrameRecorder), and [SkyboxPanoramicShader](https://github.com/Unity-Technologies/SkyboxPanoramicShader) which this is built on top of.
