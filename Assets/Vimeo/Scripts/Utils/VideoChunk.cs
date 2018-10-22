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
        private int m_indexByte;
        public int indexByte {
            get {
                return m_indexByte;
            }
        }
        private string m_url;
        public string url {
            get {
                return m_url;
            }
        }
        public byte[] bytes;
        private string m_filePath;
        public string filePath {
            get {
                return m_filePath;
            }
        }
        
        private int m_chunkSize;
        public int chunkSize {
            get {
                return m_chunkSize;
            }
        }

        private bool m_isUploadingChunk = false;
        public bool isUploadingChunk {
            get {
                return m_isUploadingChunk;
            }
        }
        private bool m_isFinishedUploading = false;
        public bool isFinishedUploading {
            get {
                return m_isFinishedUploading;
            }
        }

        public delegate void UploadEvent(VideoChunk chunk, string msg = "");
        public event UploadEvent OnChunkUploadComplete;
        public event UploadEvent OnChunkUploadError;

        private UnityWebRequest m_chunkUploadRequest;
        public UnityWebRequest chunkUploadRequest {
            get {
                return m_chunkUploadRequest;
            }
        }

        public void Init(int _indexByte, string _url, string _filePath, int _chunkSize)
        {
            m_chunkSize = _chunkSize;
            m_filePath = _filePath;
            m_indexByte = _indexByte;
            m_url = _url;
            bytes = new byte[m_chunkSize];
        }

        public void ReadBytes()
        {
            using (BinaryReader reader = new BinaryReader(new FileStream(filePath, FileMode.Open, FileAccess.Read))) {
                reader.BaseStream.Seek(m_indexByte, SeekOrigin.Begin);
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
            using (UnityWebRequest uploadRequest = UnityWebRequest.Put(m_url, bytes)) {
                uploadRequest.chunkedTransfer = false;
                uploadRequest.method = "PATCH";
                uploadRequest.SetRequestHeader("Tus-Resumable", "1.0.0");
                uploadRequest.SetRequestHeader("Upload-Offset", (m_indexByte).ToString());
                uploadRequest.SetRequestHeader("Content-Type", "application/offset+octet-stream");
                m_chunkUploadRequest = uploadRequest;
                m_isUploadingChunk = true;

                yield return VimeoApi.SendRequest(uploadRequest);

                if (uploadRequest.isNetworkError || uploadRequest.isHttpError) {
                    m_isUploadingChunk = false;
                    if (OnChunkUploadError != null) {
                        OnChunkUploadError(this, "[Error] " + uploadRequest.error + " error code is: " + uploadRequest.responseCode);
                    }
                } else {
                    m_isUploadingChunk = false;
                    m_isFinishedUploading = true;
                    if (OnChunkUploadComplete != null) {
                        OnChunkUploadComplete(this, uploadRequest.GetResponseHeader("Upload-Offset"));
                    }
                }
            }
            DisposeBytes();
        }

        public ulong GetBytesUploaded()
        {
            if (m_isUploadingChunk) {
                return m_chunkUploadRequest.uploadedBytes;
            } else if (m_isFinishedUploading) {
                return (ulong)m_chunkSize;
            }
            return 0;
        }

        public void Upload()
        {
            StartCoroutine(SendTusRequest());
        }

        public bool areEventsRegistered()
        {
            return OnChunkUploadError != null && OnChunkUploadComplete != null;
        }
    }
}