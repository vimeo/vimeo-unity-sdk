using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vimeo;

namespace Vimeo.Fun {
	public class AmbientLighting : MonoBehaviour {

		private static float COLOR_MULTIPLIER = 255.999f;

		private VimeoPlayer vimeoPlayer;

        public Light[] lights;

		private float refreshRate = 0.5f;
		private float timeSinceRefresh = 0f;

		private bool videoReady = false;

		void Start () {
			vimeoPlayer = GetComponent<VimeoPlayer>();	
			vimeoPlayer.OnVideoStart += InitLighting;
		}

		private void OnDisable()
		{
			vimeoPlayer.OnVideoStart -= InitLighting;
		}

		private void InitLighting()
		{
			videoReady = true;
		}
		
		void LateUpdate () {
			if (videoReady) {
                StartCoroutine(RefreshLights());
			}
		}

		IEnumerator RefreshLights()
		{
			var videoTex = vimeoPlayer.video.videoPlayer.texture as RenderTexture;
			timeSinceRefresh += Time.deltaTime;

			if (videoTex != null && timeSinceRefresh > refreshRate) {
				var videoTex2D = RenderTextureToTexture2D (videoTex);

                for (int i = 0; i < lights.Length; i++) {
                    lights[i].color = GetAverageColorFromRect (videoTex2D, new Rect(0, 0, videoTex2D.width, videoTex2D.height));
                }

                Destroy(videoTex2D);
				timeSinceRefresh = 0f;
			}
            yield return null;
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

            Color averageColor = new Color (r, g, b) * 0.0039f;
			return averageColor;
		}

        private Color GetAverageColorFromRect(Texture2D texture, Rect pixelBlock) {
			Color[] pixels = GetRectPixelsFromTexture (texture, pixelBlock);
            return GetAverageColor(pixels);
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