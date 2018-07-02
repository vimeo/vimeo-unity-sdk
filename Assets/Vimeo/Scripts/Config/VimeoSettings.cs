using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace Vimeo 
{  
    public class VimeoSettings : MonoBehaviour
    {
        public VimeoVideo currentVideo;
        public List<VimeoVideo> vimeoVideos = new List<VimeoVideo>();

        public VimeoFolder currentFolder;
        public List<VimeoFolder> vimeoFolders = new List<VimeoFolder>();

        public string vimeoToken;
        public bool   saveVimeoToken = true;
        public bool   vimeoSignIn = false;
        private const string VIMEO_TOKEN_PREFIX = "vimeo-token-";


        public int GetCurrentFolderIndex()
        {
            if (currentFolder != null) {
                for (int i = 0; i < vimeoFolders.Count; i++) {
                    if (vimeoFolders[i].uri == currentFolder.uri) {
                        return i;
                    }
                }
            }
            return 0;
        }

        public int GetCurrentVideoIndex()
        {
            if (currentVideo != null) {
                for (int i = 0; i < vimeoVideos.Count; i++) {
                    if (vimeoVideos[i].uri == currentVideo.uri) {
                        return i;
                    }
                }
            }
            return 0;
        }

        private string GetTokenKey()
        {
            return VIMEO_TOKEN_PREFIX + this.GetType().Name + "-" + this.gameObject.scene.name;
        }

        public string GetVimeoToken()
        {
            if (saveVimeoToken) {
                return vimeoToken;
            }
            else {
                return PlayerPrefs.GetString(GetTokenKey());
            }
        }

        public void SetVimeoToken(string token)
        {
            string token_name = GetTokenKey();

            if (saveVimeoToken) {
                SetKey(token_name, null);
                vimeoToken = token;
            }
            else {
                vimeoToken = null;
                SetKey(token_name, token);
            }
        }

        public void SetKey(string key, string val)
        {
            if (val == null || val == "") {
                PlayerPrefs.DeleteKey(key);
            } 
            else {
                PlayerPrefs.SetString(key, val);
            }
            PlayerPrefs.Save(); 
        }
    }
}