using System; 
using System.Text.RegularExpressions;
using SimpleJSON;

namespace Vimeo
{
    [System.Serializable]
    public class VimeoVideo : IComparable<VimeoVideo>
    {
        public string name;
        public string uri;
        public int id;
        public int duration;
        public int width;
        public int height;
        
        public VimeoVideo(string _name, string _uri = null)
        {
            name = _name;
            uri = _uri;
            if (_uri != null) {
                string[] matches = Regex.Split(_uri, "/([0-9]+)$"); 
                if (matches.Length > 1) {
                    id = int.Parse(matches[1]);
                }
            }
        }

        public VimeoVideo(JSONNode video)
        {
            name        = video["name"];
            uri         = video["uri"];
            duration    = int.Parse(video["duration"]);
            width       = int.Parse(video["width"]);
            height      = int.Parse(video["height"]);

            if (uri != null) {
                string[] matches = Regex.Split(uri, "/([0-9]+)$"); 
                if (matches.Length > 1) {
                    id = int.Parse(matches[1]);
                    name = name + " (" + id + ")";
                }
            }
        }
        
        public int CompareTo(VimeoVideo other)
        {
            if (other == null)
            {
                return 1;
            }
            
            return 0;
        }
        
        public override string ToString()
        {   
            return name;
        }        
    } 
}