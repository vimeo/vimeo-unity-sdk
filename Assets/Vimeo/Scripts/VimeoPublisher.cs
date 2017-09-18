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
        Anyone,
        OnlyMe,
        OnlyPeopleIFollow,
        OnlyPeopleIChoose,
        Password,
        PrivateLink,
        Hidden,
    }

    [SerializeField] protected PrivacyMode m_privacyMode = PrivacyMode.Anyone;

	void Start () {
		recorder = camera.GetComponent<MovieRecorder> ();
        api = gameObject.AddComponent<VimeoApi> ();
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
        api.UploadVideoFile(recorder.outputPath + ".mp4", vimeoToken);
	}
}
