using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vimeo.Player;

public class TinyPlanetCam : MonoBehaviour {

	[Range(1f, 100f)]
	public float fovAmplitude = 1f;

	[Range(1f, 100f)]
	public float fovRange = 1f;

	[Range(0f, 0.1f)]
	public float yAxisAmplitude = 0.0001f;

	[Range(0f, 20f)]
	public float yAxisRange = 1f;

	[Range(0f, 1f)]
	public float rotationAmplitude = 0.001f;

	[Range(0f, 20f)]
	public float rotationRange = 1f;

    private float startYPos;

	public VimeoPlayer vimeoPlayer;
	private float timePassed = 0;

    void Start()
    {
        startYPos = Camera.main.transform.position.y;

		if (vimeoPlayer != null) {
			vimeoPlayer.OnFrameReady += VideoFrameUpdate;
		}
    }

	void Update() 
    {
		if (vimeoPlayer == null) {
			timePassed = Time.time;
			UpdateCamera();
		}
	}

	public void VideoFrameUpdate()
	{
		// Update time passed based upon the frame rate of the video
		timePassed += 1f / vimeoPlayer.controller.videoPlayer.frameRate;
		UpdateCamera();
	}

	private void UpdateCamera()
	{
		Camera.main.fieldOfView = 120f + Mathf.Sin(timePassed / fovRange) * fovAmplitude;
        Camera.main.transform.Translate(new Vector3(0, 0, Mathf.Cos(timePassed / yAxisRange) * yAxisAmplitude));
		Camera.main.transform.Rotate(new Vector3(0, 0, Mathf.Cos(timePassed / rotationRange) * rotationAmplitude));
	}

	private void OnDisable()
	{
		if (vimeoPlayer != null) {
			vimeoPlayer.OnFrameReady -= VideoFrameUpdate;
		}
	}
}
