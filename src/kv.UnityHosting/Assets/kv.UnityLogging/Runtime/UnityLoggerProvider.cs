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
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace kv.UnityLogging
{
    public sealed class UnityLoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentDictionary<string, UnityLogger> _loggers;
        private readonly SimpleUnityFormatter _formatter;

        public UnityLoggerProvider()
        {
            _loggers = new ConcurrentDictionary<string, UnityLogger>();
            _formatter = new SimpleUnityFormatter();
        }
        
        public void Dispose()
        {
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.TryGetValue(categoryName, out var logger)
                ? logger
                : _loggers.GetOrAdd(categoryName, new UnityLogger(categoryName, UnityEngine.Debug.unityLogger, _formatter));
        }
    }
}