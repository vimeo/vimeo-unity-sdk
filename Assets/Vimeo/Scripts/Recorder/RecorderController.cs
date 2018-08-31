#if UNITY_EDITOR && UNITY_2017_2_OR_NEWER

using UnityEditor.Media;
using UnityEngine;
using UnityEngine.Rendering;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace Vimeo.Recorder 
{
    public class RecorderController : MonoBehaviour
    {
        [HideInInspector] public string outputPath = Path.GetTempPath();
        [HideInInspector] public VimeoRecorder recorder;
        [HideInInspector] public string encodedFilePath;
        [HideInInspector] public bool isRecording = false;
        [HideInInspector] public bool manualFrameCapture = false;

        [HideInInspector] public VideoTrackAttributes videoAttrs;
        [HideInInspector] public AudioTrackAttributes audioAttrs;

        [HideInInspector] public int currentFrame = 0;

        private Material matCopy;
        private Shader shaderCopy;
        private Mesh fullscreenQuad;

        private MediaEncoder encoder;
        private RenderTexture renderBuffer;
        private CommandBuffer commandBuffer;

        private VideoInput videoInput;
#if UNITY_2018_1_OR_NEWER
        private AudioInput audioInput;
#endif

        public void Init(VimeoRecorder r)
        {
            recorder = r;
        }

        public void BeginRecording()
        {
            isRecording = true;
            BeginMediaEncoderRecording();
        }

        public int GetOutputWidth()
        {
            return videoInput.outputWidth;
        }

        public int GetOutputHeight()
        {
            return videoInput.outputHeight;
        }

        private void BeginMediaEncoderRecording()
        {
            InitInputs();

            if (recorder.realTime) {
                Application.targetFrameRate = recorder.frameRate;
            }
            else {
                Time.captureFramerate = recorder.frameRate;
            }

            // Configure encoder
            AudioSpeakerMode speakerMode = AudioSettings.speakerMode;

            audioAttrs = new AudioTrackAttributes
            {
                sampleRate = new MediaRational
                {
                    numerator = AudioSettings.outputSampleRate,
                    denominator = 1
                },
                channelCount = (ushort)speakerMode,
                language = ""
            };

            videoInput.BeginRecording();

            videoAttrs = new VideoTrackAttributes
            {
                frameRate = new MediaRational(recorder.frameRate),
                width  = (uint)videoInput.outputWidth,
                height = (uint)videoInput.outputHeight,
                includeAlpha = false
            };

            encodedFilePath = Path.Combine(outputPath, GetFileName());
            Debug.Log("[VimeoRecorder] Recording to " + GetFileName());

            if (!recorder.realTime) {
                recorder.recordAudio = false;
            }   

            if (recorder.recordAudio) {
#if UNITY_2018_1_OR_NEWER 
                audioInput.BeginRecording();
                encoder = new UnityEditor.Media.MediaEncoder(encodedFilePath, videoAttrs, audioAttrs);
#else
                encoder = new UnityEditor.Media.MediaEncoder(encodedFilePath, videoAttrs);
#endif
            }
            else {
                encoder = new UnityEditor.Media.MediaEncoder(encodedFilePath, videoAttrs);
            }
        }

        public string GetFileName()
        {
            string name = "Vimeo Unity Recording %R %TS";
            return recorder.ReplaceSpecialChars(name) + ".mp4";
        }


        public void EndRecording()
        {
            if (encoder != null) {
                encoder.Dispose();
                encoder = null;
            }

            if (videoInput != null) {
                videoInput.EndRecording();
                 Destroy(videoInput);
            }

#if UNITY_2018_1_OR_NEWER            
            if (audioInput != null) {
                if (recorder.recordAudio) audioInput.EndRecording();
                Destroy(audioInput);
            }
#endif
            
            Time.captureFramerate = 0;

            currentFrame = 0;
            isRecording = false;
        }

        public void DeleteVideoFile()
        {
            File.Delete(encodedFilePath);
        }

        IEnumerator RecordFrame()
        {
            yield return new WaitForEndOfFrame();
            if (!manualFrameCapture) {
                AddFrame();
            }
        }

        public void AddFrame()
        {
            if (encoder != null && isRecording) {
                encoder.AddFrame(videoInput.GetFrame() as Texture2D);
                videoInput.EndFrame();

#if UNITY_2018_1_OR_NEWER
                if (recorder.recordAudio) {
                    audioInput.StartFrame();
                    encoder.AddSamples(audioInput.GetBuffer());
                    audioInput.EndFrame();
                }
#endif

                currentFrame++;
            }
        }

        public void LateUpdate()
        {
            if (encoder != null && isRecording) {
                if (recorder.recordMode == RecordMode.Duration) {
                    if (currentFrame > recorder.frameRate * recorder.recordDuration) {
                        recorder.EndRecording();
                    }
                    else {
                        StartCoroutine(RecordFrame());
                    }
                }
                else {
                    StartCoroutine(RecordFrame());
                }
            }
        }
        
        private void InitInputs()
        {
            if (videoInput != null) {
                Destroy(videoInput);
            }

#if UNITY_2018_1_OR_NEWER
            if (audioInput != null) {
                Destroy(audioInput);
            }

            audioInput = gameObject.AddComponent<AudioInput>();
            audioInput.encoder = this;
            audioInput.recorder = recorder;
#endif 

            switch(recorder.defaultVideoInput) {
                case VideoInputType.Screen:
                    videoInput = gameObject.AddComponent<ScreenInput>();
                    break;
#if UNITY_2018_1_OR_NEWER        
                case VideoInputType.Camera360:
#endif
                case VideoInputType.Camera:
                    videoInput = gameObject.AddComponent<CameraInput>();
                    break;
                case VideoInputType.RenderTexture:
                     videoInput = gameObject.AddComponent<RenderTextureInput>();
                     break;
            }
            
            videoInput.recorder = recorder;
        }

        void OnDestroy()
        {
            Destroy(videoInput);
        }
    }
}
#endif