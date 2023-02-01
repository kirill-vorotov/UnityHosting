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

using Microsoft.Extensions.Logging;

namespace kv.UnityHosting.Internal
{
    internal static class LoggerEventIds
    {
        public static readonly EventId Starting = new(1, nameof(Starting));
        public static readonly EventId Started = new(2, nameof(Started));
        public static readonly EventId Stopping = new(3, nameof(Stopping));
        public static readonly EventId Stopped = new(4, nameof(Stopped));
        public static readonly EventId StoppedWithException = new(5, nameof(StoppedWithException));
        public static readonly EventId ApplicationStartupException = new(6, nameof(ApplicationStartupException));
        public static readonly EventId ApplicationStoppingException = new(7, nameof(ApplicationStoppingException));
        public static readonly EventId ApplicationStoppedException = new(8, nameof(ApplicationStoppedException));
        public static readonly EventId BackgroundServiceFaulted = new(9, nameof(BackgroundServiceFaulted));
        public static readonly EventId BackgroundServiceStoppingHost = new(10, nameof(BackgroundServiceStoppingHost));
    }
}