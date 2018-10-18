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

        public delegate void UploadEvent(VideoChunk chunk, string msg = "");
        public event UploadEvent OnChunkUploadComplete;
        public event UploadEvent OnChunkUploadError;

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

                yield return VimeoApi.SendRequest(uploadRequest);

                if (uploadRequest.isNetworkError || uploadRequest.isHttpError) {
                    OnChunkUploadError(this, "[Error] " + uploadRequest.error + " error code is: " + uploadRequest.responseCode);
                } else {
                    OnChunkUploadComplete(this, uploadRequest.GetResponseHeader("Upload-Offset"));
                }
            }
            DisposeBytes();
        }

        public void Upload()
        {
            StartCoroutine(SendTusRequest());
        }
    }
}