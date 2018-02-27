#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using Vimeo.Recorder;

namespace Vimeo.Recorder
{
    [CustomEditor(typeof(VimeoRecorder))]
    public class VimeoRecorderEditor : BaseEditor
    {
        static bool recordingFold;
        static bool publishFold;
        static bool vimeoFold;
        static bool slackFold;

        void OnDisable()
        {
            EditorPrefs.SetBool("recordingFold", recordingFold);
            EditorPrefs.SetBool("publishFold", publishFold);
            EditorPrefs.SetBool("vimeoFold", vimeoFold);
            EditorPrefs.SetBool("slackFold", slackFold);
        }

        void OnEnable()
        {
            recordingFold = EditorPrefs.GetBool("recordingFold");
            publishFold = EditorPrefs.GetBool("publishFold");
            vimeoFold = EditorPrefs.GetBool("vimeoFold");
            slackFold = EditorPrefs.GetBool("slackFold");
        }

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

                recordingFold = EditorGUILayout.Foldout(recordingFold, "Recording");
                if (recordingFold) {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(so.FindProperty("defaultVideoInput"), new GUIContent("Input"));
                    EditorGUILayout.PropertyField(so.FindProperty("defaultResolution"), new GUIContent("Resolution"));

                    if (recorder.defaultResolution != Vimeo.Recorder.Resolution.Window) {
                        EditorGUILayout.PropertyField(so.FindProperty("defaultAspectRatio"));
                    }
                    EditorGUILayout.PropertyField(so.FindProperty("recordOnStart"));
                    EditorGUI.indentLevel--;
                }

                publishFold = EditorGUILayout.Foldout(publishFold, "Publish to");
                
                if (publishFold) {
                    EditorGUI.indentLevel++;
                    vimeoFold = EditorGUILayout.Foldout(vimeoFold, "Vimeo");

                    if (vimeoFold) {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(so.FindProperty("videoName"));
                        EditorGUILayout.PropertyField(so.FindProperty("privacyMode"));
                        EditorGUILayout.PropertyField(so.FindProperty("openInBrowser"));
                        EditorGUI.indentLevel--;
                    }

                    DrawSlackConfig(recorder);
                    EditorGUI.indentLevel--;
                }
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
            slackFold = EditorGUILayout.Foldout(slackFold, "Slack");

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