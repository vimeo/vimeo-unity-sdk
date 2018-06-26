﻿#if UNITY_2018_1_OR_NEWER
#if UNITY_EDITOR

using UnityEditor.Media;
using UnityEngine;
using Unity.Collections;
using UnityEngine.Rendering;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

#if AVPROCAPTURE_SUPPORT
using RenderHeads.Media.AVProMovieCapture;
#endif

namespace Vimeo.Recorder {

    public class RecorderController : MonoBehaviour
    {
        [HideInInspector] public string outputPath = Path.GetTempPath();
        [HideInInspector] public VimeoRecorder recorder;
        [HideInInspector] public string encodedFilePath;
        [HideInInspector] public bool isRecording = false;
        [HideInInspector] public bool manualFrameCapture = true;

        [HideInInspector] public VideoTrackAttributes videoAttrs;
        [HideInInspector] public AudioTrackAttributes audioAttrs;
        private int sampleFramesPerVideoFrame;

        [HideInInspector] public int currentFrame = 0;

        private Material matCopy;
        private Shader shaderCopy;
        private Mesh fullscreenQuad;

        private MediaEncoder encoder;
        private NativeArray<float> audioBuffer;
        private RenderTexture renderBuffer;
        private CommandBuffer commandBuffer;

        private VideoInput videoInput;
        private AudioInput audioInput;

        public void BeginRecording()
        {
            isRecording = true;
            if (recorder.encoderType == EncoderType.AVProCapture) {
                BeginAVProCaptureRecording();
            }
            else {
                BeginMediaEncoderRecording();
            }
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
            Debug.Log("Recording: " + GetFileName());

            if (recorder.recordAudio) {
                audioInput.BeginRecording();
                encoder = new UnityEditor.Media.MediaEncoder(encodedFilePath, videoAttrs, audioAttrs);
            }
            else {
                encoder = new UnityEditor.Media.MediaEncoder(encodedFilePath, videoAttrs);
            }
        }

        private void BeginAVProCaptureRecording()
        {
#if AVPROCAPTURE_SUPPORT
            if (recorder.encoderObject != null) {
                recorder.encoderObject.StartCapture();
            }
#endif             
        }

        public string GetVideoName()
        {
            return ReplaceSpecialChars(recorder.videoName);
        }

        public string GetFileName()
        {
            string name = "Vimeo Unity Recording %R %TS";
            return ReplaceSpecialChars(name) + ".mp4";
        }

        public string ReplaceSpecialChars(string input)
        {
            if (input.Contains("%R")) {
                input = input.Replace("%R", videoInput.outputWidth + "x" + videoInput.outputHeight);
            }

            if (input.Contains("%TS")) {
                input = input.Replace("%TS", String.Format("{0:yyyy.MM.dd H.mm.ss}", System.DateTime.Now));
            }

            return input;
        }

        public void EndRecording()
        {
            if (encoder != null) {
                encoder.Dispose();
                encoder = null;
            }

            if (videoInput != null) {
                videoInput.EndRecording();

                if (recorder.recordAudio) {
                    audioInput.EndRecording();
                }
            }

            Destroy(videoInput);

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
                if (recorder.recordAudio) {
                    audioInput.StartFrame();
                }

                encoder.AddFrame(videoInput.GetFrame());
                videoInput.EndFrame();

                if (recorder.recordAudio) {
                    encoder.AddSamples(audioInput.GetBuffer());
                    audioInput.EndFrame();
                }

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

            if (audioInput != null) {
                Destroy(audioInput);
            }

            audioInput = gameObject.AddComponent<AudioInput>();

            switch(recorder.defaultVideoInput) {
                case VideoInputType.Screen:
                    videoInput = gameObject.AddComponent<ScreenInput>();
                    break;
                
                case VideoInputType.Camera360:
                case VideoInputType.Camera:
                    videoInput = gameObject.AddComponent<CameraInput>();
                    break;
            }

            videoInput.recorder = recorder;
            audioInput.recorder = recorder;
        }

        void OnDestroy()
        {
            Destroy(videoInput);
        }
    }
}
#endif
#endif