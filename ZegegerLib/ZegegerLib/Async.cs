using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ZegegerLib
{
    public abstract class AsyncResult : IAsyncResult
    {
        private readonly AsyncCallback _callback;
        private Exception _threadException;

        protected Exception ThreadException
        {
            get { return _threadException; }
            set { _threadException = value; }
        }

        private readonly object _asyncState;
        public object AsyncState
        {
            get
            {
                return _asyncState;
            }
        }

        private readonly ManualResetEvent _asyncWaitHandle;
        public WaitHandle AsyncWaitHandle
        {
            get
            {
                return _asyncWaitHandle;
            }
        }

        private bool _completedSynchronously;
        public bool CompletedSynchronously
        {
            get
            {
                return _completedSynchronously;
            }
        }

        private bool _isCompleted;
        public bool IsCompleted
        {
            get
            {
                return _isCompleted;
            }
        }

        protected AsyncResult(AsyncCallback callback, object asyncState)
        {
            _callback = callback;
            _asyncState = asyncState;
            _completedSynchronously = false;
            _isCompleted = false;
            _asyncWaitHandle = new ManualResetEvent(false);
        }

        protected void CompletedProcess()
        {
            this._isCompleted = true;
            _asyncWaitHandle.Set();
            if (_callback != null)
                _callback(this);
        }

        protected void InternalEnd()
        {
            if (!IsCompleted)
            {
                _completedSynchronously = true;
                AsyncWaitHandle.WaitOne();
            }
            if (_threadException != null)
                throw _threadException;
        }
    }
}
