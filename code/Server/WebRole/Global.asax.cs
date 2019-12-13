// <copyright file="Global.asax.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server
{
    using System;
    using System.Web.Http;

    using SocialPlus.Logging;
    using SocialPlus.Server.App_Start;
    using SocialPlus.Server.DependencyResolution;
    using SocialPlus.Server.WebRoleCommon.DependencyResolution;
    using StructureMap;

    /// <summary>
    /// Start and end of HTTP requests
    /// </summary>
#pragma warning disable SA1649 // This comment is needed to disable Stylecop warning 1649: File name must match first type name
    public class WebApiApplication : System.Web.HttpApplication
#pragma warning restore SA1649 // File name must match first type name
    {
        /// <summary>
        /// IoC container
        /// </summary>
        private IContainer container;

        /// <summary>
        /// Log
        /// </summary>
        private ILog log;

        /// <summary>
        /// Initialize
        /// </summary>
        public override void Init()
        {
            this.container = IoC<Registry>.Instance;
            this.log = this.container.GetInstance<ILog>();
        }

        /// <summary>
        /// Configure start of HTTP request processing
        /// </summary>
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
        }

        /// <summary>
        /// Application end request message
        /// On errors, add log messages
        /// </summary>
        protected void Application_EndRequest()
        {
            if (this.Context.Response.StatusCode < 200 || (this.Context.Response.StatusCode >= 300 && this.Context.Response.StatusCode != 301))
            {
                string failureMessage = "Failed request. Response code = " + this.Context.Response.StatusCode + " on " + this.Context.Request.HttpMethod + " " +
                    this.Context.Request.RawUrl + Environment.NewLine;

                this.log.LogError(failureMessage, showStackTrace: false);
            }
        }
    }
}
