using System.Collections;using UnityEngine;using UnityEngine.Networking;using SimpleJSON;


// Dev token 93f6a1e01b096a554624c5378f34d59a
namespace Vimeo{    public class VimeoApi : MonoBehaviour    {        public delegate void RequestAction(string response);        public event RequestAction OnRequestComplete;        public string token;        private static string API_URL = "https://api.vimeo.com";        public void GetVideoFileUrlByVimeoId(int vimeo_id)        {            StartCoroutine("Request", "/videos/" + vimeo_id);        }        IEnumerator Request(string api_path)        {
            if (token != null)
            {
                UnityWebRequest request = UnityWebRequest.Get(API_URL + api_path);                request.SetRequestHeader("Authorization", "Bearer " + token);                yield return request.Send();

                if (OnRequestComplete != null) {
                    OnRequestComplete(request.downloadHandler.text);
                }            }        }    }}