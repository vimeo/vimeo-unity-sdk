#if UNITY_2017_1_OR_NEWER
// Used with permission from http://www.depthkit.tv/

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.IO;

namespace Vimeo
{
    [InitializeOnLoad]
    public class VimeoPluginProcesser : AssetPostprocessor
    {
        static void AddPlayerDefine(string target)
        {
            
            //set the target across all supported platforms
            for (int pIndex = 0; pIndex < VimeoPlugin.SupportedPlatforms.Length; pIndex++)
            {
                //get the exisiting defines
                string existingDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(VimeoPlugin.SupportedPlatforms[pIndex]);
                int defineIndex;
                List<string> defineList;
                if(!DefineExistsInPlatformDefines(existingDefines, target, out defineList, out defineIndex))
                {
                    //add the new define
                    defineList.Add(target);

                    //combine the strings back into the proper define style
                    string newDefines = string.Join(";", defineList.ToArray());
                    //add the defines
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(VimeoPlugin.SupportedPlatforms[pIndex], newDefines);
                }
            }
        }

        static void RemovePlayerDefine(string target)
        {
            //remove the target across all supported platforms
            for (int pIndex = 0; pIndex < VimeoPlugin.SupportedPlatforms.Length; pIndex++)
            {
                //get the exisiting defines
                string existingDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(VimeoPlugin.SupportedPlatforms[pIndex]);
                int defineIndex;
                List<string> defineList;
                if(DefineExistsInPlatformDefines(existingDefines, target, out defineList, out defineIndex))
                {
                    defineList.RemoveAt(defineIndex); 

                    //combine the strings back into the proper define style
                    string newDefines = string.Join(";", defineList.ToArray());

                    //add the defines
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(VimeoPlugin.SupportedPlatforms[pIndex], newDefines);
                }
            }
        }

        static bool DefineExistsInPlatformDefines(string platformDefines, string targetDefine, out List<string> defineList, out int index)
        {
            //assign index a bum value
            index = 0;

            //split the platform defines
            string[] defines = platformDefines.Split(';');

            //make the new define list
            defineList = new List<string>(defines);

            //check if the define exists
            for (int i = defineList.Count-1; i >= 0; i--)
            {
                if(defines[i].Contains(targetDefine))
                {
                    index = i;
                    return true;
                }
            }

            return false;
        }

        public static List<PluginType> GetsupportedPluginsInAssembly()
        {
            string existingDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            string[] defines = existingDefines.Split(';');
            List<PluginType> supportedPlugins = new List<PluginType>();

            //find out if this has already been definied
            for (int i = 0; i < defines.Length; i++ )
            {
                if(defines[i].Contains("VIMEO_"))
                {
                    supportedPlugins.Add(VimeoPlugin.DirectiveDict[defines[i]]);
                }
            }

            return supportedPlugins;
        } 

        public static void UpdateDefines()
        {
            foreach (KeyValuePair<string,string> item in VimeoPlugin.AssetSearchDict)
            {
                
                string[] assets = AssetDatabase.FindAssets(item.Key);                
                bool playerFound = false;
                if(assets.Length > 0) //the file exists
                {
                    foreach (string guid in assets)
                    {
                        //need to do this because FindAssets can return files where string is only part of the name, i.e. MediaPlayer
                        if(item.Key == Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(guid)))
                        {
                            playerFound = true;
                            AddPlayerDefine(item.Value);
                            break; 
                        }
                    }                    
                }
                
                if(!playerFound)
                {
                    //asset doesn't exist at all so remove the define
                    RemovePlayerDefine(item.Value);             
                }
            }
        }

        static void OnPostprocessAllAssets (string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) 
        {       
            UpdateDefines();
        }
    }
}
#endif 

