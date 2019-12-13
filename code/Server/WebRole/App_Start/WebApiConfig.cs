// <copyright file="WebApiConfig.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.App_Start
{
    using System;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.Http.Dispatcher;
    using System.Web.Http.ExceptionHandling;
    using System.Web.Http.Filters;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Newtonsoft.Json.Serialization;
    using SocialPlus.Logging;
    using SocialPlus.Server.DependencyResolution;
    using SocialPlus.Server.Managers;
    using SocialPlus.Server.Metrics;
    using SocialPlus.Server.WebRoleCommon.App_Start;
    using SocialPlus.Server.WebRoleCommon.DependencyResolution;
    using SocialPlus.Server.WebRoleCommon.Filters;
    using SocialPlus.Server.WebRoleCommon.RateLimiting;
    using SocialPlus.Server.WebRoleCommon.Versioning;
    using SocialPlus.Utils;
    using StructureMap;

    /// <summary>
    /// Web API config class
    /// </summary>
    public static class WebApiConfig
    {
        /// <summary>
        /// The Register method configures <c>StructureMap</c>, <c>Filters</c>, <c>JsonFormattters</c>,
        /// versioned routing for controllers, batching, and rate limiting.
        /// </summary>
        /// <remarks>
        /// 1. configure StructureMap to take care of DI.
        /// 2. replace the global exception logger with our own to catch exceptions for any remaining init steps
        /// 3. configure remaining steps in the order .NET invokes them on an incoming request. This order is:
        ///         MessageHandlers, global filter, batching, versioning.
        /// </remarks>
        /// <param name="config">http configuration</param>
        public static void Register(HttpConfiguration config)
        {
            // service version info. Current available versions are: 0.6 through 0.8
            int firstMajorVersion = 0;
            int numMajorVersions = 1;
            MinorVersionInfo[] minorVersionInfos = new MinorVersionInfo[1] { new MinorVersionInfo(6, 8) };
            var serviceVersionInfo = new ServiceVersionInfo(firstMajorVersion, numMajorVersions, minorVersionInfos);

            // Configure StructureMap. First set its service version info
            IoC<Registry>.ServiceVersionInfo = serviceVersionInfo;
            IContainer container = IoC<Registry>.Instance;

            var log = container.GetInstance<ILog>();
            if (log == null)
            {
                // Since log is null, we cannot log an error message.  Instead, we just throw an exception.
                throw new Exception("Cannot find an instance of ILog in the StructureMap container");
            }

            // Add a global exception logger
            config.Services.Add(typeof(IExceptionLogger), new GlobalExceptionLogger(log));

            bool tableInit = false;
            Exception exception = null;
            try
            {
                // initialize CTStore after the global exception logger
                var ctStore = container.GetInstance<ICTStoreManager>();
                if (ctStore == null)
                {
                    log.LogException("Cannot find an instance of ICTStoreManager in the StructureMap container");
                }

                // use Task.Run to ensure that the async Initialize routine runs on a threadpool thread
                Task<bool> task = Task<bool>.Run(() => ctStore.Initialize());

                // task.Result blocks until the result is ready
                tableInit = task.Result;
            }
            catch (Exception e)
            {
                exception = e;
            }

            if (tableInit == false)
            {
                string errorMessage = "CTstore initialization failed." + Environment.NewLine +
                    "This is often due to forgetting to provision your storage account or" + Environment.NewLine +
                    "to having a storage account with an older version number." + Environment.NewLine +
                    "Check if your tables are empty, and if they are, provision your storage." + Environment.NewLine +
                    "Otherwise, verify that the version number in storage is the same as the version number" + Environment.NewLine +
                    "defined in the CTStoreManager code.  If these numbers do not match, then you'll need" + Environment.NewLine +
                    "to perform data conversion to upgrade your storage.";
                log.LogException(errorMessage, exception);
            }

            config.Services.Replace(typeof(IHttpControllerActivator), new StructureMapWebApiControllerActivator(container));

            // Add message handlers. We add performance logging first and then rate limiting because
            // perf logging should capture the impact of rate limiting
            var performanceMetrics = container.GetInstance<IPerformanceMetrics>();
            if (performanceMetrics == null)
            {
                log.LogException("Cannot find an instance of IPerformanceMetrics in the StructureMap container");
            }

            config.MessageHandlers.Add(new PerformanceLoggingMessageHandler(log, performanceMetrics));

            var settingsReader = container.GetInstance<ISettingsReader>();
            if (settingsReader == null)
            {
                log.LogException("Cannot find an instance of ISettingsReader in the StructureMap container");
            }

            config.MessageHandlers.Add(new CustomThrottlingHandler(log, settingsReader));

            // Register the filter injector for authentication filter
            var authFilter = container.GetInstance<IAuthenticationFilter>();
            if (authFilter == null)
            {
                log.LogException("Cannot find an instance of IAuthenticationFilter in the StructureMap container");
            }

            config.Filters.Add(authFilter);

            config.Filters.Add(new ModelValidationAttribute());
            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            config.Formatters.JsonFormatter.SerializerSettings.Formatting = Formatting.Indented;
            config.Formatters.JsonFormatter.SerializerSettings.Converters.Add(new StringEnumConverter());
            config.Formatters.JsonFormatter.UseDataContractJsonSerializer = false;

            // Add batching
            config.Routes.MapHttpBatchRoute(
                routeName: "batch",
                routeTemplate: "batch",
                batchHandler: new ParallelHttpBatchHandler(GlobalConfiguration.DefaultServer));

            // Web API routes to the versioning route provider
            config.MapHttpAttributeRoutes(new VersionedDirectRouteProvider(serviceVersionInfo));

            // Start a separate thread to go fetch the Bing wallpaper
            BingWallpaperFetcher fetcher = new BingWallpaperFetcher();
            Task fetch = Task.Run(() => fetcher.FetchOneTime());
        }
    }
}
