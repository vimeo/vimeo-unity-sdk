#if UNITY_2018_1_OR_NEWER
#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

namespace Vimeo.Recorder
{
    public class ScreenInput : VideoInput
    {
        private Texture2D image;

        public override Texture2D GetFrame()
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