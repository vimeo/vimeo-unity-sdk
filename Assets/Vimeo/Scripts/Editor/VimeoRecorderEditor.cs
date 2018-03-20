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

            var recorder = target as VimeoRecorder;
            recorder.recordOnStart = false;
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

                DrawRecorderConfig(recorder);

                publishFold = EditorGUILayout.Foldout(publishFold, "Publish to");
                
                if (publishFold) {
                    EditorGUI.indentLevel++;

                    GUILayout.BeginHorizontal();
                    vimeoFold = EditorGUILayout.Foldout(vimeoFold, "Vimeo");
                    if (GUILayout.Button("Sign out", GUILayout.Width(60))) {
                        recorder.vimeoSignIn = false;
                        recorder.SetVimeoToken(null);
                    }
                    GUILayout.EndHorizontal();

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

                DrawRecordingControls();
            }

            DrawVimeoAuth(recorder);

            so.ApplyModifiedProperties();
        }

        public void DrawRecordingControls()
        {
            var recorder = target as VimeoRecorder;
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            if (EditorApplication.isPlaying) {
                if (recorder.isRecording) {
                    GUI.backgroundColor = Color.green;

                    if (GUILayout.Button("Finish & Upload", GUILayout.Height(30))) {
                        recorder.EndRecording();
                    }

                    GUI.backgroundColor = Color.red;
                    if (GUILayout.Button("Cancel", GUILayout.Height(30))) {
                        recorder.CancelRecording();
                    }
                }
                else if (recorder.isUploading) {
                    GUI.backgroundColor = Color.red;
                    if (GUILayout.Button("Cancel", GUILayout.Height(30))) {
                        recorder.CancelRecording();
                    }
                }
                else {
                    GUI.backgroundColor = Color.green;
                    if (GUILayout.Button("Start Recording", GUILayout.Height(30))) {
                        recorder.BeginRecording();
                    }
                }

                GUI.backgroundColor = Color.white;
            }
            else {
                GUI.backgroundColor = Color.green;
                if (GUILayout.Button("Start Recording", GUILayout.Height(30))) {
                    recorder.recordOnStart = true;
                    EditorApplication.isPlaying = true;
                }
                GUI.backgroundColor = Color.white;
            }
            GUILayout.EndHorizontal();

            // Progress bar
            if (EditorApplication.isPlaying && (recorder.isRecording || recorder.isUploading)) {
                EditorGUILayout.Space();
                var rect = EditorGUILayout.BeginHorizontal();
                rect.height = 20;
                GUILayout.Box("", GUILayout.Height(20));

                int seconds = recorder.recorder.currentFrame / recorder.frameRate;
                float progress = recorder.recorder.currentFrame / (float)(recorder.recordDuration * recorder.frameRate);

                if (recorder.recordMode != RecordMode.Duration) {
                    progress = 0;
                }

                if (recorder.isUploading) {
                    EditorGUI.ProgressBar(rect, progress, "Uploading to Vimeo...");
                }
                else {
                    EditorGUI.ProgressBar(rect, progress, seconds + " seconds (" + recorder.recorder.currentFrame.ToString() + " frames)");
                }
                GUILayout.EndHorizontal();
            }
        }

        public void DrawRecorderConfig(VimeoRecorder recorder)
        {
            var so = serializedObject;

            GUILayout.BeginHorizontal();
            recordingFold = EditorGUILayout.Foldout(recordingFold, "Recording");
            GUILayout.EndHorizontal();

            if (recordingFold) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(so.FindProperty("defaultVideoInput"), new GUIContent("Input"));

                if (recorder.defaultVideoInput == Vimeo.Recorder.VideoInputType.Camera) {
                    EditorGUILayout.PropertyField(so.FindProperty("defaultCamera"), new GUIContent("Camera"));
                }
                else if (recorder.defaultVideoInput == Vimeo.Recorder.VideoInputType.Camera360) {
                    EditorGUILayout.PropertyField(so.FindProperty("defaultCamera360"), new GUIContent("Camera"));
                    EditorGUILayout.PropertyField(so.FindProperty("defaultRenderMode360"), new GUIContent("Render mode"));
                }

                if (recorder.defaultVideoInput != Vimeo.Recorder.VideoInputType.Camera360) {
                    EditorGUILayout.PropertyField(so.FindProperty("defaultResolution"), new GUIContent("Resolution"));
                    
                    if (recorder.defaultResolution != Vimeo.Recorder.Resolution.Window) {
                        EditorGUILayout.PropertyField(so.FindProperty("defaultAspectRatio"));
                    }
                }

                EditorGUILayout.PropertyField(so.FindProperty("frameRate"));
                EditorGUILayout.PropertyField(so.FindProperty("realTime"));

                EditorGUILayout.PropertyField(so.FindProperty("recordMode"));

                if (recorder.recordMode == RecordMode.Duration) {
                    EditorGUILayout.PropertyField(so.FindProperty("recordDuration"), new GUIContent("Duration (sec)"));
                }

                EditorGUI.indentLevel--;
            }
        }

        public void DrawSlackAuth(string _token, VimeoRecorder recorder)
        {
            GUIStyle customstyle = new GUIStyle();
            customstyle.margin = new RectOffset(40, 0, 0, 0);
            
            GUILayout.BeginHorizontal();

            var so = serializedObject;
            if (!Authenticated(_token)) {
                EditorGUILayout.PropertyField(so.FindProperty("slackToken"));
                if (recorder.slackToken == null || recorder.slackToken == "") {
                    if (GUILayout.Button("Get Token", GUILayout.Width(80))) {
                        Application.OpenURL("https://authy.vimeo.com/auth/slack");
                    }
                }
                else {                
                    if (GUILayout.Button("Get Token", GUILayout.Width(80))) {
                        Application.OpenURL("https://authy.vimeo.com/auth/slack");
                    }

                    GUILayout.EndHorizontal();        
                    GUILayout.BeginHorizontal(customstyle);
                    GUI.backgroundColor = Color.green;
                    if (GUILayout.Button("Sign in")) {
                       recorder.SetSlackToken(recorder.slackToken);
                       recorder.slackToken = null;
                       GUI.FocusControl(null);
                    }
                    GUI.backgroundColor = Color.white;
                }
            } 

            GUILayout.EndHorizontal();
        }

        public void DrawSlackConfig(VimeoRecorder recorder)
        {
            var so = serializedObject;

            GUILayout.BeginHorizontal();
            slackFold = EditorGUILayout.Foldout(slackFold, "Slack");
            if (Authenticated(recorder.GetSlackToken()) && GUILayout.Button("Sign out", GUILayout.Width(60))) {
                recorder.SetSlackToken(null);   
            }
            GUILayout.EndHorizontal();

            if (slackFold) {
                EditorGUI.indentLevel++;
                if (Authenticated(recorder.GetSlackToken())) {
                    EditorGUILayout.PropertyField(so.FindProperty("slackChannel"));
                    EditorGUILayout.PropertyField(so.FindProperty("defaultShareLink"));
                    EditorGUILayout.PropertyField(so.FindProperty("autoPostToChannel"));
                } 

                DrawSlackAuth(recorder.GetSlackToken(), recorder);
                EditorGUI.indentLevel--;
            }
        }
    }
}

#endif