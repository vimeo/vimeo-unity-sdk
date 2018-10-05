using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using Vimeo;
using SimpleJSON;

namespace Vimeo.Recorder
{
    [AddComponentMenu("Video/Vimeo Recorder")]
    [HelpURL("https://github.com/vimeo/vimeo-unity-sdk")]
    public class VimeoRecorder : RecorderSettings
    {
        public delegate void RecordAction();
        public event RecordAction OnUploadComplete;

        public VimeoPublisher publisher;

        public bool isRecording = false;
        public bool isUploading = false;
        public float uploadProgress = 0;

        public void Start()
        {
            if (encoder == null) {
                encoder = gameObject.AddComponent<EncoderManager>();
                encoder.Init(this);
            }

            if (recordOnStart) {
                BeginRecording();
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
                Debug.Log("[Vimeo] Video did not automatically upload. VimeoPlayer.autoUpload is set to false.");
            }
        }

        public void CancelRecording()
        {
            isRecording = false;
            isUploading = false;
            encoder.CancelRecording();
            Destroy(publisher);
        }

        //Used if you want to publish the latest recorded video
        public void PublishVideo()
        {
            isUploading = true;
            uploadProgress = 0;

            if (publisher == null) {
                publisher = gameObject.AddComponent<VimeoPublisher>();
                publisher.Init(this);

                publisher.OnUploadProgress += UploadProgress;
                publisher.OnNetworkError += NetworkError;
            }

            publisher.PublishVideo(encoder.GetVideoFilePath());
        }

        private void UploadProgress(string status, float progress)
        {
            uploadProgress = progress;

            if (status == "SaveInfoComplete") {
                isUploading = false;
                encoder.DeleteVideoFile();

                if (OnUploadComplete != null) {
                    OnUploadComplete();
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