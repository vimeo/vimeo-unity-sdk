#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using Vimeo;

namespace Vimeo.Config 
{  
    public class VimeoConfig : Editor 
    {
        bool slackFold;
        bool vimeoFold;

        public bool Authenticated(string token)
        {
            return token != "" && token != null;
        }

        public void DrawVimeoAuth(VimeoBehavior auth)
        {
            var so = serializedObject;

            if (!Authenticated(auth.GetVimeoToken()) || !auth.vimeoSignIn) {
                EditorGUILayout.PropertyField(so.FindProperty("vimeoToken"));
                EditorGUILayout.PropertyField(so.FindProperty("saveVimeoToken"), new GUIContent("Save token with scene"));

                GUILayout.BeginHorizontal("box");
                if (GUILayout.Button("Get Token", GUILayout.Height(30))) {
                    if (auth is VimeoPlayer) {
                        Application.OpenURL("https://authy.vimeo.com/auth/vimeo/unity?scope=public%20private%20video_files");
                    }
                    else {
                        Application.OpenURL("https://authy.vimeo.com/auth/vimeo/unity");
                    }
                }

                GUI.backgroundColor = Color.green;
                if (Authenticated(auth.vimeoToken)) {
                    if (GUILayout.Button("Sign into Vimeo", GUILayout.Height(30))) {
                        auth.SetVimeoToken(auth.vimeoToken);
                        auth.vimeoSignIn = true;
                        GUI.FocusControl(null);
                    }
                }

                GUILayout.EndHorizontal();
            } 
            else {
                
                GUI.backgroundColor = Color.red;
                if (GUILayout.Button("Switch accounts")) {
                    auth.vimeoSignIn = false;
                    auth.SetVimeoToken(null);
                }
                GUI.backgroundColor = Color.white;
            }
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

#if UNITY_2017_3_OR_NEWER
        public void DrawVimeoConfig(VimeoPublisher publisher)
        {
            var so = serializedObject;

            if (Authenticated(publisher.GetVimeoToken()) && publisher.vimeoSignIn) {
                EditorGUILayout.Space();

                EditorGUILayout.PropertyField(so.FindProperty("_camera"));
                EditorGUILayout.PropertyField(so.FindProperty("recordOnStart"));
                EditorGUILayout.PropertyField(so.FindProperty("openInBrowser"));

                vimeoFold = EditorGUILayout.Foldout(vimeoFold, "Vimeo Default Settings");
                if (vimeoFold) {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(so.FindProperty("videoName"));
                    EditorGUILayout.PropertyField(so.FindProperty("m_privacyMode"));
                    EditorGUI.indentLevel--;
                }

                DrawSlackConfig(publisher);
            }

            DrawVimeoAuth(publisher);
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
                    var t = target as VimeoPublisher;
                    t.SetSlackToken(null);
                }
            }
        }

        public void DrawSlackConfig(VimeoPublisher publisher)
        {
            var so = serializedObject;
            slackFold = EditorGUILayout.Foldout(slackFold, "Share to Slack");

            if (slackFold) {
                EditorGUI.indentLevel++;
                if (Authenticated (publisher.GetSlackToken())) {
                    EditorGUILayout.PropertyField(so.FindProperty("slackChannel"));
                    EditorGUILayout.PropertyField(so.FindProperty("defaultShareLink"));
                    EditorGUILayout.PropertyField(so.FindProperty("autoPostToChannel"));
                } 

                DrawSlackAuth(publisher.GetSlackToken());
                EditorGUI.indentLevel--;
            }
        }
#endif
    }
}

#endif