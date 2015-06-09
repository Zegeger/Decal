using System;
using System.Collections.Generic;
using System.Text;

namespace Zegeger.Decal.Plugins.AgentC
{
    interface IFileLogWriter
    {
        void WriteLine(int type, string color, int tag, DateTime time, string text);
        void ChangePath(string path);
        void Close();
    }
}
