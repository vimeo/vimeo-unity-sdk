using System; 
using System.Text.RegularExpressions;

namespace Vimeo
{
    [System.Serializable]
    public class VimeoVideo : IComparable<VimeoVideo>
    {
        public string name;
        public string uri;
        public int id;
        
        public VimeoVideo(string _name, string _uri)
        {
            name = _name;
            uri = _uri;
            if (_uri != null) {
                string[] matches = Regex.Split(_uri, "/([0-9]+)$"); 
                id = int.Parse(matches[1]);
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
    } 
}