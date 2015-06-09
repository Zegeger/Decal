using System;
using System.Collections.Generic;
using System.Text;
using System.Media;
using System.IO;
using Zegeger.Diagnostics;

namespace Zegeger.Audio
{
    public class SoundManager
    {
        private Dictionary<string, SoundPlayer> _soundPlayers;

        public SoundManager(string folderPath)
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Verbose);
                _soundPlayers = new Dictionary<string, SoundPlayer>();
                DirectoryInfo di = new DirectoryInfo(folderPath);
                foreach (FileInfo fi in di.GetFiles("*.wav"))
                {
                    TraceLogger.Write("Found sound file " + fi.Name, TraceLevel.Verbose);
                    SoundPlayer sp = new SoundPlayer(fi.FullName);
                    sp.LoadAsync();
                    _soundPlayers.Add(fi.Name, sp);
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        ~SoundManager()
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Verbose);
                foreach (SoundPlayer sp in _soundPlayers.Values)
                {
                    sp.Stop();
                    sp.Dispose();
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        public void PlaySound(string fileName)
        {
            try
            {
                TraceLogger.Write("Enter, filename " + fileName, TraceLevel.Verbose);
                SoundPlayer tmp = this[fileName];
                if (tmp != null)
                {
                    tmp.Play();
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        public void LoopSound(string fileName)
        {
            try
            {
                TraceLogger.Write("Enter, filename " + fileName, TraceLevel.Verbose);
                SoundPlayer tmp = this[fileName];
                if (tmp != null)
                {
                    tmp.PlayLooping();
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        public void StopSound(string fileName)
        {
            try
            {
                TraceLogger.Write("Enter, filename " + fileName, TraceLevel.Verbose);
                SoundPlayer tmp = this[fileName];
                if (tmp != null)
                {
                    tmp.Stop();
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        public IEnumerable<string> SoundFileNames
        {
            get
            {
                return _soundPlayers.Keys;
            }
        }

        public SoundPlayer this[string fileName]
        {
            get
            {
                if (_soundPlayers.ContainsKey(fileName))
                {
                    return _soundPlayers[fileName];
                }
                return null;
            }
        }
    }
}
