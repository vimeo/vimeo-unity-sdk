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
#if VIMEO_AVPRO_VIDEO_SUPPORT
        AVProVideo,
#endif
#if VIMEO_DEPTHKIT_SUPPORT
        Depthkit
#endif 
    }

    public class PlayerSettings : VimeoSettings
    {
        public VideoPlayerType videoPlayerType = VideoPlayerType.UnityPlayer;
#if VIMEO_AVPRO_VIDEO_SUPPORT
        public RenderHeads.Media.AVProVideo.MediaPlayer mediaPlayer;
#endif         
#if VIMEO_DEPTHKIT_SUPPORT
        public Depthkit.Depthkit_Clip depthKitClip;
#endif         
        public StreamingResolution selectedResolution = StreamingResolution.x2160p_4K;
        public bool muteAudio = false;
        public bool autoPlay = true;
        public int startTime = 0;
    }
}