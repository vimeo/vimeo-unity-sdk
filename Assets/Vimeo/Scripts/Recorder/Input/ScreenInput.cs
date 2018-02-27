#if UNITY_2017_3_OR_NEWER

using UnityEngine;
using UnityEditor;

namespace Vimeo.Recorder
{
    public class ScreenInput : VideoInput
    {
        bool m_ModifiedResolution;

        public Texture2D image { get; private set; }

        // public ScreenCaptureInputSettings scSettings
        // {
        //     get { return (ScreenCaptureInputSettings)settings; }
        // }

        public override Texture2D GetFrame()
        {
            return ScreenCapture.CaptureScreenshotAsTexture();
        }

        public override void BeginRecording()
        {
            int screenWidth = Screen.width;
            int screenHeight = Screen.height;

            switch (recorder.defaultResolution)
            {
                case Resolution.Window:
                {
                    outputWidth  = (Screen.width + 1) & ~1;
                    outputHeight = (Screen.height + 1) & ~1;
                    break;
                }

                default:
                {
                    outputHeight = (int)recorder.defaultResolution;
                    outputWidth = (int)(outputHeight * AspectRatioHelper.GetRealAR(recorder.defaultAspectRatio));

                    outputWidth = (outputWidth + 1) & ~1;
                    outputHeight = (outputHeight + 1) & ~1;
                    break;
                }
            }
            
            Debug.Log("Screen resolution: " + outputWidth + "x" + outputHeight);

            object size = UnityEngine.Recorder.Input.GameViewSize.SetCustomSize(outputWidth, outputHeight);
            UnityEngine.Recorder.Input.GameViewSize.SelectSize(size);
            //Screen.SetResolution(outputWidth, outputHeight, false);
            
            //int w, h;
            //GameViewSize.GetGameRenderSize(out w, out h);
            // if (w != outputWidth || h != outputHeight)
            // {
            //     var size = GameViewSize.SetCustomSize(outputWidth, outputHeight) ?? GameViewSize.AddSize(outputWidth, outputHeight);
            //     if (GameViewSize.m_ModifiedResolutionCount == 0)
            //         GameViewSize.BackupCurrentSize();
            //     else
            //     {
            //         if (size != GameViewSize.currentSize)
            //         {
            //             Debug.LogError("Requestion a resultion change while a recorder's input has already requested one! Undefined behaviour.");
            //         }
            //     }
            //     GameViewSize.m_ModifiedResolutionCount++;
            //     m_ModifiedResolution = true;
            //     GameViewSize.SelectSize(size);
            // }
        }

        // public override void FrameDone(RecordingSession session)
        // {
        //     UnityHelpers.Destroy(image);
        //     image = null;
        // }

        // protected override void Dispose(bool disposing)
        // {
        //     if (disposing)
        //     {
        //         if (m_ModifiedResolution)
        //         {
        //             GameViewSize.m_ModifiedResolutionCount--;
        //             if (GameViewSize.m_ModifiedResolutionCount == 0)
        //                 GameViewSize.RestoreSize();
        //         }
        //     }

        //     base.Dispose(disposing);
        // }
    }
}

#endif 