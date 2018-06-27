using UnityEngine;
using System;
using Vimeo.Recorder;

#if AVPROCAPTURE_SUPPORT
using RenderHeads.Media.AVProMovieCapture;
#endif

namespace Vimeo.Recorder 
{
    public class EncoderManager : MonoBehaviour
    {
        private VimeoRecorder _recorder;
        public bool isRecording = false;

        #if AVPROCAPTURE_SUPPORT
            private RenderHeads.Media.AVProMovieCapture.CaptureBase _avproEncoder;
        #endif 

        #if UNITY_EDITOR            
            private RecorderController _vimeoEncoder;
        #endif

        public void Init(VimeoRecorder r)
        {
            _recorder = r;

            if (_recorder.encoderType == EncoderType.MediaEncoder) {
#if UNITY_EDITOR                
                _vimeoEncoder = gameObject.AddComponent<RecorderController>();
                _vimeoEncoder.Init(_recorder);
#endif
            }
            else if (_recorder.encoderType == EncoderType.AVProCapture) {
#if AVPROCAPTURE_SUPPORT
                _avproEncoder = _vimeoEncoder.avproEncoder;
#endif            
            }
        }

        public void BeginRecording()
        {
            isRecording = true;

            if (_recorder.encoderType == EncoderType.MediaEncoder) {
#if UNITY_EDITOR                                
                _vimeoEncoder.BeginRecording();
#endif                
            }
            else {
#if AVPROCAPTURE_SUPPORT
                _avproEncoder.StartCapture();
#endif             
            }
        }

        public void EndRecording()
        {
            isRecording = false;   

            if (_recorder.encoderType == EncoderType.MediaEncoder) {
#if UNITY_EDITOR                          
                _vimeoEncoder.EndRecording();
#endif
            }
            else {
#if AVPROCAPTURE_SUPPORT
                // _avproEncoder.StartCapture();
#endif                    
            }
        }

        public void CancelRecording()
        {
            EndRecording();
            DeleteVideoFile();
        }

        public void AddFrame()
        {
#if UNITY_EDITOR                             
            if (_recorder.encoderType == EncoderType.MediaEncoder) {
                _vimeoEncoder.AddFrame();                
            }
            else {
                Debug.LogWarning("[VimeoRecorder] AddFrame is only available for MediaEncoder.");
            }
#endif            
        }

        public string GetVideoFilePath()
        {
            if (_recorder.encoderType == EncoderType.MediaEncoder) {
#if UNITY_EDITOR                                             
                return _vimeoEncoder.encodedFilePath;
#endif                
            }
            else {
#if AVPROCAPTURE_SUPPORT
                return _avproEncoder.LastFilePath;
#endif                    
            }

            return null;
        }

        public int GetCurrentFrame()
        {
            if (_recorder.encoderType == EncoderType.MediaEncoder) {
#if UNITY_EDITOR                
                return _vimeoEncoder.currentFrame;
#endif
            }
            else if (_recorder.encoderType == EncoderType.AVProCapture) {
#if AVPROCAPTURE_SUPPORT
                Debug.LogWarning("[VimeoRecorder] GetCurrentFrame not supported for AVProCapture");
                return -1;
#endif            
            }

            return -1;
        }

        public void DeleteVideoFile()
        {
#if UNITY_EDITOR               
            // controller.DeleteVideoFile();
#endif            
        }

        public void ManualFrameCapture()
        {
#if UNITY_EDITOR              
            if (_recorder.encoderType == EncoderType.MediaEncoder) {
                _vimeoEncoder.manualFrameCapture = true;
            }
            else {
                Debug.LogWarning("[VimeoRecorder] ManualFrameCapture is only available for MediaEncoder.");
            }
#endif            
        }

        void OnDestroy()
        {
#if UNITY_EDITOR             
            if (_vimeoEncoder != null) {
                Destroy(_vimeoEncoder);
            }
#endif            
        }
        
        void Update()
        {
#if AVPROCAPTURE_SUPPORT
            // Hooking into AVPro by monitoring status changes
            if (_recorder.encoderType == EncoderType.AVProCapture) {
                if (_avproEncoder.IsCapturing()) {
                    isRecording = true;
                }
                else if (isRecording) { // it was recording, but no longer
                    isRecording = false;

                    if (File.Exists(_avproEncoder.LastFilePath)) { // if the file exists, then it finished
                        Debug.Log("[VimeoRecorder] Uploading video - " + _avproEncoder.LastFilePath);
                        PublishVideo(_avproEncoder.LastFilePath);
                    }
                    else {
                        Debug.Log("[VimeoRecorder] Recording cancelled");
                    }
                }
            }
#endif
        }
    }
}