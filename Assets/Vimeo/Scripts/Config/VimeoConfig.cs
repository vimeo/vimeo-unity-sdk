using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Vimeo;

namespace Vimeo.Config 
{
    public class VimeoConfig : Editor 
    {
        public void DrawVimeoConfig(VimeoPlayer player)
        {
            var so = serializedObject;
            player.videoQualityIndex = EditorGUILayout.Popup("Max quality", player.videoQualityIndex, player.videoQualities);
            DrawVimeoAuth(player.accessToken);

            so.ApplyModifiedProperties();
        }

        public void DrawVimeoConfig(VimeoPublisher publisher)
        {
            var so = serializedObject;
            DrawVimeoAuth (publisher.accessToken);

            if (Authenticated(publisher.accessToken)) {
                EditorGUILayout.Space();

                EditorGUILayout.PropertyField(so.FindProperty("camera"));
                EditorGUILayout.PropertyField(so.FindProperty("videoName"));
                EditorGUILayout.PropertyField(so.FindProperty("m_privacyMode"));
                EditorGUILayout.PropertyField(so.FindProperty("openInBrowser"));

                DrawSlackConfig(publisher);
            }

            so.ApplyModifiedProperties();
        }

        public void DrawVimeoAuth(string _token)
        {
            var so = serializedObject;
            if (!Authenticated(_token)) {
                EditorGUILayout.PropertyField (so.FindProperty ("accessToken"));
                //GUILayout.Box("To generate a Vimeo access token, create or edit a developer app, visit the Authentication tab, and generate a new access token with Upload & Edit scopes.");
                if (GUILayout.Button ("Sign into Vimeo")) {
                    Application.OpenURL ("https://vimeo-unity.herokuapp.com/auth/vimeo");
                }
            } 
            else {
                if (GUILayout.Button("Switch accounts")) {
                    var t = target as VimeoPublisher;
                    t.accessToken = null;
                    //Application.OpenURL("https://vimeo-unity.herokuapp.com/auth/vimeo");
                }
            }
        }

        public void DrawSlackAuth(string _token)
        {
            var so = serializedObject;
            if (!Authenticated(_token)) {
                EditorGUILayout.PropertyField (so.FindProperty ("slackToken"));
                if (GUILayout.Button ("Sign into Slack")) {
                    Application.OpenURL ("https://vimeo-unity.herokuapp.com/auth/slack");
                }
            } 
            else {
                if (GUILayout.Button("Disable")) {
                    var t = target as VimeoPublisher;
                    t.slackToken = null;
                }
            }
        }

        public bool Authenticated(string token)
        {
            return token != "" && token != null;
        }

        public void DrawSlackConfig(VimeoPublisher publisher)
        {
            var so = serializedObject;
            EditorGUILayout.PropertyField(so.FindProperty("postToSlack"));

            if (publisher.postToSlack == true) {

                if (Authenticated (publisher.slackToken)) {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField (so.FindProperty ("slackChannel"));
                    EditorGUILayout.PropertyField (so.FindProperty ("slackMessage"));
                    EditorGUI.indentLevel--;
                } 
                else {
                    DrawSlackAuth (publisher.slackToken);
                }
            }
        }
    }
}