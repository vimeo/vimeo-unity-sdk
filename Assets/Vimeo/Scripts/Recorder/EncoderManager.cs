#if UNITY_2017_3_OR_NEWER && UNITY_EDITOR
#define MEDIA_ENCODER_SUPPORT
#endif

using UnityEngine;
using System;
using System.IO;
using Vimeo.Recorder;

#if VIMEO_LOOKING_GLASS_SUPPORT
using HoloPlay;
#endif // VIMEO_LOOKING_GLASS_SUPPORT

#if VIMEO_AVPRO_CAPTURE_SUPPORT
using RenderHeads.Media.AVProMovieCapture;
#endif // VIMEO_AVPRO_CAPTURE_SUPPORT

namespace Vimeo.Recorder
{
    public class EncoderManager : MonoBehaviour
    {
        private VimeoRecorder _recorder;
        public bool isRecording = false;

#if VIMEO_AVPRO_CAPTURE_SUPPORT
            private CaptureBase _avproEncoder;
#endif // VIMEO_AVPRO_CAPTURE_SUPPORT

#if MEDIA_ENCODER_SUPPORT
        private RecorderController _vimeoEncoder;
#endif // MEDIA_ENCODER_SUPPORT

        private void Start()
        {
            this.hideFlags = HideFlags.HideInInspector;
        }

        public void Init(VimeoRecorder r)
        {
            _recorder = r;

            if (_recorder.encoderType == EncoderType.MediaEncoder) {
#if MEDIA_ENCODER_SUPPORT                
                _vimeoEncoder = gameObject.AddComponent<RecorderController>();
                _vimeoEncoder.Init(_recorder);
#else
                Debug.LogError("[Vimeo] Recording is only avaialabe in 2017.2 or higher.");
#endif // MEDIA_ENCODER_SUPPORT
            } else if (_recorder.encoderType == EncoderType.AVProMovieCapture) {
#if VIMEO_AVPRO_CAPTURE_SUPPORT
                _avproEncoder = r.avproEncoder;
#endif // VIMEO_AVPRO_CAPTURE_SUPPORT
            }

            EncoderSetup();
        }

        public void BeginRecording()
        {
            if (_recorder.encoderType == EncoderType.MediaEncoder) {
#if MEDIA_ENCODER_SUPPORT                                
                _vimeoEncoder.BeginRecording();
#endif // MEDIA_ENCODER_SUPPORT            
            } else {
#if VIMEO_AVPRO_CAPTURE_SUPPORT
                _avproEncoder.StartCapture();
#endif // VIMEO_AVPRO_CAPTURE_SUPPORT      
            }
        }

        private void EncoderSetup()
        {
            isRecording = true;

#if VIMEO_LOOKING_GLASS_SUPPORT
            if (_recorder.captureLookingGlassRT) {
                Quilt quilt = GetHoloPlayQuilt();
                if (quilt != null) {
                    _recorder.renderTextureTarget = quilt.quiltRT;

#if VIMEO_AVPRO_CAPTURE_SUPPORT
                    if (_recorder.encoderType == EncoderType.AVProMovieCapture && _recorder.avproEncoder != null) {
                        RenderHeads.Media.AVProMovieCapture.CaptureFromTexture avproTexture = _recorder.avproEncoder.gameObject.GetComponent<RenderHeads.Media.AVProMovieCapture.CaptureFromTexture>();
                        if (avproTexture != null) {
                            avproTexture.SetSourceTexture(_recorder.renderTextureTarget);
                        }
                        else {
                            Debug.LogError("[VimeoRecorder] In order to use AVPro and HoloPlay together, you need to use the CaptureFromTexture component.");
                        }
                    }
#endif // VIMEO_AVPRO_CAPTURE_SUPPORT
                }
                else {
                    Debug.LogError("[VimeoRecorer] HoloPlay SDK was not found.");
                }
            }
#endif // VIMEO_LOOKING_GLASS_SUPPORT
        }

#if VIMEO_LOOKING_GLASS_SUPPORT
        private Quilt GetHoloPlayQuilt()
        {
            GameObject[] objects = gameObject.scene.GetRootGameObjects();

            for (int i = 0; i < objects.Length; i++) {
                if (objects[i].GetComponent<Quilt>() != null) {
                    return objects[i].GetComponent<Quilt>();
                }

                Quilt[] quilts = objects[i].GetComponentsInChildren<Quilt>();
                if (quilts.Length > 0) {
                    return quilts[0];
                }
            }

            return null;
        }
#endif // VIMEO_LOOKING_GLASS_SUPPORT

