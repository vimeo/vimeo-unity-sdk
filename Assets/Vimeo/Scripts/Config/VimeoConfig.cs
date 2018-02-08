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
                    Application.OpenURL("https://vimeo-authy.herokuapp.com/auth/vimeo/unity?scope=public%20private%20video_files");
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
    }
}

#endif