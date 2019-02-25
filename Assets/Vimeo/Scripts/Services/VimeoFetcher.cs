#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Vimeo.Player;
using Vimeo.Recorder;
using Vimeo.SimpleJSON;

namespace Vimeo
{
    public class VimeoFetcher : MonoBehaviour
    {
        VimeoSettings target;
        VimeoApi api;

        public delegate void FetchAction(string response);
        public event FetchAction OnFetchComplete;
        public event FetchAction OnFetchError;

        private void Start()
        {
            this.hideFlags = HideFlags.HideInInspector;
        }

        public void Init(VimeoSettings _settings)
        {
            target = _settings;
        }

        public void FetchFolders()
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

        public void GetVideosInFolder(VimeoFolder folder)
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

            api.GetVideosInFolder(folder, "name,uri,description"); // conserve description
        }

        public void GetRecentVideos()
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

        private void InitAPI()
        {
            var settings = target as VimeoSettings;
            if (api == null)
            {
                if (settings.gameObject.GetComponent<VimeoApi>())
                {
                    api = settings.gameObject.GetComponent<VimeoApi>();
                }
                else
                {
                    api = settings.gameObject.AddComponent<VimeoApi>();
                }
            }

            api.token = settings.GetVimeoToken();
        }

        protected bool IsSelectExisting(VimeoSettings settings)
        {
            return (settings is VimeoPlayer) ||
                (settings is VimeoRecorder && (settings as VimeoRecorder).replaceExisting);
        }

        private void GetVideosComplete(string response)
        {
            var settings = target as VimeoSettings;
            settings.vimeoVideos.Clear();

            api.OnRequestComplete -= GetVideosComplete;
            api.OnError -= OnRequestError;

#if UNITY_EDITOR
            if (!EditorApplication.isPlaying)
#endif
            {
                DestroyImmediate(settings.gameObject.GetComponent<VimeoApi>());
            }

            var json = JSONNode.Parse(response);
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

            Debug.Log("[VimeoFetcher] Completed with " + (settings.vimeoVideos.Count - 1) + " existing videos");

            if (OnFetchComplete != null)
            {
                OnFetchComplete.Invoke(response);
            }
        }

        private void OnRequestError(string error)
        {
            var settings = target as VimeoSettings;
#if UNITY_EDITOR
            if (!EditorApplication.isPlaying)
#endif
            {
                DestroyImmediate(settings.gameObject.GetComponent<VimeoApi>());
            }
            settings.signInError = true;

            if (OnFetchError != null)
            {
                OnFetchError.Invoke("");
            }

            Debug.LogError("[VimeoFetcher] Error: " + error);
        }

        private void GetFoldersComplete(string response)
        {
            var settings = target as VimeoSettings;
            settings.vimeoFolders.Clear();

            api.OnRequestComplete -= GetFoldersComplete;

#if UNITY_EDITOR
            if (!EditorApplication.isPlaying)
#endif
            {
                DestroyImmediate(settings.gameObject.GetComponent<VimeoApi>());
            }

            var json = JSONNode.Parse(response);
            var folderData = json["data"];

            string folder_prefix = "";

            if (IsSelectExisting(settings))
            {
                var player = target as VimeoSettings;
                player.vimeoFolders.Add(new VimeoFolder("---- Find a video ----", null));
                player.vimeoFolders.Add(new VimeoFolder("Get video by ID or URL", "custom"));
                player.vimeoFolders.Add(new VimeoFolder("Most recent videos", "recent"));

                if (player.currentFolder == null || !player.currentFolder.IsValid())
                {
                    if (player.vimeoVideoId != null && player.vimeoVideoId != "")
                    {
                        player.currentFolder = player.vimeoFolders[1];
                    }
                    else
                    {
                        player.currentFolder = player.vimeoFolders[0];
                    }
                }
                folder_prefix = "Projects / ";
            }
            else
            {
                settings.vimeoFolders.Add(new VimeoFolder("No project", null));
            }

            for (int i = 0; i < folderData.Count; i++)
            {
                VimeoFolder folder = new VimeoFolder(folder_prefix + folderData[i]["name"], folderData[i]["uri"]);
                settings.vimeoFolders.Add(folder);
            }

            if (OnFetchComplete != null)
            {
                OnFetchComplete.Invoke("");
            }

            Debug.Log("[VimeoFetcher] Completed with " + (settings.vimeoFolders.Count - 1) + " existing folders");
        }


    }
}
