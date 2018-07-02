#if UNITY_2017_2_OR_NEWER && UNITY_EDITOR
    #define MEDIA_ENCODER_SUPPORT
#endif

using UnityEngine;
using System;
using System.IO;
using Vimeo.Recorder;

#if VIMEO_AVPRO_CAPTURE_SUPPORT
using RenderHeads.Media.AVProMovieCapture;
#endif

namespace Vimeo.Recorder 
{
    public class EncoderManager : MonoBehaviour
    {
        private VimeoRecorder _recorder;
        public bool isRecording = false;

        #if VIMEO_AVPRO_CAPTURE_SUPPORT
            private CaptureBase _avproEncoder;
        #endif 

        #if MEDIA_ENCODER_SUPPORT            
            private RecorderController _vimeoEncoder;
        #endif

        public void Init(VimeoRecorder r)
        {
            _recorder = r;

            if (_recorder.encoderType == EncoderType.MediaEncoder) {
#if MEDIA_ENCODER_SUPPORT                
                _vimeoEncoder = gameObject.AddComponent<RecorderController>();
                _vimeoEncoder.Init(_recorder);
#endif
            }
            else if (_recorder.encoderType == EncoderType.AVProCapture) {
#if VIMEO_AVPRO_CAPTURE_SUPPORT
                _avproEncoder = r.avproEncoder;
#endif            
            }
        }

        public void BeginRecording()
        {
            isRecording = true;

            if (_recorder.encoderType == EncoderType.MediaEncoder) {
#if MEDIA_ENCODER_SUPPORT                                
                _vimeoEncoder.BeginRecording();
#endif                
            }
            else {
#if VIMEO_AVPRO_CAPTURE_SUPPORT
                _avproEncoder.StartCapture();
#endif             
            }
        }

        public void EndRecording()
        {
            isRecording = false;   

            if (_recorder.encoderType == EncoderType.MediaEncoder) {
#if MEDIA_ENCODER_SUPPORT                          
                _vimeoEncoder.EndRecording();
#endif
            }
            else {
#if VIMEO_AVPRO_CAPTURE_SUPPORT
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
#if MEDIA_ENCODER_SUPPORT                             
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
#if MEDIA_ENCODER_SUPPORT                                             
                return _vimeoEncoder.encodedFilePath;
#endif                
            }
            else if (_recorder.encoderType == EncoderType.AVProCapture) {
#if VIMEO_AVPRO_CAPTURE_SUPPORT
                return _avproEncoder.LastFilePath;
#endif                    
            }
        
            return null;
        }

        public int GetCurrentFrame()
        {
            if (_recorder.encoderType == EncoderType.MediaEncoder) {
#if MEDIA_ENCODER_SUPPORT                
                return _vimeoEncoder.currentFrame;
#endif
            }
            else if (_recorder.encoderType == EncoderType.AVProCapture) {
#if VIMEO_AVPRO_CAPTURE_SUPPORT
                Debug.LogWarning("[VimeoRecorder] GetCurrentFrame not supported for AVProCapture");
                return -1;
#endif            
            }

            return -1;
        }

        public void DeleteVideoFile()
        {
#if MEDIA_ENCODER_SUPPORT               
            // controller.DeleteVideoFile();
#endif            
        }

        public void ManualFrameCapture()
        {
#if MEDIA_ENCODER_SUPPORT              
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
#if MEDIA_ENCODER_SUPPORT             
            if (_vimeoEncoder != null) {
                Destroy(_vimeoEncoder);
            }
#endif            
        }
        
        void Update()
        {
#if VIMEO_AVPRO_CAPTURE_SUPPORT
            // Hooking into AVPro by monitoring status changes
            if (_recorder.encoderType == EncoderType.AVProCapture) {
                if (_avproEncoder.IsCapturing()) {
                    isRecording = true;
                }
                else if (isRecording) { // it was recording, but no longer
                    isRecording = false;

                    if (File.Exists(_avproEncoder.LastFilePath)) { // if the file exists, then it finished
                        Debug.Log("[VimeoRecorder] AVPro recording finished");
                        _recorder.EndRecording();
                    }
                    else {
                        Debug.Log("[VimeoRecorder] AVPro recording cancelled");
                    }
                }
            }
#endif
        }
        
    }
}