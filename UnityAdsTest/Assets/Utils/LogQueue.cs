using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Utils
{
    public sealed class LogQueue : IDisposable
    {
        [Flags]
        public enum LogLevel
        {
            All = 0,
            Info = 1 << 0,
            Warning = 1 << 1,
            Error = 1 << 2,
        }

        public readonly struct Line
        {
            public readonly string Text;
            public readonly LogLevel Level;

            public Line(string text, LogLevel level)
            {
                Text = text;
                Level = level;
            }
        }

        readonly LinkedList<Line> _lines;
        readonly int _maxSavedLineCount;

        public LogQueue(int maxSavedLineCount)
        {
            _maxSavedLineCount = maxSavedLineCount;
            _lines = new LinkedList<Line>();

            Application.logMessageReceivedThreaded += OnLog;
        }

        public bool IsDirty { get; set; }
        public int LogCount { get; private set; }

        public void Dispose()
        {
            Application.logMessageReceivedThreaded -= OnLog;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        void OnLog(string condition, string stackTrace, LogType type)
        {
            LogCount += 1;

            var line = MakeLine(condition, stackTrace, type);
            _lines.AddFirst(line);

            while (_lines.Count > _maxSavedLineCount)
            {
                _lines.RemoveLast();
            }

            IsDirty = true;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public IEnumerable<Line> GetLines(string filter, LogLevel level)
        {
            if (string.IsNullOrEmpty(filter) && level == 0)
            {
                return _lines;
            }

            return _lines
                .Where(l => l.Text.Contains(filter ?? ""))
                .Where(l => level.HasFlag(l.Level))
                .ToArray();
        }

        static Line MakeLine(string condition, string stackTrace, LogType type)
        {
            switch (type)
            {
                case LogType.Error:
                case LogType.Assert:
                case LogType.Exception:
                {
                    var shortStackTrace = stackTrace.TakeFirstLines(4);
                    var text = $"{condition}\n{shortStackTrace}\n";
                    return new Line(text, LogLevel.Error);
                }
                case LogType.Warning:
                {
                    return new Line(condition, LogLevel.Warning);
                }
                case LogType.Log:
                {
                    return new Line(condition, LogLevel.Info);
                }
                default:
                {
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
                }
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Clear()
        {
            _lines.Clear();
        }
    }
}