using System;
using System.Text;
using System.IO;

namespace OpenNoxLibrary.Log
{
    public class LogHandlerFile : LightLog.LogHandler
    {
        StreamWriter textLogWriter = null;

        public LogHandlerFile(string filename)
        {
            textLogWriter = File.CreateText(filename);
        }

        public void ProcessEntry(LightLog.LogEntry ent)
        {
            if (textLogWriter != null)
            {
                textLogWriter.WriteLine("[{0}] [{1}] {2}", ent.Level, ent.Time.ToShortTimeString(), ent.Text);
                textLogWriter.Flush();
            }
        }

        public void Close()
        {
            if (textLogWriter != null)
                textLogWriter.Close();
        }
    }
}
