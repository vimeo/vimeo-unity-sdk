using System.IO;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using SimpleJSON;
using Vimeo;

namespace Vimeo
{
    public class VimeoUploader : VimeoApi
    {
        public delegate void ChunkUploadEvent(VideoChunk chunk, string msg = "");
        public event ChunkUploadEvent OnChunckUploadComplete;
        public event ChunkUploadEvent OnChunckUploadError;
        public delegate void UploadAction(string status, float progress);
        public event UploadAction OnUploadProgress;
        public event RequestAction OnUploadComplete;

        private List<VideoChunk> m_chunks;
        public List<VideoChunk> chunks {
            get {
                return m_chunks;
            }
        }
        private string m_file;
        public string file {
            get {
                return m_file;
            }
        }
        private string m_vimeoUrl;
        public string vimeo_url {
            get {
                return m_vimeoUrl;
            }
        }
        private FileInfo m_fileInfo;
        public FileInfo fileInfo {
            get {
                return m_fileInfo;
            }
        }
        private bool isUploading = false;
        // private int m_concurent_chunks = 4; Not used
        private int m_maxChunkSize;
        public int maxChunkSize {
            get {
                return m_maxChunkSize;
            }
        }
        private int m_numChunks;
        public int numChunks {
            get {
                return m_numChunks;
            }
        }
        private int currentChunkIndex = 0;
        private VideoChunk lastChunk;
        public float progressIntervalTime = 0.5f;
        private float savedTime = 0.0f;

        public void Init(string _token, int _maxChunkByteSize = 1024 * 1024 * 128)
        {
            currentChunkIndex = 0;
            m_chunks = new List<VideoChunk>();
            token = _token;
            m_maxChunkSize = _maxChunkByteSize;
        }

        private void Update() 
        {
            if (isUploading) {
                if ((Time.time - savedTime) > progressIntervalTime ) {
                    OnUploadProgress("Uploading", (float)GetTotalBytesUploaded() / (float)fileInfo.Length);

                    savedTime = Time.time; 
                }
            }
            
        }

        private void RequestComplete(string response)
        {
            OnRequestComplete -= RequestComplete;

            JSONNode rawJSON = JSON.Parse(response);

            string tusUploadLink = rawJSON["upload"]["upload_link"].Value;
            m_vimeoUrl = rawJSON["link"].Value;
            CreateChunks(m_fileInfo, tusUploadLink);

            VideoChunk currentChunk = m_chunks[currentChunkIndex];
            RegisterChunkEvents(currentChunk);
            currentChunk.Upload();
        }

        public void Upload(string _file)
        {
            m_file = _file;
            m_fileInfo = new FileInfo(m_file);

            OnRequestComplete += RequestComplete;
            StartCoroutine(RequestTusResource("me/videos", m_fileInfo.Length));
            isUploading = true;
        }

        private void OnCompleteChunk(VideoChunk chunk, string latestUploadedByte)
        {
            if (OnChunckUploadComplete != null) {
                OnChunckUploadComplete(chunk, latestUploadedByte);
            }

            if (HasChunksLeftToUpload()){
                UploadNextChunk();
            } else {
                isUploading = false;
                ClearAllChunks();

                if (OnUploadProgress != null) {
                    OnUploadProgress("UploadComplete", 1f);
                }
                if (OnUploadComplete != null) {    
                    OnUploadComplete(m_vimeoUrl);
                }
            }
        }

        public ulong GetTotalBytesUploaded()
        {
            ulong sum = 0;
            for (int i = 0; i < m_chunks.Count; i++) {
                sum += m_chunks[i].GetBytesUploaded();
            }
            return sum;
        }

        public void ClearAllChunks()
        {
            m_chunks.Clear();
        }

        private void RegisterChunkEvents(VideoChunk chunk)
        {
            chunk.OnChunkUploadComplete += OnCompleteChunk;
            chunk.OnChunkUploadError += OnChunkError;
        }

        private void DisposeChunkEvents(VideoChunk chunk)
        {
            chunk.OnChunkUploadComplete -= OnCompleteChunk;
            chunk.OnChunkUploadError -= OnChunkError;
        }

        private void OnChunkError(VideoChunk chunk, string err)
        {
            if (OnChunckUploadError != null && chunk != null) {
                OnChunckUploadError(chunk, err);
            }
        }

        public void CreateChunks(FileInfo fileInfo, string tusUploadLink)
        {
            //Create the chunks
            m_numChunks = (int)Mathf.Ceil((float)fileInfo.Length / (float)m_maxChunkSize);

            for (int i = 0; i < m_numChunks; i++) {
                int indexByte = m_maxChunkSize * i;
                VideoChunk chunk = gameObject.AddComponent<VideoChunk>();
                chunk.hideFlags = HideFlags.HideInInspector;

                //If we are at the last chunk set the max chunk size to the fractional remainder
                if (i == m_numChunks - 1) {
                    int remainder = (int)fileInfo.Length - (m_maxChunkSize * i);
                    chunk.Init(indexByte, tusUploadLink, fileInfo.FullName, remainder);
                } else {
                    chunk.Init(indexByte, tusUploadLink, fileInfo.FullName, m_maxChunkSize);
                }
                m_chunks.Add(chunk);
            }
        }

        public void UploadNextChunk()
        {
            if (lastChunk != null) {
                DisposeChunkEvents(lastChunk);
            }

            currentChunkIndex++;

            VideoChunk currentChunk = m_chunks[currentChunkIndex];
            RegisterChunkEvents(currentChunk);

            currentChunk.Upload();

            //Store the reference to latest uploaded chunk to de-register events
            lastChunk = currentChunk;
        }

        private bool HasChunksLeftToUpload()
        {
           return TotalChunksRemaining() > 0;
        }

        private int TotalChunksRemaining()
        {
            int remainingChunks = m_chunks.Count;
            for (int i = 0; i < m_chunks.Count; i++) {
                if (m_chunks[i].isFinishedUploading) {
                    remainingChunks--;
                }
            }
            return remainingChunks;
        }
    }
}