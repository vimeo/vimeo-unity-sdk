using UnityEngine;
using UnityEditor;

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
            
            if (Authenticated(player.GetVimeoToken()) && player.vimeoSignIn) {
                EditorGUILayout.PropertyField(so.FindProperty("videoScreen"));
                EditorGUILayout.PropertyField(so.FindProperty("audioSource"));
                EditorGUILayout.PropertyField(so.FindProperty("vimeoVideoId"), new GUIContent("Vimeo Video URL"));
                player.videoQualityIndex = EditorGUILayout.Popup("Video Quality", player.videoQualityIndex, player.videoQualities);

                EditorGUILayout.Space();
            }

            DrawVimeoAuth(player);
            so.ApplyModifiedProperties();
        }
    }
}