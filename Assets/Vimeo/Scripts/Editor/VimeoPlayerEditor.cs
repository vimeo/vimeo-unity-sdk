using UnityEngine;
using UnityEditor;
using Vimeo.Player;
using System.Linq;

namespace Vimeo
{
    [CustomEditor(typeof(VimeoPlayer))]
    public class VimeoPlayerEditor : BaseEditor
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

        public void DrawVimeoConfig(VimeoPlayer player)
        {
            var so = serializedObject;

             // Help Nav            
            GUILayout.BeginHorizontal();
            var style = new GUIStyle();
            style.border = new RectOffset(0,0,0,0);
            GUILayout.Box("", style);

            if (Authenticated(player.GetVimeoToken()) && player.vimeoSignIn && GUILayout.Button("⇪ Upload", GUILayout.Width(70))) {
                Application.OpenURL("https://vimeo.com/upload");
            }

            if (GUILayout.Button("Help", GUILayout.Width(50))) {
                Application.OpenURL("https://github.com/vimeo/vimeo-unity-sdk");
            }

            GUISignOutButton();
            
            GUILayout.EndHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            
            if (Authenticated(player.GetVimeoToken()) && player.vimeoSignIn) {
#if VIMEO_AVPRO_VIDEO_SUPPORT
                if (!player.IsVideoMetadataLoaded()) {
                    EditorGUILayout.PropertyField(so.FindProperty("videoPlayerType"), new GUIContent("Video Player"));
                    if (player.videoPlayerType == VideoPlayerType.AVProVideo) {
                        EditorGUILayout.PropertyField(so.FindProperty("mediaPlayer"), new GUIContent("AVPro Media Player"));
                        if (player.mediaPlayer == null) {
                            EditorGUILayout.HelpBox("You need to select a MediaPlayer object.", MessageType.Warning);
                        }
                    }
                }
#else
                player.videoPlayerType = VideoPlayerType.UnityPlayer;
#endif

                bool updated = GUISelectFolder();
                GUISelectVideo(updated);

                EditorGUILayout.PropertyField(so.FindProperty("selectedResolution"), new GUIContent("Resolution"));
                
                if (player.selectedResolution == StreamingResolution.Adaptive && player.videoPlayerType == VideoPlayerType.UnityPlayer) {
                    EditorGUILayout.HelpBox("Adaptive video support (HLS/DASH) is only available with the AVPro Video plugin which is available in the Unity Asset Store.", MessageType.Error);
                }

                if (player.videoPlayerType != VideoPlayerType.AVProVideo && !player.IsVideoMetadataLoaded()) {
                    EditorGUILayout.PropertyField(so.FindProperty("videoScreen"));
                    EditorGUILayout.PropertyField(so.FindProperty("audioSource"));
                    EditorGUILayout.PropertyField(so.FindProperty("muteAudio"), new GUIContent("Mute Audio"));
                    EditorGUILayout.PropertyField(so.FindProperty("autoPlay"));
                    EditorGUILayout.PropertyField(so.FindProperty("startTime"));
                }

                EditorGUILayout.Space();

                if (EditorApplication.isPlaying) {
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Load Video", GUILayout.Height(30), GUILayout.Width(100))) {
                        player.autoPlay = true;
                        player.LoadVimeoVideoByUrl(player.vimeoVideoId);   
                    }

                    if (player.videoPlayerType == VideoPlayerType.UnityPlayer && player.IsVideoMetadataLoaded()) {
                        if (!player.IsPlaying()) {
                            GUI.backgroundColor = Color.green;
                            if (GUILayout.Button("Play Video", GUILayout.Height(30))) {
                                player.Play();
                            }
                        }

                        GUI.backgroundColor = Color.white;
                        if (player.IsPlaying() && GUILayout.Button("Pause Video", GUILayout.Height(30))) {
                            player.Pause();
                        }
                    }
                    GUILayout.EndHorizontal();
                }
            }

            DrawVimeoAuth(player);
            so.ApplyModifiedProperties();
        }

        private void GUISelectVideo(bool refreshVideos = false)
        {
            var so = serializedObject;
            var player = target as VimeoPlayer;

            if (player.currentFolder.uri == "custom") {
                EditorGUILayout.PropertyField(so.FindProperty("vimeoVideoId"), new GUIContent("Vimeo Video URL"));
            }
            else if (player.currentFolder.uri != null && player.currentFolder.uri != "") {
                GUILayout.BeginHorizontal();
                int cur_video_index = player.GetCurrentVideoIndex();
                int new_video_index = EditorGUILayout.Popup(" ", cur_video_index, player.vimeoVideos.Select(v => v.name).ToArray()); 

                if (new_video_index != cur_video_index) {
                    player.currentVideo = player.vimeoVideos[new_video_index];
                    player.vimeoVideoId = player.currentVideo.id.ToString();
                }

                if (GUILayout.Button("↪", GUILayout.Width(25))) {
                    
                }

                if (GUILayout.Button("↺", GUILayout.Width(25)) || 
                    refreshVideos || 
                    (player.vimeoVideos.Count == 0 && player.GetComponent<VimeoApi>() == null)) {
                        
                    if (player.currentFolder.uri == "recent") {
                        GetRecentVideos();
                    }
                    else if (player.currentFolder.id > 0) {
                        GetVideosInFolder(player.currentFolder);
                    }
                }

                GUILayout.EndHorizontal();
            }
        }

    }
}