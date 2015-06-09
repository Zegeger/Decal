using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Xml.Serialization;
using System.Threading;
using Zegeger.Diagnostics;

namespace ZegegerLib
{
    public class UploadDataResults
    {
        public bool UploadSuccess { get; private set; }

        public UploadDataResults (bool success)
        {
            UploadSuccess = success;
        }
    }

    public class UploadDataAsyncResult : AsyncResult
    {
        private UploadDataResults _results;
        private string _uri;
        private byte[] _file;

        public UploadDataAsyncResult(string uri, byte[] file, AsyncCallback callback, object asyncState) : base(callback, asyncState)
        {
            _results = new UploadDataResults(false);
            _uri = uri;
            _file = file;
        }

        public void Begin()
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(_uri);
            request.Method = "POST";
            request.ContentType = "application/octet-stream";
            Stream requestStream = request.GetRequestStream();

            requestStream.Write(_file, 0, _file.Length);
            requestStream.Close();

            request.BeginGetResponse(result =>
            {
                try
                {
                    HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(result);
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        Stream responseStream = response.GetResponseStream();
                        StreamReader reader = new StreamReader(responseStream);
                        string strbody = reader.ReadToEnd();
                        TraceLogger.Write("Response body: " + strbody, TraceLevel.Verbose);
                        _results = new UploadDataResults(true);
                    }
                }
                catch (Exception ex)
                {
                    ThreadException = ex;
                }
                base.CompletedProcess();
            }
            , null);
        }

        public UploadDataResults End()
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            base.InternalEnd();
            TraceLogger.Write("Exit", TraceLevel.Verbose);
            return _results;
        }
    }

    class Uploader
    {
        string _uri;

        public Uploader(string uri)
        {
            _uri = uri;
        }

        public IAsyncResult BeginUploadFile(byte[] file, AsyncCallback callback, object state)
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Verbose);
                UploadDataAsyncResult res = new UploadDataAsyncResult(_uri + "?action=upload", file, callback, state);
                res.Begin();
                TraceLogger.Write("Exit", TraceLevel.Verbose);
                return res;
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit null", TraceLevel.Verbose);
            return null;
        }

        UploadDataResults EndUploadFile(IAsyncResult result)
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            UploadDataResults results = ((UploadDataAsyncResult)result).End();
            TraceLogger.Write("Exit", TraceLevel.Verbose);
            return results;
        }
    }
}
