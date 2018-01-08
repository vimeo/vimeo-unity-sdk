using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

using Vimeo.Services;
using Vimeo.Config;
using System.Text.RegularExpressions;

namespace Vimeo
{
#if UNITY_EDITOR  
    using UnityEditor;
	[CustomEditor (typeof(VimeoPlayer))]
    public class VimeoPlayerInspector : VimeoConfig
	{
        [MenuItem("GameObject/Video/Vimeo Player (Canvas)")]
        private static void CreateCanvasPrefab() {
            GameObject go = Instantiate(Resources.Load("Prefabs/[VimeoPlayerCanvas]") as GameObject);
            go.name = "[VimeoPlayerCanvas]";
        }

        [MenuItem("GameObject/Video/Vimeo Player (Plane)")]
        private static void CreatePlanePrefab() {
            GameObject go = Instantiate(Resources.Load("Prefabs/[VimeoPlayer]") as GameObject);
            go.name = "[VimeoPlayer]";
        }

        [MenuItem("GameObject/Video/Vimeo Player (360)")]
        private static void Create360Prefab() {
            GameObject go = Instantiate(Resources.Load("Prefabs/[VimeoPlayer360]") as GameObject);
            go.name = "[VimeoPlayer360]";
        }

		public override void OnInspectorGUI()
		{
			var player = target as VimeoPlayer;
            DrawVimeoConfig(player); 
            EditorUtility.SetDirty(target);
		}
    }
#endif

	[AddComponentMenu("Video/Vimeo Player")]
	public class VimeoPlayer : VimeoBehavior 
    {
        public delegate void VimeoEvent();
        public event VimeoEvent OnVideoStart;
        public event VimeoEvent OnPause;
        public event VimeoEvent OnPlay;

        public GameObject videoScreen;
        public GameObject audioSource;

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
        public VideoController controller;

		private void Start()
        {
            if (GetVimeoToken() != null) {
                api = gameObject.AddComponent<VimeoApi>();
                api.token = GetVimeoToken();
                api.OnRequestComplete += OnLoadVimeoVideoComplete;
            }

            if (videoScreen == null) {
                Debug.LogWarning("No video screen was specified.");
            }

            if (audioSource && audioSource.GetType() == typeof(GameObject)) {
                if (audioSource.GetComponent<AudioSource>() != null) {
                    controller.audioSource = audioSource.GetComponent<AudioSource>();
                }
                else {
                    Debug.LogWarning("No AudioSource component found on " + audioSource.name + " GameObject");
                }
            }

            controller = gameObject.AddComponent<VideoController>();
            controller.videoScreenObject = videoScreen;

            controller.OnVideoStart += VideoStarted;
            controller.OnPlay       += VideoPlay;
            controller.OnPause      += VideoPaused;

            // Bootup video
            if (vimeoVideoId != null && vimeoVideoId != "") {
                vimeoVideoId = Regex.Split(vimeoVideoId, "/?([0-9]+)")[1];
                LoadVimeoVideo(int.Parse(vimeoVideoId));
            } 
            else {
                Debug.LogWarning("No Vimeo video ID was specified");
            }
        }

        public void LoadVimeoVideo(int id)
        {
            api.GetVideoFileUrlByVimeoId(id);
        }

		public void Play()
		{
			controller.Play();
		}

		public void Pause()
		{
			controller.Pause();
		}

        public void Seek(float seek)
        {
            controller.Seek(seek);
        }

		public void SeekBackward(float seek)
		{
			controller.SeekBackward(seek);
		}

		public void SeekForward(float seek)
		{
			controller.SeekForward(seek);
		}

        public void ToggleVideoPlayback()
        {
            controller.TogglePlayback();
        }

        public int GetWidth()
        {
            return controller.width;
        }

        public int GetHeight()
        {
            return controller.height;
        }

		public float GetProgress()
		{
            if (controller != null && controller.videoPlayer != null) {
				return (float)controller.GetCurrentFrame() / (float)controller.GetTotalFrames();
			}
			return 0;
		}

