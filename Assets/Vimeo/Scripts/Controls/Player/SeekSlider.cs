using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Vimeo.Player;

public class SeekSlider : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {

	public VimeoPlayer vimeoPlayer;

	private bool dragging = false;

	public void OnPointerDown(PointerEventData e)
	{
		dragging = true;
		vimeoPlayer.Pause();
	}

	public void OnPointerUp(PointerEventData e)
	{
		vimeoPlayer.Seek(GetComponent<Slider>().normalizedValue);
		dragging = false;
		vimeoPlayer.Play();
	}

	void Update()
	{
		if (vimeoPlayer != null && !dragging) {
        	GetComponent<Slider>().normalizedValue = vimeoPlayer.GetProgress();
        }
	}
}
