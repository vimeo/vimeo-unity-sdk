using System.Collections;
using System.Collections.Generic;
using UTJ.FrameCapturer;
using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using SimpleJSON;
using Vimeo;

namespace Vimeo {

    [CustomEditor (typeof(VimeoPublisher))]
    public class VimeoPublisherInspector : Editor
    {
        private string token;

        void OnEnable()
        {
            var player = target as VimeoPublisher;
            token = player.accessToken;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var player = target as VimeoPublisher;

            if (GUI.changed) {
                // Token field touched
                if (token != player.accessToken) {
                    player.validAccessTokenCheck = false;
                }
            }

            if (player.accessToken == "" || player.accessToken == null) {
                 GUILayout.Box ("To generate a Vimeo access token, create or edit a developer app, visit the Authentication tab, and generate a new access token with Upload & Edit scopes.");
                if (GUILayout.Button ("Generate Access Token")) {
                    Application.OpenURL ("https://developer.vimeo.com/apps");
                }
            } 
            else {
                if (player.validAccessTokenCheck != true) {
                    ValidateToken(player.accessToken);
                    player.validAccessTokenCheck = true; // to prevent checking too often
                }

                if (player.validAccessToken == false) {
                    GUILayout.Box ("Invalid token!");
                }

                if (GUILayout.Button("Generate New Access Token")) {
                    player.validAccessToken = true;
                    player.validAccessTokenCheck = false;
                    Application.OpenURL("https://developer.vimeo.com/apps");
                }
            }

            EditorUtility.SetDirty(target);
        }

        private void ValidateToken(string token) 
        {
            var player = target as VimeoPublisher;
            using(UnityWebRequest www = UnityWebRequest.Get(VimeoApi.API_URL + "/?access_token=" + token)) {
                www.Send();

                // Wait until request is finished
                while (www.responseCode <=  0) { }
                if(www.responseCode != 200) {
                    player.validAccessToken = false;
                }
                else {
                    Debug.Log ("Valid Vimeo access token!");
                    player.validAccessToken = true;
                    //Debug.Log(www.downloadHandler.text);
                }
            }
        }
    }

    public class VimeoPublisher : MonoBehaviour {

        private MovieRecorder recorder;
        private VimeoApi api;

    	public Camera camera;
        public enum PrivacyMode
        {
            anybody,
            contacts,
            disable,
            nobody,
            //password,
            unlisted,
            //users,
        }

        public string videoName;
        [SerializeField] protected PrivacyMode m_privacyMode = PrivacyMode.anybody;
        public string accessToken;
        [HideInInspector] public bool validAccessToken;
        [HideInInspector] public bool validAccessTokenCheck;
        public bool openInBrowser;

    	void Start () {
    		recorder = camera.GetComponent<MovieRecorder> ();
            api = gameObject.AddComponent<VimeoApi> ();
            api.token = accessToken;
    	}

    	public void StartRecording()
    	{
            Debug.Log ("Recording...");
    		recorder.BeginRecording();
    	}

    	public void EndRecording()
    	{
            recorder.EndRecording();
            
            PublishVideo ();
    	}

    	private void PublishVideo()
    	{
            Debug.Log ("Uploading to Vimeo: " + recorder.outputPath);
            api.OnUploadComplete += UploadComplete;
            api.UploadVideoFile(recorder.outputPath + ".mp4");
    	}

        private void UploadComplete(string video_uri)
        {
            string[] uri_pieces = video_uri.Split ("/"[0]);
            string video_id = uri_pieces[2];

            if (videoName != null && videoName != "") {
                api.SetVideoName(videoName);
            }

            api.SetVideoViewPrivacy(PrivacyMode.unlisted.ToString());

            api.SaveVideo(video_id);

            if (openInBrowser == true) {
                Application.OpenURL("https://vimeo.com/" + video_id);
            }
        }

    }
}