		public string GetTimecode()
		{
			if (controller != null) {
				float sec = Mathf.Floor ((float)controller.videoPlayer.time % 60);
				float min = Mathf.Floor ((float)controller.videoPlayer.time / 60f);

				string secZeroPad = sec > 9 ? "" : "0";
				string minZeroPad = min > 9 ? "" : "0";

				return minZeroPad + min + ":" + secZeroPad + sec;
			}

			return null;
		}

        // Events below
        private void VideoStarted(VideoController controller) {
            if (OnVideoStart != null) {
                OnVideoStart();
            }
        }

        private void VideoPlay(VideoController controller)
        {
            if (OnPlay != null) {
                OnPlay();
            }
        }

        private void VideoPaused(VideoController controller)
        {
            if (OnPause != null) {
                OnPause();
            }
        }

        private void OnLoadVimeoVideoComplete(string response)
        {
			var json = JSON.Parse(response);
            if (json["error"] == null) {
                controller.PlayVideos(GetVideoFiles(json), is3D, videoStereoFormat);
            } 
            else {
                Debug.LogError("Video could not be found");
            }
        }

        private List<JSONNode> GetVideoFiles(JSONNode json)
        {
            if (json["play"] == null && json["files"] == null) {
                Debug.LogError("VimeoPlayer: You do not have access to this video's files. You must be a Vimeo Pro or Business customer and use videos from your own account.");
                return null;
            }

            // Set the metadata
            videoName = json["name"];
            videoThumbnailUrl = json["pictures"]["sizes"][4]["link"];

            if (json["user"]["pictures"] != null && !json["user"]["pictures"].IsNull) {
               authorThumbnailUrl = json["user"]["pictures"]["sizes"][2]["link"];
            }

            is3D = false;
            videoStereoFormat = "mono";

            if (json["spatial"] != null && !json["spatial"].IsNull) {
                is3D = true;
                videoProjection   = json["spatial"]["projection"];
                videoStereoFormat = json["spatial"]["stereo_format"];
            }

            List<JSONNode> qualities = new List<JSONNode>();

            // New Vimeo file response format
            if (json["play"] != null) {
                JSONNode progressiveFiles = json["play"]["progressive"];

                // Sort the quality
                for (int i = 0; i < progressiveFiles.Count; i++) {
                    qualities.Add(progressiveFiles[i]);
                }   
                qualities.Sort(SortByQuality);
            }

            // Current Vimeo file API response format
            if (json["files"] != null) {
                JSONNode progressiveFiles = json["files"];

                // Get all progressive video files 
                for (int i = 0; i < progressiveFiles.Count; i++) {
                    if (progressiveFiles[i]["height"] != null && progressiveFiles[i]["type"].Value == "video/mp4") {
                        qualities.Add(progressiveFiles[i]);
                    }
                }
                qualities.Sort(SortByQuality);
            }

            // If set to highest, get the first one
            if (videoQualities[videoQualityIndex] == "Highest") {
                return qualities;
            } 
            else {
                return GetPreferredQualities(qualities, videoQualities[videoQualityIndex]);
            }
        }

		private List<JSONNode> GetPreferredQualities(List<JSONNode> qualities, string quality)
		{
            List<JSONNode> preferred_qualities = new List<JSONNode>();
			for (int i = 0; i < qualities.Count; i++) {
                if (int.Parse(qualities[i]["height"]) <= int.Parse(quality)) {
					Debug.Log("Loading " + qualities[i]["height"] + "p file");
                    preferred_qualities.Add(qualities[i]);
				}
			}

			Debug.LogWarning("Couldnt find quality. Defaulting to " + qualities[0]["height"] + "p");
			return qualities;
		}

		private static int SortByQuality(JSONNode q1, JSONNode q2)
		{
			return int.Parse(q2["height"]).CompareTo(int.Parse(q1["height"]));
		}
    }
}
