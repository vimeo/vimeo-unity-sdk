using System.Collections.Generic;

namespace Vimeo.Player
{
    public enum StreamingResolution
    {
        x2160p_4K = 2160,
        x1440p_QHD = 1440,
        x1080p_FHD = 1080,
        x720p_HD = 720,
        x540p = 540,
        x360p = 360,
        Adaptive = 0,
    }

    public enum VideoPlayerType
    {
        UnityPlayer,
        AVProVideo
    }
    
    public class PlayerSettings : VimeoAuth
    {
        public VideoPlayerType videoPlayerType = VideoPlayerType.UnityPlayer;
#if VIMEO_AVPRO_VIDEO_SUPPORT
        public RenderHeads.Media.AVProVideo.MediaPlayer mediaPlayer;
#endif         
        public StreamingResolution selectedResolution = StreamingResolution.x2160p_4K; 
        public string vimeoVideoId;
        public bool muteAudio = false;
        public bool autoPlay = true;
        public int startTime = 0;
    }
}