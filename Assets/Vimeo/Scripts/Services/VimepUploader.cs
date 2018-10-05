using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using SimpleJSON;

namespace Vimeo
{
    class VimeoUploader : MonoBehaviour
    {
        //Events
        public delegate void UploadAction(string status, float progress = 0.0f);
        public event UploadAction OnUploadProgress;
        public event UploadAction OnUploadError;
        public event UploadAction OnUploadComplete;

        public delegate void UploadEvent(VideoChunk chunk, string msg = "");
        public event UploadEvent OnChunckUploadComplete;
        public event UploadEvent OnChunckUploadError;

        //Private members
        private Queue<VideoChunk> myChunks;


        //Public members
        public int maxChunkSize;
        public int numChunks;

        public IEnumerator Init(string _file, string _token, int _maxChunkSize = 1000)
        {
            myChunks = new Queue<VideoChunk>();
            maxChunkSize = _maxChunkSize;
            FileInfo fileInfo = new FileInfo(_file);
            Debug.Log("File size in bytes" + fileInfo.Length.ToString());
            string tusResourceRequestBody = "{ \"upload\": { \"approach\": \"tus\", \"size\": \"" + fileInfo.Length.ToString() + "\" } }";

            using (UnityWebRequest request = UnityWebRequest.Put("https://api.vimeo.com/me/videos", tusResourceRequestBody))
            {
                UnityWebRequest videoResource = PrepareTusResourceRequest(request, _token);

                yield return VimeoApi.SendRequest(videoResource);

                if (videoResource.isNetworkError || videoResource.isHttpError)
                {
                    if (OnUploadError != null)
                    {
                        OnUploadError(videoResource.error + " error code is: " + videoResource.responseCode);
                    }
                }
                else
                {

                    string tusUploadLink = GetTusUploadLink(videoResource);

                    CreateChunks(_file, fileInfo, tusUploadLink);
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
            if (OnUploadProgress != null)
            {
                OnUploadProgress("Uploading", (float)numChunks / myChunks.Count);
            }
            VideoChunk firstChunk = myChunks.Dequeue();
            firstChunk.Upload();
        }

        private void OnCompleteChunk(VideoChunk chunk, string msg)
        {
            //Emit the event
            if (OnChunckUploadComplete != null)
            {
                OnChunckUploadComplete(chunk, msg);
            }

            //Destroy the chunk
            Destroy(chunk);

            //Make sure the queue is not empty
            if (myChunks.Count != 0)
            {
                VideoChunk nextChunk = myChunks.Dequeue();
                // OnUploadProgress("Uploading", (float)numChunks / myChunks.Count);
                nextChunk.Upload();
            }
            else
            {
                //Set the progress back to 0
                if (OnUploadProgress != null)
                {
                    OnUploadProgress("Idle", 0.0f);
                }
                //Emit upload complete
                if (OnUploadComplete != null)
                {
                    OnUploadComplete("Completed upload!");
                }
            }
        }

        private void OnChunkError(VideoChunk chunk, string err)
        {
            if (OnChunckUploadError != null)
            {
                OnChunckUploadError(chunk, err);
            }
        }
        private void CreateChunks(string filePath, FileInfo fileInfo, string tusUploadLink)
        {
            //Create the chunks
            numChunks = (int)Mathf.Ceil((int)fileInfo.Length / maxChunkSize);

            for (int i = 0; i < numChunks; i++)
            {
                int indexByte = maxChunkSize * i;
                VideoChunk chunk = this.gameObject.AddComponent<VideoChunk>();
                chunk.hideFlags = HideFlags.HideInInspector;

                //If we are at the last chunk set the max chunk size to the fractional remainder
                if (i + 1 == numChunks)
                {
                    int remainder = (int)fileInfo.Length - (maxChunkSize * i);
                    Debug.Log("Created last chunk and the remainder is: " + remainder);
                    chunk.Init(indexByte, tusUploadLink, filePath, remainder);
                }
                else
                {
                    chunk.Init(indexByte, tusUploadLink, filePath, maxChunkSize);
                }

                //Register evenets
                chunk.OnChunckUploadComplete += OnCompleteChunk;
                chunk.OnChunckUploadError += OnChunkError;

                //Push it to the queue
                myChunks.Enqueue(chunk);

            }
        }
        private UnityWebRequest PrepareTusResourceRequest(UnityWebRequest req, string token)
        {
            //Prep headers
            req.chunkedTransfer = false;
            req.method = "POST";
            req.SetRequestHeader("Authorization", "bearer " + token);
            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("Accept", "application/vnd.vimeo.*+json;version=3.4");

            return req;
        }
        private string GetTusUploadLink(UnityWebRequest response)
        {
            JSONNode rawJSON = JSON.Parse(response.downloadHandler.text);
            return rawJSON["upload"]["upload_link"].Value;
        }
    }
}