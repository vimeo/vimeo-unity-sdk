using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using System.Text.RegularExpressions;
using Vimeo;

namespace Vimeo.Player
{
    [AddComponentMenu("Video/Vimeo Player")]
    [HelpURL("https://github.com/vimeo/vimeo-unity-sdk")]
    public class VimeoPlayer : PlayerSettings 
    {
        public delegate void VimeoEvent();
        public event VimeoEvent OnLoad;
        public event VimeoEvent OnVideoStart;
        public event VimeoEvent OnPause;
        public event VimeoEvent OnPlay;
        public event VimeoEvent OnFrameReady;

        public GameObject videoScreen;
        public AudioSource audioSource;

        public string videoName;
        public string videoThumbnailUrl;
        public string authorThumbnailUrl;
        public bool is3D;
        public string videoProjection;
        public string videoStereoFormat;

        private VimeoApi api;
        public VideoController controller;
        private List<JSONNode> videoFiles;

        private void Start()
        {
            if (!vimeoSignIn) {
                Debug.LogWarning("You have not signed into the Vimeo Player.");
                return;
            }

            if (vimeoVideoId == null || vimeoVideoId == "") {
                Debug.LogWarning("No Vimeo video URL was specified");
            }

            if (GetVimeoToken() != null) {
                api = gameObject.AddComponent<VimeoApi>();
                api.token = GetVimeoToken();
            }

            if (videoScreen == null && videoPlayerType == VideoPlayerType.UnityPlayer) {
                Debug.LogWarning("No video screen was specified.");
            }

            if (audioSource && audioSource is AudioSource) {
                if (audioSource != null) {
                    controller.audioSource = audioSource;
                }
                else {
                    videoScreen.gameObject.AddComponent<AudioSource>();
                }
            }

            if (videoPlayerType == VideoPlayerType.UnityPlayer) {
                // TODO abstract this out into a VideoPlayerManager (like EncoderManager.cs)
                controller = gameObject.AddComponent<VideoController>();
                controller.videoScreenObject = videoScreen;
                controller.playerSettings = this;

                controller.OnVideoStart += VideoStarted;
                controller.OnPlay       += VideoPlay;
                controller.OnPause      += VideoPaused;
                controller.OnFrameReady += VideoFrameReady;
            }

            LoadVimeoVideoByUrl(vimeoVideoId);

            if (OnLoad != null) OnLoad();
        }

        public void LoadVimeoVideoByUrl(string vimeo_url)
        {
            if (vimeo_url != null && vimeo_url != "") {
                videoFiles = null;
                string[] matches = Regex.Split(vimeo_url, "(vimeo.com)?(/channels/[^/]+)?/?([0-9]+)"); // See https://regexr.com/3prh6
                if (matches[3] != null) {
                    vimeoVideoId = matches[3];
                    api.OnRequestComplete += OnLoadVimeoVideoComplete;
                    LoadVimeoVideoById(int.Parse(vimeoVideoId));
                }
                else {
                    Debug.LogWarning("Invalid Vimeo URL");
                }
            }
        }

        public void LoadVimeoVideoById(int vimeo_id)
        {
             api.GetVideoFileUrlByVimeoId(vimeo_id);
        }

        public bool IsPlaying()
        {
            if (IsPlayerLoaded()) { 
                return controller.videoPlayer.isPlaying;
            }
            else {
                return false;
            }
        }

        public bool IsVideoMetadataLoaded()
        {
            return videoFiles != null;
        }

        public bool IsPlayerLoaded()
        {
            return controller != null && controller.videoPlayer != null;
        }

        public void Play()
        {
            if (!IsPlayerLoaded()) {
                autoPlay = true;
                LoadVideo();
            }
            else {
                controller.Play();
            }
        }

        public void Pause()
        {
            controller.Pause();
        }

        public void Seek(float seek)
        {
            controller.Seek(seek);
        }

        public void SeekBySeconds(int seconds)
        {
            controller.SeekBySeconds(seconds);
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
            if (startTime > 0) {
                controller.SeekBySeconds(startTime);
            }

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

        private void VideoFrameReady(VideoController controller)
        {
            if (OnFrameReady != null) {
                OnFrameReady();
            }
        }

        private void OnLoadVimeoVideoComplete(string response)
        {
            JSONNode json = JSON.Parse(response);
            api.OnRequestComplete -= OnLoadVimeoVideoComplete;
            
            if (json["error"] == null) {
                videoFiles = GetVideoFiles(json);

#if VIMEO_AVPRO_VIDEO_SUPPORT                
                if (videoPlayerType == VideoPlayerType.AVProVideo && mediaPlayer != null) {
                    string file_url = null;

                    if (this.selectedResolution == StreamingResolution.Adaptive) {
                        file_url = GetAdaptiveVideoFileURL(json);
                    }
                    else {
                        file_url = videoFiles[0]["link"];
                    }
                    
                    mediaPlayer.OpenVideoFromFile(RenderHeads.Media.AVProVideo.MediaPlayer.FileLocation.AbsolutePathOrURL, file_url, autoPlay);
                }
                else {
                    LoadVideo();
                }   
#else  
                LoadVideo();
#endif
            } 
            else {
                Debug.LogError("Video could not be found");
            }
        }

        public void LoadVideo()
        {
            if (IsVideoMetadataLoaded()) {
                controller.PlayVideos(videoFiles, is3D, videoStereoFormat, autoPlay);
            }
        }

        private string GetAdaptiveVideoFileURL(JSONNode json) 
        {
            JSONNode progressiveFiles = json["files"];
            for (int i = 0; i < progressiveFiles.Count; i++) {
                if (progressiveFiles[i]["quality"].Value == "hls") {
                    Debug.Log(progressiveFiles[i]["link"]);
                    return progressiveFiles[i]["link"];
                }
            }
            return null;
        }

        private List<JSONNode> GetVideoFiles(JSONNode json)
        {
            if (json["play"] == null && json["files"] == null) {
                Debug.LogError("VimeoPlayer: You do not have access to this video's files. You must be a Vimeo Pro or Business customer and use videos from your own account.");
                return null;
            }

            // Set the metadata
            // TODO separate this from fetching video files
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

            return GetPreferredQualities(qualities, selectedResolution);
        }

        private List<JSONNode> GetPreferredQualities(List<JSONNode> qualities, StreamingResolution resolution)
        {
            if (resolution == StreamingResolution.Adaptive) {
                return null;
            }

            bool resolution_found = false;

            List<JSONNode> preferred_qualities = new List<JSONNode>();
            for (int i = 0; i < qualities.Count; i++) {
                if (int.Parse(qualities[i]["height"]) <= (int)resolution) {
                    preferred_qualities.Add(qualities[i]);

                    if (int.Parse(qualities[i]["height"]) == (int)resolution) {
                        resolution_found = true;
                    }
                }
            }

            if (!resolution_found) {
                Debug.Log("[VimeoPlayer] This video does not have a " + resolution + " resolution. Defaulting to " + preferred_qualities[0]["height"] + "p.");
            }

            return preferred_qualities;
        }

        private static int SortByQuality(JSONNode q1, JSONNode q2)
        {
            return int.Parse(q2["height"]).CompareTo(int.Parse(q1["height"]));
        }
    }
}
