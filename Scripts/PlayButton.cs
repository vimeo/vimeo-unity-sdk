using Improbable.Unity;
using Improbable.Unity.Visualizer;
using UnityEngine;
using UnityEngine.Video;

namespace Vimeo
{
    [WorkerType(WorkerPlatform.UnityClient)]
    public class PlayButton : MonoBehaviour
    {
        public GameObject videoScreen;

        private bool videoIsPlaying = true;

        // Use this for initialization
        void Start()
        {
            Debug.Log("test!" + gameObject.name);
        }

        // Update is called once per frame
        void Update()
        {

        }

        void OnCollisionEnter(Collision collision)
        {
            Debug.Log("COLLISION!");

            if (videoIsPlaying) {
                videoIsPlaying = false;
                videoScreen.GetComponent<VideoPlayer>().Pause();
            }
            else {
                videoIsPlaying = true;
                videoScreen.GetComponent<VideoPlayer>().Play();
            }
            
        }
    }
}