using UnityEngine;
using Vimeo.Recorder;
using Vimeo.Player;
using System.Collections;


namespace Vimeo.Recorder 
{
	public class TinyPlanetRecorder : MonoBehaviour {
		VimeoRecorder recorder;
		public VimeoPlayer vimeoPlayer;

		void Start() 
		{
			vimeoPlayer.OnLoad += VideoLoad;
			vimeoPlayer.OnVideoStart += VideoStart;

			recorder = gameObject.GetComponent<VimeoRecorder>();
			recorder.defaultVideoInput = Vimeo.Recorder.VideoInputType.Screen;		
			recorder.defaultResolution = Vimeo.Recorder.Resolution.x2160p_4K;
			recorder.defaultAspectRatio = Vimeo.Recorder.AspectRatio.x16_9;
			recorder.frameRate = 30;
			recorder.recordMode = Vimeo.Recorder.RecordMode.Duration;
			recorder.recordDuration = 60; // in seconds

			
			recorder.privacyMode = Vimeo.Services.VimeoApi.PrivacyModeDisplay.OnlyPeopleWithPrivateLink;
			recorder.autoPostToChannel = false; 

			recorder.OnUploadComplete += UploadComplete;
		}

		void VideoLoad()
		{
			//vimeoPlayer.vimeoVideoId = "https://vimeo.com/4632707";
			// vimeoPlayer.vimeoVideoId = "https://vimeo.com/channels/360vr/261363083";
			//vimeoPlayer.vimeoVideoId = "https://vimeo.com/channels/360vr/260442141";
			// vimeoPlayer.vimeoVideoId = "https://vimeo.com/channels/360vr/265562046";
			//vimeoPlayer.LoadVimeoVideoByUrl("https://vimeo.com/channels/360vr/224846999");
			vimeoPlayer.LoadVimeoVideoByUrl("https://vimeo.com/channels/360vr/266948610");
		}

		void VideoStart()
		{
			Debug.Log("VideoStart");
			//vimeoPlayer.Seek(30);
			recorder.videoName = vimeoPlayer.videoName + " (Tiny Planet)";
			recorder.frameRate = (int)vimeoPlayer.controller.videoPlayer.frameRate;
			vimeoPlayer.controller.OnFrameReady += FrameReady; 
			recorder.BeginRecording(); 
		}

		void FrameReady(VideoController controller)
		{
			StartCoroutine(RecordFrame());
		}

		IEnumerator RecordFrame()
		{
			yield return new WaitForEndOfFrame();
			recorder.recorder.AddFrame();	
		}
	
		void UploadComplete() 
		{
			Debug.Log("Uploaded to Vimeo!");
		}
	}
}