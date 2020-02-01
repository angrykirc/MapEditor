using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace OpenNoxLibrary.Log
{
    /// <summary>
    /// Provides a simple, but quite efficient, write-on-demand logger implementation.
    /// </summary>
    public class LightLog
    {
        /// <summary>
        /// Holds all log entries, before they are sent to .
        /// </summary>
        private Queue<LogEntry> incomingQueue;

        /// <summary>
        /// Holds all registered log processors/handlers.
        /// </summary>
        private List<LogHandler> externalHandlers;

        private Level minLevel = Level.INFO;

        /// <summary>
        /// Interface for a log event processor.
        /// </summary>
        public interface LogHandler
        {
            void ProcessEntry(LogEntry ent);
            void Close();
        }

        public enum Level : byte
        {
            DEBUG = 0,
            INFO = 1,
            WARN = 2,
            CRITICAL = 3
        }

        /// <summary>
        /// Log entry data container.
        /// </summary>
        public struct LogEntry
        {
            public string Text;
            public DateTime Time;
            public Level Level;

            public LogEntry(string text, DateTime time, Level level)
            {
                Text = text;
                Time = time;
                Level = level;
            }
        }

        public LightLog()
        {
            incomingQueue = new Queue<LogEntry>();
            externalHandlers = new List<LogHandler>();
        }

        public void RegisterHandler(LogHandler lh)
        {
            if (!externalHandlers.Contains(lh))
                externalHandlers.Add(lh);
        }

        public void UnregisterHandler(LogHandler lh)
        {
            lh.Close();
            externalHandlers.Remove(lh);
        }

        /// <summary>
        /// Forces all log entries to be processed, and then gracefully closes all registered LogHandlers.
        /// </summary>
        public void Close()
        {
            ProcessLogQueue();

            lock (externalHandlers)
            {
                foreach (LogHandler lh in externalHandlers)
                    lh.Close();

                externalHandlers.Clear();
            }
        }

        /// <summary>
        /// Sets the level of log entries that will not be discarded upon arrival.
        /// </summary>
        public void SetLevel(Level lvl)
        {
            minLevel = lvl;
        }

        public Level GetLevel()
        {
            return minLevel;
        }

        /// <summary>
        /// Processes all LogEntries that were registered after the previous ProcessLogQueue() call.
        /// This method is not protected from race-condition error, but will work fine if not used asynchronously.
        /// </summary>
        public void ProcessLogQueue()
        {
            LogEntry[] entries;
            lock (incomingQueue) // lock only on the copying operation, so the other lock doesn't have to wait all the way down
            {
                entries = incomingQueue.ToArray();
                incomingQueue.Clear(); // Erase all entries
            }

            LogHandler[] handlers;
            lock (externalHandlers)
            {
                handlers = externalHandlers.ToArray();
            }

            foreach (LogEntry le in entries)
            {
                foreach (LogHandler lh in handlers)
                    lh.ProcessEntry(le);
            }
        }

        public void Log(string text, Level lvl)
        {
            if (lvl < minLevel)
                return; // Skip this entry

            var entry = new LogEntry(text, DateTime.Now, lvl);

            lock (incomingQueue)
                incomingQueue.Enqueue(entry);
        }

        public void Log(string format, Level lvl, params object[] args)
        {
            string text = String.Format(format, args);

            Log(text, lvl);
        }

        public void Debug(string text)
        {
            Log(text, Level.DEBUG);
        }

        public void Debug(string text, params object[] args)
        {
            Log(text, Level.DEBUG, args);
        }

        public void Info(string text)
        {
            Log(text, Level.INFO);
        }

        public void Info(string text, params object[] args)
        {
            Log(text, Level.INFO, args);
        }

        public void Warn(string text)
        {
            Log(text, Level.WARN);
        }

        public void Warn(string text, params object[] args)
        {
            Log(text, Level.WARN, args);
        }

        public void Critical(string text)
        {
            Log(text, Level.CRITICAL);
        }

        public void Critical(string text, params object[] args)
        {
            Log(text, Level.CRITICAL, args);
        }
    }
}
