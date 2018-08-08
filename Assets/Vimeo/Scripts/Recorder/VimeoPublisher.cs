using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Vimeo;
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
        private VimeoVideo video;

        private Coroutine saveCoroutine;

        public void Init(VimeoRecorder _recorder) 
        {
            recorder = _recorder;
            
            if (vimeoApi == null) {
                vimeoApi = gameObject.AddComponent<VimeoApi>();
                vimeoApi.OnPatchComplete  += VideoUpdated;
                vimeoApi.OnUploadComplete += UploadComplete;
                vimeoApi.OnUploadProgress += UploadProgress;
                vimeoApi.OnError          += ApiError;
                vimeoApi.OnNetworkError   += NetworkError;

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
             
            return "https://vimeo.com/" + video.id;
        }

        public void PublishVideo(string filename)
        {
            Debug.Log("[VimeoRecorder] Uploading to Vimeo");            
            vimeoApi.UploadVideoFile(filename);
        }


        private void UploadProgress(string status, float progress)
        {
            if (OnUploadProgress != null) {
                OnUploadProgress(status, progress);
            }
        }

        private void UploadComplete(string video_uri)
        {
            UploadProgress("SavingInfo", .999f);

            video = new VimeoVideo("", video_uri);

#if UNITY_2018_1_OR_NEWER
            if (recorder.defaultVideoInput == VideoInputType.Camera360) {
                vimeoApi.SetVideoSpatialMode("equirectangular", recorder.defaultRenderMode360 == RenderMode360.Stereo ? "top-bottom" : "mono");
            }
#endif

            vimeoApi.SetVideoDescription("Recorded and uploaded with the Vimeo Unity SDK: https://github.com/vimeo/vimeo-unity-sdk");
            if (recorder.enableDownloads == false) {
                vimeoApi.SetVideoDownload(recorder.enableDownloads);
            }
            vimeoApi.SetVideoComments(recorder.commentMode);
            // vimeoApi.SetVideoReviewPage(recorder.enableReviewPage);
            SetVideoName(recorder.GetVideoName());

            if (recorder.privacyMode == VimeoApi.PrivacyModeDisplay.OnlyPeopleWithAPassword) {
                vimeoApi.SetVideoPassword(recorder.videoPassword);
            }
            SetVideoPrivacyMode(recorder.privacyMode);
        }

        private void VideoUpdated(string response)
        {
            JSONNode json = JSON.Parse(response);
            recorder.videoPermalink = json["link"];
            recorder.videoReviewPermalink = json["review_link"];

            if (recorder.openInBrowser == true) {
                OpenVideo();
            }

            if (recorder.autoPostToChannel == true) {
                PostToSlack();
            }

            if (recorder.currentFolder.uri != null) {
                vimeoApi.AddVideoToFolder(video, recorder.currentFolder);
            }

            UploadProgress("SaveInfoComplete", 1f);
        }

        private void NetworkError(string response){
            Debug.LogError("It seems like you are not connected to the internet, or having connection problems which disables the uploading of the video.");
        }

        private void ApiError(string response)
        {
            JSONNode json = JSON.Parse(response);

            if (json["invalid_parameters"] != null) {
                for (int i = 0; i < json["invalid_parameters"].Count; i++) {
                    // TODO use .Value
                    if (json["invalid_parameters"][i]["field"].ToString() == "\"privacy.download\"") {
                        Debug.LogError("You must upgrade your Vimeo account in order to disable downloads on your video. https://vimeo.com/upgrade");
                    }
                    else if (json["invalid_parameters"][i]["field"].ToString() == "\"privacy.view\"") {
                        Debug.LogError("You must upgrade your Vimeo account in order to access this privacy feature. https://vimeo.com/upgrade");
                    }
                    else {
                        Debug.LogError(json["invalid_parameters"][i]["field"] + ": " + json["invalid_parameters"][i]["error"]);
                    }
                }
            }
            UploadProgress("SaveInfoComplete", 1f);
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

            if (video != null) {
                vimeoApi.SaveVideo(video);
            }
        }

        public void OpenVideo()
        {
            Application.OpenURL(GetVimeoPermalink());
        }

        public void OpenSettings()
        {
            Application.OpenURL("https://vimeo.com/" + video.id + "/settings");
        }

        public void PostToSlack()
        {
            if (recorder.slackChannel != null) {
                if (recorder.GetSlackToken() != null && recorder.GetSlackToken() != "" && recorder.slackChannel != "") {
                    slackApi.Init(recorder.GetSlackToken(), recorder.slackChannel);
                    slackApi.PostVideoToChannel(recorder.GetVideoName(), GetVimeoPermalink());
                }
            }
        }

        void OnDestroy()
        {
            Dispose();            
        }

        public void Dispose()
        {
            Destroy(vimeoApi);
            Destroy(slackApi);
        }
    }
}