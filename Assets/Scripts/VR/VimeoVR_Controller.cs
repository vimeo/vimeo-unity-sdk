using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
	private GameObject timeline_group;
	private GameObject timecode_group; 
	private GameObject timecode_text; 
	private GameObject progress_bar_group; 
	private GameObject progress_bar; 

	private bool wasPlaying = true;

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
		controller.PadClicked     += HandlePadClicked;
	}
	
	void Update () {
		device = SteamVR_Controller.Input((int)trackedObject.index);

		// Debug
//		Debug.DrawRay(transform.position, transform.up, Color.green);
//		Debug.DrawRay(transform.position, transform.forward, Color.blue);
//		Debug.DrawRay(transform.position, transform.right, Color.red);

		if (controls) {
//			Debug.DrawRay (controls.transform.position, controls.transform.up, Color.green);
//			Debug.DrawRay (controls.transform.position, controls.transform.forward, Color.blue);
//			Debug.DrawRay (controls.transform.position, controls.transform.right, Color.red);

			// Update controls
			timecode_text.GetComponent<Text>().text = selectedPlayer.GetTimecode();
			//timecode_group.transform.forward = -GameObject.FindObjectOfType<Camera> ().transform.forward;

			var padding = timeline_group.GetComponent<RectTransform> ().sizeDelta.y / 2;
			var width = progress_bar_group.GetComponent<RectTransform> ().sizeDelta.x - padding * 2;

			var timecodeRect = timecode_group.GetComponent<RectTransform> ();
			var progressRect = progress_bar.GetComponent<RectTransform> ();

			timecodeRect.anchoredPosition = new Vector2(selectedPlayer.GetProgress() * width, timecodeRect.anchoredPosition.y);
			progressRect.sizeDelta = new Vector2((1 - selectedPlayer.GetProgress()) * -width, progressRect.sizeDelta.y);

			Debug.Log ("padding: " + timeline_group.GetComponent<RectTransform> ().sizeDelta + " width: " + width + " progress: " + selectedPlayer.GetProgress()  + " anchoredPosition: " + timecodeRect.anchoredPosition +  " progresswidth: " + ((1 - selectedPlayer.GetProgress()) * width));
			//Debug.Log (rect.);

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
					selectedPlayer.SeekForward(distance * 120f);
				}
				else {
					selectedPlayer.SeekBackward(distance * 120f);
				}
			}

			if (input == new Vector2(0, 0) && wasPlaying) {
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

	private void SpawnCanvasPlayerControls()
	{
		controls = GameObject.Find("VimeoControlsCanvas");
		if (controls == null) { 
			controls = Instantiate (Resources.Load ("VimeoControlsCanvas")) as GameObject; 
			controls.name = "VimeoControlsCanvas";
		}

		controls.transform.parent = this.transform;
		controls.transform.position = this.transform.position;

		// Position the controls in front of the 
		controls.transform.Translate(new Vector3(0, -0.005f, 0.07f));
		controls.transform.rotation = Quaternion.LookRotation(-this.transform.up, this.transform.forward);

		// Set the Title text and get its width
		Text txt = GetChild("TitleText", controls.transform).gameObject.GetComponent<UnityEngine.UI.Text> ();
		txt.text = selectedPlayer.videoTitle;
		TextGenerationSettings settings = txt.GetGenerationSettings (txt.rectTransform.rect.size);
		float width = txt.cachedTextGenerator.GetPreferredWidth (txt.text, settings);

		// Adjust bg to be width of text
		RectTransform rt = GetChild ("Title BG", controls.transform).gameObject.GetComponent<RectTransform> ();
		rt.sizeDelta = new Vector2 (width + 15, 25);

		progress_bar_group = GetChild ("ProgressBarContainer", controls.transform).gameObject; 
		timeline_group = GetChild ("Timeline", progress_bar_group.transform).gameObject; 
		timecode_group = GetChild ("Timecode", timeline_group.transform).gameObject; 
		timecode_text  = GetChild ("TimecodeText", timecode_group.transform).gameObject;
		progress_bar   = GetChild ("Bar", timeline_group.transform).gameObject;

		// Load images
		StartCoroutine(LoadImage(selectedPlayer.authorThumbnailUrl, GetChild("UserImage", controls.transform).gameObject));
	}

	private Transform GetChild(string name, Transform obj)
	{
		foreach (Transform t in obj.transform) {
			if (t.name == name) {
				return t;
			}
		}

		return null;
	}

	private IEnumerator LoadImage(string url, GameObject obj) {
		Texture2D tmp = new Texture2D (0, 0);
		WWW www = new WWW (url);
		yield return www;

		tmp = www.texture;
		Sprite sprite = Sprite.Create(tmp, new Rect(0,0,tmp.width, tmp.height), new Vector2(0,0));
		obj.GetComponent<Image>().sprite = sprite;
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

	private void HandlePadClicked(object sender, ClickedEventArgs e)
	{
		if (selectedPlayer) {
			wasPlaying = false;
			selectedPlayer.ToggleVideoPlayback();
		}
	}

	private void HandleTriggerClicked(object sender, ClickedEventArgs e) 
	{
		if (highlightedPlayer) {
			selectedPlayer = highlightedPlayer;	
			selectedPlayer.OnPlay += HandleOnPlay;
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
			if (wasPlaying) {
				selectedPlayer.Play ();
			}
			Debug.Log (device.GetAxis ().x + " " + device.GetAxis ().y);
		}
	}

	private void HandleOnPlay()
	{
		Debug.Log ("HandleOnPlay");
		wasPlaying = true;
	}

}
