using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Xml.Serialization;
using System.Threading;
using Zegeger.Decal;
using Zegeger.Diagnostics;

namespace Zegeger.Versioning
{
    public class VersionNumber
    {
        public int Major { get; private set;}
        public int Minor { get; private set;}
        public int Build { get; private set;}
        public int Revision { get; private set;}

        public VersionNumber(int major = 0, int minor = 0, int build = 0, int revision = 0)
        {
            Major = major;
            Minor = minor;
            Build = build;
            Revision = revision;
        }

        public VersionNumber(string Version)
        {
            Major = 0;
            Minor = 0;
            Build = 0;
            Revision = 0;
            string[] numbers = Version.Trim().Split('.');
            if (numbers.Length >= 1)
            {
                Major = Int32.Parse(numbers[0]);
            }
            if (numbers.Length >= 2)
            {
                Minor = Int32.Parse(numbers[1]);
            }
            if (numbers.Length >= 3)
            {
                Build = Int32.Parse(numbers[2]);
            }
            if (numbers.Length >= 4)
            {
                Revision = Int32.Parse(numbers[3]);
            }
        }

        public override string ToString()
        {
            return String.Format("{0}.{1}.{2}.{3}", Major, Minor, Build, Revision);
        }

        public override bool Equals(object obj)
        {
            if (obj is VersionNumber)
                return this == (VersionNumber)obj;
            return false;
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        public static bool operator ==(VersionNumber v1, VersionNumber v2)
        {
            if (Object.ReferenceEquals(v1, v2)) return true;
            if ((object)v1 == null || (object)v2 == null) return false;

            if (v1.Major == v2.Major && v1.Minor == v2.Minor && v1.Build == v2.Build && v1.Revision == v2.Revision)
            {
                return true;
            }
            return false;
        }

        public static bool operator !=(VersionNumber v1, VersionNumber v2)
        {
            return !(v1 == v2);
        }

        public static bool operator <=(VersionNumber v1, VersionNumber v2)
        {
            if (v1 == v2 || v1 < v2)
                return true;
            return false;
        }

        public static bool operator >=(VersionNumber v1, VersionNumber v2)
        {
            if (v1 == v2 || v1 > v2)
                return true;
            return false;
        }

        public static bool operator <(VersionNumber v1, VersionNumber v2)
        {
            if ((object)v1 == null || (object)v2 == null) return false;
            if (v1.Major < v2.Major)
            {
                return true;
            }
            else if (v1.Major > v2.Major)
            {
                return false;
            }
            else
            {
                if (v1.Minor < v2.Minor)
                {
                    return true;
                }
                else if (v1.Minor > v2.Minor)
                {
                    return false;
                }
                else
                {
                    if (v1.Build < v2.Build)
                    {
                        return true;
                    }
                    else if (v1.Build > v2.Build)
                    {
                        return false;
                    }
                    else
                    {
                        if (v1.Revision < v2.Revision)
                        {
                            return true;
                        }
                        else if (v1.Revision > v2.Revision)
                        {
                            return false;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
        }

        public static bool operator >(VersionNumber v1, VersionNumber v2)
        {
            if ((object)v1 == null || (object)v2 == null) return false;
            if (v1.Major > v2.Major)
            {
                return true;
            }
            else if (v1.Major < v2.Major)
            {
                return false;
            }
            else
            {
                if (v1.Minor > v2.Minor)
                {
                    return true;
                }
                else if (v1.Minor < v2.Minor)
                {
                    return false;
                }
                else
                {
                    if (v1.Build > v2.Build)
                    {
                        return true;
                    }
                    else if (v1.Build < v2.Build)
                    {
                        return false;
                    }
                    else
                    {
                        if (v1.Revision > v2.Revision)
                        {
                            return true;
                        }
                        else if (v1.Revision < v2.Revision)
                        {
                            return false;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
        }
    }

    public enum UpdateAction
    {
        Application,
        CreateUpdate,
        Delete
    }

    public class UpdateResult
    {
        public UpdateAction action { get; set; }
        public string fileName { get; set; }
        public string newVersion { get; set; }
        public string minVersion { get; set; }
    }

    public class FetchedFile
    {
        public string fileName { get; set; }
        public byte[] file { get; set; }
    }

    public enum ResultStatus
    {
        Success,
        Failure
    }

    public class UpdateCheckResults
    {
        public List<UpdateResult> Results { get; internal set; }
        public ResultStatus Status { get; internal set; }

        public UpdateCheckResults(ResultStatus status, List<UpdateResult> results)
        {
            Results = results;
            Status = status;
        }
    }

    public class FilesFetchedResults
    {
        public List<FetchedFile> Files { get; internal set; }
        public ResultStatus Status { get; internal set; }

        public FilesFetchedResults(ResultStatus status, List<FetchedFile> files)
        {
            Files = files;
            Status = status;
        }
    }

    internal class CheckVersionAsyncResult : IAsyncResult
    {
        public object AsyncState { get; private set; }
        public WaitHandle AsyncWaitHandle { get; private set; }
        public bool CompletedSynchronously { get; private set; }
        public bool IsCompleted { get; private set; }

        private AsyncCallback _callback;
        private UpdateCheckResults _results;
        private Exception _exception;

        public CheckVersionAsyncResult(AsyncCallback callback, object asyncState)
        {
            _callback = callback;
            _exception = null;
            AsyncState = asyncState;
            AsyncWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
            CompletedSynchronously = false;
            IsCompleted = false;
        }

        public void Begin(string uri)
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = "GET";
            TraceLogger.Write("Starting GetResponse", TraceLevel.Verbose);
            request.BeginGetResponse(result =>
            {
                try
                {
                    TraceLogger.Write("HTTP GetResponse Callback", TraceLevel.Verbose);
                    HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(result);
                    TraceLogger.Write("HTTP response StatusCode: " + response.StatusCode, TraceLevel.Verbose);
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        Stream responseStream = response.GetResponseStream();
                        List<UpdateResult> results = new List<UpdateResult>();

                        StreamReader reader = new StreamReader(responseStream);
                        string strbody = reader.ReadToEnd();
                        TraceLogger.Write("Response body: " + strbody, TraceLevel.Verbose);
                        string[] entities = strbody.Split(';');
                        foreach (string entity in entities)
                        {
                            string[] attrib = entity.Split(':');
                            if (attrib.Length >= 3)
                            {
                                UpdateResult newResult = new UpdateResult();
                                switch (attrib[0])
                                {
                                    case "a": newResult.action = UpdateAction.Application;
                                        break;
                                    case "c": newResult.action = UpdateAction.CreateUpdate;
                                        break;
                                    case "d": newResult.action = UpdateAction.Delete;
                                        break;
                                    default:
                                        newResult.action = UpdateAction.CreateUpdate;
                                        break;
                                }
                                newResult.fileName = attrib[1];
                                newResult.newVersion = attrib[2];
                                if (attrib.Length == 4)
                                    newResult.minVersion = attrib[3];
                                results.Add(newResult);
                            }
                        }

                        _results = new UpdateCheckResults(ResultStatus.Success, results);
                    }
                    else
                    {
                        _results = new UpdateCheckResults(ResultStatus.Failure, null);
                    }
                }
                catch (Exception ex)
                {
                    _exception = ex;
                }
                finally
                {
                    IsCompleted = true;
                    ((EventWaitHandle)AsyncWaitHandle).Set();
                    if (_callback != null)
                        _callback(this);
                }
            }
            , null);
            TraceLogger.Write("End", TraceLevel.Verbose);
        }

        public UpdateCheckResults End()
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            if (!IsCompleted)
                AsyncWaitHandle.WaitOne();
            if (_exception != null)
                throw _exception;
            TraceLogger.Write("Exit", TraceLevel.Verbose);
            return _results;
        }
    }

    internal class FilesFetchedAsyncResult : IAsyncResult
    {
        public object AsyncState { get; private set; }
        public WaitHandle AsyncWaitHandle { get; private set; }
        public bool CompletedSynchronously { get; private set; }
        public bool IsCompleted { get; private set; }

        private AsyncCallback _callback;
        private FilesFetchedResults _results;
        Exception _exception;

        public FilesFetchedAsyncResult(AsyncCallback callback, object asyncState)
        {
            _callback = callback;
            _exception = null;
            AsyncState = asyncState;
            AsyncWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
            CompletedSynchronously = false;
            IsCompleted = false;
        }

        public void Begin(string uri, IEnumerable<UpdateResult> updateList)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uri);
            request.Method = "POST";
            request.ContentType = "text/plain";
            Stream requestStream = request.GetRequestStream();

            foreach (UpdateResult result in updateList)
            {
                byte[] file = Encoding.UTF8.GetBytes(result.fileName + ":" + result.newVersion + ";");
                requestStream.Write(file, 0, file.Length);
            }
            requestStream.Close();
            request.BeginGetResponse(result =>
            {
                try
                {
                    HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(result);
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        List<FetchedFile> results = new List<FetchedFile>();
                        Stream responseStream = response.GetResponseStream();
                        StreamReader reader = new StreamReader(responseStream);
                        string strbody = reader.ReadToEnd();
                        TraceLogger.Write("Response body: " + strbody, TraceLevel.Verbose);
                        string[] entities = strbody.Split(';');
                        foreach (string entity in entities)
                        {
                            string[] attrib = entity.Split(':');
                            if (attrib.Length == 2)
                            {
                                FetchedFile newResult = new FetchedFile();
                                newResult.fileName = attrib[0];
                                newResult.file = Convert.FromBase64String(attrib[1]);
                                results.Add(newResult);
                            }
                        }
                        _results = new FilesFetchedResults(ResultStatus.Success, results);
                    }
                    else
                    {
                        _results = new FilesFetchedResults(ResultStatus.Failure, null);
                    }
                }
                catch (Exception ex)
                {
                    _exception = ex;
                }
                finally
                {
                    IsCompleted = true;
                    ((EventWaitHandle)AsyncWaitHandle).Set();
                    if (_callback != null)
                        _callback(this);
                }
            }
            , null);
        }

        public FilesFetchedResults End()
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            if (!IsCompleted)
                AsyncWaitHandle.WaitOne();
            if (_exception != null)
                throw _exception;
            TraceLogger.Write("Exit", TraceLevel.Verbose);
            return _results;
        }
    }

    public class Updater
    {
        private string _uri;

        public Updater(string uri)
        {
            _uri = uri;
        }

        public IAsyncResult BeginCheckVersions(string appName, string version, AsyncCallback callback, object state)
        {
            try
            {
                TraceLogger.Write("Enter with name " + appName, TraceLevel.Verbose);
                CheckVersionAsyncResult res = new CheckVersionAsyncResult(callback, state);
                res.Begin(_uri + "?action=check&plugin=" + appName + "&version=" + version);
                TraceLogger.Write("Exit", TraceLevel.Verbose);
                return res;
            }
            catch(Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit null", TraceLevel.Verbose);
            return null;
        }

        public UpdateCheckResults EndCheckVersions(IAsyncResult result)
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            UpdateCheckResults results = ((CheckVersionAsyncResult)result).End();
            TraceLogger.Write("Exit", TraceLevel.Verbose);
            return results;
        }

        public IAsyncResult BeginFetchFiles(IEnumerable<UpdateResult> updateList, AsyncCallback callback, object state)
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Verbose);
                FilesFetchedAsyncResult res = new FilesFetchedAsyncResult(callback, state);
                res.Begin(_uri + "?action=fetch", updateList);
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

        public  FilesFetchedResults EndFetchFiles(IAsyncResult result)
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            FilesFetchedResults results = ((FilesFetchedAsyncResult)result).End();
            TraceLogger.Write("Exit", TraceLevel.Verbose);
            return results;
        }
    }
}
