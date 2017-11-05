using System.Collections;
using System.Collections.Generic;
using UTJ.FrameCapturer;
using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using Vimeo;
using Vimeo.Config;
using Vimeo.Services;
using SimpleJSON;

namespace Vimeo {

    [CustomEditor (typeof(VimeoPublisher))]
    public class VimeoPublisherInspector : VimeoConfig
    {
        public override void OnInspectorGUI()
        {
            var player = target as VimeoPublisher;
            DrawVimeoConfig (player);
            EditorUtility.SetDirty(target);
        }
    }

	[AddComponentMenu("Vimeo/Video Recorder")]
    public class VimeoPublisher : MonoBehaviour {

        public enum LinkType
        {
            VideoPage,
            ReviewPage
        }

        public delegate void UploadAction(string status, float progress);
        public event UploadAction OnUploadProgress;

        public VimeoRecorder recorder;
        public VimeoApi api;
    	public Camera _camera;
    	private Slack slack;

        public VimeoApi.PrivacyMode m_privacyMode = VimeoApi.PrivacyMode.anybody;
        public LinkType defaultShareLink = LinkType.VideoPage;
        public string vimeoToken;

        public bool recordOnStart = false;
        public bool openInBrowser = false;
        public bool postToSlack = false;

        public string slackToken;
        public string slackChannel;

        public string videoName;
        public string videoId;
        public string videoPermalink;
        public string videoReviewPermalink;

        public UTJ.FrameCapturer.RecorderBase.CaptureControl captureControl;

        private bool isRecording = false;

        private Coroutine saveCoroutine;

    	void Start () {
            if (_camera == null) {
                Debug.LogWarning ("VimeoPublisher: No camera was specified.");
                return;
            }

            recorder = _camera.GetComponent<VimeoRecorder>();

            if (recorder == null) {
                recorder = _camera.gameObject.AddComponent<VimeoRecorder>();
                Debug.Log("VimeoRecorder automatically added to " + _camera.name + ": " + recorder);
            }

            api = gameObject.AddComponent<VimeoApi> ();
            api.token = GetVimeoToken();

            api.OnPatchComplete  += VideoUpdated;
            api.OnUploadComplete += UploadComplete;
            api.OnUploadProgress += UploadProgress;

            if (recordOnStart) {
				BeginRecording();
            }
    	}

        public string GetVimeoToken()
        {
            var token = PlayerPrefs.GetString("vimeo-token");
            if (token == null || token == "") {
                if (vimeoToken != null && vimeoToken != "") {
                    SetVimeoToken (vimeoToken);
                }
                token = vimeoToken;
            }

            vimeoToken = null;
            return token;
        }

        public void SetVimeoToken(string token)
        {
            SetKey("vimeo-token", token);
        }

        public string GetSlackToken()
        {
            var token = PlayerPrefs.GetString("slack-token");
            if (token == null || token == "") {
                if (slackToken != null && slackToken != "") {
                    SetSlackToken(slackToken);
                }
                token = slackToken;
            }

            slackToken = null;
            return token;
        }

        public void SetSlackToken(string token)
        {
            SetKey("slack-token", token);
        }

        private void SetKey(string key, string val)
        {
            if (val == null || val == "") {
                PlayerPrefs.DeleteKey(key);
            } else {
                PlayerPrefs.SetString(key, val);
            }
            PlayerPrefs.Save(); 
        }

    	public void BeginRecording()
        {
            videoId = null;
            videoPermalink = null;
            videoReviewPermalink = null;

			_camera.GetComponent<VimeoRecorder>().BeginRecording();
            UploadProgress ("Recording", 0);
    	}

    	public void EndRecording()
    	{
            isRecording = false;
            recorder.EndRecording();

            PublishVideo();
    	}
            
        public void CancelRecording()
        {
            isRecording = false;
            recorder.EndRecording();
            DeleteVideoFile();

            UploadProgress ("Cancelled", 0);
        }

        public string GetVideoFilePath()
        {
        	return recorder.encodedFilePath;
//            if (fcAPI.fcMP4OSIsSupported ()) {
//                return recorder.outputPath + ".mp4"; 
//            } else {
//                return recorder.outputPath + ".webm"; 
//            }
        }

        public string GetVimeoPermalink()
        {
            if (videoPermalink != null) {
                if (defaultShareLink == LinkType.ReviewPage) {
                    return videoReviewPermalink;
                } 
                else {
                    return videoPermalink;
                }
            } 
             
            return "https://vimeo.com/" + videoId;
        }

    	private void PublishVideo()
    	{
            api.UploadVideoFile(GetVideoFilePath());
    	}

        private void UploadProgress(string status, float progress)
        {
            if (OnUploadProgress != null) {
                OnUploadProgress (status, progress);
            }
        }

        private void UploadComplete(string video_uri)
		{
			string[] uri_pieces = video_uri.Split ("/" [0]);
			videoId = uri_pieces [2];

            SetVideoName(videoName);
            SetVideoPrivacyMode(m_privacyMode);

			if (openInBrowser == true) {
                OpenVideo();
			}

            DeleteVideoFile();
        }

        private void VideoUpdated(string response)
        {
            JSONNode json = JSON.Parse (response);
            videoPermalink = json["link"];
            videoReviewPermalink = json["review_link"];
        }

        private void DeleteVideoFile()
        {
            FileUtil.DeleteFileOrDirectory(GetVideoFilePath());
        }

        public void SetVideoName(string title)
        {
            if (title != null && title != "") {
                if (saveCoroutine != null) { StopCoroutine (saveCoroutine); }
                api.SetVideoName(title);
                saveCoroutine = StartCoroutine("SaveVideo");
            }
        }

        public void SetVideoPrivacyMode(VimeoApi.PrivacyMode mode)
        {
            if (saveCoroutine != null) { StopCoroutine (saveCoroutine); }
            api.SetVideoViewPrivacy(mode.ToString());
            saveCoroutine = StartCoroutine("SaveVideo");
        }

        private IEnumerator SaveVideo()
        {
            yield return new WaitForSeconds (3f);

            if (videoId != null) {
                Debug.Log ("Video saved!");
                api.SaveVideo (videoId);
            }
        }

        public void OpenVideo()
        {
            Application.OpenURL(GetVimeoPermalink());
        }

        public void OpenSettings()
        {
            Application.OpenURL("https://vimeo.com/" + videoId + "/settings");
        }

        public void PostToSlack()
        {
            if (postToSlack == true && slackChannel != null) {
                if (slack == null) {
                    slack = gameObject.AddComponent<Slack>();
                }

                slack.Init(GetSlackToken(), slackChannel);
                slack.PostVideoToChannel(videoName, GetVimeoPermalink());
            }
        }

        void LateUpdate()
        {
            if (recorder != null) {
                // Set recording state based upon MovieRecorder state
                if (!isRecording && recorder.isRecording) {
                    isRecording = true;
                }

                // If not recording manually, automatically trigger EndRecording
//                if (isRecording && !recorder.isRecording && captureControl != UTJ.FrameCapturer.RecorderBase.CaptureControl.Manual) {
//                    EndRecording ();
//                }
            }
        }

    }
}