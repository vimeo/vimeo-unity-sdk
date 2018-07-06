using System.Xml;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using SimpleJSON;

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
        public event RequestAction OnUploadComplete;
        public event RequestAction OnPatchComplete;
        public event RequestAction OnError;

        public delegate void UploadAction(string status, float progress);
        public event UploadAction OnUploadProgress;

        private string video_file_path;

        [HideInInspector]
        public string token;
        public static string API_URL = "https://api.vimeo.com";
        private WWWForm form;

        private UnityWebRequest uploader;

        void Start()
        {
            form = new WWWForm();
        }

        public void GetVideoFileUrlByVimeoId(int video_id)
        {
            StartCoroutine("Request", "/videos/" + video_id);
        }

        public void GetUserFolders()
        {
            StartCoroutine("Request", "/me/folders"); 
        }

        public void AddVideoToFolder(VimeoVideo video, VimeoFolder folder)
        {
            if (folder.id > 0 && video.uri != null) {
                IEnumerator coroutine = Put("/me/folders/" + folder.id + "/videos?uris=" + video.uri);
                StartCoroutine(coroutine);            
            }
        }

        public void GetRecentUserVideos()
        {
            StartCoroutine("Request", "/me/videos?per_page=100"); 
        }

        public void GetVideosInFolder(VimeoFolder folder)
        {
            StartCoroutine("Request", "/me/folders/" + folder.id + "/videos?per_page=100"); 
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

        public void UploadVideoFile(string file_path)
        {
            video_file_path = file_path;
            StartCoroutine(GetTicket()); 
        } 

        IEnumerator GetTicket()
        {
            if (OnUploadProgress != null) {
                OnUploadProgress("Authorizing", 0);
            }

            WWWForm form = new WWWForm();
            form.AddField("type", "streaming");

            using (UnityWebRequest request = UnityWebRequest.Post(API_URL + "/me/videos", form)) {
                PrepareHeaders(request , "3.2");
                yield return VimeoApi.SendRequest(request);

                if (IsNetworkError(request)) {
                    Debug.LogError(request.error);
                } 
                else {
                    VimeoTicket ticket = VimeoTicket.CreateFromJSON(request.downloadHandler.text);

                    if (ticket.error == null) {
                        StartCoroutine(UploadVideo(ticket));
                    } 
                    else {
                        Debug.LogError(ticket.error + " " + ticket.developer_message);
                    }
                }
            }
        }

        IEnumerator UploadVideo(VimeoTicket ticket)
        {
            if (OnUploadProgress != null) {
                OnUploadProgress("Uploading", 0);
            }

            byte[] data = new byte[0];
            bool success = false;


            // Using try/catch to wait for video to finish being
            while (success == false) {
                try {
                    // Get local video file and store it in a byte array for uploading
                    data = File.ReadAllBytes(video_file_path);
                    success = true;
                } 
                catch (IOException e) { 
                    // TODO: fix this ugly code!
                    Debug.Log("File is being accessed by another process. " + e.Message);
                }
            }

            FileInfo video_file = new FileInfo(video_file_path);

            // Upload to the Vimeo server
            using (UnityWebRequest request = UnityWebRequest.Put(ticket.upload_link_secure, data)) {
                uploader = request;
				request.chunkedTransfer = false;
                request.SetRequestHeader("Content-Type", "video/" + video_file.Extension);
                yield return VimeoApi.SendRequest(request);

                uploader = null;

                if (IsNetworkError(request)) {
                    Debug.Log(request.error);
                    Debug.Log(request.responseCode);
                } 
                else {
                    StartCoroutine(VerifyUpload(ticket));
                }
            }
        }

        IEnumerator VerifyUpload(VimeoTicket ticket)
        {
            if (OnUploadProgress != null) {
                OnUploadProgress("Verifying", 0.9999999f);
            }

            byte[] data = new byte[] { 0x00 };

            using (UnityWebRequest request = UnityWebRequest.Put(ticket.upload_link_secure, data)) {
                request.chunkedTransfer = false;
                request.SetRequestHeader("Content-Range", "bytes */*");
                yield return VimeoApi.SendRequest(request);

                if (request.responseCode == 308) {
                    StartCoroutine(CompleteUpload(ticket));
                } 
                else {
                    Debug.Log(request.responseCode);
                }
            }
        }

        IEnumerator CompleteUpload(VimeoTicket ticket) 
        {
            if (OnUploadProgress != null) {
                OnUploadProgress("Complete", 1f);
            }

            using (UnityWebRequest request = UnityWebRequest.Delete(API_URL + ticket.complete_uri)) {
                PrepareHeaders(request);
                yield return VimeoApi.SendRequest(request);

                if (OnUploadComplete != null) {
                    OnUploadComplete(request.GetResponseHeader("Location"));
                }
            }
        }

        IEnumerator Patch(string url)
        {
            using (UnityWebRequest request = UnityWebRequest.Post(url, form)) {
                PrepareHeaders(request);
                request.SetRequestHeader("X-HTTP-Method-Override", "PATCH");
                yield return VimeoApi.SendRequest(request);

                // Reset the form
                form = new WWWForm();

                if (request.responseCode != 200) {
                    Debug.LogError(request.downloadHandler.text);
                    if (OnError != null) OnError(request.downloadHandler.text);
                }
                else if (OnPatchComplete != null) {
                    OnPatchComplete(request.downloadHandler.text);
                }
            }
        }

        IEnumerator Put(string api_path)
        {
            if (token != null) {
                byte[] data = new byte[] { 0x00 };
                using(UnityWebRequest request = UnityWebRequest.Put(API_URL + api_path, data)) {
                    PrepareHeaders(request);
                    yield return VimeoApi.SendRequest(request);

                    if (request.error != null) {
                        Debug.LogError(request.downloadHandler.text);
                        if (OnError != null) OnError(request.downloadHandler.text);
                    } 
                    else {
                        // TODO create event hook
                    }
                }
            }
        }

        IEnumerator Request(string api_path)
        {
            if (token != null)
            {
                UnityWebRequest request = UnityWebRequest.Get(API_URL + api_path);
                PrepareHeaders(request);
                yield return VimeoApi.SendRequest(request);

                if (request.responseCode != 200) {
                    if (request.responseCode == 401) {
                        Debug.LogError("[VimeoApi] Unauthorized request.");
                    }
                    if (OnError != null) OnError(request.downloadHandler.text);
                }
                else if (OnRequestComplete != null) {
                    OnRequestComplete(request.downloadHandler.text);
                }
            }
        }

        private void PrepareHeaders(UnityWebRequest r, string apiVersion = "3.4")
        {
            r.chunkedTransfer = false;
            r.SetRequestHeader("Authorization", "Bearer " + token);
            r.SetRequestHeader("Accept", "application/vnd.vimeo.*+json;version=" + apiVersion);
        }

        public static bool IsNetworkError(UnityWebRequest req) {
#if UNITY_2017_1_OR_NEWER
            return req.isNetworkError;
#else
            return req.isError;
#endif
        }

        public static AsyncOperation SendRequest(UnityWebRequest req) {
#if UNITY_2017_3_OR_NEWER
            return req.SendWebRequest();
#else
            return req.Send();
#endif
        }

        void FixedUpdate()
        {
            if (OnUploadProgress != null && uploader != null && uploader.uploadProgress != 1) {
                OnUploadProgress("Uploading", uploader.uploadProgress);
            }
        }
    }
}
