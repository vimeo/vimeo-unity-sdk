#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using Vimeo.Config;
using Vimeo.Player;

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
                
                GUILayout.BeginHorizontal();

                EditorGUILayout.PropertyField(so.FindProperty("vimeoToken"));
                
                if (GUILayout.Button("Get token", GUILayout.Width(80))) {
                    if (auth is VimeoPlayer) {
                        Application.OpenURL("https://authy.vimeo.com/auth/vimeo/unity?scope=public%20private%20video_files");
                    }
                    else {
                        Application.OpenURL("https://authy.vimeo.com/auth/vimeo/unity");
                    }
                }
                GUILayout.EndHorizontal();

                EditorGUILayout.PropertyField(so.FindProperty("saveVimeoToken"), new GUIContent("Save token with object"));

                GUI.backgroundColor = Color.green;
                if (Authenticated(auth.vimeoToken)) {
                    if (GUILayout.Button("Sign into Vimeo", GUILayout.Height(30))) {
                        auth.SetVimeoToken(auth.vimeoToken);
                        auth.vimeoSignIn = true;
                        GUI.FocusControl(null);
                    }
                }
                GUI.backgroundColor = Color.white;
                
            } 
        }


    }
}

#endif