#if UNITY_2018_1_OR_NEWER 

using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using Vimeo.Config;
using Vimeo.Services;
using SimpleJSON;

namespace Vimeo.Recorder
{
    [AddComponentMenu("Video/Vimeo Recorder")]
    public class VimeoRecorder : RecorderSettings 
    {
        public delegate void RecordAction();
        public event RecordAction OnUploadComplete;

        public RecorderController recorder;
        public VimeoPublisher publisher;

        public bool isRecording = false;
        public bool isUploading = false;
        public float uploadProgress = 0;

        void Start() 
        {
            if (recordOnStart) {
                BeginRecording();
            }
        }
    
        public void BeginRecording()
        {
            if (!isRecording) {
                if (recorder == null) {
                    recorder = gameObject.AddComponent<RecorderController>();
                    recorder.recorder = this;
                }

                recorder.BeginRecording();
                isRecording = true;
            }
        }

        public void EndRecording()
        {
            isRecording = false;
            recorder.EndRecording();

            isUploading = true;
            uploadProgress = 0;
           
            PublishVideo();
        }
            
        public void CancelRecording()
        {
            isRecording = false;
            isUploading = false;
            recorder.EndRecording();
            DeleteVideoFile();

            Dispose();
        }

        private void PublishVideo()
        {
            if (publisher == null) {
                publisher = gameObject.AddComponent<VimeoPublisher>();
                publisher.Init(this);

                publisher.OnUploadProgress += UploadProgress;
            }
            
            publisher.PublishVideo(recorder.encodedFilePath);
        }

        private void DeleteVideoFile()
        {
            recorder.DeleteVideoFile();
        }

        private void UploadProgress(string status, float progress)
        {
            uploadProgress = progress;

            if (status == "SaveInfoComplete") {
                isUploading = false;
                DeleteVideoFile();

                if (OnUploadComplete != null) {
                    OnUploadComplete();
                }
            }
        }

        private void Dispose()
        {
            Destroy(recorder);
            Destroy(publisher);
        }

        void OnDisable()
        {
            if (isRecording) {
                CancelRecording();
            }
        }

        void OnDestroy()
        {
            if (isRecording) {
                CancelRecording();
            }
        }

        void LateUpdate()
        {
            if (recorder != null) {
                // Set recording state based upon VimeoRecorder state
                if (!isRecording && recorder.isRecording) {
                    isRecording = true;
                }
            }
        }
    }
}

#endif