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

        private Queue<VideoChunk> m_chunks;
        public Queue<VideoChunk> chunks {
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
        private string m_vimeo_url;
        public string vimeo_url {
            get {
                return m_vimeo_url;
            }
        }
        private FileInfo m_file_info;
        public FileInfo file_info {
            get {
                return m_file_info;
            }
        }
        private int m_concurent_chunks = 4;
        private int m_max_chunk_size;
        public int max_chunk_size {
            get {
                return m_max_chunk_size;
            }
        }
        private int m_num_chunks;
        public int num_chunks {
            get {
                return m_num_chunks;
            }
        }

        private void Start()
        {
            this.hideFlags = HideFlags.HideInInspector;
        }

        public void Init(string _token, int _maxChunkSize = 20000)
        {
            m_chunks = new Queue<VideoChunk>();
            token = _token;
            InitApi();
            OnRequestComplete += RequestComplete;
            m_max_chunk_size = _maxChunkSize;
        }

        private void RequestComplete(string response)
        {
            string tusUploadLink = VimeoUploader.GetTusUploadLink(response);
            m_vimeo_url = GetVideoPermlink(response);
            CreateChunks(m_file, m_file_info, tusUploadLink);

            VideoChunk firstChunk = m_chunks.Dequeue();
            firstChunk.Upload();
        }

        public void Upload(string _file)
        {
            m_file = _file;
            m_file_info = new FileInfo(m_file);
            StartCoroutine(RequestTusResource("me/videos", m_file_info.Length));
        }

        private void OnCompleteChunk(VideoChunk chunk, string msg)
        {
            //Emit the event
            if (OnChunckUploadComplete != null) {
                OnChunckUploadComplete(chunk, msg);
            }

            //Destroy the chunk
            Destroy(chunk);
            
            //And upload the next one
            UploadNextChunk();
        }

        private void OnChunkError(VideoChunk chunk, string err)
        {
            if (OnChunckUploadError != null) {
                OnChunckUploadError(chunk, err);
            }
        }

        private void CreateChunks(string filePath, FileInfo fileInfo, string tusUploadLink)
        {
            //Create the chunks
            m_num_chunks = (int)Mathf.Ceil((int)fileInfo.Length / m_max_chunk_size);

            for (int i = 0; i < m_num_chunks; i++) {
                int indexByte = m_max_chunk_size * i;
                VideoChunk chunk = this.gameObject.AddComponent<VideoChunk>();
                chunk.hideFlags = HideFlags.HideInInspector;

                //If we are at the last chunk set the max chunk size to the fractional remainder
                if (i + 1 == m_num_chunks) {
                    int remainder = (int)fileInfo.Length - (m_max_chunk_size * i);
                    chunk.Init(indexByte, tusUploadLink, filePath, remainder);
                } else {
                    chunk.Init(indexByte, tusUploadLink, filePath, m_max_chunk_size);
                }

                chunk.OnChunkUploadComplete += OnCompleteChunk;
                chunk.OnChunkUploadError += OnChunkError;
                m_chunks.Enqueue(chunk);

            }
        }

        private void UploadNextChunk()
        {
            //Make sure the queue is not empty
            if (m_chunks.Count != 0) {
                VideoChunk currentChunk = m_chunks.Dequeue();
                
                float progress = ((float)m_chunks.Count / (float)m_num_chunks) * -1.0f + 1.0f;
                if (OnUploadProgress != null) {
                    OnUploadProgress("Uploading", progress);
                }
                currentChunk.Upload();
            } else {
                if (OnUploadProgress != null) {
                    OnUploadProgress("UploadComplete", 1f);
                }
                if (OnUploadComplete != null) {
                    OnUploadComplete(m_vimeo_url);
                }
            }
        }

        private static string GetTusUploadLink(string response)
        {
            JSONNode rawJSON = JSON.Parse(response);
            return rawJSON["upload"]["upload_link"].Value;
        }

        public static string GetVideoPermlink(string response)
        {
            JSONNode rawJSON = JSON.Parse(response);
            return rawJSON["link"].Value;
        }
    }
}