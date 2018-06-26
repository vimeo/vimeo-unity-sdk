#if UNITY_2018_1_OR_NEWER 
#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using System.IO;
using Vimeo.Services;
using SimpleJSON;


namespace Vimeo.Recorder
{
    [AddComponentMenu("Video/Vimeo Recorder")]
    public class VimeoRecorder : RecorderSettings 
    {
        public delegate void RecordAction();
        public event RecordAction OnUploadComplete;

        public RecorderController controller;
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
                if (controller == null) {
                    controller = gameObject.AddComponent<RecorderController>();
                    controller.recorder = this;
                }
                controller.BeginRecording();
                isRecording = true;
            }
        }

        // Used if you want to script when you want to call AddFrame
        public void BeginManualRecording()
        {
            BeginRecording();
            controller.manualFrameCapture = true;
        }

        public void EndRecording()
        {
            isRecording = false;
            controller.EndRecording();

            PublishVideo(controller.encodedFilePath);
        }
            
        public void CancelRecording()
        {
            isRecording = false;
            isUploading = false;
            controller.EndRecording();
            DeleteVideoFile();

            Dispose();
        }

        private void PublishVideo(string filePath)
        {
            isUploading = true;
            uploadProgress = 0;

            if (publisher == null) {
                publisher = gameObject.AddComponent<VimeoPublisher>();
                publisher.Init(this);

                publisher.OnUploadProgress += UploadProgress;
            }
            
            publisher.PublishVideo(filePath);
        }

        private void DeleteVideoFile()
        {
            controller.DeleteVideoFile();
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
            Destroy(controller);
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

        
        void Update()
        {
#if AVPROCAPTURE_SUPPORT
            if (encoderType == EncoderType.AVProCapture) {
                // rename encoderObject to avpro
                if (encoderObject.IsCapturing()) {
                    isRecording = true;
                }
                else if (isRecording) {
                    isRecording = false;
                    if (File.Exists(encoderObject.LastFilePath)) {
                        Debug.Log("[VimeoRecorder] Uploading video - " + encoderObject.LastFilePath);
                        PublishVideo(encoderObject.LastFilePath);
                    }
                    else {
                        Debug.Log("[VimeoRecorder] Recording cancelled");
                    }
                }
            }
#endif
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
#endif