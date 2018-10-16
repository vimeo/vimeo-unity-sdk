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

        public delegate void RequestAction(string error_message);
        public event RequestAction OnNetworkError;

        [HideInInspector] public VimeoRecorder recorder; // recorder contains all the settings

        private VimeoUploader vimeoUploader;
        private VimeoVideo video;

        private Coroutine saveCoroutine;
        private void Start()
        {
            // this.hideFlags = HideFlags.HideInInspector;
        }
        public void Init(VimeoRecorder _recorder)
        {
            recorder = _recorder;

            if (vimeoUploader == null) {
                vimeoUploader = gameObject.AddComponent<VimeoUploader>();
                vimeoUploader.Init(recorder.GetVimeoToken());

                vimeoUploader.OnPatchComplete += VideoUpdated;
                vimeoUploader.OnUploadProgress += UploadProgress;
                vimeoUploader.OnUploadComplete += UploadComplete;
                vimeoUploader.OnNetworkError += NetworkError;
                vimeoUploader.OnError += ApiError;
            }
        }

        public string GetVimeoPermalink()
        {
            if (recorder.videoPermalink != null) {
                if (recorder.defaultShareLink == LinkType.ReviewPage) {
                    return recorder.videoReviewPermalink;
                } else {
                    return recorder.videoPermalink;
                }
            }

            return "https://vimeo.com/" + video.id;
        }

        public void PublishVideo(string filename)
        {
            Debug.Log("[VimeoRecorder] Uploading to Vimeo");
            vimeoUploader.Upload(filename);
        }
        void UploadProgress(string status, float progress)
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
                vimeoUploader.SetVideoSpatialMode("equirectangular", recorder.defaultRenderMode360 == RenderMode360.Stereo ? "top-bottom" : "mono");
            }
#endif

            vimeoUploader.SetVideoDescription("Recorded and uploaded with the Vimeo Unity SDK: https://github.com/vimeo/vimeo-unity-sdk");
            if (recorder.enableDownloads == false) {
                vimeoUploader.SetVideoDownload(recorder.enableDownloads);
            }
            vimeoUploader.SetVideoComments(recorder.commentMode);
            vimeoUploader.SetVideoReviewPage(recorder.enableReviewPage);
            SetVideoName(recorder.GetVideoName());

            if (recorder.privacyMode == VimeoApi.PrivacyModeDisplay.OnlyPeopleWithAPassword) {
                vimeoUploader.SetVideoPassword(recorder.videoPassword);
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

            if (recorder.currentFolder != null && recorder.currentFolder.uri != null) {
                vimeoUploader.AddVideoToFolder(video, recorder.currentFolder);
            }

            UploadProgress("SaveInfoComplete", 1f);
        }

        private void NetworkError(string error_message)
        {
            if (OnNetworkError != null) {
                OnNetworkError("It seems like you are not connected to the internet or are having connection problems.");
            }
        }

        private void ApiError(string response)
        {
            JSONNode json = JSON.Parse(response);

            if (json["invalid_parameters"] != null) {

                for (int i = 0; i < json["invalid_parameters"].Count; i++) {
                    // TODO use .Value
                    if (json["invalid_parameters"][i]["field"].ToString() == "\"privacy.download\"") {
                        if (OnNetworkError != null) {
                            OnNetworkError("You must upgrade your Vimeo account in order to access this privacy feature. https://vimeo.com/upgrade");
                        }
                    } else if (json["invalid_parameters"][i]["field"].ToString() == "\"privacy.view\"") {
                        if (OnNetworkError != null) {
                            OnNetworkError("You must upgrade your Vimeo account in order to access this privacy feature. https://vimeo.com/upgrade");
                        }
                    } else {
                        if (OnNetworkError != null) {
                            OnNetworkError(json["invalid_parameters"][i]["field"] + ": " + json["invalid_parameters"][i]["error"]);
                        }
                    }
                }

            }
            UploadProgress("SaveInfoComplete", 1f);
        }

        public void SetVideoName(string title)
        {
            if (title != null && title != "") {
                if (saveCoroutine != null) { StopCoroutine(saveCoroutine); } // DRY
                vimeoUploader.SetVideoName(title);
                saveCoroutine = StartCoroutine("SaveVideo");
            }
        }

        public void SetVideoPrivacyMode(VimeoApi.PrivacyModeDisplay mode)
        {
            if (saveCoroutine != null) { StopCoroutine(saveCoroutine); }
            vimeoUploader.SetVideoViewPrivacy(mode);
            saveCoroutine = StartCoroutine("SaveVideo");
        }

        private IEnumerator SaveVideo()
        {
            yield return new WaitForSeconds(1f);

            if (video != null) {
                vimeoUploader.SaveVideo(video);
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

        void OnDestroy()
        {
            Dispose();
        }

        public void Dispose()
        {
            Destroy(vimeoUploader);
        }
    }
}