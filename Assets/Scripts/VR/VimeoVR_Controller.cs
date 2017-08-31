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
	private VimeoPlayer highlightedPlayer;
	private VimeoPlayer selectedPlayer;

	private Vector2 lastTouchPosition = new Vector2(0, 0);

	private GameObject controls;

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

		// Debug
		Debug.DrawRay(transform.position, transform.up, Color.green);
		Debug.DrawRay(transform.position, transform.forward, Color.blue);
		Debug.DrawRay(transform.position, transform.right, Color.red);

		if (controls) {
			Debug.DrawRay (controls.transform.position, controls.transform.up, Color.green);
			Debug.DrawRay (controls.transform.position, controls.transform.forward, Color.blue);
			Debug.DrawRay (controls.transform.position, controls.transform.right, Color.red);


			//controls.transform.right = this.transform.right;
		}
		DrawLineToVideo ();
		SeekVideo ();
	}

	private void SeekVideo()
	{
		if (controller.padTouched && selectedPlayer != null) {

			var input = new Vector2(device.GetAxis ().x, device.GetAxis ().y);
			float distance = Vector2.Distance(input, lastTouchPosition);

			if (distance > 0.01f) {
				selectedPlayer.Pause ();
				if (GetInputRotationDirection(input, lastTouchPosition)) {
					selectedPlayer.SeekForward(distance * 60f);
				}
				else {
					selectedPlayer.SeekBackward(distance * 60f);
				}
			}

			if (input == new Vector2 (0, 0)) {
				selectedPlayer.Play();
			}

			lastTouchPosition = input;
		}
	}

	// true = left
	private bool GetInputRotationDirection(Vector2 p1, Vector2 p2)
	{
		float angle1 = Mathf.Atan2(p1.x, p1.y) * Mathf.Rad2Deg + 180;
		float angle2 = Mathf.Atan2(p2.x, p2.y) * Mathf.Rad2Deg + 180;

		if (angle1 - angle2 > 0) {
			return true;
		}

		return false;
	}

	private void DrawLineToVideo()
	{
		RaycastHit hit;
		Ray forwardRay = new Ray(transform.position, transform.forward);

		line.enabled = false;
		highlightedPlayer = null;

		if (Physics.Raycast (forwardRay, out hit)) {
			if (hit.transform.GetComponent<TriggerVRControls> () != null) {
				Debug.Log (highlightedPlayer);
				line.enabled = true;
				line.SetPosition (0, transform.position);	
				line.SetPosition (1, hit.point);

				highlightedPlayer = hit.transform.GetComponent<TriggerVRControls> ().vimeoPlayer.GetComponent<VimeoPlayer> ();
			}
		}
	} 

	private void HandleTriggerClicked(object sender, ClickedEventArgs e) 
	{
		if (highlightedPlayer) {
			selectedPlayer = highlightedPlayer;
			SpawnCanvasPlayerControls ();
		}
	}

	private void HandlePadTouched(object sender, ClickedEventArgs e) 
	{
		if (selectedPlayer != null) {
			Debug.Log (device.GetAxis ().x + " " + device.GetAxis ().y);
		}
	}

	private void HandlePadUntouched(object sender, ClickedEventArgs e) 
	{
		if (selectedPlayer != null) {
			selectedPlayer.Play ();
			Debug.Log (device.GetAxis ().x + " " + device.GetAxis ().y);
		}
	}

	private void SpawnCanvasPlayerControls()
	{
		controls = GameObject.Find("VimeoControlsCanvas");
		if (controls == null) { 
			controls = Instantiate (Resources.Load ("VimeoControlsCanvas")) as GameObject; 
			controls.name = "VimeoControlsCanvas";
		}

		controls.transform.parent = this.transform;
		controls.transform.position = this.transform.position;
		//controls.transform.position.y = 0.001f;
		controls.transform.rotation = Quaternion.LookRotation(-this.transform.up, this.transform.forward);

		foreach (Transform t in controls.transform) {
			if (t.name == "TitleText") {
				t.gameObject.GetComponent<UnityEngine.UI.Text> ().text = selectedPlayer.videoTitle;
			}
		}
	}

	private void Spawn3DPlayerControls()
	{
		GameObject controls = GameObject.Find("/VimeoVRControls");
		if (controls == null) { 
			controls = Instantiate (Resources.Load ("VimeoVRControls")) as GameObject; 
			controls.name = "VimeoVRControls";
		}

		controls.GetComponent<VimeoVR_PlayerControls> ().Init (selectedPlayer);
	}
}
