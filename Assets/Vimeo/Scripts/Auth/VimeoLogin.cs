using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Vimeo;

namespace Vimeo.Auth 
{
    public class VimeoLogin : Editor 
    {
        public string token;
        public bool validAccessToken;
        public bool validAccessTokenCheck;

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
            if (GUI.changed) {
                // Token field touched
                if (token != _token) {
                    validAccessTokenCheck = false;
                }
            }

            if (_token == "" || _token == null) {
                GUILayout.Box("To generate a Vimeo access token, create or edit a developer app, visit the Authentication tab, and generate a new access token with Upload & Edit scopes.");
                if (GUILayout.Button ("Generate Access Token")) {
                    Application.OpenURL ("https://developer.vimeo.com/apps");
                }
            } 
            else {
                if (validAccessTokenCheck != true) {
                    validAccessToken = VimeoApi.ValidateToken(_token);
                    validAccessTokenCheck = true; // to prevent checking too often
                }

                if (validAccessToken == false) {
                    GUILayout.Box ("Invalid token!");
                }

                if (GUILayout.Button("Generate New Access Token")) {
                    validAccessToken = true;
                    validAccessTokenCheck = false;
                    Application.OpenURL("https://developer.vimeo.com/apps");
                }
            }

        }
    }
}