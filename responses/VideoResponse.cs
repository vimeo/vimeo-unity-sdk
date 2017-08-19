using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vimeo.Responses
{
    public class VideoResponse : MonoBehaviour
    {
        public string       uri;
        public new string   name;
        public string       description;
        public string       link;
        public int          width;
        public int          height;
        public object       files;

        // Example response
        // https://developer.vimeo.com/api/playground/videos/2

        public static VideoResponse CreateFromJSON(string jsonString)
        {
            return JsonUtility.FromJson<VideoResponse>(jsonString);
        }
    }
}
