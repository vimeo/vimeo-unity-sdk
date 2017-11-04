using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using Vimeo.Config;

namespace Vimeo
{
	[CustomEditor (typeof(VimeoPlayer))]
    public class VimeoPlayerInspector : VimeoConfig
	{
		public override void OnInspectorGUI()
		{
			var publisher = target as VimeoPlayer;
            DrawVimeoConfig(publisher); 
            EditorUtility.SetDirty(target);
		}
    }

	[AddComponentMenu("Vimeo/Video Player")]
	public class VimeoPlayer : MonoBehaviour 
    {
        public delegate void VimeoEvent();
        public event VimeoEvent OnVideoStart;
        public event VimeoEvent OnPause;
        public event VimeoEvent OnPlay;

        public GameObject videoScreen;
        public GameObject audioSource;
        public string vimeoToken;
        public string vimeoVideoId;

		public string[] videoQualities = new [] { "Highest", "1080", "720", "540", "480", "360" }; 
		public int videoQualityIndex = 0;

		public string videoName;
		public string videoThumbnailUrl;
		public string authorThumbnailUrl;
        public bool is3D;
        public string videoProjection;
        public string videoStereoFormat;

        private VimeoApi api;
        public VideoController video;

		private void Start()
        {
            if (GetVimeoToken() != null) {
                api = gameObject.AddComponent<VimeoApi>();
                api.token = GetVimeoToken();
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
            if (vimeoVideoId != null && vimeoVideoId != "") {
                LoadVimeoVideo (int.Parse (vimeoVideoId));
            } else {
                Debug.LogWarning ("No Vimeo video ID was specified");
            }
        }

        public string GetVimeoToken()
        {
            var token = PlayerPrefs.GetString("vimeo-player-token");
            if (token == null || token == "") {
                if (vimeoToken != null && vimeoToken != "") {
                    SetVimeoToken (vimeoToken);
                }
                token = vimeoToken;
            }

            vimeoToken = null;
            return token;
        }

        public void SetVimeoToken(string token)
        {
            SetKey("vimeo-player-token", token);
        }

        private void SetKey(string key, string val)
        {
            if (val == null || val == "") {
                PlayerPrefs.DeleteKey(key);
            } else {
                PlayerPrefs.SetString(key, val);
            }
            PlayerPrefs.Save(); 
        }

        private void OnDisable()
        {
            api.OnRequestComplete -= OnLoadVimeoVideoComplete;
            video.OnVideoStart    -= VideoStarted;
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

        public void Seek(float seek)
        {
            video.Seek(seek);
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
            if (video != null && video.videoPlayer != null) {
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
			var json = JSON.Parse(response);
            if (json ["error"] == null) {
                video.PlayVideoByUrl (GetVideoFileUrl (json), is3D, videoStereoFormat);
            } 
            else {
                Debug.LogError("Video could not be found");
            }
        }

        private string GetVideoFileUrl(JSONNode json)
        {
            // Set the metadata
            videoName = json ["name"];
            videoThumbnailUrl = json ["pictures"] ["sizes"] [4] ["link"];
            authorThumbnailUrl = json ["user"] ["pictures"] ["sizes"] [2] ["link"];
            is3D = false;
            videoStereoFormat = "mono";

            if (json ["spatial"] != null) {
                is3D = true;
                videoProjection   = json ["spatial"] ["projection"];
                videoStereoFormat = json ["spatial"] ["stereo_format"];
            }

            // New Vimeo file response format
            if (json ["play"] != null) {
                List<JSONNode> qualities = new List<JSONNode> ();
                JSONNode progressiveFiles = json ["play"] ["progressive"];

                // Sort the quality
                for (int i = 0; i < progressiveFiles.Count; i++) {
                    qualities.Add (progressiveFiles [i]);
                }   
                qualities.Sort (SortByQuality);

                if (videoQualities [videoQualityIndex] == "Highest") {
                    return qualities [0] ["link"];
                } 
                else {
                    return FindByQuality (qualities, videoQualities [videoQualityIndex])["link"];
                }
            }

            // Current Vimeo file response
            if (json ["files"] != null) {
                List<JSONNode> qualities = new List<JSONNode> ();
                JSONNode progressiveFiles = json ["files"];

                for (int i = 0; i < progressiveFiles.Count; i++) {
                    if (progressiveFiles[i]["height"] != null && progressiveFiles[i]["type"].Value == "video/mp4") {
                        qualities.Add (progressiveFiles [i]);
                    }
                }
                qualities.Sort(SortByQuality);

                if (videoQualities[videoQualityIndex] == "Highest") {
                   return qualities [0] ["link_secure"];
                } 
                else {
                    return FindByQuality (qualities, videoQualities [videoQualityIndex])["link_secure"];
                }
            }

            Debug.LogError ("VimeoPlayer: You do not have access to this video's files. You must be a Vimeo Pro or Business customer and use videos from your own account.");
            return null;
        }

		private JSONNode FindByQuality(List<JSONNode> qualities, string quality)
		{
			for (int i = 0; i < qualities.Count; i++) {
                if (int.Parse(qualities[i]["height"]) <= int.Parse(quality)) {
					Debug.Log ("Loading " + qualities[i]["height"] + "p file");
					return qualities[i];
				}
			}

			Debug.LogWarning("Couldnt find quality. Defaulting to " + qualities[0]["height"] + "p");
			return qualities [0];
		}

		private static int SortByQuality(JSONNode q1, JSONNode q2)
		{
			return int.Parse (q2["height"]).CompareTo (int.Parse (q1["height"]));
		}
    }
}
