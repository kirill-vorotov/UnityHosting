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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;

namespace kv.UnityHosting
{
    public static class UnityConfigurationExtensions
    {
        public static IConfigurationBuilder AddUnityConfig(this IConfigurationBuilder builder, string path, bool optional)
        {
            return AddUnityConfig(builder, provider: null, path: path, optional: optional);
        }

        public static IConfigurationBuilder AddUnityConfig(this IConfigurationBuilder builder, IFileProvider provider, string path, bool optional)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }
            
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException(nameof(path));
            }

            return builder.AddUnityConfig(s =>
            {
                s.FileProvider = provider;
                s.Path = path;
                s.Optional = optional;
                s.ReloadOnChange = false;
                s.ResolveFileProvider();
            });
        }
        
        public static IConfigurationBuilder AddUnityConfig(this IConfigurationBuilder builder, Action<UnityConfigurationSource> configureSource)
            => builder.Add(configureSource);
    }
}