using System.Collections;
using System.Collections.Generic;
using UTJ.FrameCapturer;
using UnityEngine;
using Vimeo;

public class VimeoPublisher : MonoBehaviour {

    private MovieRecorder recorder;
    private VimeoApi api;

	public Camera camera;
	public string vimeoToken;

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

    public string name;

    [SerializeField] protected PrivacyMode m_privacyMode = PrivacyMode.anybody;


	void Start () {
		recorder = camera.GetComponent<MovieRecorder> ();
        api = gameObject.AddComponent<VimeoApi> ();
        api.token = vimeoToken;
	}

	public void StartRecording()
	{
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

        if (name != null) {
            api.SetVideoName(name);
        }

        api.SetVideoViewPrivacy(PrivacyMode.unlisted.ToString());

        api.SaveVideo(video_id);
    }
}
