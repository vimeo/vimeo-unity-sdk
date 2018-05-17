using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vimeo 
{  
    public class VimeoAuth : MonoBehaviour
    {
        public string vimeoToken;
        public bool   saveVimeoToken = true;
        public bool   vimeoSignIn = false;
        private const string VIMEO_TOKEN_PREFIX = "vimeo-token-";

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