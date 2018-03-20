using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Vimeo.Services 
{
    public class SlackApi : MonoBehaviour {

        private string _token;
        private string _channel;

        public void Init(string token, string channel)
        {
            _token = token;	
            _channel = channel;	
        }
        
        public void PostVideoToChannel(string message, string video_url)
        {
            string api_url = "https://slack.com/api/chat.postMessage";

            WWWForm form = new WWWForm ();
            form.AddField("token", _token);
            form.AddField("channel", "#" + _channel.Replace("#", ""));
            form.AddField("text", message + " " + video_url);
            form.AddField("as_user", "false");
            form.AddField("pretty", "1");

            StartCoroutine(Post(api_url, form));
        }

        IEnumerator Post(string url, WWWForm form) 
        {
            Debug.Log(url);
            using (UnityWebRequest request = UnityWebRequest.Post(url, form)) {
                yield return request.SendWebRequest();

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