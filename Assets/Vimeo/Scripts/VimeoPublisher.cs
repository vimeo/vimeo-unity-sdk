#if UNITY_2017_3_OR_NEWER 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using Vimeo;
using Vimeo.Config;
using Vimeo.Services;
using SimpleJSON;

namespace Vimeo {

    [AddComponentMenu("Vimeo/Video Recorder")]
    public class VimeoPublisher : MonoBehaviour {
    
        public delegate void UploadAction(string status, float progress);
        public event UploadAction OnUploadProgress;

        public VimeoRecorder recorder; // recorder contains all the settings

        private VimeoApi vimeoApi;
        private SlackApi slackApi;
        private string videoId;

        private Coroutine saveCoroutine;

        void Init(VimeoRecorder _recorder) 
        {
            recorder = _recorder;
            
            if (vimeoApi == null) {
                vimeoApi = gameObject.AddComponent<VimeoApi>();
                vimeoApi.token = recorder.GetVimeoToken();
            }

            if (slackApi == null) {
                slackApi = gameObject.AddComponent<SlackApi>();
            }
        }

        public string GetVimeoPermalink()
        {
            if (recorder.videoPermalink != null) {
                if (recorder.defaultShareLink == RecorderSettings.LinkType.ReviewPage) {
                    return recorder.videoReviewPermalink;
                } 
                else {
                    return recorder.videoPermalink;
                }
            } 
             
            return "https://vimeo.com/" + videoId;
        }

        private void UploadVideo()
        {
            vimeoApi.UploadVideoFile(recorder.GetVideoFilePath());
        }


        private void UploadProgress(string status, float progress)
        {
            if (OnUploadProgress != null) {
                OnUploadProgress (status, progress);
            }
        }

        private void UploadComplete(string video_uri)
        {
            string[] uri_pieces = video_uri.Split("/" [0]);
            videoId = uri_pieces [2];

            if (vimeoApi != null) {
                vimeoApi.OnPatchComplete  += VideoUpdated;
                vimeoApi.OnUploadComplete += UploadComplete;
                vimeoApi.OnUploadProgress += UploadProgress;
            }
           
            SetVideoName(recorder.videoName);
            SetVideoPrivacyMode(recorder.privacyMode);

            Debug.Log("Video saved!");
            vimeoApi.SaveVideo(videoId);

            //recorder.DeleteVideoFile();
        }

        private void VideoUpdated(string response)
        {
            JSONNode json = JSON.Parse (response);
            recorder.videoPermalink = json["link"];
            recorder.videoReviewPermalink = json["review_link"];

            if (recorder.openInBrowser == true) {
                recorder.openInBrowser = false;
                OpenVideo();
            }

            if (recorder.autoPostToChannel == true) {
                recorder.autoPostToChannel = false;
                PostToSlack();
            }
        }

        public void SetVideoName(string title)
        {
            if (title != null && title != "") {
                if (saveCoroutine != null) { StopCoroutine (saveCoroutine); } // DRY
                vimeoApi.SetVideoName(title);
                saveCoroutine = StartCoroutine("SaveVideo");
            }
        }

        public void SetVideoPrivacyMode(VimeoApi.PrivacyMode mode)
        {
            if (saveCoroutine != null) { StopCoroutine (saveCoroutine); }
            vimeoApi.SetVideoViewPrivacy(mode.ToString());
            saveCoroutine = StartCoroutine("SaveVideo");
        }

        private IEnumerator SaveVideo()
        {
            yield return new WaitForSeconds(3f);

            if (videoId != null) {
                Debug.Log("Video saved!");
                vimeoApi.SaveVideo(videoId);
            }
        }

        public void OpenVideo()
        {
            Application.OpenURL(GetVimeoPermalink());
        }

        public void OpenSettings()
        {
            Application.OpenURL("https://vimeo.com/" + videoId + "/settings");
        }

        public void PostToSlack()
        {
            if (recorder.shareToSlack == true && recorder.slackChannel != null) {
                if (recorder.GetSlackToken() != null && recorder.GetSlackToken() != "") {
                    slackApi.Init(recorder.GetSlackToken(), recorder.slackChannel);
                    slackApi.PostVideoToChannel(recorder.videoName, GetVimeoPermalink());
                }
                else {
                    Debug.LogWarning("You are not signed into Slack.");
                }
            }
        }
    }
}

#endif