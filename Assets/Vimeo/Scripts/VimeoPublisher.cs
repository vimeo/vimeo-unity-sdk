using System.Collections;
using System.Collections.Generic;
using UTJ.FrameCapturer;
using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using SimpleJSON;
using Vimeo;
using Vimeo.Auth;
using Vimeo.Services;

namespace Vimeo {

    [CustomEditor (typeof(VimeoPublisher))]
    public class VimeoPublisherInspector : VimeoLogin
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var player = target as VimeoPublisher;

            DrawVimeoAuth (player);
            EditorUtility.SetDirty(target);
        }
    }

    public class VimeoPublisher : MonoBehaviour {

        public delegate void UploadAction(string status, float progress);
        public event UploadAction OnUploadProgress;

        private MovieRecorder recorder;

        [HideInInspector] 
        public VimeoApi api;
    	public Camera camera;
    	private Slack slack;

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

        public bool postToSlack;
        public string slackToken;
        public string slackChannel;
		public string slackMessage;

    	void Start () {
    		recorder = camera.GetComponent<MovieRecorder> ();
            api = gameObject.AddComponent<VimeoApi> ();
            api.token = accessToken;
    	}

    	public void StartRecording()
    	{
    		recorder.BeginRecording();
            UploadProgress ("Recording", 0);
    	}

    	public void EndRecording()
    	{
            recorder.EndRecording();
            PublishVideo();
    	}

        public void CancelRecording()
        {
            recorder.EndRecording ();
            DeleteVideoFile();

            UploadProgress ("Cancelled", 0);
        }

        public string GetVideoFilePath()
        {
            return recorder.outputPath + ".webm"; 
        }

    	private void PublishVideo()
    	{
            //Debug.Log ("Uploading to Vimeo: " + recorder.outputPath);
            api.OnUploadComplete += UploadComplete;
            api.OnUploadProgress += UploadProgress;
            api.UploadVideoFile(GetVideoFilePath());
    	}

        private void UploadProgress(string status, float progress)
        {
            if (OnUploadProgress != null) {
                OnUploadProgress (status, progress);
            }
        }

        private void UploadComplete (string video_uri)
		{
			string[] uri_pieces = video_uri.Split ("/" [0]);
			string video_id = uri_pieces [2];

			if (videoName != null && videoName != "") {
				api.SetVideoName (videoName);
			}

			api.SetVideoViewPrivacy (PrivacyMode.unlisted.ToString ());
			api.SaveVideo (video_id);

			if (openInBrowser == true) {
				Application.OpenURL ("https://vimeo.com/" + video_id);
			}

			if (postToSlack == true) {
				if (slack == null) {
					slack = gameObject.AddComponent<Slack> ();
				}
				slack.Init(slackToken, slackChannel);
				slack.PostVideoToChannel("https://vimeo.com/" + video_id);
			}

            DeleteVideoFile();
        }

        private void DeleteVideoFile()
        {
            FileUtil.DeleteFileOrDirectory(GetVideoFilePath());
        }

    }
}