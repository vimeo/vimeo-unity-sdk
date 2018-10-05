using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using SimpleJSON;
using Vimeo;

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
        public VimeoApi vimeoApi;
        //TODO: In the future this will be stored in a hash table to provide batch uploading
        private string file;
        private string vimeo_url;
        private FileInfo fileInfo;


        //Public members
        public int maxChunkSize;
        public int numChunks;
        private void Start()
        {
            this.hideFlags = HideFlags.HideInInspector;
        }

        public void Init(string _token, int _maxChunkSize = 1000)
        {
            //A queue of video chunks to upload
            myChunks = new Queue<VideoChunk>();

            //Instantiate the Vimeo Api
            if (vimeoApi == null)
            {
                vimeoApi = gameObject.AddComponent<VimeoApi>();
                vimeoApi.OnError += ApiError;
                vimeoApi.OnNetworkError += NetworkError;
                vimeoApi.OnRequestComplete += RequestComplete;

                vimeoApi.token = _token;
            }

            maxChunkSize = _maxChunkSize;

        }
        private void RequestComplete(string response)
        {
            string tusUploadLink = GetTusUploadLink(response);
            vimeo_url = GetVideoPermlink(response);
            CreateChunks(file, fileInfo, tusUploadLink);

            //Kick off first chunks, others will be called on OnCompleteChunk()
            VideoChunk firstChunk = myChunks.Dequeue();
            firstChunk.Upload();
        }
        public void SetChunkSize(int size)
        {
            maxChunkSize = size;
        }
        private void ApiError(string response)
        {

        }
        private void NetworkError(string response)
        {

        }
        public void Upload(string _file)
        {
            file = _file;
            fileInfo = new FileInfo(_file);

            //Send the request, response will be catched in the RequestComplete() method
            StartCoroutine(
                vimeoApi.RequestTusResource("me/videos", fileInfo.Length)
            );
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
                float progres = ((float)myChunks.Count / (float)numChunks) * -1.0f + 1.0f;
                OnUploadProgress("Uploading", progres);
                Debug.Log(progres);
                nextChunk.Upload();
            } else
            {
                //Set the progress back to 0
                if (OnUploadProgress != null)
                {
                    OnUploadProgress("Idle", 0.0f);
                }
                //Emit upload complete
                if (OnUploadComplete != null)
                {
                    OnUploadComplete(vimeo_url);
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
                } else
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
        private string GetTusUploadLink(string response)
        {
            JSONNode rawJSON = JSON.Parse(response);
            return rawJSON["upload"]["upload_link"].Value;
        }
        private string GetVideoPermlink(string response)
        {
            JSONNode rawJSON = JSON.Parse(response);
            return rawJSON["link"].Value;
        }
    }
}