#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using Vimeo.Config;

namespace Vimeo
{  
    public class BaseEditor : Editor 
    {

        public bool Authenticated(string token)
        {
            return token != "" && token != null;
        }

        public void DrawVimeoAuth(VimeoAuth auth)
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


    }
}

#endif