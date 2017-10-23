using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace Vimeo.Controls {
    public class PublisherCanvas : MonoBehaviour
    {
        private VimeoPublisher vimeoPublisher;
        public GameObject statusText;
        public GameObject cancelButton;
        public GameObject recordButton;
        public GameObject finishButton;
        public GameObject recordStatus;
        public GameObject progressBar;
        public GameObject titleField;
        public Text titleFieldText;

        //public Dropdown cameraDropdown;

        void Start()
        {
            vimeoPublisher = GetComponent<VimeoPublisher> ();
            vimeoPublisher.OnUploadProgress += UploadProgress;

            finishButton.SetActive (false);
            cancelButton.SetActive (false);
            recordStatus.SetActive (false);
            progressBar.SetActive (false);

            titleField.GetComponent<InputField>().text = vimeoPublisher.videoName;
            titleField.GetComponent<InputField> ().onValueChanged.AddListener (delegate {
                TitleFieldChange();
            });
        }
//
//            cameraDropdown.ClearOptions ();
//            foreach (Camera cam in Camera.allCameras) {
//                cam.enabled = false;
//                cameraDropdown.options.Add (new Dropdown.OptionData(cam.gameObject.name));
//            }
//            cameraDropdown.value = 0;
//            cameraDropdown.RefreshShownValue ();
//            cameraDropdown.onValueChanged.AddListener (delegate {
//                CameraChange();
//            });
//
//            SelectCamera (cameraDropdown.value);
//        }
//
//        private void CameraChange()
//        {
//            SelectCamera (cameraDropdown.value);
//        }
//
//        private void SelectCamera(int index)
//        {
//            string cam_name = cameraDropdown.options [index].text;
//            GameObject cam = GameObject.Find (cam_name);
//            cam.GetComponent<Camera>().enabled = true;
//
//            Debug.Log ("Changed cam: " + cam_name);
//        }
//
        private void TitleFieldChange()
        {
            vimeoPublisher.videoName = titleField.GetComponent<InputField> ().text;
        }

        private void UploadProgress(string status, float progress)
        {
            if (status == "Uploading" || status == "Verifying" || status == "Authorizing") {
                progressBar.SetActive (true);

                cancelButton.SetActive (false);
                finishButton.SetActive (false);
                recordStatus.SetActive (false);
                recordButton.SetActive (false);
                titleField.SetActive (false);
            }
            else if (status != "Complete" && status != "Cancelled") {
                // Recording
                finishButton.SetActive (true);
                cancelButton.SetActive (true);
                recordStatus.SetActive (true);

                titleField.SetActive (false);
                recordButton.SetActive (false);
            }
            else {
                // Finished
                recordButton.SetActive (true);
                titleField.SetActive (true);

                finishButton.SetActive (false);
                cancelButton.SetActive (false);
                recordStatus.SetActive (false);
                progressBar.SetActive (false);
            }



            statusText.GetComponent<Text>().text = status;
        }

        void Destroy() {
            //cameraDropdown.onValueChanged.RemoveAllListeners ();
        }
    }

}

