using System.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using SimpleJSON;

namespace Vimeo 
{
    class VideoChunk : MonoBehaviour
    {
        private int indexByte;
        private string url;
        private byte[] bytes;
        private int chuckSize;
        public delegate void UploadEvent(string response);
        public event UploadEvent OnChunckUploadComplete;
        public event UploadEvent OnChunckUploadError;

        public VideoChunk(int _indexByte, string _url, int _chucnkSize)
        {
            bytes = new byte[_chucnkSize];
            indexByte = _indexByte;
            url = _url;
        }

        private void ReadBytes()
        {
            using (BinaryReader reader = new BinaryReader(new FileStream(url, FileMode.Open))) {
                reader.BaseStream.Seek(indexByte, SeekOrigin.Begin);
                reader.Read(bytes, 0, bytes.Length);
            }
        }

        private void DisposeBytes()
        {
            Array.Clear(bytes, 0, bytes.Length);
            OnChunckUploadComplete("Finished uploading chunck");
        }

        private IEnumerator SendTusRequest()
        {
            ReadBytes();
            using (UnityWebRequest uploadRequest = UnityWebRequest.Put(url, bytes)) {
                        uploadRequest.chunkedTransfer = false;
                        uploadRequest.method = "PATCH";
                        uploadRequest.SetRequestHeader("Tus-Resumable", "1.0.0");
                        uploadRequest.SetRequestHeader("Upload-Offset", indexByte.ToString());
                        uploadRequest.SetRequestHeader("Content-Type", "application/offset+octet-stream");

                        yield return VimeoApi.SendRequest(uploadRequest);

                        if(uploadRequest.isNetworkError || uploadRequest.isHttpError) {
                            Debug.Log("[Error] " + uploadRequest.error + " error code is: " + uploadRequest.responseCode);
                        } else {
                            Debug.Log("[Vimeo] Tus ticket request complete with response code " + uploadRequest.responseCode);
                            Debug.Log(uploadRequest.downloadHandler.text);   
                        }
            }
            DisposeBytes();
        }

        public void Upload()
        {
            StartCoroutine(SendTusRequest());
        }

    }
    class VimeoUploader : MonoBehaviour
    {

        private Queue<VideoChunk> myChunks;
        public int maxChunkSize;
        
        public VimeoUploader(string file, string token)
        {

            FileInfo fileInfo = new FileInfo(file);
            
            StartCoroutine(
                Init(fileInfo, token)
            );

        }

        public IEnumerator Init(FileInfo _file_info, string _token)
        {
            string tusResourceRequestBody = "{ \"upload\": { \"approach\": \"tus\", \"size\": \"" + _file_info.Length.ToString() + "\" } }";

            using (UnityWebRequest request = UnityWebRequest.Put("https://api.vimeo.com/me/videos", tusResourceRequestBody)) {
                //Prep headers
                request.chunkedTransfer = false;
                request.method = "POST";
                request.SetRequestHeader("Authorization", "bearer " + _token);
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Accept", "application/vnd.vimeo.*+json;version=3.4");

                yield return VimeoApi.SendRequest(request);

                if(request.isNetworkError || request.isHttpError) {
                    OnError("[Error] " + request.error + " error code is: " + request.responseCode);
                } else {

                    JSONNode rawJSON = JSON.Parse(request.downloadHandler.text);
                    string tusUploadLink = rawJSON["upload"]["upload_link"].Value;
                    Debug.Log("[Vimeo] Secure tus upload link is: " + tusUploadLink);
                    
                    //Create the chunks
                    float chunkFileRatio = (int)_file_info.Length / maxChunkSize;
                    int numChunks = (int)Mathf.Ceil(chunkFileRatio);

                    for (int i = 0; i < numChunks; i++){

                        int indexByte = ((int)_file_info.Length / numChunks) * i;
                        VideoChunk chunk = new VideoChunk(indexByte, tusUploadLink, maxChunkSize);

                        //Register evenets
                        chunk.OnChunckUploadComplete += OnCompleteChunk;
                        
                        //Push it to the queue
                        myChunks.Enqueue(chunk);

                    }

                }
            }

        }

        public void SetChunkSize(int size)
        {
            maxChunkSize = size;
        }

        public void Upload()
        {

        }

        private void OnCompleteChunk(string msg)
        {

        }

        private void OnChunkError(string err)
        {

        }

        private void OnError(string err)
        {

        }
    }
}