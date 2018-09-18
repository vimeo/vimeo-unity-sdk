#if UNITY_2017_2_OR_NEWER
#if UNITY_EDITOR

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
#endif 