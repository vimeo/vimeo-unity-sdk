#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using Vimeo.Player;
using SimpleJSON;
using System.Linq;

namespace Vimeo
{  
    public class BaseEditor : Editor 
    {
        VimeoApi api;

        private void InitAPI()
        {
            var settings = target as VimeoAuth;
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
            InitAPI();
            var settings = target as VimeoAuth;

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
            InitAPI();
            var settings = target as VimeoAuth;

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
            
            if (!EditorApplication.isPlaying) {
                DestroyImmediate(settings.gameObject.GetComponent<VimeoApi>());
            }

            var json = JSON.Parse(response);
            JSONNode videoData = json["data"];

            for (int i = 0; i < videoData.Count; i++) {
                settings.vimeoVideos.Add(
                    new VimeoVideo(videoData[i])
                );
            }

            if (settings.vimeoVideos.Count > 0) {
                settings.currentVideo = settings.vimeoVideos[0];
                settings.vimeoVideoId = settings.currentVideo.id.ToString();
            }
            else {
                settings.vimeoVideos.Add(new VimeoVideo("(No videos found)"));
            }
        }

        protected void FetchFolders()
        {
            InitAPI();

            var recorder = target as VimeoAuth;

            recorder.vimeoFolders.Clear();
            recorder.vimeoFolders.Add(
                new VimeoFolder("Loading...", null)
            );

            api.OnRequestComplete += GetFoldersComplete;
            api.OnError += OnRequestError;
            api.GetUserFolders();
        }

        private void GetFoldersComplete(string response)
        {
            var settings = target as VimeoAuth;
            settings.vimeoFolders.Clear();

            api.OnRequestComplete -= GetFoldersComplete;

            if (!EditorApplication.isPlaying) {
                DestroyImmediate(settings.gameObject.GetComponent<VimeoApi>());
            }

            var json = JSON.Parse(response);
            var folderData = json["data"];

            string folder_prefix = "";

            if (settings is VimeoPlayer) {
                settings.vimeoFolders.Add(new VimeoFolder("---- Select a video ----", null));
                settings.vimeoFolders.Add(new VimeoFolder("Get video by ID or URL", "custom"));
                settings.vimeoFolders.Add(new VimeoFolder("Most recent videos", "recent"));

                if (settings.currentFolder.uri == null && settings.currentVideo.uri == null) {
                    settings.currentFolder = settings.vimeoFolders[0];
                }
                else if (settings.currentFolder.uri == null && settings.currentVideo.uri != null) {
                    settings.currentFolder = settings.vimeoFolders[1];
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
            var settings = target as VimeoAuth;
            if (!EditorApplication.isPlaying) {
                DestroyImmediate(settings.gameObject.GetComponent<VimeoApi>());
            }

        }

        protected bool GUISelectFolder()
        {   
            var so = serializedObject;
            var settings = target as VimeoAuth;
            
            // Folder selection
            GUILayout.BeginHorizontal();
            bool folderChanged = false;

            int cur_index = settings.GetCurrentFolderIndex();
            int new_index = EditorGUILayout.Popup(settings is VimeoPlayer ? "Vimeo Video" : "Add to Project", cur_index, settings.vimeoFolders.Select(folder => folder.name).ToArray()); 

            if (new_index != cur_index) {
                folderChanged = true;
                settings.currentFolder = settings.vimeoFolders[new_index];
            }

            if (GUILayout.Button("↺", GUILayout.Width(25)) || (settings.vimeoFolders.Count == 0 && settings.GetComponent<VimeoApi>() == null)) { // Refresh folders
                FetchFolders();
            }

            GUILayout.EndHorizontal();

            return folderChanged;
        }
        
        protected void GUISignOutButton()
        {
            var settings = target as VimeoAuth;
            if (Authenticated(settings.GetVimeoToken()) && settings.vimeoSignIn && GUILayout.Button("Sign out", GUILayout.Width(60))) {
                settings.vimeoVideos.Clear();
                settings.vimeoFolders.Clear();
                settings.vimeoSignIn = false;
                settings.SetVimeoToken(null);
            }
        }

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