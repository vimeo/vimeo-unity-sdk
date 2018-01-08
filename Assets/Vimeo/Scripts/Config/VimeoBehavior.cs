using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vimeo.Utils;

namespace Vimeo.Config 
{  
    public class VimeoBehavior : UniqueId
    {
        public string vimeoToken;
        public bool   saveVimeoToken = true;
        public bool   vimeoSignIn = false;
        private const string VIMEO_TOKEN_NAME = "vimeo-token-";

        public string GetVimeoToken()
        {
            if (saveVimeoToken) {
                return vimeoToken;
            }
            else {
                return PlayerPrefs.GetString(VIMEO_TOKEN_NAME + uniqueId);
            }
        }

        public void SetVimeoToken(string token)
        {
            // Wasn't able to DRY this up - PlayerPrefs started causing seg faults :/
            string token_name = VIMEO_TOKEN_NAME + uniqueId;

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