#if UNITY_2017_3_OR_NEWER

using UnityEngine;
using Unity.Collections;

namespace Vimeo.Recorder
{
    public class AudioInput : VideoInput
    {
        private NativeArray<float> buffer;

        public NativeArray<float> GetBuffer()
        {
            Debug.Log(buffer.ToString());
            Debug.Log("length: " +buffer.Length);
            AudioRenderer.Render(buffer);
            return buffer;
        }

        public override void EndFrame() { 
            //buffer.Dispose();
        }

        public override void BeginRecording() {
            AudioRenderer.Start();
            var sampleFramesPerVideoFrame = recorder.recorder.audioAttrs.channelCount * recorder.recorder.audioAttrs.sampleRate.numerator / recorder.frameRate;
            // Debug.Log("sampleFramesPerVideoFrame: " + sampleFramesPerVideoFrame);
            buffer = new NativeArray<float>(sampleFramesPerVideoFrame, Allocator.Temp);
            
            //buffer = new NativeArray<float>(AudioRenderer.GetSampleCountForCaptureFrame() * recorder.recorder.audioAttrs.channelCount, Allocator.Temp);
            
        }

        public override void EndRecording() { 
            AudioRenderer.Stop();
            buffer.Dispose();
        }

    }
}

#endif 