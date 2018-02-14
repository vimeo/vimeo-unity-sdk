#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

namespace Vimeo 
{
    [CustomEditor(typeof(VimeoRecorder))]
    public class VimeoRecorderEditor : BaseEditor
    {
        bool recordingFold;
        bool vimeoFold;
        bool slackFold;

        public override void OnInspectorGUI()
        {
            var recorder = target as VimeoRecorder;
            DrawConfig(recorder);
            EditorUtility.SetDirty(target);
        }
            
        public void DrawConfig(VimeoRecorder recorder)
        {
            var so = serializedObject;

            if (Authenticated(recorder.GetVimeoToken()) && recorder.vimeoSignIn) {
                EditorGUILayout.Space();

                EditorGUILayout.PropertyField(so.FindProperty("recordOnStart"));
                EditorGUILayout.PropertyField(so.FindProperty("openInBrowser"));

                recordingFold = EditorGUILayout.Foldout(recordingFold, "Recording");
                if (recordingFold) {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(so.FindProperty("videoInput"));
                    EditorGUILayout.PropertyField(so.FindProperty("videoResolution"));
                    EditorGUILayout.PropertyField(so.FindProperty("videoAspectRatio"));
                    EditorGUI.indentLevel--;
                }

                vimeoFold = EditorGUILayout.Foldout(vimeoFold, "Publish to");
                if (vimeoFold) {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(so.FindProperty("videoName"));
                    EditorGUILayout.PropertyField(so.FindProperty("privacyMode"));
                    EditorGUI.indentLevel--;
                }

                DrawSlackConfig(recorder);
            }

            DrawVimeoAuth(recorder);
            so.ApplyModifiedProperties();
        }

        public void DrawSlackAuth(string _token)
        {
            var so = serializedObject;
            if (!Authenticated(_token)) {
                EditorGUILayout.PropertyField(so.FindProperty("slackToken"));
                if (GUILayout.Button("Sign into Slack")) {
                    Application.OpenURL("https://authy.vimeo.com/auth/slack");
                }
            } 
            else {
                if (GUILayout.Button("Switch Slack Team")) {
                    var t = target as VimeoRecorder;
                    //t.SetSlackToken(null);
                }
            }
        }

        public void DrawSlackConfig(VimeoRecorder recorder)
        {
            var so = serializedObject;
            slackFold = EditorGUILayout.Foldout(slackFold, "Share to Slack");

            if (slackFold) {
                EditorGUI.indentLevel++;
                if (Authenticated(recorder.GetSlackToken())) {
                    EditorGUILayout.PropertyField(so.FindProperty("slackChannel"));
                    EditorGUILayout.PropertyField(so.FindProperty("defaultShareLink"));
                    EditorGUILayout.PropertyField(so.FindProperty("autoPostToChannel"));
                } 

                DrawSlackAuth(recorder.GetSlackToken());
                EditorGUI.indentLevel--;
            }
        }
    }
}

#endif