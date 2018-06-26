using UnityEngine;
using Vimeo.Recorder;
using Vimeo.Player;
using System.Collections;

#if AVPROCAPTURE_SUPPORT
using RenderHeads.Media.AVProMovieCapture;
#endif

namespace Vimeo.Recorder 
{
	public class VideoExporter : MonoBehaviour {
		private VimeoRecorder recorder;
		public VimeoPlayer vimeoPlayer;
		public bool syncDuration = false;

        void Awake()
        {
            vimeoPlayer.OnLoad		 += VideoLoad;
            vimeoPlayer.OnVideoStart += VideoStart;
        }

		void Start() 
		{
			recorder = gameObject.GetComponent<VimeoRecorder>();
			recorder.defaultVideoInput = Vimeo.Recorder.VideoInputType.Screen;		
			recorder.frameRate = 30;
			recorder.realTime = false;
			recorder.recordAudio = false;
			recorder.OnUploadComplete += UploadComplete;
		}

		void VideoLoad()
		{
			
		}

		void VideoStart()
		{
			recorder.videoName = vimeoPlayer.videoName + " (Tiny Planet)";

			vimeoPlayer.controller.SendFrameReadyEvents();
			vimeoPlayer.controller.EnableFrameStepping();
			vimeoPlayer.controller.OnFrameReady += FrameReady; 

			recorder.frameRate = (int)vimeoPlayer.controller.videoPlayer.frameRate;

			if (syncDuration) {
				recorder.recordMode     = Vimeo.Recorder.RecordMode.Duration;
				recorder.recordDuration = vimeoPlayer.controller.GetDuration(); 
			}

			recorder.BeginManualRecording(); 
		}

		void FrameReady(VideoController controller)
		{
			StartCoroutine(RecordFrame());
		}

		IEnumerator RecordFrame()
		{
 			yield return new WaitForEndOfFrame();
			recorder.controller.AddFrame();	
		}
	
		void UploadComplete() 
		{
			// Debug.Log("Uploaded to Vimeo!");
		}

		void OnDisable()
		{
			if (vimeoPlayer != null) {
				vimeoPlayer.controller.OnFrameReady -= FrameReady; 
			}
		}
	}
}