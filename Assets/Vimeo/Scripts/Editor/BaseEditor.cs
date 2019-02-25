#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Linq;
using Vimeo.Recorder;
using Vimeo.Player;
using System;

namespace Vimeo
{
    public class BaseEditor : Editor
    {
        VimeoFetcher fetcher;

        private void InitFetcher()
        {
            var settings = target as VimeoSettings;
            if (fetcher == null) {
                if (settings.gameObject.GetComponent<VimeoFetcher>()) {
                    fetcher = settings.gameObject.GetComponent<VimeoFetcher>();
                }
                else {
                    fetcher = settings.gameObject.AddComponent<VimeoFetcher>();
                }
            }

            fetcher.OnFetchComplete += DestroyFetcher;
            fetcher.OnFetchError += DestroyFetcher;
            fetcher.Init(settings);
        }

        private void DestroyFetcher(string response)
        {
            fetcher.OnFetchComplete -= DestroyFetcher;
            fetcher.OnFetchError -= DestroyFetcher;

            var settings = target as VimeoSettings;
#if UNITY_EDITOR
            if (!EditorApplication.isPlaying)
#endif
            {
                DestroyImmediate(settings.gameObject.GetComponent<VimeoFetcher>());
            }
        }

        private void FetchFolders()
        {
            InitFetcher();
            fetcher.FetchFolders();
        }

        private void GetVideosInFolder(VimeoFolder currentFolder)
        {
            InitFetcher();
            fetcher.GetVideosInFolder(currentFolder);
        }

        private void GetRecentVideos()
        {
            InitFetcher();
            fetcher.GetRecentVideos();
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
            int new_index = EditorGUILayout.Popup("Project", cur_index, settings.vimeoFolders.Select(folder => folder.name).ToArray()); 

            if (new_index != cur_index) {
                folderChanged = true;
                settings.currentFolder = settings.vimeoFolders[new_index];
            }

            if (settings is RecorderSettings && GUILayout.Button("+", GUILayout.Width(25))) {
                Application.OpenURL("https://vimeo.com/manage/folders");
            }

            if (GUILayout.Button("↺", GUILayout.Width(25)) || (settings.vimeoFolders.Count == 0 && settings.GetComponent<VimeoFetcher>() == null)) { // Refresh folders
                FetchFolders();
            }

            GUILayout.EndHorizontal();

            return folderChanged;
        }

        protected void GUISelectVideo(bool refreshVideos = false)
        {
            var so = serializedObject;
            var settings = target as VimeoSettings;

            if (settings.currentFolder.uri == "custom") {
                EditorGUILayout.PropertyField(so.FindProperty("vimeoVideoId"), new GUIContent("Vimeo Video URL"));
            } else if (settings.currentFolder.uri != null && settings.currentFolder.uri != "") {
                GUILayout.BeginHorizontal();
                int cur_video_index = settings.GetCurrentVideoIndex();
                int new_video_index = EditorGUILayout.Popup("Selected Video", cur_video_index, settings.vimeoVideos.Select(v => v.name).ToArray()); 

                if (new_video_index != cur_video_index) {
                    settings.currentVideo = settings.vimeoVideos[new_video_index];
                    settings.vimeoVideoId = new_video_index > 0 ? settings.currentVideo.id.ToString() : null;
                    if (settings is RecorderSettings)
                    {
                        var recorder = settings as RecorderSettings;
                        recorder.videoName = new_video_index > 0 ? settings.currentVideo.GetVideoName() : "";
                        recorder.description = new_video_index > 0 ? settings.currentVideo.description : "";
                    }
                }

                if (GUILayout.Button("↺", GUILayout.Width(25)) ||
                    refreshVideos ||
                    (settings.vimeoVideos.Count == 0 && settings.GetComponent<VimeoFetcher>() == null)) {
                    UpdateVideosList();
                }

                GUILayout.EndHorizontal();
            }
        }

        protected void UpdateVideosList()
        {
            var settings = target as VimeoSettings;

            if (settings.currentFolder.uri == "recent")
            {
                GetRecentVideos();
            }
            else if (settings.currentFolder.id > 0)
            {
                GetVideosInFolder(settings.currentFolder);
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
                    } else {
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