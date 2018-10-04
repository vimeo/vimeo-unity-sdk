using System.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using SimpleJSON;

/*
    TODO:
    - Format the whole code
    - Get rid of unused namespaces
    - Split functionality into seprate methods
    - Split VideoChunk and VimeoUploader to seprate files
*/

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
            using (BinaryReader reader = new BinaryReader(new FileStream(file_path, FileMode.Open))) {
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

                        if(uploadRequest.isNetworkError || uploadRequest.isHttpError) {
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
    class VimeoUploader : MonoBehaviour
    {

        private Queue<VideoChunk> myChunks;
        public int maxChunkSize;

        public IEnumerator Init(string _file, string _token)
        {
            myChunks = new Queue<VideoChunk>();

            maxChunkSize = 10000;

            FileInfo fileInfo = new FileInfo(_file);

            string tusResourceRequestBody = "{ \"upload\": { \"approach\": \"tus\", \"size\": \"" + fileInfo.Length.ToString() + "\" } }";

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
                    int numChunks = (int)Mathf.Ceil((int)fileInfo.Length / maxChunkSize);
                    Debug.Log("Number of chunks is " + numChunks + " and file size is " + fileInfo.Length.ToString());
                    for (int i = 0; i < numChunks; i++){
                        int indexByte = maxChunkSize * i;
                        VideoChunk chunk = this.gameObject.AddComponent<VideoChunk>();

                        //If we are at the last chunk set the max chunk size to the fractional remainder
                        if (i + 1 == numChunks) {
                            int remainder = (int)fileInfo.Length - (maxChunkSize * i);
                            Debug.Log("Created last chunk and the remainder is: " + remainder);
                            chunk.Init(indexByte, tusUploadLink, _file, remainder);
                        } else {
                            chunk.Init(indexByte, tusUploadLink, _file, maxChunkSize);
                        }
                        
                        //Register evenets
                        chunk.OnChunckUploadComplete += OnCompleteChunk;
                        chunk.OnChunckUploadError += OnChunkError;

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
            //Kick off the first chunk, currently will call other chunks via the OnCompleteChunk event
            VideoChunk firstChunk = myChunks.Dequeue();
            firstChunk.Upload();
        }

        private void OnCompleteChunk(VideoChunk chunk, string msg)
        {
            Debug.Log(myChunks.Count);
            //Make sure the queue is not empty
            if (myChunks.Count != 0) {
                VideoChunk nextChunk = myChunks.Dequeue();
                nextChunk.Upload();
            }
        }

        private void OnChunkError(VideoChunk chunk, string err)
        {

        }

        private void OnError(string err)
        {

        }
    }
}