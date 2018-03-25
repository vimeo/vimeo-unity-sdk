#if UNITY_2017_3_OR_NEWER 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using Vimeo.Config;
using Vimeo.Services;
using SimpleJSON;

namespace Vimeo.Recorder
{
    public class VimeoPublisher : MonoBehaviour 
    {
        public delegate void UploadAction(string status, float progress);
        public event UploadAction OnUploadProgress;

        [HideInInspector] public VimeoRecorder recorder; // recorder contains all the settings

        private VimeoApi vimeoApi;
        private SlackApi slackApi;
        private string videoId;

        private Coroutine saveCoroutine;

        public void Init(VimeoRecorder _recorder) 
        {
            recorder = _recorder;
            
            if (vimeoApi == null) {
                vimeoApi = gameObject.AddComponent<VimeoApi>();
                vimeoApi.OnPatchComplete  += VideoUpdated;
                vimeoApi.OnUploadComplete += UploadComplete;
                vimeoApi.OnUploadProgress += UploadProgress;

                vimeoApi.token = recorder.GetVimeoToken();
            }

            if (slackApi == null) {
                slackApi = gameObject.AddComponent<SlackApi>();
            }
        }

        public string GetVimeoPermalink()
        {
            if (recorder.videoPermalink != null) {
                if (recorder.defaultShareLink == LinkType.ReviewPage) {
                    return recorder.videoReviewPermalink;
                } 
                else {
                    return recorder.videoPermalink;
                }
            } 
             
            return "https://vimeo.com/" + videoId;
        }

        public void PublishVideo(string filename)
        {
            vimeoApi.UploadVideoFile(filename);
        }


        private void UploadProgress(string status, float progress)
        {
            // Debug.Log("UploadProgress: " + status + " - "  + progress);
            if (OnUploadProgress != null) {
                OnUploadProgress(status, progress);
            }
        }

        private void UploadComplete(string video_uri)
        {
            UploadProgress("SavingInfo", .999f);

            string[] uri_pieces = video_uri.Split("/" [0]);
            videoId = uri_pieces [2];

            if (recorder.defaultVideoInput == VideoInputType.Camera360) {
                vimeoApi.SetVideoSpatialMode("equirectangular", recorder.defaultRenderMode360 == RenderMode360.Stereo ? "top-bottom" : "mono");
            }

            vimeoApi.SetVideoDescription("Recorded and uploaded with the Vimeo Unity SDK: https://github.com/vimeo/vimeo-unity-sdk");
            SetVideoName(recorder.recorder.GetVideoName());
            
            if (recorder.privacyMode == VimeoApi.PrivacyModeDisplay.OnlyPeopleWithAPassword) {
                vimeoApi.SetVideoPassword(recorder.videoPassword);
            }
            SetVideoPrivacyMode(recorder.privacyMode);
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

            UploadProgress("SaveInfoComplete", 1f);

            Dispose();
        }

        public void SetVideoName(string title)
        {
            if (title != null && title != "") {
                if (saveCoroutine != null) { StopCoroutine (saveCoroutine); } // DRY
                vimeoApi.SetVideoName(title);
                saveCoroutine = StartCoroutine("SaveVideo");
            }
        }

        public void SetVideoPrivacyMode(VimeoApi.PrivacyModeDisplay mode)
        {
            if (saveCoroutine != null) { StopCoroutine (saveCoroutine); }
            vimeoApi.SetVideoViewPrivacy(mode);
            saveCoroutine = StartCoroutine("SaveVideo");
        }

        private IEnumerator SaveVideo()
        {
            yield return new WaitForSeconds(1f);

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
            if (recorder.slackChannel != null) {
                if (recorder.GetSlackToken() != null && recorder.GetSlackToken() != "") {
                    slackApi.Init(recorder.GetSlackToken(), recorder.slackChannel);
                    slackApi.PostVideoToChannel(recorder.recorder.GetVideoName(), GetVimeoPermalink());
                }
                else {
                    Debug.LogWarning("You are not signed into Slack.");
                }
            }
        }

        public void Dispose()
        {
            Destroy(vimeoApi);
            Destroy(slackApi);
        }
    }
}

#endif