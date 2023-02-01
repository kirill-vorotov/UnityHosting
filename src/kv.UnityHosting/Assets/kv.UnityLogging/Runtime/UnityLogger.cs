/*
 * MIT License
 *
 * Copyright (c) 2023 Kirill Vorotov
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

#nullable enable
using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using UnityEngine;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace kv.UnityLogging
{
    public sealed class UnityLogger : ILogger
    {
        private readonly string _name;
        private readonly UnityEngine.ILogger _logger;

        private static StringWriter? _stringWriter;

        internal SimpleUnityFormatter Formatter { get; set; }

        public UnityLogger(string name, UnityEngine.ILogger logger, SimpleUnityFormatter formatter)
        {
            _name = name;
            _logger = logger;
            Formatter = formatter;
        }
        
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }
            
            _stringWriter ??= new StringWriter();
            LogEntry<TState> logEntry = new(logLevel, _name, eventId, state, exception, formatter);
            Formatter.Write(in logEntry, _stringWriter);

            var sb = _stringWriter.GetStringBuilder();
            if (sb.Length == 0)
            {
                return;
            }

            var str = sb.ToString();
            sb.Clear();
            if (sb.Capacity > 1024)
            {
                sb.Capacity = 1024;
            }

            if (exception is not null)
            {
                _logger.LogException(exception);
            }

            switch (logLevel)
            {
                case LogLevel.Trace:
                case LogLevel.Debug:
                case LogLevel.Information:
                {
                    _logger.Log(LogType.Log, str);
                    break;
                }
                case LogLevel.Warning:
                {
                    _logger.Log(LogType.Warning, str);
                    break;
                }
                case LogLevel.Error:
                case LogLevel.Critical:
                {
                    _logger.Log(LogType.Error, str);
                    break;
                }
                case LogLevel.None:
                {
                    break;
                }
                default:
                {
                    throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null);
                }
            }
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            if (!_logger.logEnabled)
            {
                return false;
            }
            
            switch (logLevel)
            {
                case LogLevel.Trace:
                case LogLevel.Debug:
                case LogLevel.Information:
                {
                    return _logger.IsLogTypeAllowed(LogType.Log);
                }
                case LogLevel.Warning:
                {
                    return _logger.IsLogTypeAllowed(LogType.Warning);
                }
                case LogLevel.Error:
                {
                    return _logger.IsLogTypeAllowed(LogType.Error);
                }
                case LogLevel.Critical:
                {
                    return _logger.IsLogTypeAllowed(LogType.Exception);
                }
                case LogLevel.None:
                {
                    return false;
                }
                default:
                {
                    throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null);
                }
            }
        }

        public IDisposable BeginScope<TState>(TState state) => default!;
    }
}