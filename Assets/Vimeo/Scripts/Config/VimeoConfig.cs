using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

        public void DrawVimeoConfig(VimeoPlayer player)
        {
            var so = serializedObject;
            DrawVimeoAuth(player.GetVimeoToken());
          
            if (Authenticated(player.GetVimeoToken())) {
                EditorGUILayout.Space();

                EditorGUILayout.PropertyField(so.FindProperty("videoScreen"));
                EditorGUILayout.PropertyField(so.FindProperty("audioSource"));
                EditorGUILayout.PropertyField(so.FindProperty("vimeoVideoId"));
                player.videoQualityIndex = EditorGUILayout.Popup("Max video quality", player.videoQualityIndex, player.videoQualities);
            }

            so.ApplyModifiedProperties();
        }

        public void DrawVimeoConfig(VimeoPublisher publisher)
        {
            var so = serializedObject;
            DrawVimeoAuth (publisher.GetVimeoToken());

            if (Authenticated(publisher.GetVimeoToken())) {
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

            so.ApplyModifiedProperties();
        }

        public void DrawVimeoAuth(string _token)
        {
            var so = serializedObject;
            if (!Authenticated(_token)) {
				EditorGUILayout.PropertyField(so.FindProperty ("vimeoToken"));
                if (GUILayout.Button ("Sign into Vimeo")) {
                    Application.OpenURL ("https://vimeo-authy.herokuapp.com/auth/vimeo/unity");
                }
            } 
            else {
                if (GUILayout.Button("Switch accounts")) {
                    if (target.GetType().ToString() == "Vimeo.VimeoPublisher") {
                        (target as VimeoPublisher).SetVimeoToken (null);
                    } else {
                        (target as VimeoPlayer).SetVimeoToken (null);
                    }
                }
            }
        }

        public void DrawSlackAuth(string _token)
        {
            var so = serializedObject;
            if (!Authenticated(_token)) {
				EditorGUILayout.PropertyField(so.FindProperty ("slackToken"));
                if (GUILayout.Button ("Sign into Slack")) {
					Application.OpenURL ("https://vimeo-authy.herokuapp.com/auth/slack");
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
                    EditorGUILayout.PropertyField(so.FindProperty ("slackChannel"));
					EditorGUILayout.PropertyField(so.FindProperty("defaultShareLink"));
					EditorGUILayout.PropertyField(so.FindProperty ("autoPostToChannel"));
                } 

                DrawSlackAuth (publisher.GetSlackToken());
				EditorGUI.indentLevel--;
			}
        }
    }
}