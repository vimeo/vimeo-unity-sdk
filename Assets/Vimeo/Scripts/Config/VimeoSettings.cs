using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace Vimeo 
{  
    public class VimeoSettings : MonoBehaviour
    {
        public string uid;

        public VimeoVideo currentVideo;
        public List<VimeoVideo> vimeoVideos = new List<VimeoVideo>();

        public VimeoFolder currentFolder;
        public List<VimeoFolder> vimeoFolders = new List<VimeoFolder>();

        private string m_vimeoToken;

        [Tooltip("The token is used to give your app access to your Vimeo account.")]
        public string vimeoToken;

        public bool   buildMode = false;
        public bool   vimeoSignIn = false;
        public bool   signInError = false;
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
            if (uid == null || uid == "") {
                uid = System.Guid.NewGuid().ToString();
            }
            
            return VIMEO_TOKEN_PREFIX + this.GetType().Name + "-" + uid;
        }

        public bool Authenticated()
        {
            string token = GetVimeoToken();
            return token != "" && token != null;
        }

        public string GetVimeoToken()
        {
            if (buildMode) {
                return vimeoToken;
            }
            else {
                string _t = PlayerPrefs.GetString(GetTokenKey());
                return _t == "" ? null : _t;
            }
        }

        public void SetVimeoToken(string token)
        {
            vimeoToken = null;
            SetKey(GetTokenKey(), token);
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

        public void EnableBuildMode()
        {
            vimeoToken = PlayerPrefs.GetString(GetTokenKey());
            buildMode  = true;
        }

        public virtual void SignIn(string _token)
        {
            SetVimeoToken(_token);

            if (GetVimeoToken() != null) {
                vimeoSignIn = true;
            }
        }

        public void SignOut()
        {
            SetVimeoToken(null);
            vimeoVideos.Clear();
            vimeoFolders.Clear();

            currentVideo = null;
            currentFolder = null;
            vimeoSignIn = false;
            signInError = false;
        }
    }
}