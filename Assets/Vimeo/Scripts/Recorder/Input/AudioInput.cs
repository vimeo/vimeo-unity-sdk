#if UNITY_EDITOR && UNITY_2018_1_OR_NEWER

using UnityEngine;
using Unity.Collections;

namespace Vimeo.Recorder
{
    public class AudioInput : VideoInput
    {
        private NativeArray<float> buffer;

        public NativeArray<float> GetBuffer()
        {
            AudioRenderer.Render(buffer);
            return buffer;
        }

        public override void StartFrame()
        {
            buffer = new NativeArray<float>(AudioRenderer.GetSampleCountForCaptureFrame() * encoder.audioAttrs.channelCount, Allocator.Temp);
        }

        public override void EndFrame() { 
            buffer.Dispose();
        }

        public override void BeginRecording() {
            AudioRenderer.Start();
        }

        public override void EndRecording() { 
            AudioRenderer.Stop();
        }

    }
}
#endif