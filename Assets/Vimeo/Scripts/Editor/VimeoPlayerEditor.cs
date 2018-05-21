using UnityEngine;
using UnityEditor;
using Vimeo.Player;

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

            if (Authenticated(player.GetVimeoToken()) && player.vimeoSignIn && GUILayout.Button("Sign out", GUILayout.Width(60))) {
                player.vimeoSignIn = false;
                player.SetVimeoToken(null);
            }

            if (GUILayout.Button("Need help?", GUILayout.Width(70))) {
                Application.OpenURL("https://github.com/vimeo/vimeo-unity-sdk");
            }
            GUILayout.EndHorizontal();
            EditorGUILayout.Space();
            
            if (Authenticated(player.GetVimeoToken()) && player.vimeoSignIn) {
                EditorGUILayout.PropertyField(so.FindProperty("videoScreen"));
                EditorGUILayout.PropertyField(so.FindProperty("audioSource"));
                EditorGUILayout.PropertyField(so.FindProperty("vimeoVideoId"), new GUIContent("Vimeo Video URL"));
                EditorGUILayout.PropertyField(so.FindProperty("selectedResolution"), new GUIContent("Resolution"));
                EditorGUILayout.PropertyField(so.FindProperty("muteAudio"), new GUIContent("Mute audio?"));

                EditorGUILayout.Space();
            }

            DrawVimeoAuth(player);
            so.ApplyModifiedProperties();
        }
    }
}