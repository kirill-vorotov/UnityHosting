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

namespace kv.UnityLogging
{
    public sealed class SimpleUnityFormatter
    {
        private const string logLevelPadding = ": ";
        private static readonly string messagePadding = new string(' ', GetLogLevelString(LogLevel.Information).Length + logLevelPadding.Length);
        private static readonly string newLineWithMessagePadding = Environment.NewLine + messagePadding;
        
        public void Write<TState>(in LogEntry<TState> logEntry, TextWriter textWriter)
        {
            var message = logEntry.Formatter?.Invoke(logEntry.State, logEntry.Exception);
            if (logEntry.Exception == null && message == null)
            {
                return;
            }
            
            var logLevel = logEntry.LogLevel;
            var logLevelString = GetLogLevelString(logLevel);
            
            string? timestamp = null;
            string? timestampFormat = "yyyy/MM/dd HH:mm:ss.ff"; // TODO: option to set the timestamp format
            if (timestampFormat != null)
            {
                var dateTimeOffset = DateTimeOffset.UtcNow; // TODO: options to choose between Now and UtcNow
                timestamp = dateTimeOffset.ToString(timestampFormat);
            }
            
            if (timestamp != null)
            {
                textWriter.Write(timestamp);
            }
            
            textWriter.Write(' ');
            textWriter.Write('[');
            textWriter.Write(logLevelString);
            textWriter.Write(']');
            
            const bool singleLine = true;
            var eventId = logEntry.EventId.Id;
            var exception = logEntry.Exception;
            textWriter.Write(logLevelPadding);
            textWriter.Write(logEntry.Category);
            textWriter.Write('[');
            textWriter.Write(eventId.ToString());
            textWriter.Write(']');

            if (!singleLine)
            {
                textWriter.Write(Environment.NewLine);
            }
            
            WriteMessage(textWriter, message, singleLine);
            
            if (exception != null)
            {
                // exception message
                WriteMessage(textWriter, exception.ToString(), singleLine);
            }

            if (singleLine)
            {
                textWriter.Write(Environment.NewLine);
            }
        }
        
        private static string GetLogLevelString(LogLevel logLevel)
        {
            return logLevel switch
            {
                LogLevel.Trace => "TRACE",
                LogLevel.Debug => "DEBUG",
                LogLevel.Information => "INFO",
                LogLevel.Warning => "WARN",
                LogLevel.Error => "ERROR",
                LogLevel.Critical => "CRIT",
                _ => throw new ArgumentOutOfRangeException(nameof(logLevel))
            };
        }
        
        private static void WriteMessage(TextWriter textWriter, string? message, bool singleLine)
        {
            if (!string.IsNullOrEmpty(message))
            {
                if (singleLine)
                {
                    textWriter.Write(' ');
                    WriteReplacing(textWriter, Environment.NewLine, " ", message);
                }
                else
                {
                    textWriter.Write(messagePadding);
                    WriteReplacing(textWriter, Environment.NewLine, newLineWithMessagePadding, message);
                    textWriter.Write(Environment.NewLine);
                }
            }

            static void WriteReplacing(TextWriter writer, string oldValue, string newValue, string message)
            {
                string newMessage = message.Replace(oldValue, newValue);
                writer.Write(newMessage);
            }
        }
    }
}