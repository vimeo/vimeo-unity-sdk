#if UNITY_2017_3_OR_NEWER
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Vimeo;
using Vimeo.Services;

namespace Vimeo.Controls {
    public class PublisherCanvas : MonoBehaviour
    {
        private VimeoRecorder recorder;

        public GameObject container;
        public GameObject cancelButton;
        public GameObject recordButton;
        public GameObject finishButton;
        public GameObject recordStatus;
        public GameObject progressBar;
        public GameObject titleField;
        public Dropdown recordTypeDropdown;
        public GameObject recordInputField;
        public Canvas shareWindow;
        public GameObject slackChannelField;
        public Dropdown privacyTypeDropdown;

        void Start()
        {
            recorder = GetComponent<VimeoRecorder>();
            recorder.OnUploadProgress += UploadProgress;

            titleField.GetComponent<InputField>().text = recorder.videoName;
            slackChannelField.GetComponent<InputField>().text = recorder.slackChannel;
            privacyTypeDropdown.value = GetPrivacyTypeIndex(recorder.privacyMode);

            recordButton.GetComponent<Button>().onClick.AddListener(delegate {
                recorder.BeginRecording();
            });

            cancelButton.GetComponent<Button>().onClick.AddListener(delegate {
                recorder.CancelRecording();
            });

            finishButton.GetComponent<Button>().onClick.AddListener(delegate {
                recorder.EndRecording();
            });

            titleField.GetComponent<InputField>().onValueChanged.AddListener (delegate {
                TitleFieldChange();
            });

            recordInputField.GetComponent<InputField>().onValueChanged.AddListener(delegate {
                RecordInputFieldChange();
            });

            slackChannelField.GetComponent<InputField>().onValueChanged.AddListener(delegate {
                SlackChannelFieldChange();
            });

            recordTypeDropdown.onValueChanged.AddListener(delegate {
                RecordTypeChange();
            });

            privacyTypeDropdown.onValueChanged.AddListener(delegate {
                PrivacyTypeChange();
            });

            if (recorder.recordOnStart) {
                container.SetActive (true);
                RecordingState();
            } else {
                StartState ();
                container.SetActive (false);
            }
        }

        void Update()
        {
            if ((Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.LeftControl)) && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.R)) {
                container.SetActive(!container.activeSelf);
            }
        }

        private void TitleFieldChange()
        {
            recorder.videoName = titleField.GetComponent<InputField>().text;
            //recorder.SetVideoName(recorder.videoName);
        }

        private void PrivacyTypeChange()
        {
            recorder.privacyMode = GetPrivacyType();
            //recorder.SetVideoPrivacyMode(recorder.m_privacyMode);
        }

        private void SlackChannelFieldChange()
        {
            recorder.slackChannel = slackChannelField.GetComponent<InputField>().text;
        }

        // TODO: clean this logic up
        private int GetPrivacyTypeIndex(VimeoApi.PrivacyMode mode)
        {
            switch (mode) {
                case VimeoApi.PrivacyMode.anybody:
                      return 0;

                case VimeoApi.PrivacyMode.nobody:
                    return 1;

                default:
                case VimeoApi.PrivacyMode.unlisted:
                    return 2;

                case VimeoApi.PrivacyMode.disable:
                    return 3;
            }
        }

        private VimeoApi.PrivacyMode GetPrivacyType()
        {
            int index = privacyTypeDropdown.value;

            switch (privacyTypeDropdown.options [index].text) {
                case "Anyone":
                    return VimeoApi.PrivacyMode.anybody;

                case "Only me":
                    return VimeoApi.PrivacyMode.nobody;

                default:
                case "Only people with the private link":
                    return VimeoApi.PrivacyMode.unlisted;

                case "Hide this video from Vimeo.com":
                    return VimeoApi.PrivacyMode.disable;
            }
        }

        private void RecordInputFieldChange()
        {
        }

        private void RecordTypeChange()
        {
            int index = recordTypeDropdown.value;
            string txt = recordTypeDropdown.options[index].text;

            StartState();
        }

        private void UploadProgress(string status, float progress)
        {
            if (status == "Uploading" || status == "Verifying" || status == "Authorizing") {
                UploadingState();
            }
            else if (status == "Complete") {
                FinishState();
            }
            else if (status != "Cancelled") {
                RecordingState();
            }
            else {
                StartState();
            }
        }

        public void StartState()
        {
            recordButton.SetActive (true);
            recordTypeDropdown.gameObject.SetActive(false); // temporarily disabled

            recordInputField.SetActive (false);

            finishButton.SetActive (false);
            cancelButton.SetActive (false);
            recordStatus.SetActive (false);
            progressBar.SetActive (false);

            HideShareWindow();
        }

        private void RecordingState()
        {
            finishButton.SetActive (true);

            cancelButton.SetActive (true);
            recordStatus.SetActive (true);

            recordTypeDropdown.gameObject.SetActive(false);
            recordInputField.SetActive (false);
            recordButton.SetActive (false);
            progressBar.SetActive (false);

            HideShareWindow();
        }

        private void UploadingState()
        {
            progressBar.SetActive (true);

            recordTypeDropdown.gameObject.SetActive(false);
            recordInputField.SetActive (false);
            cancelButton.SetActive (false);
            finishButton.SetActive (false);
            recordStatus.SetActive (false);
            recordButton.SetActive (false);

            HideShareWindow();
        }

        private void FinishState()
        {
            progressBar.SetActive (false);
            recordTypeDropdown.gameObject.SetActive(false);
            recordInputField.SetActive (false);
            cancelButton.SetActive (false);
            finishButton.SetActive (false);
            recordStatus.SetActive (false);
            recordButton.SetActive (false);

            ShowShareWindow();
        }

        public void ShowShareWindow()
        {
            shareWindow.gameObject.SetActive (true);
        }

        public void HideShareWindow()
        {
            shareWindow.gameObject.SetActive (false);
        }

        void Destroy() {
            recordTypeDropdown.onValueChanged.RemoveAllListeners ();
            titleField.GetComponent<InputField> ().onValueChanged.RemoveAllListeners ();
        }

    }

}
#endif