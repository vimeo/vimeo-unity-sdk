using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using Vimeo;
using System;

namespace Vimeo.Recorder
{
    [AddComponentMenu("Video/Vimeo Recorder")]
    [HelpURL("https://github.com/vimeo/vimeo-unity-sdk")]
    public class VimeoRecorder : RecorderSettings
    {
        public delegate void RecordAction();
        public event RecordAction OnUploadComplete;
        public event RecordAction OnUploadError;

        public VimeoPublisher publisher;

        public bool isRecording = false;
        public bool isUploading = false;
        public float uploadProgress = 0;

        VimeoFetcher fetcher;

        private int m_byteChunkSize = 1024 * 1024 * 128;
        public int byteChunkSize {
            set {
                m_byteChunkSize = value;
            }
        }

        public void Start()
        {
            if (encoder == null) {
                encoder = gameObject.AddComponent<EncoderManager>();
                encoder.Init(this);
            }

            if (recordOnStart) {
                BeginRecording();
            }
            else if (replaceExisting)
            {
                // we want an updated video list, without a need for the editor
                FetchVideos();
            }
        }

        public void BeginRecording()
        {
            if (!isRecording) {
                encoder.BeginRecording();
                isRecording = true;
            }
        }

        // Used if you want to script when you want to call AddFrame
        public void BeginManualRecording()
        {
            BeginRecording();
            encoder.ManualFrameCapture();
        }

        public void EndRecording()
        {
            isRecording = false;
            encoder.EndRecording();

            if (autoUpload) {
                PublishVideo();
            } else {
                Debug.Log("[VimeoRecorder] Video did not automatically upload. VimeoRecorder.autoUpload is set to false.");
            }
        }

        public void CancelRecording()
        {
            isRecording = false;
            isUploading = false;
            encoder.CancelRecording();
            Destroy(publisher);
        }

        void FetchVideos()
        {
            if (fetcher == null)
            {
                fetcher = gameObject.AddComponent<VimeoFetcher>();
                fetcher.Init(this);
                fetcher.GetVideosInFolder(currentFolder);
                fetcher.OnFetchComplete += OnFetchComplete;
                fetcher.OnFetchError += OnFetchError;
            }
        }

        private void OnFetchError(string response)
        {
            DestroyFetcher();
        }

        private void OnFetchComplete(string response)
        {
            DestroyFetcher();
        }

        private void DestroyFetcher()
        {
            if (fetcher != null)
            {
                Destroy(fetcher);
                fetcher = null;
            }
        }

        //Used if you want to publish the latest recorded video
        public void PublishVideo()
        {
            isUploading = true;
            uploadProgress = 0;

            if (publisher == null) {
                publisher = gameObject.AddComponent<VimeoPublisher>();
                publisher.Init(this, m_byteChunkSize);

                publisher.OnUploadProgress += UploadProgress;
                publisher.OnNetworkError += NetworkError;
            }

            if (replaceExisting)
            {
                if (fetcher != null)
                {
                    // bad situation - need some waiting point
                    Debug.LogError("Videos fetching is not complete before replacing publishing");
                }

                if (string.IsNullOrEmpty(vimeoVideoId) && 
                    !string.IsNullOrEmpty(videoName))
                {
                    SetVimeoIdFromName();
                }

                publisher.PublishVideo(encoder.GetVideoFilePath(), vimeoVideoId);
            }
            else
            {
                publisher.PublishVideo(encoder.GetVideoFilePath());
            }
        }

        private void UploadProgress(string status, float progress)
        {
            uploadProgress = progress;

            if (status == "UploadComplete") {
                publisher.OnUploadProgress -= UploadProgress;
                publisher.OnNetworkError -= NetworkError;

                isUploading = false;
                encoder.DeleteVideoFile();
                Destroy(publisher);

                if (OnUploadComplete != null) {
                    OnUploadComplete();
                }
            }
            else if (status == "UploadError")
            {
                publisher.OnUploadProgress -= UploadProgress;
                publisher.OnNetworkError -= NetworkError;

                isUploading = false;
                encoder.DeleteVideoFile();
                Destroy(publisher);

                if (OnUploadError != null)
                {
                    OnUploadError();
                }
            }
        }

        private void NetworkError(string status)
        {
            Debug.LogError(status);
        }

        private void Dispose()
        {
            if (isRecording) {
                CancelRecording();
            }
            Destroy(encoder);
            Destroy(publisher);
        }

        void OnDisable()
        {
            Dispose();
        }

        void OnDestroy()
        {
            Dispose();
        }

        void LateUpdate()
        {
            if (encoder != null) {
                // Set recording state based upon VimeoRecorder state
                if (!isRecording && encoder.isRecording) {
                    isRecording = true;
                }
            }
        }
    }
}