using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vimeo.Misc
{
    public class RandomColor : MonoBehaviour
    {
    	public GameObject lightObject;

        private string[] colors = new string[9] {
            "#55D6FF",
            "#FF55B5",
            "#1e3957",
            "#7eac18",
            "#ddaf02",
            "#ce5449",
            "#663d67",
            "#3b305b",
            "#389fda"
        };

        void Start()
        {
            string color_hex = colors[Random.Range(0, colors.Length)];
            Color color;
            ColorUtility.TryParseHtmlString(color_hex, out color);

            var renderer = GetComponent<Renderer>();
            renderer.material.color = color;

			Color emissionColor = color * Mathf.LinearToGammaSpace(0.4f);
			renderer.material.SetColor("_EmissionColor", emissionColor);

            if (lightObject != null) {
                lightObject.GetComponent<Light>().color = color;
            }

			//renderer.material.SetColor("_Color", color);
            //gameObject.transform.renderer
            //Material mat = GetComponent<Material>();
            //mat.SetColor("_Color", color);
        }

        void Update()
        {

        }

    }


}

