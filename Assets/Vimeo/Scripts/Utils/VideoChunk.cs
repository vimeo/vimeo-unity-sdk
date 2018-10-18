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
        private int m_index_byte;
        public int index_byte {
            get {
                return m_index_byte;
            }
        }
        private string m_url;
        public string url {
            get {
                return m_url;
            }
        }
        public byte[] bytes;
        private string m_file_path;
        public string file_path {
            get {
                return m_file_path;
            }
        }
        
        private int m_chunk_size;
        public int chunk_size {
            get {
                return m_chunk_size;
            }
        }

        public delegate void UploadEvent(VideoChunk chunk, string msg = "");
        public event UploadEvent OnChunkUploadComplete;
        public event UploadEvent OnChunkUploadError;

        public void Init(int _indexByte, string _m_url, string _file_path, int _chunkSize)
        {
            m_chunk_size = _chunkSize;
            m_file_path = _file_path;
            m_index_byte = _indexByte;
            m_url = _m_url;
            bytes = new byte[m_chunk_size];
        }

        public void ReadBytes()
        {
            using (BinaryReader reader = new BinaryReader(new FileStream(file_path, FileMode.Open, FileAccess.Read))) {
                reader.BaseStream.Seek(m_index_byte, SeekOrigin.Begin);
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
                uploadRequest.SetRequestHeader("Upload-Offset", (m_index_byte).ToString());
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