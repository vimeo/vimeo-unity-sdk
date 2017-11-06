Vimeo Unity SDK
========================

The goal of the Vimeo Unity SDK is to solve the following:

  - Easily stream your Vimeo videos into your Unity app.
  - Easily record and publish video to Vimeo from in-editor or in-game.
  - Provide example scenes and design patterns to inspire or kickstart a project you are working on.

There is a lot we want to do with this SDK, but we have released this SDK sooner than later to get feedback from the community and learn more about what people are building. The SDK comes with the following example Unity scenes in the `Vimeo/scenes` folder:

  - **Player** - Simple video streaming playback with basic controls.
  - **360Player** - Demos how to stream 360 videos from Vimeo (supports 3D / stereoscopic).
  - **Publisher** - Demos how to record in-game footage and automatically upload to your Vimeo account. 
  
Check those scenes out if you want to jump right in or continue reading to get more details.

Usage
-------

There are two major components to the Vimeo Unity SDK. The `VimeoPlayer.cs` for playback and the `VimeoPublisher.cs` for recording and uploading to Vimeo. 

##### VimeoPlayer

> Note: The `VimeoPlayer` component requires video file access via the Vimeo API. Accessing video files is limited to [Vimeo Pro and Business](https://vimeo.com/upgrade) customers. 

##### VimeoPublisher 

The `VimeoPublisher` leverages Unity's [FrameCapturer](https://github.com/unity3d-jp/FrameCapturer) plugin for recording. 


  
Support
-------------

We do not support the following features yet, but we hope to in the future:
  
  - Streaming playback is limited to your own Vimeo videos. You can only stream your own Vimeo videos.
  - Adaptive video playback (HLS/DASH). The `VimeoPlayer` component does allow you to set your streaming quality preference.
  - Live streaming playback 
  - Live streaming recording
  - 360 recording


Contributing
-------------

There are many ways you can contribute. Report bugs. Make feature requests. If you see a problem, please file a [GitHub issue](https://github.com/vimeo/unity-vimeo-player/issues).
