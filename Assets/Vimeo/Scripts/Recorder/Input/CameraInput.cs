#if UNITY_2018_1_OR_NEWER
#if UNITY_EDITOR

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Vimeo.Recorder
{
    public class CameraInput : VideoInput
    {
        public RenderTexture outputRT { get; set; }

        private float stereoSeparation = 0.064f;
        private int cubeMapSize = 2048 * 2;
        private int outputWidth360 = 4096;
        private int outputHeight360 = 2048;
        private Texture2D readTexture;

        private Camera targetCamera;
        private CommandBuffer cbCopyFB;

        private RenderTexture cubemap1; // left or main
        private RenderTexture cubemap2; // right

        Shader m_copyShader;
        public Shader copyShader {
            get {
                if (m_copyShader == null) {
                    m_copyShader = Shader.Find("Hidden/Recorder/Inputs/CBRenderTexture/CopyFB");
                }
                return m_copyShader;
            }

            set { m_copyShader = value; }
        }

        Material m_copyMaterial;
        public Material copyMaterial {
            get {
                if (m_copyMaterial == null)
                {
                    m_copyMaterial = new Material(copyShader);
                    copyMaterial.EnableKeyword("OFFSCREEN");
                    // if (cbSettings.m_AllowTransparency)
                    //     m_copyMaterial.EnableKeyword("TRANSPARENCY_ON");
                }
                return m_copyMaterial;
            }
        }

        private void AddCameraFollower()
        {
            // rename to camera source?
            switch(recorder.defaultCamera) {
                case CameraType.MainCamera: 
                {
                    targetCamera = Camera.main;
                    break;
                }

            }
        }

        public override Texture2D GetFrame()
        {
            if (recorder.defaultVideoInput == VideoInputType.Camera360) {
                Get360Frame();
            }

            if (!readTexture) {
                readTexture = new Texture2D(outputRT.width, outputRT.height, TextureFormat.RGBA32, false);
            }

            var backupActiveRT = RenderTexture.active;
            RenderTexture.active = outputRT;
            readTexture.ReadPixels(new Rect(0, 0, outputRT.width, outputRT.height), 0, 0);
            RenderTexture.active = backupActiveRT;
            return readTexture;
        }

        public void Get360Frame()
        {
            var eyesEyeSepBackup = targetCamera.stereoSeparation;
            var eyeMaskBackup = targetCamera.stereoTargetEye;

            if (recorder.defaultRenderMode360 == RenderMode360.Stereo)
            {
                targetCamera.stereoSeparation = stereoSeparation;
                targetCamera.stereoTargetEye = StereoTargetEyeMask.Left;
                targetCamera.RenderToCubemap(cubemap1, 63, Camera.MonoOrStereoscopicEye.Left);

                targetCamera.stereoSeparation = stereoSeparation;
                targetCamera.stereoTargetEye = StereoTargetEyeMask.Right;
                targetCamera.RenderToCubemap(cubemap2, 63, Camera.MonoOrStereoscopicEye.Right);

                cubemap1.ConvertToEquirect(outputRT, Camera.MonoOrStereoscopicEye.Left);
                cubemap2.ConvertToEquirect(outputRT, Camera.MonoOrStereoscopicEye.Right);
            }
            else
            {
                targetCamera.RenderToCubemap(cubemap1, 63, Camera.MonoOrStereoscopicEye.Mono);
                cubemap1.ConvertToEquirect(outputRT, Camera.MonoOrStereoscopicEye.Mono);
            }
            
            targetCamera.stereoSeparation = eyesEyeSepBackup;
            targetCamera.stereoTargetEye = eyeMaskBackup;
        }

        public override void EndFrame()
        {
            //ReleaseBuffer();
        }

        public override void BeginRecording()
        {
            if (recorder.defaultVideoInput != VideoInputType.Camera360) {
                base.BeginRecording();
                AddCameraFollower();
                SetupCommandBuffer();
            }
            else {
                outputWidth  = outputWidth360;
                outputHeight = outputHeight360;

                PrepFrameRenderTexture();
                targetCamera = Camera.main;
            }
        }

        public override void EndRecording()
        {
            base.EndRecording();

            if (recorder.defaultVideoInput != VideoInputType.Camera360) {
                ReleaseCamera();
                ReleaseBuffer();
            }
        }

        private void SetupCommandBuffer()
        {
            bool isNewTexture = PrepFrameRenderTexture();

            if (targetCamera != null && isNewTexture) {
                if (cbCopyFB != null)
                {
                    targetCamera.RemoveCommandBuffer(CameraEvent.AfterEverything, cbCopyFB);
                    cbCopyFB.Release();
                }

                var tid = Shader.PropertyToID("_TmpFrameBuffer");
                cbCopyFB = new CommandBuffer { name = "Recorder: copy frame buffer" };
                cbCopyFB.GetTemporaryRT(tid, -1, -1, 0, FilterMode.Bilinear);
                cbCopyFB.Blit(BuiltinRenderTextureType.CurrentActive, tid);
                cbCopyFB.SetRenderTarget(outputRT);
                cbCopyFB.DrawMesh(CreateFullscreenQuad(), Matrix4x4.identity, copyMaterial, 0, 0);
                cbCopyFB.ReleaseTemporaryRT(tid);
                
                targetCamera.AddCommandBuffer(CameraEvent.AfterEverything, cbCopyFB);
            }
        }

        private bool PrepFrameRenderTexture()
        {
            if (outputRT != null) {
                if (outputRT.IsCreated() && outputRT.width == outputWidth && outputRT.height == outputHeight) {
                    return false;
                }

                ReleaseBuffer();
            } 

            if (recorder.defaultVideoInput != VideoInputType.Camera360) {
                outputRT = new RenderTexture(outputWidth, outputHeight, 0, RenderTextureFormat.ARGB32) {
                    wrapMode = TextureWrapMode.Repeat
                };
            }
            else { // 360 
                outputRT = new RenderTexture(outputWidth, outputHeight, 0, RenderTextureFormat.ARGB32) {
                    wrapMode = TextureWrapMode.Repeat
                    // dimension = TextureDimension.Tex2D,
                    // antiAliasing = 8
                };

                cubemap1 = new RenderTexture(cubeMapSize, cubeMapSize, 24, RenderTextureFormat.ARGB32) {
                    dimension = TextureDimension.Cube,
                    antiAliasing = 8
                };

                cubemap2 = new RenderTexture(cubeMapSize, cubeMapSize, 24, RenderTextureFormat.ARGB32) {
                    dimension = TextureDimension.Cube,
                    antiAliasing = 8
                };
            }

            outputRT.Create();

            return true;
        }

        private void ReleaseCamera()
        {
            if (cbCopyFB != null)
            {
                if (targetCamera != null) {
                    targetCamera.RemoveCommandBuffer(CameraEvent.AfterEverything, cbCopyFB);
                }

                cbCopyFB.Release();
                cbCopyFB = null;
            }

            if (copyMaterial != null) {
                Object.Destroy(m_copyMaterial);
            }
        }

        private void ReleaseBuffer()
        {
            if (outputRT != null) {
                if (outputRT == RenderTexture.active) {
                    RenderTexture.active = null;
                }

                outputRT.Release();
                outputRT = null;
            }
            
            if (cubemap1) {
                cubemap1.Release();
                cubemap1 = null;
            }

            if(cubemap2) {
                cubemap2.Release();
                cubemap2 = null;
            }
        }

        public static Mesh CreateFullscreenQuad()
        {
            var vertices = new Vector3[4]
            {
                new Vector3(1.0f, 1.0f, 0.0f),
                new Vector3(-1.0f, 1.0f, 0.0f),
                new Vector3(-1.0f, -1.0f, 0.0f),
                new Vector3(1.0f, -1.0f, 0.0f),
            };
            var indices = new[] { 0, 1, 2, 2, 3, 0 };

            var r = new Mesh
            {
                vertices = vertices,
                triangles = indices
            };
            return r;
        }
    }
}

#endif
#endif