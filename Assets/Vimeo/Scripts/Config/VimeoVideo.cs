using System; 
using System.Text.RegularExpressions;
using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using Vimeo.Player;
using UnityEngine;

namespace Vimeo
{
    [System.Serializable]
    public class VimeoVideo : IComparable<VimeoVideo>
    {
        public string name;
        public string uri;
        public int id;
        public string description;
        public int duration;
        public int width;
        public int height;
        public bool is3D = false;
        public string stereoFormat = "mono";
        public string projection;

        private JSONNode files;
        private List<JSONNode> progressiveFiles;
        
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
            // if (video["user"]["account"].Value == "basic") {
            //     Debug.LogError("[VimeoPlayer] You do not have permission to stream videos. You must be a Vimeo Pro or Business customer. https://vimeo.com/upgrade");
            // }

            // if ((video["play"] == null || video["play"]["progressive"] == null) && video["files"] == null) {
            //     Debug.LogError("[VimeoPlayer] You do not have permission to access to this video. You must be a Vimeo Pro or Business customer and use videos from your own account. https://vimeo.com/upgrade");
            // }

            name = video["name"].Value;
            uri = video["uri"].Value;
            
            if (video["description"] != null) {
                description = video["description"].Value;
            }
            
            if (video["duration"] != null) {
                duration = int.Parse(video["duration"].Value);
            }
            if (video["width"] != null) {
                width = int.Parse(video["width"].Value);
            }
            if (video["height"] != null) {
                height = int.Parse(video["height"].Value);
            }

            if (video["spatial"] != null && !video["spatial"].IsNull) {
                is3D = true;
                projection   = video["spatial"]["projection"].Value;
                stereoFormat = video["spatial"]["stereo_format"].Value;
            }

            if (uri != null) {
                string[] matches = Regex.Split(uri, "/([0-9]+)$"); 
                if (matches.Length > 1) {
                    id = int.Parse(matches[1]);
                    name = name + " (" + id + ")";
                }
            }

            files = video["play"];

            // Sort the progressive files quality
            progressiveFiles = new List<JSONNode>();
            if (files != null) {
                for (int i = 0; i < files["progressive"].Count; i++) {
                    progressiveFiles.Add(files["progressive"][i]);
                }   
                progressiveFiles.Sort(SortByQuality);
            }
        }
        
        public int CompareTo(VimeoVideo other)
        {
            if (other == null) {
                return 1;
            }
            
            return 0;
        }

        public float GetHeightByWidth(float _width)
        {
            return _width * ((float)height / (float)width);
        }
        
        public override string ToString()
        {   
            return name;
        }  
        
        public string GetAdaptiveVideoFileURL() 
        {
            switch (Application.platform)
            {
                case RuntimePlatform.OSXDashboardPlayer:
                case RuntimePlatform.OSXPlayer:
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.IPhonePlayer:
                case RuntimePlatform.tvOS:
                    return files["hls"]["link"];

                default:
                    return files["dash"]["link"];
            }
            
        }  

        public JSONNode GetHighestResolutionVideoFileURL()
        {
            return progressiveFiles[0]["link"];
        }

        public string GetVideoFileUrlByResolution(StreamingResolution resolution)
        {
            return GetVideoFileByResolution(resolution)["link"];
        }
                
        public JSONNode GetVideoFileByResolution(StreamingResolution resolution)
        {
            if (resolution == StreamingResolution.Adaptive) {
                return null;
            }

            bool resolution_found = false;

            List<JSONNode> _preferred_qualities = new List<JSONNode>();
            for (int i = 0; i < progressiveFiles.Count; i++) {
                if (int.Parse(progressiveFiles[i]["height"]) <= (int)resolution) {
                    _preferred_qualities.Add(progressiveFiles[i]);

                    if (int.Parse(progressiveFiles[i]["height"]) == (int)resolution) {
                        resolution_found = true;
                    }
                }
            }

            if (!resolution_found) {
                Debug.Log("[VimeoVideo] This video does not have a " + resolution + " resolution. Defaulting to " + _preferred_qualities[0]["height"] + "p.");
            }

            return _preferred_qualities[0];
        }
        
        private static int SortByQuality(JSONNode q1, JSONNode q2)
        {
            return int.Parse(q2["height"]).CompareTo(int.Parse(q1["height"]));
        }
    
    } 
}