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
    public class PlayerSettings : VimeoAuth
    {
        public StreamingResolution selectedResolution = StreamingResolution.x2160p_4K; 
        public string vimeoVideoId;
        public bool muteAudio = false;
    }
}