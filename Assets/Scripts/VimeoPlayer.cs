using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

namespace Vimeo
{
	[CustomEditor (typeof(VimeoPlayer))]
	public class VimeoPlayerInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector ();
			var player = target as VimeoPlayer;
			player.videoQualityIndex = EditorGUILayout.Popup("Default quality", player.videoQualityIndex, player.videoQualities);
			EditorUtility.SetDirty(target);
		}
	}

	public class VimeoPlayer : MonoBehaviour 
    {
        public delegate void VimeoEvent();        public event VimeoEvent OnVideoStart;
        public event VimeoEvent OnPause;
        public event VimeoEvent OnPlay;

        public GameObject videoScreen;
        public GameObject audioSource;
        public string vimeoApiToken;
        public string vimeoVideoId;

		[HideInInspector] public string[] videoQualities = new [] { "Highest", "1080", "720", "SD" };
		[HideInInspector] public int videoQualityIndex = 0;

		[HideInInspector] public string videoTitle;
		[HideInInspector] public string videoThumbnailUrl;
		[HideInInspector] public string authorThumbnailUrl;

        private VimeoApi api;

		[HideInInspector]
        public VideoController video;

		// Deprecate
        private int[] video_collection = new int[4] { 2, 2998622, 213868, 80924717 };

//		void OnGUI ()
//		{
//			// Choose an option from the list
//			_choiceIndex = EditorGUILayout.Popup(_choiceIndex, _choices);
//			// Update the selected option on the underlying instance of SomeClass
//			var someClass = target as SomeClass;
//			someClass.choice = _choices[_choiceIndex];
//			// Custom inspector code goes here
//			EditorUtility.SetDirty(target);
//		}

		private void Start()
        {
            if (vimeoApiToken != null) {
                api = gameObject.AddComponent<VimeoApi>();
                api.token = vimeoApiToken;
                api.OnRequestComplete += OnLoadVimeoVideoComplete;
            }

            if (videoScreen != null) {
                video = gameObject.AddComponent<VideoController>();
                video.videoScreenObject = videoScreen;

                if (audioSource) {
                    video.audioSource = audioSource.GetComponent<AudioSource>();
                }
                video.OnVideoStart += VideoStarted;
                video.OnPlay       += VideoPlay;
                video.OnPause      += VideoPaused;
            }

            // Bootup video
            if (vimeoVideoId != null) {
                LoadVimeoVideo(int.Parse(vimeoVideoId));
            }
        }

        private void OnDisable()
        {
            api.OnRequestComplete -= OnLoadVimeoVideoComplete;
            video.OnVideoStart    -= VideoStarted;
        }	

		// Deprecate
        public void RandomVideo()
        {
           LoadVimeoVideo(video_collection[Random.Range(0, video_collection.Length)]);
        }

        public void LoadVimeoVideo(int id)
        {
            api.GetVideoFileUrlByVimeoId(id);
        }

		public void Play()
		{
			video.Play ();
		}

		public void Pause()
		{
			video.Pause ();
		}

		public void SeekBackward(float seek)
		{
			video.SeekBackward(seek);
		}

		public void SeekForward(float seek)
		{
			video.SeekForward(seek);
		}

        public void ToggleVideoPlayback()
        {
            video.TogglePlayback();
        }

        public int GetWidth()
        {
            return video.width;
        }

        public int GetHeight()
        {
            return video.height;
        }

		public float GetProgress()
		{
			if (video != null) {
				return (float)video.videoPlayer.frame / (float)video.videoPlayer.frameCount;
			}
			return 0;
		}

		public string GetTimecode()
		{
			if (video != null) {
				float sec = Mathf.Floor ((float)video.videoPlayer.time % 60);
				float min = Mathf.Floor ((float)video.videoPlayer.time / 60f);

				string secZeroPad = sec > 9 ? "" : "0";
				string minZeroPad = min > 9 ? "" : "0";

				return minZeroPad + min + ":" + secZeroPad + sec;
			}

			return null;
		}

        // Events below!
        private void VideoStarted(VideoController controller) {
            if (OnVideoStart != null) {
                OnVideoStart();
            }
        }

        private void VideoPlay(VideoController controller)
        {
            if (OnPlay != null)
            {
                OnPlay();
            }
        }

        private void VideoPaused(VideoController controller)
        {
            if (OnPause != null)
            {
                OnPause();
            }
        }

        private void OnLoadVimeoVideoComplete(string response)
        {
			Debug.Log (response);
			var json = JSON.Parse(response);
			List<JSONNode> qualities = new List<JSONNode> ();
			JSONNode progressiveFiles = json["play"]["progressive"];

			// Set the metadata
			videoTitle = json["name"];
			videoThumbnailUrl = json ["pictures"]["sizes"][4]["link"];
			authorThumbnailUrl = json ["user"] ["pictures"] ["sizes"] [2] ["link"];
			//Debug.Log(json);

			// Sort the quality
			for (int i = 0; i < progressiveFiles.Count; i++) {
				qualities.Add (progressiveFiles [i]);
			}	
			qualities.Sort(SortByQuality);

			if (videoQualities[videoQualityIndex] == "Highest") {
				video.PlayVideoByUrl(qualities[0]["link"]);
			} 
			else if (videoQualities[videoQualityIndex] == "1080") {
				video.PlayVideoByUrl (FindByQuality (qualities, "1080")["link"]);
			} 
			else if (videoQualities[videoQualityIndex] == "720") {
				video.PlayVideoByUrl (FindByQuality (qualities, "720")["link"]);
			}
        }

		private JSONNode FindByQuality(List<JSONNode> qualities, string quality)
		{
			for (int i = 0; i < qualities.Count; i++) {
				if (qualities[i]["height"].ToString() == quality) {
					Debug.Log ("Loading " + qualities[i]["height"] + "p file");
					return qualities[i];
				}
			}

			Debug.Log ("Couldnt find quality. defaulting to " + qualities[0]["height"]);
			return qualities [0];
		}

		private static int SortByQuality(JSONNode q1, JSONNode q2)
		{
			return int.Parse (q2["height"]).CompareTo (int.Parse (q1["height"]));
		}
    }
}
