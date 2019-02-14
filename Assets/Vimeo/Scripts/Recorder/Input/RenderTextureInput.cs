#if UNITY_EDITOR && UNITY_2017_3_OR_NEWER

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Vimeo.Recorder
{
    public class RenderTextureInput : VideoInput
    {
        public Texture2D image;

        public override Texture GetFrame()
        {
            image = new Texture2D(outputWidth, outputHeight, TextureFormat.RGBA32, false);
            RenderTexture.active = recorder.renderTextureTarget;
            image.ReadPixels(new Rect(0, 0, outputWidth, outputHeight), 0, 0);
            image.Apply();
            return image;
        }

        public override void BeginRecording()
        {
            outputWidth = recorder.renderTextureTarget.width;
            outputHeight = recorder.renderTextureTarget.height;
        }

        public override void EndFrame()
        {
            Object.Destroy(image);
            image = null;
        }
    }
}

#endif