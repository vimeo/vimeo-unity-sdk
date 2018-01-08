using System.Xml;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using SimpleJSON;

namespace Vimeo.Services
{
    public class VimeoApi : MonoBehaviour
    {
        public enum PrivacyMode
        {
            anybody,
            disable,
            nobody,
            unlisted
        }

        public delegate void RequestAction(string response);
        public event RequestAction OnRequestComplete;
        public event RequestAction OnUploadComplete;
        public event RequestAction OnPatchComplete;

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

        public void GetVideoFileUrlByVimeoId(int vimeo_id)
        {
            StartCoroutine("Request", "/videos/" + vimeo_id);
        }

        public void SetVideoViewPrivacy(string type) 
        {
            form.AddField("privacy.view", type);
        }

        public void SetVideoName(string name) 
        {
            form.AddField("name", name);
        }

        public void SaveVideo(string vimeo_id)
        {
            StartCoroutine(Patch(API_URL + "/videos/" + vimeo_id));
        }

        public void UploadVideoFile(string file_path)
        {
            video_file_path = file_path;
            StartCoroutine(GetTicket()); 
        } 

        public static bool ValidateToken(string t)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(VimeoApi.API_URL)) {
				request.chunkedTransfer = false;
                request.SetRequestHeader("Authorization", "Bearer " + t);
                VimeoApi.SendRequest(request);

                // Wait until request is finished
                while (request.responseCode <=  0) { }

                if (request.responseCode != 200) {
                    Debug.LogError(request.responseCode + ": " + request.downloadHandler.text);
                    return false;
                }
                else {
                    return true;
                }
            }
        }

        IEnumerator Patch(string url)
        {
            using (UnityWebRequest request = UnityWebRequest.Post (url, form)) {
				request.chunkedTransfer = false;
                request.SetRequestHeader("Authorization", "Bearer " + token);
                request.SetRequestHeader("X-HTTP-Method-Override", "PATCH");
                yield return VimeoApi.SendRequest(request);

                // Reset the form
                form = new WWWForm();

                if (IsNetworkError(request)) {
                    Debug.Log(request.error);
                } 
                else {
                    if (OnPatchComplete != null) {
                        OnPatchComplete(request.downloadHandler.text);
                    }
                }
            }
        }

        IEnumerator GetTicket()
        {
        	Debug.Log("VimeoApi: GetTicket");
            if (OnUploadProgress != null) {
                OnUploadProgress ("Authorizing", 0);
            }

            WWWForm form = new WWWForm ();
            form.AddField ("type", "streaming");

            using (UnityWebRequest request = UnityWebRequest.Post(API_URL + "/me/videos", form)) {
				request.chunkedTransfer = false;
                request.SetRequestHeader("Authorization", "Bearer " + token);
                yield return VimeoApi.SendRequest(request);

                if (IsNetworkError(request)) {
                    Debug.LogError (request.error);
                } 
                else {
                    VimeoTicket ticket = VimeoTicket.CreateFromJSON (request.downloadHandler.text);

                    if (ticket.error == null) {
                        StartCoroutine(UploadVideo (ticket));
                    } 
                    else {
                        Debug.LogError(ticket.error);
                    }
                }
            }
        }

        IEnumerator UploadVideo(VimeoTicket ticket)
        {
            if (OnUploadProgress != null) {
                OnUploadProgress ("Uploading", 0);
            }

            byte[] data = new byte[0];
            bool success = false;


            // Using try/catch to wait for video to finish being
            while (success == false) {
                try {
                    // Get local video file and store it in a byte array for uploading
                    data = File.ReadAllBytes(video_file_path);
                    success = true;
                } catch (IOException e) { 
                    // TODO: fix this ugly code!
                    Debug.Log ("File is being accessed by another process. " + e.Message);
                }
            }

            FileInfo video_file = new FileInfo(video_file_path);

            // Upload to the Vimeo server
            using (UnityWebRequest request = UnityWebRequest.Put(ticket.upload_link_secure, data)) {
                uploader = request;
				request.chunkedTransfer = false;
                request.SetRequestHeader ("Content-Type", "video/" + video_file.Extension);
                yield return VimeoApi.SendRequest(request);

                uploader = null;

                if (IsNetworkError(request)) {
                    Debug.Log (request.error);
                    Debug.Log (request.responseCode);
                } else {
                    StartCoroutine(VerifyUpload(ticket));
                }
            }
        }

        IEnumerator VerifyUpload(VimeoTicket ticket)
        {
            if (OnUploadProgress != null) {
                OnUploadProgress ("Verifying", 0.9999999f);
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
                    Debug.Log (request.responseCode);
                }
            }
        }

        IEnumerator CompleteUpload(VimeoTicket ticket) 
        {
            if (OnUploadProgress != null) {
                OnUploadProgress ("Complete", 1f);
            }

            // Debug.Log (API_URL + ticket.complete_uri);
            using (UnityWebRequest request = UnityWebRequest.Delete(API_URL + ticket.complete_uri)) {
				request.chunkedTransfer = false;
                request.SetRequestHeader ("Authorization", "Bearer " + token);
                yield return VimeoApi.SendRequest(request);

                if (OnUploadComplete != null) {
                    OnUploadComplete (request.GetResponseHeader("Location"));
                }
            }
        }

        IEnumerator Request(string api_path)
        {
            if (token != null)
            {
                UnityWebRequest request = UnityWebRequest.Get(API_URL + api_path);
				request.chunkedTransfer = false;
                request.SetRequestHeader("Authorization", "Bearer " + token);
                yield return VimeoApi.SendRequest(request);

                if (OnRequestComplete != null) {
                    if (request.responseCode != 200) {
                        Debug.LogError(request.downloadHandler.text);
                    }
                    else {
                        OnRequestComplete(request.downloadHandler.text);
                    }
                }
            }
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
            if (uploader != null && uploader.uploadProgress != 1) {
                OnUploadProgress("Uploading", uploader.uploadProgress);
            }
        }
    }
}
