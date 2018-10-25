#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using Vimeo.Player;
using Vimeo.Recorder;
using Vimeo.SimpleJSON;
using System.Linq;

namespace Vimeo
{  
    public class BaseEditor : Editor 
    {
        VimeoApi api;

        private void InitAPI()
        {
            var settings = target as VimeoSettings;
            if (api == null) {
                if (settings.gameObject.GetComponent<VimeoApi>()) {
                    api = settings.gameObject.GetComponent<VimeoApi>();
                }
                else {
                    api = settings.gameObject.AddComponent<VimeoApi>();
                }
            }

            api.token = settings.GetVimeoToken();
        }

        protected void GetRecentVideos()
        {
            var settings = target as VimeoSettings;
            if (!settings.Authenticated()) return;
            InitAPI();

            settings.vimeoVideos.Clear();
            settings.vimeoVideos.Add(
                new VimeoVideo("Loading...", null)
            );

            api.OnRequestComplete += GetVideosComplete;
            api.OnError += OnRequestError;

            api.GetRecentUserVideos();
        }

        protected void GetVideosInFolder(VimeoFolder folder)
        {
            var settings = target as VimeoSettings;
            if (!settings.Authenticated()) return;
            InitAPI();

            settings.vimeoVideos.Clear();
            settings.vimeoVideos.Add(
                new VimeoVideo("Loading...", null)
            );
            
            api.OnRequestComplete += GetVideosComplete;
            api.OnError += OnRequestError;

            api.GetVideosInFolder(folder);
        }

        private void GetVideosComplete(string response)
        {
            var settings = target as VimeoPlayer;
            settings.vimeoVideos.Clear();

            api.OnRequestComplete -= GetVideosComplete;
            api.OnError -= OnRequestError;
            
            if (!EditorApplication.isPlaying) {
                DestroyImmediate(settings.gameObject.GetComponent<VimeoApi>());
            }

            var json = JSON.Parse(response);
            JSONNode videoData = json["data"];

            if (videoData.Count == 0) {
                settings.vimeoVideos.Add(new VimeoVideo("(No videos found)"));
            }
            else {
                settings.vimeoVideos.Add(new VimeoVideo("---- Select a video ----", null));

                for (int i = 0; i < videoData.Count; i++) {
                    settings.vimeoVideos.Add(
                        new VimeoVideo(videoData[i])
                    );
                }
            }
        }

        protected void FetchFolders()
        {
            var settings = target as VimeoSettings;
            if (!settings.Authenticated()) return;

            InitAPI();
            settings.vimeoFolders.Clear();
            settings.vimeoFolders.Add(
                new VimeoFolder("Loading...", null)
            );

            api.OnRequestComplete += GetFoldersComplete;
            api.OnError += OnRequestError;
            api.GetUserFolders();
        }

        private void GetFoldersComplete(string response)
        {
            var settings = target as VimeoSettings;
            settings.vimeoFolders.Clear();

            api.OnRequestComplete -= GetFoldersComplete;

            if (!EditorApplication.isPlaying) {
                DestroyImmediate(settings.gameObject.GetComponent<VimeoApi>());
            }

            var json = JSON.Parse(response);
            var folderData = json["data"];

            string folder_prefix = "";

            if (settings is VimeoPlayer) {
                var player = target as VimeoPlayer;
                player.vimeoFolders.Add(new VimeoFolder("---- Find a video ----", null));
                player.vimeoFolders.Add(new VimeoFolder("Get video by ID or URL", "custom"));
                player.vimeoFolders.Add(new VimeoFolder("Most recent videos", "recent"));

                if (player.currentFolder == null || !player.currentFolder.IsValid()) {
                    if (player.vimeoVideoId != null && player.vimeoVideoId != "") {
                        player.currentFolder = player.vimeoFolders[1];    
                    }
                    else {
                        player.currentFolder = player.vimeoFolders[0];
                    }
                }
                folder_prefix = "Projects / ";
            }
            else {
                settings.vimeoFolders.Add(new VimeoFolder("No project", null));
            }

            for (int i = 0; i < folderData.Count; i++) {
                VimeoFolder folder = new VimeoFolder(folder_prefix + folderData[i]["name"], folderData[i]["uri"]);
                settings.vimeoFolders.Add(folder);
            }
        }

