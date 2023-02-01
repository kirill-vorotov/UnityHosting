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
using System.IO;
using Microsoft.Extensions.Configuration;

namespace kv.UnityHosting
{
    public sealed class UnityConfigurationProvider : ConfigurationProvider
    {
        public FileConfigurationSource Source { get; }
        
        public UnityConfigurationProvider(UnityConfigurationSource source)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
        }

        public override bool TryGet(string key, out string value)
        {
            return base.TryGet(key, out value);
        }

        public override void Load()
        {
            Load(reload: false);
        }

        private void Load(bool reload)
        {
            if (string.IsNullOrWhiteSpace(Source.Path))
            {
                Data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }
            else
            {
                var path = Path.GetFileNameWithoutExtension(Source.Path);
                var textAsset = UnityEngine.Resources.Load<UnityEngine.TextAsset>(path);
                if (textAsset is null)
                {
                    Data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    return;
                }
                var jString = textAsset.text;
                UnityEngine.Resources.UnloadAsset(textAsset);
                Data = UnityConfigurationFileParser.Parse(jString);
            }
        }
    }
}