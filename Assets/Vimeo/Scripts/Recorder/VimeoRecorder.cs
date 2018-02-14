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
    [AddComponentMenu("Video/Vimeo Recorder")]
    public class VimeoRecorder : RecorderSettings 
    {
        public delegate void UploadAction(string status, float progress);
        public event UploadAction OnUploadProgress;

        public RecorderController controller;

        private bool isRecording = false;

        void Start() 
        {
            if (controller == null) {
                controller = gameObject.AddComponent<RecorderController>();
            }

            if (recordOnStart) {
                BeginRecording();
            }
        }
    
        public void BeginRecording()
        {
            controller.BeginRecording();
            // UploadProgress("Recording", 0);
        }

        public void EndRecording()
        {
            isRecording = false;
            controller.EndRecording();

            // PublishVideo();
        }
            
        public void CancelRecording()
        {
            isRecording = false;
            controller.EndRecording();
            DeleteVideoFile();

            // UploadProgress("Cancelled", 0);
        }

        public string GetVideoFilePath()
        {
            return controller.encodedFilePath;
        }

        // private void PublishVideo()
        // {
        //     api.UploadVideoFile(GetVideoFilePath());
        // }

        private void DeleteVideoFile()
        {
            FileUtil.DeleteFileOrDirectory(GetVideoFilePath());
        }

        void LateUpdate()
        {
            if (controller != null) {
                // Set recording state based upon VimeoRecorder state
                if (!isRecording && controller.isRecording) {
                    isRecording = true;
                }
            }
        }
    }
}

#endif