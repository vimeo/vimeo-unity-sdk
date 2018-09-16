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
        private VimeoVideo vimeoVideo;

        private void Start()
        {
            Application.runInBackground = true; 
            
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

            if (videoPlayerType == VideoPlayerType.UnityPlayer) {
                // TODO abstract this out into a VideoPlayerManager (like EncoderManager.cs)
                controller = gameObject.AddComponent<VideoController>();
                controller.playerSettings = this;

                controller.OnVideoStart += VideoStarted;
                controller.OnPlay       += VideoPlay;
                controller.OnPause      += VideoPaused;
                controller.OnFrameReady += VideoFrameReady;
            }

            if (audioSource && audioSource is AudioSource) {
                if (audioSource != null) {
                    controller.audioSource = audioSource;
                }
                else {
                    videoScreen.gameObject.AddComponent<AudioSource>();
                }
            }

            if (api != null){
                api.OnError += ApiError;
                api.OnNetworkError += NetworkError;
            }

            LoadVimeoVideoByUrl(vimeoVideoId);

            if (OnLoad != null) OnLoad();
        }

        public void LoadVimeoVideoByUrl(string vimeo_url)
        {
            if (vimeo_url != null && vimeo_url != "") {
                vimeoVideo = null;
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
            controller.videoScreenObject = videoScreen;

            if (videoScreen == null && videoPlayerType == VideoPlayerType.UnityPlayer) {
                Debug.LogWarning("No video screen was specified.");
            }

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
            return vimeoVideo != null && vimeoVideo.uri != null && vimeoVideo.uri != "";
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
                if (json["user"]["account"].Value == "basic") {
                    Debug.LogError("[VimeoPlayer] You do not have permission to stream videos. You must be a Vimeo Pro or Business customer. https://vimeo.com/upgrade");
                }

                if ((json["play"] == null || json["play"]["progressive"] == null) && json["files"] == null) {
                    Debug.LogError("[VimeoPlayer] You do not have permission to access to this video. You must be a Vimeo Pro or Business customer and use videos from your own account. https://vimeo.com/upgrade");
                }
                
                vimeoVideo = new VimeoVideo(json);

#if VIMEO_AVPRO_VIDEO_SUPPORT                
                string file_url = null;

                if (this.selectedResolution == StreamingResolution.Adaptive) {
                    file_url = vimeoVideo.GetAdaptiveVideoFileURL();
                }
                else {
                    file_url = vimeoVideo.GetVideoFileUrlByResolution(selectedResolution);
                }
                
                if (videoPlayerType == VideoPlayerType.AVProVideo && mediaPlayer != null) {
                    mediaPlayer.OpenVideoFromFile(RenderHeads.Media.AVProVideo.MediaPlayer.FileLocation.AbsolutePathOrURL, file_url, autoPlay);
                }
#if VIMEO_DEPTHKIT_SUPPORT
                else if (videoPlayerType == VideoPlayerType.DepthKit && depthKitClip != null) {
                    // depthKitClip._moviePath = file_url;
                    // depthKitClip._metaDataText = vimeoVideo.description;
                    // depthKitClip.GetComponent<RenderHeads.Media.AVProVideo.MediaPlayer>().OpenVideoFromFile(RenderHeads.Media.AVProVideo.MediaPlayer.FileLocation.AbsolutePathOrURL, file_url, autoPlay);
                    // depthKitClip.RefreshRenderer();
                }
#endif  
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
                controller.PlayVideo(vimeoVideo, selectedResolution, autoPlay);
            }
        }

        private void ApiError(string response)
        {
            Debug.LogError(response);
        }

        private void NetworkError(string error_message)
        {
            Debug.LogError("It seems like you are not connected to the internet or are having connection problems.");
        }
    }
}
