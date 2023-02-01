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
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace kv.UnityHosting
{
    public static class GameHostBuilderExtensions
    {
        public static IHostBuilder UseEnvironment(this IHostBuilder hostBuilder, string environment)
        {
            return hostBuilder.ConfigureHostConfiguration(configBuilder =>
            {
                configBuilder.AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string>(HostDefaults.EnvironmentKey,
                        environment ?? throw new ArgumentNullException(nameof(environment)))
                });
            });
        }
        
        public static IHostBuilder ConfigureLogging(this IHostBuilder hostBuilder, Action<HostBuilderContext, ILoggingBuilder> configureLogging)
        {
            return hostBuilder.ConfigureServices((context, collection) => collection.AddLogging(builder => configureLogging(context, builder)));
        }
        
        public static IHostBuilder ConfigureLogging(this IHostBuilder hostBuilder, Action<ILoggingBuilder> configureLogging)
        {
            return hostBuilder.ConfigureServices((context, collection) => collection.AddLogging(builder => configureLogging(builder)));
        }
        
        public static IHostBuilder UseContentRoot(this IHostBuilder hostBuilder, string contentRoot)
        {
            return hostBuilder.ConfigureHostConfiguration(configBuilder =>
            {
                configBuilder.AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string>(HostDefaults.ContentRootKey,
                        contentRoot ?? throw new ArgumentNullException(nameof(contentRoot)))
                });
            });
        }
        
        public static IHostBuilder ConfigureDefaults(this IHostBuilder builder)
        {
            var contentRoot = UnityEngine.Application.dataPath;
            builder.UseContentRoot(contentRoot);
            
            builder.ConfigureHostConfiguration(config =>
            {
                config.AddEnvironmentVariables(prefix: "UNITY_");
            });

            builder.ConfigureAppConfiguration((context, config) =>
            {
                var env = context.HostingEnvironment;
                
                config
                    .AddUnityConfig("gamesettings.json", optional: true)
                    .AddUnityConfig($"gamesettings.{env.EnvironmentName}.json", optional: true);
                
                config.AddEnvironmentVariables();
            })
            .ConfigureLogging((context, loggingBuilder) =>
            {
                loggingBuilder.AddConfiguration(context.Configuration.GetSection("Logging"));
                
                loggingBuilder.Configure(options =>
                {
                    options.ActivityTrackingOptions =
                        ActivityTrackingOptions.SpanId |
                        ActivityTrackingOptions.TraceId |
                        ActivityTrackingOptions.ParentId;
                });
            });
            
            return builder;
        }
    }
}