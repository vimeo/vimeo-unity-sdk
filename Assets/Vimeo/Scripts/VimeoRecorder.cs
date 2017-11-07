using UnityEditor.Media;
using UnityEngine;
using UnityEngine.Collections;
using UnityEngine.Rendering;
using System;
using System.IO;
using System.Collections;

namespace Vimeo {

	[RequireComponent(typeof(Camera))]
	public class VimeoRecorder : MonoBehaviour
	{
		public string outputPath = Path.GetTempPath();

		[HideInInspector] public string encodedFilePath;
		[HideInInspector] public bool isRecording = false;

		private Camera _camera;

		private VimeoPublisher vimeoPublisher;

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

	    public void BeginRecording()
		{
			Debug.Log("VimeoRecorder: BeginRecording()");
			isRecording = true;

			_camera = GetComponent<Camera>();
			encodedFilePath = Path.Combine(outputPath, "test-recording.mp4");

			Debug.Log(encodedFilePath);

			// Setup shader/material/quad
			if (shaderCopy == null) {
				shaderCopy = Shader.Find("Hidden/FrameRecorder/CopyFrameBuffer");
			}

			if (matCopy == null) {
				matCopy = new Material(shaderCopy);
			}

			if (fullscreenQuad == null) { 
				fullscreenQuad = VimeoRecorder.CreateFullscreenQuad();
			}
			
			// Get Camera data and prepare to send to buffer
            int captureWidth = (_camera.pixelWidth + 1) & ~1;
            int captureHeight = (_camera.pixelHeight + 1) & ~1;

			renderBuffer = new RenderTexture(captureWidth, captureHeight, 0);
			renderBuffer.wrapMode = TextureWrapMode.Repeat;
			renderBuffer.Create();

            Debug.Log ("WxH: " + captureWidth + "x" + captureHeight);

			// Configure encoder
			videoAttrs = new VideoTrackAttributes
	        {
	            frameRate = new MediaRational(30),
				width = (uint)captureWidth,
				height = (uint)captureHeight,
	            includeAlpha = false
	        };

	        audioAttrs = new AudioTrackAttributes
	        {
	            sampleRate = new MediaRational(48000),
	            channelCount = 2,
	            language = "en"
	        };

			encoder = new MediaEncoder(encodedFilePath, videoAttrs, audioAttrs);

			//sampleFramesPerVideoFrame = audioAttrs.channelCount * audioAttrs.sampleRate.numerator / videoAttrs.frameRate.numerator;
	        //audioBuffer = new NativeArray<float>(sampleFramesPerVideoFrame, Allocator.Temp);

			// Setup the command buffer 
            // TODO: Support RenderTexture
			int tid = Shader.PropertyToID("_TmpFrameBuffer");
			commandBuffer = new CommandBuffer();
			commandBuffer.name = "VimeoRecorder: copy frame buffer";

            commandBuffer.GetTemporaryRT(tid, -1, -1, 0, FilterMode.Bilinear);
            commandBuffer.Blit(BuiltinRenderTextureType.CurrentActive, tid);
			commandBuffer.SetRenderTarget(renderBuffer);
			commandBuffer.DrawMesh(fullscreenQuad, Matrix4x4.identity, matCopy, 0, 0);
            commandBuffer.ReleaseTemporaryRT(tid);

            _camera.AddCommandBuffer(CameraEvent.AfterEverything, commandBuffer);
	    }

	    public void EndRecording()
	    {
			if (encoder != null) {
                encoder.Dispose();
                encoder = null;
            }

			if (commandBuffer != null) {
				_camera.RemoveCommandBuffer(CameraEvent.AfterEverything, commandBuffer);
				commandBuffer.Release();
				commandBuffer = null;
            }

            if (renderBuffer != null) {
				renderBuffer.Release();
				renderBuffer = null;
            }

            if (isRecording) {
                Debug.Log("VimeoRecorder: EndRecording()");
            }

			isRecording = false;
	    }

		IEnumerator OnPostRender()
		{
            if (isRecording && encoder != null) {
				yield return new WaitForEndOfFrame();

				// Fill 'tex' with the video content to be encoded into the file for this 
				//tex.ReadPixels(new Rect(0, 0, render., 0));
//				VimeoRecorder.CaptureLock(renderBuffer, (data) => {
//					encoder.AddFrame(data);
//                });

                Texture2D tex = new Texture2D(renderBuffer.width, renderBuffer.height, TextureFormat.RGBA32, false);
                RenderTexture.active = renderBuffer;
                tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0, false);
                tex.Apply();
                encoder.AddFrame(tex);
                UnityEngine.Object.Destroy(tex);



                // Fill 'audioBuffer' with the audio content to be encoded into the file for this frame.
                // ...
                //encoder.AddSamples(audioBuffer);
			}
	    }

		public static void CaptureLock(RenderTexture src, Action<Texture2D> body)
        {
            Texture2D tex = new Texture2D(src.width, src.height, TextureFormat.RGBA32, false);
            RenderTexture.active = src;
            tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0, false);
            tex.Apply();
            body(tex);
            UnityEngine.Object.Destroy(tex);
        }

		public static Mesh CreateFullscreenQuad()
        {
            var r = new Mesh();
            r.vertices = new Vector3[4] {
                    new Vector3( 1.0f, 1.0f, 0.0f),
                    new Vector3(-1.0f, 1.0f, 0.0f),
                    new Vector3(-1.0f,-1.0f, 0.0f),
                    new Vector3( 1.0f,-1.0f, 0.0f),
                };
            r.triangles = new int[6] { 0, 1, 2, 2, 3, 0 };
            r.UploadMeshData(true);
            return r;
        }
	}
}