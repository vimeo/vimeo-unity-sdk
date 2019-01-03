using System.Xml;
using System.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Vimeo
{
    [ExecuteInEditMode]
    public class VimeoApi : MonoBehaviour
    {
        public enum PrivacyModeDisplay
        {
            Anyone,
            OnlyMe,
            OnlyPeopleWithAPassword,
            OnlyPeopleWithPrivateLink,
            HideThisFromVimeo
        }

        public enum PrivacyMode
        {
            anybody,
            password,
            disable,
            nobody,
            unlisted
        }

        public enum CommentMode
        {
            Anyone,
            NoOne,
            PeopleIFollow
        }

        public delegate void RequestAction(string response);
        public event RequestAction OnRequestComplete;
        public event RequestAction OnError;
        public event RequestAction OnNetworkError;

        private string video_file_path;

        [HideInInspector]
        public string token;
        public static string API_URL = "https://api.vimeo.com";
        private WWWForm form;

        public void Start()
        {
            this.hideFlags = HideFlags.HideInInspector;
            form = new WWWForm();
        }

        public void GetVideoFileUrlByVimeoId(int video_id, string fields = "name,uri,duration,width,height,spatial,play,files,description")
        {
            StartCoroutine("Request", "/videos/" + video_id + "?fields=" + fields);
        }

        public void GetUserFolders()
        {
            StartCoroutine("Request", "/me/folders?fields=name,uri");
        }

        public void AddVideoToFolder(VimeoVideo video, VimeoFolder folder)
        {
            if (folder.id > 0 && video.uri != null) {
                IEnumerator coroutine = Put("/me/folders/" + folder.id + "/videos?uris=" + video.uri);
                StartCoroutine(coroutine);
            }
        }

        public void GetRecentUserVideos(string fields = "name,uri", int per_page = 100)
        {
            StartCoroutine("Request", "/me/videos?fields=" + fields + "&per_page=" + per_page);
        }

        public void GetVideosInFolder(VimeoFolder folder, string fields = "name,uri", int per_page = 100)
        {
            StartCoroutine("Request", "/me/folders/" + folder.id + "/videos?fields=" + fields + "&per_page=" + per_page);
        }

        public void SetVideoViewPrivacy(PrivacyModeDisplay mode)
        {
            switch (mode) {

                case PrivacyModeDisplay.Anyone:
                    form.AddField("privacy.view", VimeoApi.PrivacyMode.anybody.ToString());
                    break;

                case PrivacyModeDisplay.OnlyPeopleWithPrivateLink:
                    form.AddField("privacy.view", VimeoApi.PrivacyMode.unlisted.ToString());
                    break;

                case PrivacyModeDisplay.OnlyMe:
                    form.AddField("privacy.view", VimeoApi.PrivacyMode.nobody.ToString());
                    break;

                case PrivacyModeDisplay.HideThisFromVimeo:
                    form.AddField("privacy.view", VimeoApi.PrivacyMode.disable.ToString());
                    break;

                case PrivacyModeDisplay.OnlyPeopleWithAPassword:
                    form.AddField("privacy.view", VimeoApi.PrivacyMode.password.ToString());
                    break;
            }
        }

        public void SetVideoComments(CommentMode mode)
        {
            switch (mode) {
                case CommentMode.Anyone:
                    form.AddField("privacy.comments", "anybody");
                    break;

                case CommentMode.NoOne:
                    form.AddField("privacy.comments", "nobody");
                    break;

                case CommentMode.PeopleIFollow:
                    form.AddField("privacy.comments", "contacts");
                    break;
            }
        }

        public void SetVideoDownload(bool enabled)
        {
            form.AddField("privacy.download", enabled ? "true" : "false");
        }

        public void SetVideoReviewPage(bool enabled)
        {
            form.AddField("review_page.active", enabled ? "true" : "false");
        }

        public void SetVideoPassword(string password)
        {
            form.AddField("password", password);
        }

        public void SetVideoName(string name)
        {
            form.AddField("name", name);
        }

        public void SetVideoDescription(string desc)
        {
            form.AddField("description", desc);
        }

        public void SetVideoSpatialMode(string projection, string stereo_format)
        {
            form.AddField("spatial.projection", projection);
            form.AddField("spatial.stereo_format", stereo_format);
        }

        public void SaveVideo(VimeoVideo video)
        {
            StartCoroutine(Patch(API_URL + "/videos/" + video.id));
        }

        public IEnumerator Patch(string url)
        {
            using (UnityWebRequest request = UnityWebRequest.Post(url, form)) {
                PrepareHeaders(request);
                request.SetRequestHeader("X-HTTP-Method-Override", "PATCH");
                yield return VimeoApi.SendRequest(request);

                // Reset the form
                form = new WWWForm();
                ResponseHandler(request);
            }
        }

        public IEnumerator TusUploadNew(long fileByteCount)
        {
            string tusResourceRequestBody = "{ \"upload\": { \"approach\": \"tus\", \"size\": \"" + fileByteCount.ToString() + "\" } }";

            if (token != null) {
                using (UnityWebRequest request = UnityWebRequest.Put("https://api.vimeo.com/me/videos", tusResourceRequestBody)) {
                    PrepareTusHeaders(request);
                    yield return VimeoApi.SendRequest(request);
                    ResponseHandler(request);
                }
            }
        }

        public IEnumerator TusUploadReplace(string videoId, string file_path, long fileByteCount)
        {
            string tusResourceRequestBody = "{ \"file_name\": \"" + file_path + "\", \"upload\": { \"status\": \"in_progress\", \"size\": \"" + fileByteCount.ToString() + "\", \"approach\": \"tus\" } }";

            if (token != null)
            {
                using (UnityWebRequest request = UnityWebRequest.Put("https://api.vimeo.com/videos/" + videoId + "/versions", tusResourceRequestBody))
                {
                    PrepareTusHeaders(request, true);
                    yield return VimeoApi.SendRequest(request);
                    ResponseHandler(request);
                }
            }
        }

        IEnumerator Put(string api_path)
        {
            if (token != null) {
                byte[] data = new byte[] { 0x00 };
                using (UnityWebRequest request = UnityWebRequest.Put(API_URL + api_path, data)) {
                    PrepareHeaders(request);
                    yield return VimeoApi.SendRequest(request);
                    ResponseHandler(request);
                }
            }
        }

        private void ResponseHandler(UnityWebRequest request)
        {
            if (request.error != null) {
                if (request.responseCode == 401) {
                    SendError("401 Unauthorized request. Are you using a valid token?", request.downloadHandler.text);
                }
                else if (IsNetworkError(request)) {
                    Debug.LogError("[VimeoApi] It seems like you are not connected to the internet or are having connection problems.");
                    if (OnNetworkError != null) {
                        OnNetworkError(request.error);
                    }
                }
                else {
                    SendError(request.url + " - " + request.downloadHandler.text, request.downloadHandler.text);
                }
            } else if (OnRequestComplete != null) {
                OnRequestComplete(request.downloadHandler.text);
            }
        }

        private void SendError(string msg, string error)
        {
            Debug.LogError("[VimeoApi] " + msg);

            if (OnError != null) {
                OnError(error);
            }
        }

        public IEnumerator Request(string api_path)
        {
            if (token != null) {
                UnityWebRequest request = UnityWebRequest.Get(API_URL + api_path);
                PrepareHeaders(request);
                yield return VimeoApi.SendRequest(request);
                ResponseHandler(request);
            }
        }

        private void PrepareTusHeaders(UnityWebRequest r, bool withAuthorization = true, string apiVersion = "3.4")
        {
            r.method = "POST";
            r.SetRequestHeader("Content-Type", "application/json");
            PrepareHeaders(r, withAuthorization, apiVersion);
        }
        
        private void PrepareHeaders(UnityWebRequest r, bool withAuthorization = true, string apiVersion = "3.4")
        {
            r.chunkedTransfer = false;
            if (withAuthorization)
            {
                r.SetRequestHeader("Authorization", "bearer " + token);
            }
            r.SetRequestHeader("Accept", "application/vnd.vimeo.*+json;version=" + apiVersion);
        }

        public static bool IsNetworkError(UnityWebRequest req)
        {
#if UNITY_2017_1_OR_NEWER
            return req.isNetworkError;
#else
            return req.isError;
#endif
        }

        public static AsyncOperation SendRequest(UnityWebRequest req)
        {
#if UNITY_2017_2_OR_NEWER
            return req.SendWebRequest();
#else
            return req.Send();
#endif
        }
    }
}
