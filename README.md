Vimeo Unity SDK
========================

The goal of the Vimeo Unity SDK is to solve the following:

  - Easily stream your Vimeo videos into your Unity app in any format.
  - Easily record and publish video to Vimeo from in-editor or in-game, in any format.
  - Provide example scenes and design patterns to inspire or kickstart a project you are working on.

There is a lot we want to do with this SDK, but we have released this SDK sooner than later to get feedback from the community and learn more about what people are building. The SDK comes with the following example Unity scenes:

  - **Player** - Simple video streaming playback with basic controls.
  - **Publisher** - Demos how to record in-game footage and automatically upload to your Vimeo account. 
  - **SteamVR-360Player** - Demos how to setup 360 playback and also supports stereoscopic 360.
  - **SteamVR-Player** - Demos how a user can select a video and control playback with their Oculus/Vive controller.
  - **SteamVR-Publisher** - Demos how a VR user might interact with a camera in-game and record what they see.


Usage
-------

There are two major components to the Vimeo Unity SDK. The `VimeoPlayer` for playback and the `VimeoPublisher` for recording and publishing to Vimeo. Before you can use either component, you will need to create a new [Vimeo Developer App](https://developer.vimeo.com/) and genereate a token with `Public`, `Private`, `Edit`, and `Upload` scopes. 

### VimeoPlayer

> Note: The `VimeoPlayer` component requires video file access via the Vimeo API. Accessing video files is limited to [Vimeo Pro and Business](https://vimeo.com/upgrade) customers. 

### VimeoPublisher 

  
  
Support
-------------

Unfortunately we do not support the following features yet, but we hope to in the future:
  
  - Simpler authentication process. 
  - Streaming playback is limited to your own Vimeo videos. You can only stream your own Vimeo videos
  - Adaptive video playback (HLS/DASH). However, the `VimeoPlayer` component allows you to set your streaming quality preference.
  - Live streaming playback and publishing


Contributing
-------------

There are many ways you can contribute.
  - Report bugs. If you see a problem, please file a [GitHub issue](https://github.com/vimeo/unity-vimeo-player/issues).
  - Make feature requests! This
  -
