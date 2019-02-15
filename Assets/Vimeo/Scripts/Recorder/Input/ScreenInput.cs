#if UNITY_EDITOR && UNITY_2017_3_OR_NEWER

using UnityEngine;
using UnityEditor;

namespace Vimeo.Recorder
{
    public class ScreenInput : VideoInput
    {
        private Texture2D image;

        public override Texture GetFrame()
        {
            image = ScreenCapture.CaptureScreenshotAsTexture();
            return image;
        }

        public override void EndFrame()
        {
            Object.Destroy(image);
            image = null;
        }
    }
}

#endif