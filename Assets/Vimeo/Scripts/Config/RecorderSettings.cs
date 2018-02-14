using UnityEngine;
using System;
using Vimeo.Services;

namespace Vimeo.Config
{
    public enum LinkType
    {
        VideoPage,
        ReviewPage
    }
    
    public enum InputType
    {
        Screen,
        Beta360,
        ActiveCamera,
        MainCamera
    }

    public enum Resolution
    {
        Window,
        //x8640p_16K = 8640,
        x4320p_8K = 4320,
        x2880p_5K = 2880,
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
        public VimeoApi.PrivacyMode privacyMode = VimeoApi.PrivacyMode.anybody;
        public LinkType defaultShareLink = LinkType.VideoPage;

        public bool recordOnStart = false;
        public bool openInBrowser = false;

        public bool shareToSlack = false;
        public bool autoPostToChannel = false;
        public string slackToken;
        public string slackChannel;

        public string videoName;
        public string videoPermalink;
        public string videoReviewPermalink;

        public string GetSlackToken()
        {
            var token = PlayerPrefs.GetString("slack-token");
            if (token == null || token == "") {
                if (slackToken != null && slackToken != "") {
                    SetSlackToken(slackToken);
                }
                token = slackToken;
            }

            slackToken = null;
            return token;
        }

        public void SetSlackToken(string token)
        {
            SetKey("slack-token", token);
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