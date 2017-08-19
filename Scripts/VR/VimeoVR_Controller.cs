using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vimeo;
using Vimeo.VR;

public class VimeoVR_Controller : MonoBehaviour {

	private SteamVR_TrackedObject trackedObject;
	private SteamVR_TrackedController controller;
	private SteamVR_Controller.Device device;

	private LineRenderer line;
	private bool videoSelected = false;
	private VimeoPlayer currentVideo;

	private Vector2 lastTouchPosition = new Vector2(0, 0);

	void Start () {

		line = gameObject.AddComponent<LineRenderer> ();	
		line.widthMultiplier = 1;
		line.startWidth = 0.003f;

		line.material = new Material (Shader.Find ("Particles/Additive (Soft)"));
		line.startColor = new Color (255, 0, 255);
		line.endColor = new Color (0, 255, 255);

		trackedObject = GetComponent<SteamVR_TrackedObject> ();
		controller = gameObject.AddComponent<SteamVR_TrackedController> ();
		controller.TriggerClicked += HandleTriggerClicked;
		controller.PadTouched     += HandlePadTouched;
		controller.PadUntouched   += HandlePadUntouched;
	}
	
	void Update () {
		device = SteamVR_Controller.Input((int)trackedObject.index);

		DrawLineToVideo ();
		SeekVideo ();
	}

	private void SeekVideo()
	{
		if (controller.padTouched && currentVideo != null) {
			//currentVideo.Pause ();

			float distance = Vector2.Distance (new Vector2 (device.GetAxis ().x, device.GetAxis ().y), lastTouchPosition);
			if (distance > 0.01f) {
				Debug.Log (distance * 3f);
				currentVideo.SeekForward (distance * 60f);
			}
			lastTouchPosition = new Vector2 (device.GetAxis ().x, device.GetAxis ().y);
		}
	}

	private void DrawLineToVideo()
	{
		RaycastHit hit;
		Ray forwardRay = new Ray(transform.position, transform.forward);

		line.enabled = false;
		videoSelected = false;

		if (Physics.Raycast (forwardRay, out hit)) {
			if (hit.transform.GetComponent<TriggerVRControls> () != null) {
				line.enabled = true;
				line.SetPosition (0, transform.position);	
				line.SetPosition (1, hit.point);

				videoSelected = true;
				currentVideo = hit.transform.GetComponent<TriggerVRControls> ().vimeoPlayer.GetComponent<VimeoPlayer> ();
			}
		}
	}

	private void HandleTriggerClicked(object sender, ClickedEventArgs e) 
	{
		if (videoSelected != null) {
			SpawnPlayerControls ();
		}
	}

	private void HandlePadTouched(object sender, ClickedEventArgs e) 
	{
		if (currentVideo != null) {
			Debug.Log (device.GetAxis ().x + " " + device.GetAxis ().y);
		}
	}

	private void HandlePadUntouched(object sender, ClickedEventArgs e) 
	{
		if (currentVideo != null) {
			currentVideo.Play ();
			Debug.Log (device.GetAxis ().x + " " + device.GetAxis ().y);
		}
	}


	private void SpawnPlayerControls()
	{
		GameObject controls = GameObject.Find("/VimeoVRControls");
		if (controls == null) { 
			controls = Instantiate (Resources.Load ("VimeoVRControls")) as GameObject; 
			controls.name = "VimeoVRControls";
		}

		controls.GetComponent<VimeoVR_PlayerControls> ().Init (currentVideo);
	}
}
