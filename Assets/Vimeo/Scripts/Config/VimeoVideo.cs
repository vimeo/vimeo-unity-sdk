using System;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using Vimeo.Player;
using Vimeo.SimpleJSON;
using UnityEngine;

namespace Vimeo
{
    [System.Serializable]
    public class VimeoVideo : IComparable<VimeoVideo>
    {
        public string name;
        public string uri;
        public string tusUploadLink;
        public int id;
        public string description;
        public int duration;
        public int width;
        public int height;
        public bool is3D = false;
        public string stereoFormat = "mono";
        public string projection;

        private JSONNode files;
        private string dashUrl;
        private string hlsUrl;
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
            if (video["name"] != null) {
                name = video["name"].Value;
            }

            if (video["uri"] != null) {
                uri = video["uri"].Value;
            }

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
                projection = video["spatial"]["projection"].Value;
                stereoFormat = video["spatial"]["stereo_format"].Value;
            }

            if (uri != null) {
                string[] matches = Regex.Split(uri, "/([0-9]+)$");
                if (matches.Length > 1) {
                    id = int.Parse(matches[1]);
                    name = name + " (" + id + ")";
                }
            }

            progressiveFiles = new List<JSONNode>();

            if (video["play"] != null) {
                files = video["play"];

                // Sort the progressive files quality
                for (int i = 0; i < files["progressive"].Count; i++) {
                    progressiveFiles.Add(files["progressive"][i]);
                }
                progressiveFiles.Sort(SortByQuality);

                dashUrl = files["dash"]["link"].Value;
                hlsUrl = files["hls"]["link"].Value;
            }
            // If no play response, fallback to legacy files. 
            else if (video["files"] != null) {
                files = video["files"];

                for (int i = 0; i < files.Count; i++) {
                    if (files[i]["height"] != null) {
                        progressiveFiles.Add(files[i]);
                    } else if (files[i]["quality"].Value == "hls") {
                        hlsUrl = files[i]["link"].Value;
                    } else if (files[i]["quality"].Value == "dash") {
                        dashUrl = files[i]["link"].Value;
                    }
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

        public JSONNode GetMetadata()
        {
            return GetJsonFromString(this.description);
        }

        public JSONNode GetJsonFromString(string content)
        {
            var matches = Regex.Matches(content, @"(?s:\{.*\})");

            if (matches.Count == 0) {
                Debug.LogWarning("[Vimeo] No JSON was found in the video description. ");
                return null;
            }

            try {
                return JSONNode.Parse(matches[0].Value);
            } catch (System.Exception e) {
                Debug.LogError("[Vimeo] There was a problem parsing the JSON. " + e);
                return null;
            }
        }

        public float GetHeightByWidth(float _width)
        {
            return _width * ((float)height / (float)width);
        }

        public override string ToString()
        {
            return name;
        }

        public string getDashUrl()
        {
            return dashUrl;
        }

        public string getHlsUrl()
        {
            return hlsUrl;
        }

        public string GetAdaptiveVideoFileURL()
        {
            if (isHlsPlatform()) {
                return getHlsUrl();
            } else {
                if (getDashUrl() != null) {
                    return getDashUrl();
                }
                Debug.LogWarning("[Vimeo] No DASH manfiest found. Defaulting to HLS.");
                return getHlsUrl();
            }
        }

        public bool isHlsPlatform()
        {
            return Application.platform == RuntimePlatform.OSXPlayer ||
                   Application.platform == RuntimePlatform.OSXEditor ||
                   Application.platform == RuntimePlatform.IPhonePlayer ||
                   Application.platform == RuntimePlatform.tvOS;
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
                if (_preferred_qualities.Count == 0) {
                    Debug.Log("[Vimeo] This video does not have a " + resolution + " resolution. Defaulting to " + progressiveFiles[progressiveFiles.Count - 1]["height"] + "p.");
                    return progressiveFiles[progressiveFiles.Count - 1];
                } else {
                    Debug.Log("[Vimeo] This video does not have a " + resolution + " resolution. Defaulting to " + _preferred_qualities[0]["height"] + "p.");
                }
            }

            return _preferred_qualities[0];
        }

        private static int SortByQuality(JSONNode q1, JSONNode q2)
        {
            return int.Parse(q2["height"].Value).CompareTo(int.Parse(q1["height"].Value));
        }
    }
}