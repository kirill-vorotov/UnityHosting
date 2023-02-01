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
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace kv.UnityHosting.Internal
{
    public sealed class UnityLifetime : IHostLifetime, IDisposable
    {
        private CancellationTokenRegistration _applicationStartedRegistration;
        private CancellationTokenRegistration _applicationStoppingRegistration;

        private IHostEnvironment Environment { get; }
        private IHostApplicationLifetime ApplicationLifetime { get; }
        private ILogger<UnityLifetime> Logger { get; }
        
        public UnityLifetime(IHostEnvironment environment, IHostApplicationLifetime applicationLifetime, ILogger<UnityLifetime> logger)
        {
            Environment = environment;
            ApplicationLifetime = applicationLifetime;
            Logger = logger;
        }
        
        public Task WaitForStartAsync(CancellationToken cancellationToken)
        {
            _applicationStartedRegistration = ApplicationLifetime.ApplicationStarted.Register(state =>
                {
                    ((UnityLifetime)state).OnApplicationStarted();
                },
                this);
            _applicationStoppingRegistration = ApplicationLifetime.ApplicationStopping.Register(state =>
                {
                    ((UnityLifetime)state).OnApplicationStopping();
                },
                this);

            RegisterShutdownHandler();
            return Task.CompletedTask;
        }
        
        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
        
        public void Dispose()
        {
            UnregisterShutdownHandler();
            
            _applicationStartedRegistration.Dispose();
            _applicationStoppingRegistration.Dispose();
        }

        private void RegisterShutdownHandler()
        {
            UnityEngine.Application.quitting += HandleQuittingEvent;
        }

        private void UnregisterShutdownHandler()
        {
            UnityEngine.Application.quitting -= HandleQuittingEvent;
        }

        private void HandleQuittingEvent()
        {
            ApplicationLifetime.StopApplication();
        }
        
        private void OnApplicationStarted()
        {
            Logger.LogInformation("Application started");
            Logger.LogInformation("Hosting environment: {EnvName}", Environment.EnvironmentName);
            Logger.LogInformation("Content root path: {ContentRoot}", Environment.ContentRootPath);
        }
        
        private void OnApplicationStopping()
        {
            Logger.LogInformation("Application is shutting down...");
        }
    }
}