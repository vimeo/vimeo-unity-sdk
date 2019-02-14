using System.IO;
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Vimeo
{
    public class VideoChunk : MonoBehaviour
    {
        private long m_startByte;
        public long startByte
        {
            get
            {
                return m_startByte;
            }
        }
        private long m_lastByteUploaded;
        public long lastByteUploaded
        {
            get
            {
                return m_lastByteUploaded;
            }
            set
            {
                m_lastByteUploaded = value;
            }
        }

        private string m_url;
        public string url
        {
            get
            {
                return m_url;
            }
        }
        public byte[] bytes;
        private string m_filePath;
        public string filePath
        {
            get
            {
                return m_filePath;
            }
        }

        private int m_totalBytes;
        public int totalBytes
        {
            get
            {
                return m_totalBytes;
            }
        }

        private bool m_isUploading = false;
        public bool isUploading
        {
            get
            {
                return m_isUploading;
            }
            set
            {
                m_isUploading = value;
            }
        }
        private bool m_isFinishedUploading = false;
        public bool isFinishedUploading
        {
            get
            {
                return m_isFinishedUploading;
            }
            set
            {
                m_isFinishedUploading = value;
            }
        }

        public delegate void UploadEvent(VideoChunk chunk, string msg = "");
        public event UploadEvent OnChunkUploadComplete;
        public event UploadEvent OnChunkUploadError;

        private UnityWebRequest m_uploadRequest;
        public UnityWebRequest uploadRequest
        {
            get
            {
                return m_uploadRequest;
            }
        }

        private int maxRetries = 3;
        private int m_totalRetries = 0;
        public int totalRetries
        {
            get
            {
                return m_totalRetries;
            }
        }

        public void Init(int _startByte, string _url, string _filePath, int _totalBytes)
        {
            m_totalBytes = _totalBytes;
            m_filePath = _filePath;
            m_lastByteUploaded = m_startByte = _startByte;
            m_url = _url;
        }

        public void ReadBytes()
        {
            bytes = new byte[m_totalBytes - (lastByteUploaded - startByte)];
            using (BinaryReader reader = new BinaryReader(new FileStream(filePath, FileMode.Open, FileAccess.Read))) {
                reader.BaseStream.Seek(m_lastByteUploaded, SeekOrigin.Begin);
                reader.Read(bytes, 0, bytes.Length);
            }
        }

        public void DisposeBytes()
        {
            Array.Clear(bytes, 0, bytes.Length);
        }

        private IEnumerator SendTusRequest()
        {
            ReadBytes();

            m_uploadRequest = UnityWebRequest.Put(m_url, bytes);
            SetupTusRequest(m_uploadRequest);
            m_uploadRequest.method = "PATCH";
            m_uploadRequest.SetRequestHeader("Upload-Offset", m_lastByteUploaded.ToString());
            m_uploadRequest.SetRequestHeader("Content-Type", "application/offset+octet-stream");

            m_isUploading = true;

            yield return VimeoApi.SendRequest(m_uploadRequest);

            if (VimeoApi.IsNetworkError(m_uploadRequest)) {
                UploadError(m_uploadRequest.responseCode + ": " + m_uploadRequest.error);
            } else {
                m_isUploading = false;
                m_isFinishedUploading = true;
                if (OnChunkUploadComplete != null) {
                    OnChunkUploadComplete(this, m_uploadRequest.GetResponseHeader("Upload-Offset"));
                }
            }

            DisposeBytes();
        }

        public void UploadError(string msg)
        {
            if (m_totalRetries >= maxRetries) {
                m_isUploading = false;
                Debug.LogError("[VideoChunk] " + msg);

                if (OnChunkUploadError != null) {
                    OnChunkUploadError(this, msg);
                }
            } else {
                Debug.LogWarning("[VideoChunk] " + msg + " - Retrying...");
                m_totalRetries++;
                StartCoroutine(ResumeUpload());
            }
        }

        private IEnumerator ResumeUpload()
        {
            yield return new WaitForSeconds(5); // Wait a bit before trying again so there is time to reconnect

            // Make a request to check what the last upload offset is...
            UnityWebRequest uploadCheck = UnityWebRequest.Get(m_url);
            uploadCheck.method = "HEAD";
            SetupTusRequest(uploadCheck);

            yield return VimeoApi.SendRequest(uploadCheck);

            if (uploadCheck.GetResponseHeader("Upload-Offset") != null) {
                m_lastByteUploaded = long.Parse(uploadCheck.GetResponseHeader("Upload-Offset"));
            }

            uploadCheck.Dispose();
            m_uploadRequest.Dispose();
            m_uploadRequest = null;
            DisposeBytes();
            Upload();
        }

        private void SetupTusRequest(UnityWebRequest r)
        {
            r.chunkedTransfer = false;
            r.SetRequestHeader("Tus-Resumable", "1.0.0");
            r.SetRequestHeader("Accept", "application/vnd.vimeo.*+json;version=3.4");
        }

        public ulong GetBytesUploaded()
        {
            if (m_isUploading && m_uploadRequest != null) {
                return m_uploadRequest.uploadedBytes;
            } else if (m_isFinishedUploading) {
                return (ulong)totalBytes;
            }
            return 0;
        }

        public void Upload()
        {
            StartCoroutine(SendTusRequest());
        }
    }
}