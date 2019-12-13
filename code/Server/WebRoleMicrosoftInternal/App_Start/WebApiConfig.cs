// <copyright file="WebApiConfig.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.WebRoleMicrosoftInternal.App_Start
{
    using System;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.Http.Dispatcher;
    using System.Web.Http.ExceptionHandling;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Newtonsoft.Json.Serialization;
    using SocialPlus.Logging;
    using SocialPlus.Server.Managers;
    using SocialPlus.Server.Metrics;
    using SocialPlus.Server.WebRoleCommon.App_Start;
    using SocialPlus.Server.WebRoleCommon.DependencyResolution;
    using SocialPlus.Server.WebRoleCommon.Filters;
    using SocialPlus.Server.WebRoleCommon.Versioning;
    using SocialPlus.Server.WebRoleMicrosoftInternal.DependencyResolution;
    using StructureMap;

    /// <summary>
    /// Configurations settings for this web role
    /// </summary>
    public static class WebApiConfig
    {
        /// <summary>
        /// Register services, filters, and routes
        /// </summary>
        /// <param name="config">http configuration</param>
        public static void Register(HttpConfiguration config)
        {
            // service version info. Current available versions are: 0.1 through 0.3
            int firstMajorVersion = 0;
            int numMajorVersions = 1;
            MinorVersionInfo[] minorVersionInfos = new MinorVersionInfo[1] { new MinorVersionInfo(1, 3) };
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

            config.Filters.Add(new ModelValidationAttribute());
            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            config.Formatters.JsonFormatter.SerializerSettings.Formatting = Formatting.Indented;
            config.Formatters.JsonFormatter.SerializerSettings.Converters.Add(new StringEnumConverter());
            config.Formatters.JsonFormatter.UseDataContractJsonSerializer = false;

            // Web API routes to the versioning route provider
            config.MapHttpAttributeRoutes(new VersionedDirectRouteProvider(serviceVersionInfo));

            // Start a separate thread to go fetch the Bing wallpaper
            BingWallpaperFetcher fetcher = new BingWallpaperFetcher();
            Task fetch = Task.Run(() => fetcher.FetchOneTime());
        }
    }
}
