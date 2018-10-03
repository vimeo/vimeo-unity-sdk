using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using SimpleJSON;

namespace Vimeo 
{
    class VideoChunk
    {
        private int indexByte;
        private string url;
        private byte[] bytes;
        public delegate void UploadEvent(string response);
        public event UploadEvent OnChunckUploadComplete;
        public event UploadEvent OnChunckUploadError;

        public VideoChunk(int _indexByte, string _url)
        {
            indexByte = _indexByte;
            url = _url;
        }

        private void ReadBytes()
        {

        }

        private void DisposeBytes()
        {

            OnChunckUploadComplete("Finished uploading chunck");
        }

        private void SendTusRequest()
        {
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
        }

        public void Upload()
        {
            ReadBytes();
            SendTusRequest();
            DisposeBytes();
            
        }

    }
    class VimeoUploader : MonoBehaviour
    {

        private List<VideoChunk> myChunks;
        public int maxChunkSize;
        
        VimeoUploader(string file, string token)
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
                    Debug.LogError("[Error] " + request.error + " error code is: " + request.responseCode);
                } else {

                    JSONNode rawJSON = JSON.Parse(request.downloadHandler.text);
                    string tusUploadLink = rawJSON["upload"]["upload_link"].Value;
                    Debug.Log("[Vimeo] Secure tus upload link is: " + tusUploadLink);
                    
                    //Create the chunks
                    float chunkFileRatio = (int)_file_info.Length / maxChunkSize;
                    int numChunks = (int)Mathf.Ceil(chunkFileRatio);

                    for (int i = 0; i < numChunks; i++){

                        int indexByte = ((int)_file_info.Length / numChunks) * i;
                        VideoChunk chunk = new VideoChunk(indexByte, tusUploadLink);
                        chunk.OnChunckUploadComplete += OnCompleteChunk;
                        myChunks.Add(chunk);

                    }

                }
            }

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
    }
}