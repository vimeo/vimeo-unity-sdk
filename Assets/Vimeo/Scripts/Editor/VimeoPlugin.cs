// Used with permission from http://www.depthkit.tv/

#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;

namespace Vimeo
{
    public enum PluginType
    {
        AVProVideo = 2,
        AVProCapture = 3,
        DepthKit = 4,
        LookingGlass = 5
    };

    public enum AvailablePluginType
    {
#if VIMEO_AVPRO_CAPTURE_SUPPORT
        AVProCapture = PluginType.AVProCapture,
#endif
#if VIMEO_AVPRO_VIDEO_SUPPORT
        AVProVideo = PluginType.AVProVideo,
#endif
#if VIMEO_DEPTHKIT_SUPPORT
        DepthKit = PluginType.DepthKit,
#endif
#if VIMEO_LOOKING_GLASS_SUPPORT
        LookingGlass = PluginType.LookingGlass,
#endif
    }

    public class VimeoPlugin
    {
        public const string Version = "0.9.3";
        public const string AVPRO_VIDEO_DEFINE = "VIMEO_AVPRO_VIDEO_SUPPORT";
        public const string AVPRO_CAPTURE_DEFINE = "VIMEO_AVPRO_CAPTURE_SUPPORT";
        public const string DEPTHKIT_DEFINE = "VIMEO_DEPTHKIT_SUPPORT";
        public const string LOOKING_GLASS_DEFINE = "VIMEO_LOOKING_GLASS_SUPPORT";

        public static Dictionary<string, PluginType> DirectiveDict = new Dictionary<string, PluginType>(){
            {AVPRO_VIDEO_DEFINE,   PluginType.AVProVideo},
            {AVPRO_CAPTURE_DEFINE, PluginType.AVProCapture},
            {DEPTHKIT_DEFINE,      PluginType.DepthKit},
            {LOOKING_GLASS_DEFINE, PluginType.LookingGlass},
        };

        // Which asset should be searched for to see if a player has been added
        // For example, if someone adds AVProVideo, they will have a file called MediaPlayer.cs now in their project
        public static Dictionary<string, string> AssetSearchDict = new Dictionary<string, string>() {
            {"MediaPlayer",     AVPRO_VIDEO_DEFINE},
            {"CaptureBase",     AVPRO_CAPTURE_DEFINE},
            {"DepthKitPlugin",  DEPTHKIT_DEFINE},
            {"Quilt",           LOOKING_GLASS_DEFINE},
        };

#if UNITY_EDITOR
        public static BuildTargetGroup[] SupportedPlatforms = new BuildTargetGroup[] {
            BuildTargetGroup.Android,
#if !UNITY_5_5_OR_NEWER
            BuildTargetGroup.Nintendo3DS,
#endif
            BuildTargetGroup.PS4,
            BuildTargetGroup.PSP2,
            BuildTargetGroup.Standalone,
            BuildTargetGroup.tvOS,
            BuildTargetGroup.XboxOne
        };
#endif
    }
}