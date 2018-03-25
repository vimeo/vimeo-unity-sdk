using UnityEngine;
using System;
using Vimeo;
using Vimeo.Services;

namespace Vimeo.Recorder
{
    public enum LinkType
    {
        VideoPage,
        ReviewPage
    }
    
    public enum VideoInputType
    {
        Screen,
        Camera,
        Camera360
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
        x4320p_8K = 4320,
        // x2880p_5K = 2880,
        x2160p_4K = 2160,
        x1440P_QHD = 1440,
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

    public class RecorderSettings : VimeoAuth
    {
        public VimeoApi.PrivacyModeDisplay privacyMode = VimeoApi.PrivacyModeDisplay.Anyone;
        public LinkType defaultShareLink        = LinkType.VideoPage;

        public VideoInputType defaultVideoInput = VideoInputType.Screen;
        public CameraType defaultCamera         = CameraType.MainCamera;
        public Camera360Type defaultCamera360   = Camera360Type.MainCamera;
        public RenderMode360 defaultRenderMode360 = RenderMode360.Stereo;
        public Resolution defaultResolution     = Resolution.Window;        
        public AspectRatio defaultAspectRatio   = AspectRatio.x16_9;
        public RecordMode recordMode            = RecordMode.Manual;
        public int recordDuration               = 5;

        public bool recordAudio = true;
        public bool realTime = false;
        public int frameRate = 30;
        public bool recordOnStart = false;
        public bool openInBrowser = true;

        //public bool shareToSlack = false;
        public bool autoPostToChannel = true;
        public string slackToken;
        public string slackChannel;

        public string videoName;
        public string videoPermalink;
        public string videoPassword;
        public string videoReviewPermalink;

        private const string SLACK_TOKEN_NAME = "slack-token-";

        public string GetSlackToken()
        {
            return PlayerPrefs.GetString(SLACK_TOKEN_NAME + this.gameObject.scene.name);
        }

        public void SetSlackToken(string token)
        {
            SetKey(SLACK_TOKEN_NAME + this.gameObject.scene.name, token);
        }
    }

    public class AspectRatioHelper
    {
        public static float GetRealAR(AspectRatio aspectRatio)
        {
            switch (aspectRatio)
            {
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