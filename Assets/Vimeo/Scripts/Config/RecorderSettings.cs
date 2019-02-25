using UnityEngine;
using System;
using System.Collections.Generic;

namespace Vimeo.Recorder
{
    public enum EncoderType
    {
        AVProMovieCapture,
        MediaEncoder
    }

    public enum LinkType
    {
        VideoPage,
        ReviewPage
    }

    public enum VideoInputType
    {
        Screen,
        Camera,
#if UNITY_2018_1_OR_NEWER
        Camera360,
#endif    
        RenderTexture
    }

    public enum CameraType
    {
        MainCamera,
        //ActiveCameras
    }

    public enum Camera360Type
    {
        MainCamera
    }

    public enum RenderMode360
    {
        Stereo,
        Mono
    }

    public enum RecordMode
    {
        Manual,
        Duration
    }

    public enum Resolution
    {
        Window,
        //x8640p_16K = 8640,
        // x4320p_8K = 4320,
        // x2880p_5K = 2880,
        x2160p_4K = 2160,
        x1440p_QHD = 1440,
        x1080p_FHD = 1080,
        x720p_HD = 720,
        x540p = 540,
        x360p = 360
    }

    public enum AspectRatio
    {
        x16_9,
        x16_10,
        x19_10,
        x5_4,
        x4_3
    }

    public class RecorderSettings : VimeoSettings
    {
        public EncoderType encoderType = EncoderType.MediaEncoder;
#if VIMEO_AVPRO_CAPTURE_SUPPORT        
        public RenderHeads.Media.AVProMovieCapture.CaptureBase avproEncoder;
#endif        
        public EncoderManager encoder;

        public RenderTexture renderTextureTarget;

        [TextArea(2, 5)]
        public string description;

        public bool replaceExisting = false;
        public VimeoApi.PrivacyModeDisplay privacyMode = VimeoApi.PrivacyModeDisplay.Anyone;
        public VimeoApi.CommentMode commentMode = VimeoApi.CommentMode.Anyone;
        public bool enableDownloads = true;
        public bool enableReviewPage = true;
        public LinkType defaultShareLink = LinkType.VideoPage;

        public VideoInputType defaultVideoInput = VideoInputType.Screen;
        public CameraType defaultCamera = CameraType.MainCamera;
        public Camera360Type defaultCamera360 = Camera360Type.MainCamera;
        public RenderMode360 defaultRenderMode360 = RenderMode360.Stereo;
        public Resolution defaultResolution = Resolution.Window;
        public AspectRatio defaultAspectRatio = AspectRatio.x16_9;
        public RecordMode recordMode = RecordMode.Manual;
        public int recordDuration = 5;

        public bool recordAudio = true;
        public bool realTime = false;
        public int frameRate = 30;
        public bool recordOnStart = false;
        public bool openInBrowser = true;
        public bool autoUpload = true; // Controls if a video is auto uploaded upon end of recording
        public bool captureLookingGlassRT = false;

        public string videoName;
        public string videoPermalink;
        public string videoPassword;
        public string videoReviewPermalink;

        public string GetVideoName()
        {
            return ReplaceSpecialChars(videoName);
        }

        public string ReplaceSpecialChars(string input)
        {
            if (input.Contains("%R") && encoder != null) {
                input = input.Replace("%R", encoder.GetOutputWidth() + "x" + encoder.GetOutputHeight());
            }

            if (input.Contains("%TS")) {
                input = input.Replace("%TS", String.Format("{0:yyyy.MM.dd H.mm.ss}", System.DateTime.Now));
            }

            return input;
        }

        public void SetVimeoIdFromName()
        {
            if (!string.IsNullOrEmpty(videoName))
            {
                for (int i = 0; i < vimeoVideos.Count; i++)
                {
                    var video = vimeoVideos[i];
                    if (video.GetVideoName() == videoName)
                    {
                        currentVideo = video;
                        vimeoVideoId = video.id.ToString();
                        description = video.description;
                        break;
                    }
                }
            }
        }

        public void SetVimeoVideoFromId()
        {
            if (!string.IsNullOrEmpty(vimeoVideoId))
            {
                for (int i = 0; i < vimeoVideos.Count; i++)
                {
                    var video = vimeoVideos[i];
                    if (video.id.ToString() == vimeoVideoId)
                    {
                        currentVideo = video;
                        videoName = video.GetVideoName();
                        description = video.description;
                        break;
                    }
                }
            }
        }
    }

    public class AspectRatioHelper
    {
        public static float GetRealAR(AspectRatio aspectRatio)
        {
            switch (aspectRatio) {
                case AspectRatio.x16_9:
                    return 16.0f / 9.0f;
                case AspectRatio.x16_10:
                    return 16.0f / 10.0f;
                case AspectRatio.x19_10:
                    return 19.0f / 10.0f;
                case AspectRatio.x5_4:
                    return 5.0f / 4.0f;
                case AspectRatio.x4_3:
                    return 4.0f / 3.0f;
                default:
                    throw new ArgumentOutOfRangeException("aspectRatio", aspectRatio, null);
            }
        }
    }
}