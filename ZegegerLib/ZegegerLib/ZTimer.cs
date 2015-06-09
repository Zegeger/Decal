using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Decal.Adapter;

namespace Zegeger.Decal
{
    public class NotInitializedException : Exception
    {
        public NotInitializedException()
            : base()
        { }

        public NotInitializedException(string message)
            : base(message)
        { }
    }

    public delegate void ZTimerCallback(object state);

    public class ZTimer
    {
        static private ulong _frameNum;
        static private bool _Init = false;
        static private Queue<ZTimerCallback> _pendingCallbacks;
        static private CoreManager _core;

        public static void StartUp(CoreManager core)
        {
            _frameNum = 1;
            _pendingCallbacks = new Queue<ZTimerCallback>();
            _core = core;
            core.RenderFrame += core_RenderFrame;
            _Init = true;
        }

        public static void Shutdown()
        {
            _core.RenderFrame -= core_RenderFrame;
            _frameNum = 0;
            CallCallbacks();
            _core = null;
            _Init = false;
        }

        private static ulong FrameNumber
        {
            get
            {
                return _frameNum;
            }
        }

        private static void core_RenderFrame(object sender, EventArgs e)
        {
            _frameNum++;
            CallCallbacks();
        }

        private static void CallCallbacks()
        {
            while (_pendingCallbacks.Count > 0)
            {
                _pendingCallbacks.Dequeue().Invoke(null);
            }
        }

        public static ZTimer CreateInstance(ZTimerCallback callback)
        {
            if (!_Init)
            {
                throw new NotInitializedException("You must call ZTimerManager.StartUp before creating timers.");
            }
            return new ZTimer(callback);
        }

        private Timer _iTimer;
        private int _duration;
        private ulong _lastFrame;
        private bool _enabled;
        private bool _repeat;
        private ZTimerCallback _callBack;

        public int Duration
        {
            get
            {
                return _duration;
            }
        }

        public bool Enabled
        {
            get
            {
                return _enabled;
            }
        }

        public bool Repeat
        {
            get
            {
                return _repeat;
            }
            set
            {
                _repeat = value;
            }
        }

        private ZTimer(ZTimerCallback callback)
        {
            _duration = 0;
            _lastFrame = 0;
            _repeat = true;
            _enabled = false;
            _iTimer = new Timer(Timer_Callback);
            _callBack = callback;
        }

        private void Timer_Callback(object state)
        {
            if (FrameNumber == 0)
            {
                Stop();
            }
            else if (_lastFrame != FrameNumber)
            {
                _lastFrame = FrameNumber;
                _pendingCallbacks.Enqueue(_callBack);
                if (!_repeat)
                {
                    Stop();
                }
            }
        }

        public void Start(int duration)
        {
            _duration = duration;
            _enabled = true;
            _iTimer.Change(duration, duration);
        }

        public void Stop()
        {
            _enabled = false;
            _iTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        public void Restart()
        {
            Start(_duration);
        }
    }
}