        public void EndRecording()
        {
            isRecording = false;

            if (_recorder.encoderType == EncoderType.MediaEncoder) {
#if MEDIA_ENCODER_SUPPORT                          
                _vimeoEncoder.EndRecording();
#endif // MEDIA_ENCODER_SUPPORT
            } else {
#if VIMEO_AVPRO_CAPTURE_SUPPORT
                // _avproEncoder.StartCapture();
#endif // VIMEO_AVPRO_CAPTURE_SUPPORT             
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
            } else {
                Debug.LogWarning("[VimeoRecorder] AddFrame is only available for MediaEncoder.");
            }
#endif // MEDIA_ENCODER_SUPPORT         
        }

        public string GetVideoFilePath()
        {
            if (_recorder.encoderType == EncoderType.MediaEncoder) {
#if MEDIA_ENCODER_SUPPORT                                             
                return _vimeoEncoder.encodedFilePath;
#endif // MEDIA_ENCODER_SUPPORT          
            } else if (_recorder.encoderType == EncoderType.AVProMovieCapture) {
#if VIMEO_AVPRO_CAPTURE_SUPPORT
                return _avproEncoder.LastFilePath;
#endif // VIMEO_AVPRO_CAPTURE_SUPPORT                
            }

            return null;
        }

        public int GetCurrentFrame()
        {
            if (_recorder.encoderType == EncoderType.MediaEncoder) {
#if MEDIA_ENCODER_SUPPORT                
                return _vimeoEncoder.currentFrame;
#endif // MEDIA_ENCODER_SUPPORT
            } else if (_recorder.encoderType == EncoderType.AVProMovieCapture) {
#if VIMEO_AVPRO_CAPTURE_SUPPORT
                Debug.LogWarning("[VimeoRecorder] GetCurrentFrame not supported for AVProMovieCapture");
                return -1;
#endif // VIMEO_AVPRO_CAPTURE_SUPPORT        
            }

            return -1;
        }

        public int GetOutputWidth()
        {
#if MEDIA_ENCODER_SUPPORT                
            return _vimeoEncoder.GetOutputWidth();
#else
            return -1;
#endif // MEDIA_ENCODER_SUPPORT
        }

        public int GetOutputHeight()
        {
#if MEDIA_ENCODER_SUPPORT                
            return _vimeoEncoder.GetOutputHeight();
#else
            return -1;
#endif // MEDIA_ENCODER_SUPPORT
        }

        public void DeleteVideoFile()
        {
#if MEDIA_ENCODER_SUPPORT               
            // controller.DeleteVideoFile();
#endif // MEDIA_ENCODER_SUPPORT      
        }

        public void ManualFrameCapture()
        {
#if MEDIA_ENCODER_SUPPORT              
            if (_recorder.encoderType == EncoderType.MediaEncoder) {
                _vimeoEncoder.manualFrameCapture = true;
            } else {
                Debug.LogWarning("[VimeoRecorder] ManualFrameCapture is only available for MediaEncoder.");
            }
#endif // MEDIA_ENCODER_SUPPORT         
        }

        void OnDestroy()
        {
#if MEDIA_ENCODER_SUPPORT             
            if (_vimeoEncoder != null) {
                Destroy(_vimeoEncoder);
            }
#endif // MEDIA_ENCODER_SUPPORT        
        }

        void Update()
        {
#if VIMEO_AVPRO_CAPTURE_SUPPORT
            // Hooking into AVPro by monitoring status changes
            if (_recorder != null && _recorder.encoderType == EncoderType.AVProMovieCapture && _avproEncoder != null) {
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
#endif // VIMEO_AVPRO_CAPTURE_SUPPORT
        }

    }
}