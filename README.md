Vimeo Unity SDK
========================

The goal of the Vimeo Unity SDK is to make it easy for Unity developers to incorporate Vimeo into their Unity app. Some example use cases:

  - Easily stream 2D or 360/3D videos from your Vimeo account into your Unity app.
  - Record and publish snippets from your work-in-progress game to [share with your team for creative feedback](https://join.vimeo.com/video-review/).
  
Quickstart
------

There is a lot we want to do with this SDK, but we have released this SDK sooner than later to get feedback from the community and learn more about what people are building. The SDK comes with the following example Unity scenes in the `Vimeo/scenes` folder:

  - **Player** - Demos simple video streaming playback with basic controls.
  - **360Player** - Demos how to setup 360 playback and also supports stereoscopic 360.
  - **Publisher** - Demos how to record in-game footage and automatically upload to your Vimeo account. 
  
Check those scenes out if you want to jump right in or continue reading to get more details.

Usage
-------

There are two major components to the Vimeo Unity SDK. The `VimeoPlayer` for video playback and the `VimeoPublisher` for recording and publishing to Vimeo. Before you can use either of these components, you will need an [account with Vimeo](https://vimeo.com/) and be logged into Vimeo. Both components will ask you to sign in before using which will look like this: 

![image](https://user-images.githubusercontent.com/156097/33294893-a7064d02-d3a0-11e7-91be-9eeda7f5151f.png)

Clicking "Sign in to Vimeo" will ask you to authorize with your Vimeo account. Afterward it will ask you to copy the token and paste it into Unity.

#### VimeoPlayer

> Note: The `VimeoPlayer` component requires video file access via the Vimeo API. Accessing video files is limited to [Vimeo Pro and Business](https://vimeo.com/upgrade) customers. 


##### VimeoPublisher 

The `VimeoPublisher` leverages Unity's new [MediaEncoder](https://docs.unity3d.com/2017.3/Documentation/ScriptReference/Media.MediaEncoder.html) plugin for recording. `MediaEncoder` is only available in `2017.03b`, so you will need the [latest Unity beta](https://unity3d.com/unity/beta) if you want to use the recording feature.
  
Future support
-------------

We do not support the following features yet, but we hope to in the future:
  
  - Adaptive video playback (HLS/DASH). The `VimeoPlayer` component does allow you to set your streaming quality preference.
  - Live streaming playback 
  - Live streaming recording
  - 360 recording 
  - You can only stream videos from your own Vimeo account. Access to all videos is limited to partnership-level integrations. If you are interested in a partnership, reach out to casey@vimeo.com


Contributing
-------------

There are many ways you can contribute. Report bugs. Make feature requests. File them all as a [GitHub issue](https://github.com/vimeo/unity-vimeo-player/issues).
