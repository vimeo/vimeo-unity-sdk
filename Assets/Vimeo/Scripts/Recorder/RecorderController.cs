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
        public string outputPath = Path.GetTempPath();
        public VimeoRecorder recorder;

        [HideInInspector] public string encodedFilePath;
        [HideInInspector] public bool isRecording = false;

        private VideoTrackAttributes videoAttrs;
        private AudioTrackAttributes audioAttrs;
        private int sampleFramesPerVideoFrame;

        private Material matCopy;
        private Shader shaderCopy;
        private Mesh fullscreenQuad;

        private MediaEncoder encoder;
        private NativeArray<float> audioBuffer;
        private RenderTexture renderBuffer;
        private CommandBuffer commandBuffer;

        private VideoInput videoInput;

        public void BeginRecording()
        {
            Debug.Log("RecorderController: BeginRecording()");
            isRecording = true;

            InitVideoInput();
            videoInput.BeginRecording();

            // _camera = GetComponent<Camera>();
            //encodedFilePath = Path.Combine(Path.GetPathRoot(), "test-recording.mp4");
            encodedFilePath = Path.Combine("/Users/cpu/dev/unity-vimeo-player/Recordings", Path.GetRandomFileName() + ".mp4");
            Debug.Log(encodedFilePath);

            // Setup shader/material/quad
            // if (shaderCopy == null) {
            //     shaderCopy = Shader.Find("Hidden/FrameRecorder/CopyFrameBuffer");
            // }

            // if (matCopy == null) {
            //     matCopy = new Material(shaderCopy);
            // }

            // if (fullscreenQuad == null) { 
            //     fullscreenQuad = RecorderController.CreateFullscreenQuad();
            // }
            
            // Get Camera data and prepare to send to buffer
            // int captureWidth  = (_camera.pixelWidth + 1) & ~1;
            // int captureHeight = (_camera.pixelHeight + 1) & ~1;

            // renderBuffer = new RenderTexture(captureWidth, captureHeight, 0);
            // renderBuffer.wrapMode = TextureWrapMode.Repeat;
            // renderBuffer.Create();

            // Debug.Log ("WxH: " + captureWidth + "x" + captureHeight);

            // Configure encoder
            Debug.Log(videoInput.outputWidth);
            Debug.Log(videoInput.outputHeight);

            videoAttrs = new VideoTrackAttributes
            {
                frameRate = new MediaRational(60),
                width  = (uint)videoInput.outputWidth,
                height = (uint)videoInput.outputHeight,
                includeAlpha = false
            };

            audioAttrs = new AudioTrackAttributes
            {
                sampleRate = new MediaRational(48000),
                channelCount = 2,
                language = "en"
            };

            encoder = new UnityEditor.Media.MediaEncoder(encodedFilePath, videoAttrs);

            //sampleFramesPerVideoFrame = audioAttrs.channelCount * audioAttrs.sampleRate.numerator / videoAttrs.frameRate.numerator;
            //audioBuffer = new NativeArray<float>(sampleFramesPerVideoFrame, Allocator.Temp);

            // Setup the command buffer 
            // TODO: Support RenderTexture
            // int tid = Shader.PropertyToID("_TmpFrameBuffer");
            // commandBuffer = new CommandBuffer();
            // commandBuffer.name = "VimeoRecorder: copy frame buffer";

            // commandBuffer.GetTemporaryRT(tid, -1, -1, 0, FilterMode.Bilinear);
            // commandBuffer.Blit(BuiltinRenderTextureType.CurrentActive, tid);
            // commandBuffer.SetRenderTarget(renderBuffer);
            // commandBuffer.DrawMesh(fullscreenQuad, Matrix4x4.identity, matCopy, 0, 0);
            // commandBuffer.ReleaseTemporaryRT(tid);

            // _camera.AddCommandBuffer(CameraEvent.AfterEverything, commandBuffer);
        }

        public void EndRecording()
        {
            if (encoder != null) {
                encoder.Dispose();
                // encoder = null;
            }

            // if (commandBuffer != null) {
            //     // _camera.RemoveCommandBuffer(CameraEvent.AfterEverything, commandBuffer);
            //     // commandBuffer.Release();
            //     // commandBuffer = null;
            // }

            // if (renderBuffer != null) {
            //     // renderBuffer.Release();
            //     // renderBuffer = null;
            // }

            if (isRecording) {
                Debug.Log("VimeoRecorder: EndRecording()");
            }

            isRecording = false;
        }

        public void DeleteVideoFile()
        {
            //FileUtil.
        }

        IEnumerator OnPostRender()
        {
            if (isRecording && encoder != null) {
                yield return new WaitForEndOfFrame();

                encoder.AddFrame(videoInput.GetFrame());

                // RecorderController.CaptureLock(renderBuffer, (data) => {
                //     encoder.AddFrame(data);
                // });

                // Fill 'audioBuffer' with the audio content to be encoded into the file for this frame.
                // ...
                //encoder.AddSamples(audioBuffer);
            }
        }
        
        private void InitVideoInput()
        {
            if (videoInput != null) {
                Destroy(videoInput);
            }

            switch(recorder.defaultVideoInput) {
                case VideoInputType.Screen:
                    videoInput = gameObject.AddComponent<ScreenInput>();
                    break;
                
                case VideoInputType.Camera:
                    videoInput = gameObject.AddComponent<CameraInput>();
                    break;
            }

            videoInput.recorder = recorder;
        }


        // public static void CaptureLock(RenderTexture src, Action<Texture2D> body)
        // {
        //     Texture2D tex = new Texture2D(src.width, src.height, TextureFormat.RGBA32, false);
        //     RenderTexture.active = src;
        //     tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0, false);
        //     tex.Apply();
        //     body(tex);
        //     UnityEngine.Object.Destroy(tex);
        // }

        // public static Mesh CreateFullscreenQuad()
        // {
        //     var r = new Mesh();
        //     r.vertices = new Vector3[4] {
        //             new Vector3( 1.0f, 1.0f, 0.0f),
        //             new Vector3(-1.0f, 1.0f, 0.0f),
        //             new Vector3(-1.0f,-1.0f, 0.0f),
        //             new Vector3( 1.0f,-1.0f, 0.0f),
        //         };
        //     r.triangles = new int[6] { 0, 1, 2, 2, 3, 0 };
        //     r.UploadMeshData(true);
        //     return r;
        // }
    }
}
#endif