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

        private VimeoUploader m_vimeoUploader;
        public VimeoUploader vimeoUploader {
            get {
                return m_vimeoUploader;
            }
        }
        private VimeoVideo video;

        private Coroutine saveCoroutine;
        private void Start()
        {
            this.hideFlags = HideFlags.HideInInspector;
        }

        public void Init(VimeoRecorder _recorder)
        {
            recorder = _recorder;

            if (m_vimeoUploader == null) {
                m_vimeoUploader = gameObject.AddComponent<VimeoUploader>();
                m_vimeoUploader.Init(recorder.GetVimeoToken());

                m_vimeoUploader.OnPatchComplete += VideoUpdated;
                m_vimeoUploader.OnUploadProgress += UploadProgress;
                m_vimeoUploader.OnUploadComplete += UploadComplete;
                m_vimeoUploader.OnNetworkError += NetworkError;
                m_vimeoUploader.OnRequestComplete += OnUploadInit;
                m_vimeoUploader.OnError += ApiError;
            }
        }

        public void OnUploadInit(string response)
        {
            m_vimeoUploader.OnRequestComplete -= OnUploadInit;

            string video_uri = VimeoUploader.GetVideoUri(response);
            video = new VimeoVideo("", video_uri);

#if UNITY_2018_1_OR_NEWER
            if (recorder.defaultVideoInput == VideoInputType.Camera360) {
                m_vimeoUploader.SetVideoSpatialMode("equirectangular", recorder.defaultRenderMode360 == RenderMode360.Stereo ? "top-bottom" : "mono");
            }
#endif

            m_vimeoUploader.SetVideoDescription("Recorded and uploaded with the Vimeo Unity SDK: https://github.com/vimeo/vimeo-unity-sdk");
            if (recorder.enableDownloads == false) {
                m_vimeoUploader.SetVideoDownload(recorder.enableDownloads);
            }
            m_vimeoUploader.SetVideoComments(recorder.commentMode);
            m_vimeoUploader.SetVideoReviewPage(recorder.enableReviewPage);
            SetVideoName(recorder.GetVideoName());

            if (recorder.privacyMode == VimeoApi.PrivacyModeDisplay.OnlyPeopleWithAPassword) {
                m_vimeoUploader.SetVideoPassword(recorder.videoPassword);
            }
            SetVideoPrivacyMode(recorder.privacyMode);
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

            if (video != null && video.id != 0) {
                return "https://vimeo.com/" + video.id;
            }

            Debug.LogError("No vimeo video link found, try recording again");
            return null;
        }

        public void PublishVideo(string filename)
        {
            Debug.Log("[VimeoRecorder] Uploading to Vimeo");
            m_vimeoUploader.Upload(filename);
        }
        
        void UploadProgress(string status, float progress)
        {
            if (OnUploadProgress != null) {
                OnUploadProgress(status, progress);
            }
        }

        private void UploadComplete(string video_url)
        {
            if (recorder.openInBrowser == true) {
                OpenVideo();
            }
            if (OnUploadProgress != null) {
                OnUploadProgress("UploadComplete", 1f);
            }
            Debug.Log("[VimeoPublisher] Published video to " + video_url);
        }

        private void VideoUpdated(string response)
        {
            JSONNode json = JSON.Parse(response);
            recorder.videoPermalink = json["link"];
            recorder.videoReviewPermalink = json["review_link"];

            if (recorder.currentFolder != null && recorder.currentFolder.uri != null) {
                m_vimeoUploader.AddVideoToFolder(video, recorder.currentFolder);
            }
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
                    } 
                    else if (json["invalid_parameters"][i]["field"].ToString() == "\"privacy.view\"") {
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
        }

        public void SetVideoName(string title)
        {
            if (title != null && title != "") {
                if (saveCoroutine != null) { StopCoroutine(saveCoroutine); } // DRY
                m_vimeoUploader.SetVideoName(title);
                saveCoroutine = StartCoroutine("SaveVideo");
            }
        }

        public void SetVideoPrivacyMode(VimeoApi.PrivacyModeDisplay mode)
        {
            if (saveCoroutine != null) { StopCoroutine(saveCoroutine); }
            m_vimeoUploader.SetVideoViewPrivacy(mode);
            saveCoroutine = StartCoroutine("SaveVideo");
        }

        private IEnumerator SaveVideo()
        {
            yield return new WaitForSeconds(1f);

            if (video != null) {
                m_vimeoUploader.SaveVideo(video);
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
            Destroy(m_vimeoUploader);
        }
    }
}