using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vimeo;

namespace Vimeo.Fun {
	public class AmbientLighting : MonoBehaviour {

		private static float COLOR_MULTIPLIER = 255.999f;

		private VimeoPlayer vimeoPlayer;

		public Light topLeftLight;
		public Light topRightLight;
		public Light bottomLeftLight;
		public Light bottomRightLight;
		public Light screenLight;

		private float refreshRate = 0.1f;
		private float timeSinceRefresh = 0f;

		private bool videoReady = false;

		void Start () {
			vimeoPlayer = GetComponent<VimeoPlayer>();	
			vimeoPlayer.OnVideoStart += InitLighting;

			var collider = vimeoPlayer.videoScreen.GetComponent<Collider> ();

			vimeoPlayer.videoScreen.layer = 30;
			float lightPadding = 0.5f;

			topLeftLight = CreateLight (new Vector3(
				collider.bounds.min.x + collider.bounds.extents.x * lightPadding,
				collider.bounds.max.y - collider.bounds.extents.y * lightPadding,
				collider.bounds.min.z - collider.bounds.extents.z
			) + transform.forward * 0.1f);

			topRightLight = CreateLight (new Vector3(
				collider.bounds.max.x - collider.bounds.extents.x * lightPadding,
				collider.bounds.max.y - collider.bounds.extents.y * lightPadding,
				collider.bounds.min.z - collider.bounds.extents.z
			) + transform.forward * 0.1f);

			bottomLeftLight = CreateLight (new Vector3(
				collider.bounds.min.x + collider.bounds.extents.x * lightPadding,
				collider.bounds.min.y + collider.bounds.extents.y * lightPadding,
				collider.bounds.min.z - collider.bounds.extents.z
			) + transform.forward * 0.1f);

			bottomRightLight = CreateLight (new Vector3(
				collider.bounds.max.x - collider.bounds.extents.x * lightPadding,
				collider.bounds.min.y + collider.bounds.extents.y * lightPadding,
				collider.bounds.min.z - collider.bounds.extents.z
			) + transform.forward * 0.1f);

			screenLight = CreateLight (new Vector3(
				collider.bounds.min.x + collider.bounds.extents.x,
				collider.bounds.min.y + collider.bounds.extents.y,
				collider.bounds.min.z - collider.bounds.extents.z
			) + transform.forward * 10f);

			// Cast light onto everything except the video screen object
			topLeftLight.cullingMask     = ~(1 << 30);
			topRightLight.cullingMask    = ~(1 << 30);
			bottomLeftLight.cullingMask  = ~(1 << 30);
			bottomRightLight.cullingMask = ~(1 << 30);

			// Only light up the screen
			screenLight.cullingMask = 1 << 30;
			screenLight.intensity = 2f;
			screenLight.range = 20f;
		}

		private void OnDisable()
		{
			vimeoPlayer.OnVideoStart -= InitLighting;
		}

		private void InitLighting()
		{
			Debug.Log ("InitLighting");
			videoReady = true;
		}
		
		void LateUpdate () {
			if (videoReady) {
				RefreshLights();
			}
		}

		private void RefreshLights()
		{
			var videoTex = vimeoPlayer.video.videoPlayer.texture as RenderTexture;
			timeSinceRefresh += Time.deltaTime;

			if (videoTex != null && timeSinceRefresh > refreshRate) {
				var videoTex2D = RenderTextureToTexture2D (videoTex);
				bottomLeftLight.color  = GetAverageColorFromRect (videoTex2D, new Rect(0, 0, videoTex2D.width / 2, videoTex2D.height / 2));
				bottomRightLight.color = GetAverageColorFromRect (videoTex2D, new Rect(videoTex2D.width / 2, 0, videoTex2D.width / 2, videoTex2D.height / 2));
				topLeftLight.color     = GetAverageColorFromRect (videoTex2D, new Rect(0, videoTex2D.height / 2, videoTex2D.width / 2, videoTex2D.height / 2));
				topRightLight.color    = GetAverageColorFromRect (videoTex2D, new Rect(videoTex2D.width / 2, videoTex2D.height / 2, videoTex2D.width / 2, videoTex2D.height / 2));
				Destroy(videoTex2D);
				timeSinceRefresh = 0f;
			}

		}

		private Light CreateLight(Vector3 pos)
		{
			var go = new GameObject ();
			go.transform.position = pos;
			go.transform.parent = this.transform;
			go.name = "Light";
			var light = go.AddComponent<Light> ();
			light.range = 8f;
			light.intensity = 0.005f;
			return light;
		}

		private Color GetAverageColor(Color[] colors) {
			int pixelCount = colors.Length;
			float r = 0f, g = 0f, b = 0f;
			foreach (Color color in colors) {
				r += color.r;
				g += color.g;
				b += color.b;
			}

			r /= pixelCount;
			g /= pixelCount;
			b /= pixelCount;

			r = Mathf.Round (r * COLOR_MULTIPLIER);
			g = Mathf.Round (g * COLOR_MULTIPLIER);
			b = Mathf.Round (b * COLOR_MULTIPLIER);

			Color averageColor = new Color (r, g, b, 1f);
			return averageColor;
		}

		private Color GetAverageColorFromRect(Texture2D texture, Rect pixelBlock) {
			Color[] pixels = GetRectPixelsFromTexture (texture, pixelBlock);
			return GetAverageColor (pixels);
		}

		private Texture2D RenderTextureToTexture2D(RenderTexture texture)
		{
			Texture2D texture2D = new Texture2D(texture.width, texture.height);
			RenderTexture.active = texture;
			texture2D.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
			TextureScale.Point (texture2D, 300, 200);

			return texture2D;
		}

		private Color[] GetRectPixelsFromTexture(Texture2D texture, Rect pixelBlock) 
		{
			int pbx = Mathf.FloorToInt (pixelBlock.x);
			int pby = Mathf.FloorToInt (pixelBlock.y);
			int pbw = Mathf.FloorToInt (pixelBlock.width);
			int pbh = Mathf.FloorToInt (pixelBlock.height);

			if (pbx < 0 || pbx > texture.width || pbw > texture.width || pbh > texture.height) {
				return null;
			}

			Color[] rectPixels = texture.GetPixels (pbx, pby, pbw, pbh);
			return rectPixels;
		}
	}
}