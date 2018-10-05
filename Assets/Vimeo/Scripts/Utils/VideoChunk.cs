using System.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Vimeo
{
    class VideoChunk : MonoBehaviour
    {
        private int indexByte;
        private string url;
        private byte[] bytes;
        private string file_path;
        private int chuckSize;

        public delegate void UploadEvent(VideoChunk chunk, string msg = "");
        public event UploadEvent OnChunckUploadComplete;
        public event UploadEvent OnChunckUploadError;

        public void Init(int _indexByte, string _url, string _file_path, int _chucnkSize)
        {
            bytes = new byte[_chucnkSize];
            file_path = _file_path;
            indexByte = _indexByte;
            url = _url;
        }

        private void ReadBytes()
        {
            using (BinaryReader reader = new BinaryReader(new FileStream(file_path, FileMode.Open, FileAccess.Read))) {
                reader.BaseStream.Seek(indexByte, SeekOrigin.Begin);
                reader.Read(bytes, 0, bytes.Length);
            }
        }

        private void DisposeBytes()
        {
            Array.Clear(bytes, 0, bytes.Length);
        }

        private IEnumerator SendTusRequest()
        {
            ReadBytes();
            using (UnityWebRequest uploadRequest = UnityWebRequest.Put(url, bytes)) {
                uploadRequest.chunkedTransfer = false;
                uploadRequest.method = "PATCH";
                uploadRequest.SetRequestHeader("Tus-Resumable", "1.0.0");
                uploadRequest.SetRequestHeader("Upload-Offset", (indexByte).ToString());
                uploadRequest.SetRequestHeader("Content-Type", "application/offset+octet-stream");

                yield return VimeoApi.SendRequest(uploadRequest);

                if (uploadRequest.isNetworkError || uploadRequest.isHttpError) {
                    string concatErr = "[Error] " + uploadRequest.error + " error code is: " + uploadRequest.responseCode;
                    OnChunckUploadError(this, concatErr);
                } else {
                    OnChunckUploadComplete(this, uploadRequest.GetResponseHeader("Upload-Offset"));
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