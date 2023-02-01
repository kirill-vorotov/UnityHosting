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
using System.Diagnostics;
using System.IO;
using kv.UnityHosting.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace kv.UnityHosting
{
    public class GameHostBuilder : IHostBuilder
    {
        private readonly List<Action<IConfigurationBuilder>> _configureHostConfigActions = new();
        private readonly List<Action<HostBuilderContext, IConfigurationBuilder>> _configureAppConfigActions = new();
        private readonly List<Action<HostBuilderContext, IServiceCollection>> _configureServicesActions = new();
        private readonly List<IConfigureContainerAdapter> _configureContainerActions = new();
        private IServiceFactoryAdapter _serviceProviderFactory = new ServiceFactoryAdapter<IServiceCollection>(new DefaultServiceProviderFactory());
        private bool _hostBuilt;
        private IConfiguration _hostConfiguration;
        private IConfiguration _appConfiguration;
        private HostBuilderContext _hostBuilderContext;
        private GameEnvironment _hostingEnvironment;
        private IServiceProvider _appServices;
        private PhysicalFileProvider _defaultProvider;

        public IDictionary<object, object> Properties { get; } = new Dictionary<object, object>();
        
        public IHostBuilder ConfigureHostConfiguration(Action<IConfigurationBuilder> configureDelegate)
        {
            _configureHostConfigActions.Add(configureDelegate ?? throw new ArgumentNullException(nameof(configureDelegate)));
            return this;
        }

        public IHostBuilder ConfigureAppConfiguration(Action<HostBuilderContext, IConfigurationBuilder> configureDelegate)
        {
            _configureAppConfigActions.Add(configureDelegate ?? throw new ArgumentNullException(nameof(configureDelegate)));
            return this;
        }

        public IHostBuilder ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate)
        {
            _configureServicesActions.Add(configureDelegate ?? throw new ArgumentNullException(nameof(configureDelegate)));
            return this;
        }

        public IHostBuilder UseServiceProviderFactory<TContainerBuilder>(IServiceProviderFactory<TContainerBuilder> factory) where TContainerBuilder : notnull
        {
            _serviceProviderFactory = new ServiceFactoryAdapter<TContainerBuilder>(factory ?? throw new ArgumentNullException(nameof(factory)));
            return this;
        }

        public IHostBuilder UseServiceProviderFactory<TContainerBuilder>(Func<HostBuilderContext, IServiceProviderFactory<TContainerBuilder>> factory) where TContainerBuilder : notnull
        {
            _serviceProviderFactory = new ServiceFactoryAdapter<TContainerBuilder>(() => _hostBuilderContext, factory ?? throw new ArgumentNullException(nameof(factory)));
            return this;
        }

        public IHostBuilder ConfigureContainer<TContainerBuilder>(Action<HostBuilderContext, TContainerBuilder> configureDelegate)
        {
            _configureContainerActions.Add(new ConfigureContainerAdapter<TContainerBuilder>(configureDelegate ?? throw new ArgumentNullException(nameof(configureDelegate))));
            return this;
        }

        public IHost Build()
        {
            if (_hostBuilt)
            {
                throw new InvalidOperationException("Build can only be called once");
            }
            _hostBuilt = true;
            
            using var diagnosticListener = new DiagnosticListener("Microsoft.Extensions.Hosting");
            const string hostBuildingEventName = "HostBuilding";
            const string hostBuiltEventName = "HostBuilt";
            
            if (diagnosticListener.IsEnabled() && diagnosticListener.IsEnabled(hostBuildingEventName))
            {
                diagnosticListener.Write(hostBuildingEventName, this);
            }

            BuildHostConfiguration();
            CreateHostingEnvironment();
            CreateHostBuilderContext();
            BuildAppConfiguration();
            CreateServiceProvider();

            var host = _appServices.GetRequiredService<IHost>();
            
            if (diagnosticListener.IsEnabled() && diagnosticListener.IsEnabled(hostBuiltEventName))
            {
                diagnosticListener.Write(hostBuiltEventName, host);
            }
            
            return host;
        }
        
        private void BuildHostConfiguration()
        {
            var configBuilder = new ConfigurationBuilder().AddInMemoryCollection(); // Make sure there's some default storage since there are no default providers

            foreach (var buildAction in _configureHostConfigActions)
            {
                buildAction(configBuilder);
            }
            _hostConfiguration = configBuilder.Build();
        }
        
        private void CreateHostingEnvironment()
        {
            var environmentName =
#if ENV_PROD
                Environments.Production;
#elif ENV_STG
                Environments.Staging;
#elif ENV_DEV
                Environments.Development;
#else
                Environments.Development;
#endif
            
            _hostingEnvironment = new GameEnvironment
            {
                ApplicationName = _hostConfiguration[HostDefaults.ApplicationKey],
                EnvironmentName = environmentName,
                ContentRootPath = ResolveContentRootPath(_hostConfiguration[HostDefaults.ContentRootKey], UnityEngine.Application.dataPath),
            };

            if (string.IsNullOrEmpty(_hostingEnvironment.ApplicationName))
            {
                _hostingEnvironment.ApplicationName = UnityEngine.Application.productName;
            }

            _hostingEnvironment.ContentRootFileProvider = _defaultProvider = new PhysicalFileProvider(_hostingEnvironment.ContentRootPath);
        }
        
        private void CreateHostBuilderContext()
        {
            _hostBuilderContext = new HostBuilderContext(Properties)
            {
                HostingEnvironment = _hostingEnvironment,
                Configuration = _hostConfiguration
            };
        }
        
        private void BuildAppConfiguration()
        {
            IConfigurationBuilder configBuilder = new ConfigurationBuilder()
                // .SetBasePath(_hostingEnvironment.ContentRootPath)
                .AddConfiguration(_hostConfiguration, shouldDisposeConfiguration: true);
            configBuilder.Properties["FileProvider"] = new PhysicalFileProvider(_hostingEnvironment.ContentRootPath);

            foreach (Action<HostBuilderContext, IConfigurationBuilder> buildAction in _configureAppConfigActions)
            {
                buildAction(_hostBuilderContext, configBuilder);
            }
            _appConfiguration = configBuilder.Build();
            _hostBuilderContext.Configuration = _appConfiguration;
        }
        
        private void CreateServiceProvider()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IHostEnvironment>(_hostingEnvironment);
            services.AddSingleton(_hostBuilderContext);
            // register configuration as factory to make it dispose with the service provider
            services.AddSingleton(_ => _appConfiguration);
            services.AddSingleton<IHostApplicationLifetime, ApplicationLifetime>();

            services.AddSingleton<IHostLifetime, UnityLifetime>();

            services.AddSingleton<IHost>(_ => new Internal.GameHost(
                _appServices,
                _hostingEnvironment,
                _defaultProvider,
                _appServices.GetRequiredService<IHostApplicationLifetime>(),
                _appServices.GetRequiredService<ILogger<Internal.GameHost>>(),
                _appServices.GetRequiredService<IHostLifetime>(),
                _appServices.GetRequiredService<IOptions<HostOptions>>()));
            services.AddOptions().Configure<HostOptions>(options => { options.Initialize(_hostConfiguration); });
            services.AddLogging();

            foreach (Action<HostBuilderContext, IServiceCollection> configureServicesAction in _configureServicesActions)
            {
                configureServicesAction(_hostBuilderContext, services);
            }

            object containerBuilder = _serviceProviderFactory.CreateBuilder(services);

            foreach (IConfigureContainerAdapter containerAction in _configureContainerActions)
            {
                containerAction.ConfigureContainer(_hostBuilderContext, containerBuilder);
            }

            _appServices = _serviceProviderFactory.CreateServiceProvider(containerBuilder);

            if (_appServices is null)
            {
                throw new InvalidOperationException("The IServiceProviderFactory returned a null IServiceProvider");
            }

            // resolve configuration explicitly once to mark it as resolved within the
            // service provider, ensuring it will be properly disposed with the provider
            _ = _appServices.GetService<IConfiguration>();
        }
        
        private static string ResolveContentRootPath(string contentRootPath, string basePath)
        {
            if (string.IsNullOrEmpty(contentRootPath))
            {
                return basePath;
            }
            
            if (Path.IsPathRooted(contentRootPath))
            {
                return contentRootPath;
            }
            
            return Path.Combine(Path.GetFullPath(basePath), contentRootPath);
        }
    }
}