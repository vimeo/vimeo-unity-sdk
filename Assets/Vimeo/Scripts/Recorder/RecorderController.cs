#if UNITY_2017_3_OR_NEWER 

using UnityEditor.Media;
using UnityEngine;

#if UNITY_2018_1_OR_NEWER
    using Unity.Collections;
#else
    using UnityEngine.Collections;
#endif

using UnityEngine.Rendering;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace Vimeo.Recorder {

    public class RecorderController : MonoBehaviour
    {
        [HideInInspector] public string outputPath = Path.GetTempPath();
        [HideInInspector] public VimeoRecorder recorder;
        [HideInInspector] public string encodedFilePath;
        [HideInInspector] public bool isRecording = false;

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
            Debug.Log("RecorderController: BeginRecording()");
            isRecording = true;

            InitInputs();

            encodedFilePath = Path.Combine(outputPath, GetFileName());
            Debug.Log(encodedFilePath);

            if (recorder.realTime) {
                Application.targetFrameRate = recorder.frameRate;
            }
            else {
                Time.captureFramerate = recorder.frameRate;
            }

            // Configure encoder
            audioAttrs = new AudioTrackAttributes
            {
                sampleRate = new MediaRational(48000),
                channelCount = 2,
                language = "en"
            };

            videoInput.BeginRecording();
            audioInput.BeginRecording();

            videoAttrs = new VideoTrackAttributes
            {
                frameRate = new MediaRational(recorder.frameRate),
                width  = (uint)videoInput.outputWidth,
                height = (uint)videoInput.outputHeight,
                includeAlpha = false
            };
            encoder = new UnityEditor.Media.MediaEncoder(encodedFilePath, videoAttrs, audioAttrs);
        }

        public string GetVideoName()
        {
            return ReplaceSpecialChars(recorder.videoName);
        }

        public string GetFileName()
        {
            string name = "Unity Recording %R %TS";
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
                audioInput.EndRecording();
            }

            Destroy(videoInput);

            Time.captureFramerate = 0;

            if (isRecording) {
                Debug.Log("RecorderController.EndRecording()");
            }

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
            if (encoder != null && isRecording) {
                encoder.AddFrame(videoInput.GetFrame());
                encoder.AddSamples(audioInput.GetBuffer());

                videoInput.EndFrame();
                audioInput.EndFrame();
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
                currentFrame++;
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

        public void OnDestroy()
        {
            Destroy(videoInput);
        }
    }
}
#endif