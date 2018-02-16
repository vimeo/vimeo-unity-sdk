#if UNITY_2017_3_OR_NEWER 

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
        public delegate void UploadAction(string status, float progress);
        public event UploadAction OnUploadProgress;

        public RecorderController recorder;
        public VimeoPublisher publisher;

        private bool isRecording = false;

        void Start() 
        {
            if (recorder == null) {
                recorder = gameObject.AddComponent<RecorderController>();
                recorder.recorder = this;
            }

            if (recordOnStart) {
                BeginRecording();
            }
        }
    
        public void BeginRecording()
        {
            recorder.BeginRecording();
            // UploadProgress("Recording", 0);
        }

        public void EndRecording()
        {
            isRecording = false;
            recorder.EndRecording();

            PublishVideo();
        }
            
        public void CancelRecording()
        {
            isRecording = false;
            recorder.EndRecording();
            DeleteVideoFile();

            // UploadProgress("Cancelled", 0);
        }

        private void PublishVideo()
        {
            publisher.PublishVideo(recorder.encodedFilePath);
        }

        private void DeleteVideoFile()
        {
            recorder.DeleteVideoFile();
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