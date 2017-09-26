using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Vimeo;

namespace Vimeo.Auth 
{
    public class VimeoLogin : Editor 
    {
        public void DrawVimeoAuth(VimeoPlayer player)
        {
            DrawVimeoAuth (player.accessToken);
        }

        public void DrawVimeoAuth(VimeoPublisher publisher)
        {
            DrawVimeoAuth (publisher.accessToken);
        }

        public void DrawVimeoAuth(string _token)
        {
            if (_token == "" || _token == null) {
                GUILayout.Box("To generate a Vimeo access token, create or edit a developer app, visit the Authentication tab, and generate a new access token with Upload & Edit scopes.");
                if (GUILayout.Button ("Generate Access Token")) {
                    Application.OpenURL ("https://developer.vimeo.com/apps");
                }
            } 
            else {
                if (GUILayout.Button("Generate New Access Token")) {
                    Application.OpenURL("https://developer.vimeo.com/apps");
                }
            }

        }
    }
}