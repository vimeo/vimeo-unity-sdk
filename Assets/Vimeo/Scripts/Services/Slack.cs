using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Vimeo.Services {
	public class Slack : MonoBehaviour {

		private string _token;
		private string _channel;

		void Start () {
		}

		public void Init (string token, string channel)
		{
			_token = token;	
			_channel = channel;	
		}
		
		public void PostVideoToChannel (string video_url)
		{
			string api_url = "https://slack.com/api/chat.postMessage";

			WWWForm form = new WWWForm ();
            form.AddField ("token", _token);
			form.AddField ("channel", "#" + _channel);
			form.AddField ("text", "Check out my latest render from " + Application.productName + " " + video_url);
			form.AddField ("as_user", "true");
			form.AddField ("pretty", "1");

			StartCoroutine(Post(api_url, form));
		}

		IEnumerator Post(string url, WWWForm form) 
		{
			Debug.Log("Posting to slack:" + url );
			Debug.Log(form.ToString());

			using (UnityWebRequest request = UnityWebRequest.Post(url, form)) {
				yield return request.Send ();

				Debug.Log(request.downloadHandler.text);

				if (request.isNetworkError) {
                    Debug.Log (request.error);
                } 
                else {
                    Debug.Log("Posted to Slack!");
            	}
			}
		}
	}
}