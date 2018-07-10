#if UNITY_EDITOR && UNITY_2017_1_OR_NEWER
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.Build;
#if UNITY_2018_1_OR_NEWER
using UnityEditor.Build.Reporting;
#endif 
using Vimeo;

namespace Vimeo 
{
#if UNITY_2018_1_OR_NEWER
    class VimeoBuildProcessor : IProcessSceneWithReport
#else    
    class VimeoBuildProcessor : IProcessScene
#endif 
    {
        public int callbackOrder { get { return 0; } }

#if UNITY_2018_1_OR_NEWER
        public void OnProcessScene(Scene scene, BuildReport report) 
#else
        public void OnProcessScene(Scene scene) 
#endif
        {
            GameObject[] objects = scene.GetRootGameObjects();
            for (int i = 0; i < objects.Length; i++) {
                if (objects[i].GetComponent<VimeoSettings>() != null) {
                    objects[i].GetComponent<VimeoSettings>().EnableBuildMode();
                }
            }
        }
    }
}
#endif