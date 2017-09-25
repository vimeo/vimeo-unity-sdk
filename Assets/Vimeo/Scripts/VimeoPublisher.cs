using System.Collections;
using System.Collections.Generic;
using UTJ.FrameCapturer;
using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using SimpleJSON;
using Vimeo;
using Vimeo.Auth;

namespace Vimeo {

    [CustomEditor (typeof(VimeoPublisher))]
    public class VimeoPublisherInspector : VimeoLogin
    {
        void OnEnable()
        {
            var player = target as VimeoPublisher;
            token = player.accessToken;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var player = target as VimeoPublisher;

            DrawVimeoAuth (player);
            EditorUtility.SetDirty(target);
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