        private void OnRequestError(string error)
        {
            var settings = target as VimeoSettings;
            if (!EditorApplication.isPlaying) {
                DestroyImmediate(settings.gameObject.GetComponent<VimeoApi>());
            }
            settings.signInError = true;
        }

        protected void GUIManageVideosButton() 
        {
            var settings = target as VimeoSettings;
            if (settings.Authenticated() && settings.vimeoSignIn && GUILayout.Button("Manage videos", GUILayout.Width(100))) {
                Application.OpenURL("https://vimeo.com/manage/videos");
            }
        }

        protected bool GUISelectFolder()
        {   
            var so = serializedObject;
            var settings = target as VimeoSettings;
            
            // Folder selection
            GUILayout.BeginHorizontal();
            bool folderChanged = false;

            int cur_index = settings.GetCurrentFolderIndex();
            int new_index = EditorGUILayout.Popup(settings is VimeoPlayer ? "Vimeo Video" : "Add to Project", cur_index, settings.vimeoFolders.Select(folder => folder.name).ToArray()); 

            if (new_index != cur_index) {
                folderChanged = true;
                settings.currentFolder = settings.vimeoFolders[new_index];
            }

            if (settings is RecorderSettings && GUILayout.Button("+", GUILayout.Width(25))) {
                Application.OpenURL("https://vimeo.com/manage/folders");
            }

            if (GUILayout.Button("↺", GUILayout.Width(25)) || (settings.vimeoFolders.Count == 0 && settings.GetComponent<VimeoApi>() == null)) { // Refresh folders
                FetchFolders();
            }

            GUILayout.EndHorizontal();

            return folderChanged;
        }

        protected void GUISelectVideo(bool refreshVideos = false)
        {
            var so = serializedObject;
            var player = target as VimeoPlayer;

            if (player.currentFolder.uri == "custom") {
                EditorGUILayout.PropertyField(so.FindProperty("vimeoVideoId"), new GUIContent("Vimeo Video URL"));
            }
            else if (player.currentFolder.uri != null && player.currentFolder.uri != "") {
                GUILayout.BeginHorizontal();
                int cur_video_index = player.GetCurrentVideoIndex();
                int new_video_index = EditorGUILayout.Popup(" ", cur_video_index, player.vimeoVideos.Select(v => v.name).ToArray()); 

                if (new_video_index != cur_video_index) {
                    player.currentVideo = player.vimeoVideos[new_video_index];
                    player.vimeoVideoId = player.currentVideo.id.ToString();
                }

                if (GUILayout.Button("↺", GUILayout.Width(25)) || 
                    refreshVideos || 
                    (player.vimeoVideos.Count == 0 && player.GetComponent<VimeoApi>() == null)) {
                        
                    if (player.currentFolder.uri == "recent") {
                        GetRecentVideos();
                    }
                    else if (player.currentFolder.id > 0) {
                        GetVideosInFolder(player.currentFolder);
                    }
                }

                GUILayout.EndHorizontal();
            }
        }
        
        protected void GUISignOutButton()
        {
            var settings = target as VimeoSettings;
            if (settings.Authenticated() && settings.vimeoSignIn && GUILayout.Button("Sign out", GUILayout.Width(60))) {
                settings.SignOut();
            }
        }

        protected void GUIHelpButton()
        {
            if (GUILayout.Button("Help", GUILayout.Width(50))) {
                Application.OpenURL("https://github.com/vimeo/vimeo-unity-sdk");
            } 
        }

        public void DrawVimeoAuth(VimeoSettings auth)
        {
            var so = serializedObject;

            if (!auth.Authenticated() || !auth.vimeoSignIn) {
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

                if (auth.vimeoToken != null && auth.vimeoToken != "") {
                    if (auth is VimeoPlayer) {
                        EditorGUILayout.HelpBox("Reminder: Streaming videos is limited to Vimeo Pro and Business customers.", MessageType.Warning);
                    }

                    GUI.backgroundColor = Color.green;
                    if (GUILayout.Button("Sign into Vimeo", GUILayout.Height(30))) {
                        auth.SignIn(auth.vimeoToken);
                        GUI.FocusControl(null);
                    }
                }
                GUI.backgroundColor = Color.white;
                
            } 
        }


    }
}

#endif