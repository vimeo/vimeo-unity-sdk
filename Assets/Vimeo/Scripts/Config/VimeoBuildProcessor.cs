#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using Vimeo;

class VimeoBuildProcessor : IProcessSceneWithReport
{
    public int callbackOrder { get { return 0; } }
    public void OnProcessScene(Scene scene, BuildReport report) 
    {
        GameObject[] objects = scene.GetRootGameObjects();

        for (int i = 0; i < objects.Length; i++) {
            if (objects[i].GetComponent<VimeoSettings>() != null) {
                objects[i].GetComponent<VimeoSettings>().EnableBuildMode();
            }
        }
    }
}
#endif