using System.Collections;
using System.Collections.Generic;
using UTJ.FrameCapturer;
using UnityEngine;

public class VimeoPublisher : MonoBehaviour {

	public Camera camera;
	private MovieRecorder recorder;
	public string vimeoApiKey;
	public string vimeoToken;

	void Start () {
		recorder = camera.GetComponent<MovieRecorder> ();
	}

	public void StartRecording()
	{
		recorder.BeginRecording ();
	}

	public void EndRecording()
	{
		recorder.EndRecording();
	}

	private void PublishVideo()
	{

	}
}
