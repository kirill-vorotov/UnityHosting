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

using System;
using System.Threading;
using kv.UnityHosting.Internal;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace kv.UnityHosting
{
    public sealed class ApplicationLifetime : IHostApplicationLifetime
    {
        private readonly CancellationTokenSource _startedSource = new();
        private readonly CancellationTokenSource _stoppingSource = new();
        private readonly CancellationTokenSource _stoppedSource = new();
        
        private readonly ILogger<ApplicationLifetime> _logger;

        public CancellationToken ApplicationStarted => _startedSource.Token;
        public CancellationToken ApplicationStopping => _stoppingSource.Token;
        public CancellationToken ApplicationStopped => _stoppedSource.Token;

        public ApplicationLifetime(ILogger<ApplicationLifetime> logger)
        {
            _logger = logger;
        }
        
        public void StopApplication()
        {
            lock (_stoppingSource)
            {
                try
                {
                    ExecuteHandlers(_stoppingSource);
                }
                catch (Exception ex)
                {
                    _logger.ApplicationError(LoggerEventIds.ApplicationStoppingException,
                        "An error occurred stopping the application",
                        ex);
                }
            }
        }
        
        /// <summary>
        /// Signals the ApplicationStarted event and blocks until it completes.
        /// </summary>
        public void NotifyStarted()
        {
            try
            {
                ExecuteHandlers(_startedSource);
            }
            catch (Exception ex)
            {
                _logger.ApplicationError(LoggerEventIds.ApplicationStartupException,
                    "An error occurred starting the application",
                    ex);
            }
        }
        
        /// <summary>
        /// Signals the ApplicationStopped event and blocks until it completes.
        /// </summary>
        public void NotifyStopped()
        {
            try
            {
                ExecuteHandlers(_stoppedSource);
            }
            catch (Exception ex)
            {
                _logger.ApplicationError(LoggerEventIds.ApplicationStoppedException,
                    "An error occurred stopping the application",
                    ex);
            }
        }

        private void ExecuteHandlers(CancellationTokenSource cancel)
        {
            // Noop if this is already cancelled
            if (cancel.IsCancellationRequested)
            {
                return;
            }

            // Run the cancellation token callbacks
            cancel.Cancel(throwOnFirstException: false);
        }
    }
}