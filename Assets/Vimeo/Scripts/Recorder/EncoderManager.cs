#if UNITY_2017_3_OR_NEWER && UNITY_EDITOR
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
#endif
            } else if (_recorder.encoderType == EncoderType.AVProMovieCapture) {
#if VIMEO_AVPRO_CAPTURE_SUPPORT
                _avproEncoder = r.avproEncoder;
#endif            
            }

            EncoderSetup();
        }

        public void BeginRecording()
        {
            if (_recorder.encoderType == EncoderType.MediaEncoder) {
#if MEDIA_ENCODER_SUPPORT                                
                _vimeoEncoder.BeginRecording();
#endif                
            } else {
#if VIMEO_AVPRO_CAPTURE_SUPPORT
                _avproEncoder.StartCapture();
#endif             
            }
        }

        private void EncoderSetup()
        {
            isRecording = true;

#if VIMEO_LOOKING_GLASS_SUPPORT
            if (_recorder.captureLookingGlassRT) {
                HoloPlay.Quilt quilt = GetHoloPlayQuilt();
                if (quilt != null) {
                    _recorder.renderTextureTarget = quilt.quiltRT;

    #if VIMEO_AVPRO_CAPTURE_SUPPORT
                    if (_recorder.avproEncoder != null) {
                        RenderHeads.Media.AVProMovieCapture.CaptureFromTexture avproTexture = _recorder.avproEncoder.gameObject.GetComponent<RenderHeads.Media.AVProMovieCapture.CaptureFromTexture>();
                        if (avproTexture != null) {
                            avproTexture.SetSourceTexture(_recorder.renderTextureTarget);
                        }
                    }
    #endif 
                }
                else {
                    Debug.LogError("[VimeoRecorer] HoloPlay SDK was not found.");
                }
            }
#endif
        }

#if VIMEO_LOOKING_GLASS_SUPPORT
        private HoloPlay.Quilt GetHoloPlayQuilt()
        {
            GameObject[] objects = gameObject.scene.GetRootGameObjects();

            for (int i = 0; i < objects.Length; i++) {
                if (objects[i].GetComponent<HoloPlay.Quilt>() != null) {
                    return objects[i].GetComponent<HoloPlay.Quilt>();
                }

                HoloPlay.Quilt[] quilts = objects[i].GetComponentsInChildren<HoloPlay.Quilt>();
                if (quilts.Length > 0) {
                    return quilts[0];
                }
            }

            return null;
        }
#endif

        public void EndRecording()
        {
            isRecording = false;

            if (_recorder.encoderType == EncoderType.MediaEncoder) {
#if MEDIA_ENCODER_SUPPORT                          
                _vimeoEncoder.EndRecording();
#endif
            } else {
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
            } else {
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
            } else if (_recorder.encoderType == EncoderType.AVProMovieCapture) {
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
            } else if (_recorder.encoderType == EncoderType.AVProMovieCapture) {
#if VIMEO_AVPRO_CAPTURE_SUPPORT
                Debug.LogWarning("[VimeoRecorder] GetCurrentFrame not supported for AVProMovieCapture");
                return -1;
#endif            
            }

            return -1;
        }

        public int GetOutputWidth()
        {
#if MEDIA_ENCODER_SUPPORT                
            return _vimeoEncoder.GetOutputWidth();
#else
            return -1;
#endif 
        }

        public int GetOutputHeight()
        {
#if MEDIA_ENCODER_SUPPORT                
            return _vimeoEncoder.GetOutputHeight();
#else
            return -1;
#endif
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
            } else {
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
            if (_recorder.encoderType == EncoderType.AVProMovieCapture) {
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