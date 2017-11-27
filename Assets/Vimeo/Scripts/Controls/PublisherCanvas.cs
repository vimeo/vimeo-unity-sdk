using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Vimeo;


namespace Vimeo.Controls {
    public class PublisherCanvas : MonoBehaviour
    {
        private VimeoPublisher vimeoPublisher;

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
            vimeoPublisher = GetComponent<VimeoPublisher>();
            vimeoPublisher.OnUploadProgress += UploadProgress;

            titleField.GetComponent<InputField>().text = vimeoPublisher.videoName;
            slackChannelField.GetComponent<InputField>().text = vimeoPublisher.slackChannel;
            privacyTypeDropdown.value = GetPrivacyTypeIndex(vimeoPublisher.m_privacyMode);

			recordButton.GetComponent<Button>().onClick.AddListener(delegate {
                vimeoPublisher.BeginRecording();
            });

			cancelButton.GetComponent<Button>().onClick.AddListener(delegate {
                vimeoPublisher.CancelRecording();
            });

			finishButton.GetComponent<Button>().onClick.AddListener(delegate {
                vimeoPublisher.EndRecording();
            });

            titleField.GetComponent<InputField> ().onValueChanged.AddListener (delegate {
                TitleFieldChange();
            });

            recordInputField.GetComponent<InputField> ().onValueChanged.AddListener (delegate {
                RecordInputFieldChange();
            });

            slackChannelField.GetComponent<InputField> ().onValueChanged.AddListener (delegate {
                SlackChannelFieldChange();
            });

            recordTypeDropdown.onValueChanged.AddListener (delegate {
                RecordTypeChange();
            });

            privacyTypeDropdown.onValueChanged.AddListener (delegate {
                PrivacyTypeChange();
            });

            if (vimeoPublisher.recordOnStart) {
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
            vimeoPublisher.videoName = titleField.GetComponent<InputField> ().text;
            vimeoPublisher.SetVideoName(vimeoPublisher.videoName);
        }

        private void PrivacyTypeChange()
        {
            vimeoPublisher.m_privacyMode = GetPrivacyType();
            vimeoPublisher.SetVideoPrivacyMode(vimeoPublisher.m_privacyMode);
        }

        private void SlackChannelFieldChange()
        {
            vimeoPublisher.slackChannel = slackChannelField.GetComponent<InputField>().text;
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

