using UnityEngine;
using Vimeo.Services;

namespace Vimeo.Config
{
    public class RecorderSettings : VimeoAuth
    {
        public enum LinkType
        {
            VideoPage,
            ReviewPage
        }

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
}