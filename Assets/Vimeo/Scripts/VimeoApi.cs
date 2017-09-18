
using System.Xml;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using SimpleJSON;


// Dev token 93f6a1e01b096a554624c5378f34d59a
namespace Vimeo
{
    public class VimeoApi : MonoBehaviour
    {
        public delegate void RequestAction(string response);
        public event RequestAction OnRequestComplete;
        public event RequestAction OnUploadComplete;

        private string video_file_path;
        public string token;
        private static string API_URL = "https://api.vimeo.com";

        public void GetVideoFileUrlByVimeoId(int vimeo_id)
        {
            StartCoroutine("Request", "/videos/" + vimeo_id);
        }

        public void UploadVideoFile(string file_path, string vimeo_token)
        {
            video_file_path = file_path;
            token = vimeo_token;

            // Start Upload Process
            StartCoroutine(GetTicket());
        }

        public void SetVideoViewPrivacy(int vimeo_id, string type) {
            WWWForm form = new WWWForm ();
            form.AddField ("privacy.view", type);

            StartCoroutine(Patch(API_URL + "/videos/" + vimeo_id, form));
        }

        IEnumerator Patch(string url, WWWForm form)
        {
            using (UnityWebRequest request = UnityWebRequest.Post (url, form)) {
                request.SetRequestHeader ("Authorization", "Bearer " + token);
                yield return request.Send ();

                if (request.isNetworkError) {
                    Debug.Log (request.error);
                } else {
                    Debug.Log (request.downloadHandler.text);
                }
            }
        }

        IEnumerator GetTicket()
        {
            WWWForm form = new WWWForm ();
            form.AddField ("type", "streaming");

            using (UnityWebRequest request = UnityWebRequest.Post (API_URL + "/me/videos", form)) {
                request.SetRequestHeader ("Authorization", "Bearer " + token);

                yield return request.Send ();

                if (request.isNetworkError) {
                    Debug.Log (request.error);
                } else {
                    Debug.Log (request.downloadHandler.text);
                    VimeoTicket ticket = VimeoTicket.CreateFromJSON (request.downloadHandler.text);
                    //Debug.Log (ticket.ticket_id);
                    StartCoroutine(UploadVideo(ticket));
                }
            }
        }

        IEnumerator UploadVideo(VimeoTicket ticket)
        {
            Debug.Log ("-----------------------UploadVideo-------------------------");
            // Get local video file and store it in a byte array for uploading

            FileInfo video_file = new FileInfo(video_file_path);
            byte[] data = File.ReadAllBytes(video_file_path);

            Debug.Log (data.Length);
            Debug.Log (video_file.Name);

            // Upload to the Vimeo server
            Debug.Log ("Uploading to " + ticket.upload_link_secure);

            using (UnityWebRequest request = UnityWebRequest.Put(ticket.upload_link_secure, data)) {
                request.SetRequestHeader("Content-Type", "video/" + video_file.Extension);

                yield return request.Send ();

                if (request.isNetworkError) {
                    Debug.Log (request.error);
                    Debug.Log (request.responseCode);
                } else {
                    Debug.Log (request.downloadHandler.text);
                    Debug.Log (request.responseCode);

                    StartCoroutine (VerifyUpload (ticket));
                }
            }
        }

        IEnumerator VerifyUpload(VimeoTicket ticket)
        {
            Debug.Log ("-----------------------VerifyUpload-------------------------");
            byte[] data = new byte[] { 0x00 };

            using (UnityWebRequest request = UnityWebRequest.Put(ticket.upload_link_secure, data)) {
                request.SetRequestHeader("Content-Range", "bytes */*");

                yield return request.Send ();

                if (request.responseCode == 308) {
                    Debug.Log ("Verified!!");
                    Debug.Log (request.GetResponseHeader ("Range"));

                    StartCoroutine (CompleteUpload (ticket));
                } else {
                    //              Debug.Log (request.downloadHandler.text);
                    Debug.Log (request.responseCode);
                }
            }
        }

        IEnumerator CompleteUpload(VimeoTicket ticket) 
        {
            Debug.Log ("-----------------------CompleteUpload-------------------------");
            Debug.Log ("https://api.vimeo.com" + ticket.complete_uri);
            using (UnityWebRequest request = UnityWebRequest.Delete(API_URL + ticket.complete_uri)) {
                request.SetRequestHeader ("Authorization", "Bearer " + token);

                yield return request.Send ();
                Debug.Log (request.responseCode);

                if (OnUploadComplete != null) {
                    OnUploadComplete (request.downloadHandler.text);
                }
            }
        }


        IEnumerator Request(string api_path)
        {
            if (token != null)
            {
                UnityWebRequest request = UnityWebRequest.Get(API_URL + api_path);
                request.SetRequestHeader("Authorization", "Bearer " + token);
                yield return request.Send();

                if (OnRequestComplete != null) {
                    OnRequestComplete(request.downloadHandler.text);
                }
            }
        }
    }
}
