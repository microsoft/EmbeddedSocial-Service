// <copyright file="WebRole.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.WebRoleMicrosoftInternal
{
    using System;
    using System.Linq;

    using Microsoft.Web.Administration;
    using Microsoft.WindowsAzure.ServiceRuntime;
    using SocialPlus.Logging;
    using SocialPlus.Server.Utils;
    using SocialPlus.Utils;

    /// <summary>
    /// Main entry point for this web service
    /// </summary>
    public class WebRole : RoleEntryPoint
    {
        /// <summary>
        /// Log
        /// </summary>
        private ILog log;

        /// <summary>
        /// Some initial configuration before starting the web service
        /// </summary>
        /// <returns>A value indicating whether the web role has started</returns>
        public override bool OnStart()
        {
            try
            {
                // In Azure, Web role initialization runs in a separate process from the ASP.NET stack
                // There is no need to create a separate IoC container for this process.  Instead, we just
                // create the objects we need right here.
                this.log = new Log(LogDestination.EventSource, Log.DefaultCategoryName);
                var registry = new WindowsRegistry(this.log);

                if (!RoleEnvironment.IsEmulated)
                {
                    // Make configuration changes to avoid this application pool from being killed periodically.
                    // Make configuration changes to accept X509 client certificates in SSL connections.
                    // Both changes require <Runtime executionContext="elevated" /> in the <WebRole> element in ServiceDefinition.csdef
                    using (ServerManager serverManager = new ServerManager())
                    {
                        foreach (var app in serverManager.Sites.SelectMany(x => x.Applications))
                        {
                            // issues a fake request that causes our app code to load into memory on web service start
                            app["preloadEnabled"] = true;
                        }

                        foreach (var appPool in serverManager.ApplicationPools)
                        {
                            // tells W3SVC to automatically start the application pool when IIS starts
                            appPool.AutoStart = true;

                            // tells WAS to always start the application pool
                            appPool["startMode"] = "AlwaysRunning";

                            // tells WWW service to not shutdown the application pool due to no requests coming in
                            appPool.ProcessModel.IdleTimeout = TimeSpan.Zero;

                            // tells IIS to not automatically restart the application pool (default is every 29 hours)
                            appPool.Recycling.PeriodicRestart.Time = TimeSpan.Zero;
                        }

                        // set SSL flags to accept client certificates
                        var siteName = RoleEnvironment.CurrentRoleInstance.Id + "_Web";
                        var config = serverManager.GetApplicationHostConfiguration();
                        var accessSection = config.GetSection("system.webServer/security/access", siteName);

                        // SslNegotiateCert means "the site or application accepts client certificates for authentication"
                        accessSection["sslFlags"] = @"SslNegotiateCert";

                        // write changes
                        serverManager.CommitChanges();
                    }

                    // Configure registry settings to allow better TCP port reuse
                    registry.ConfigureLocalTcpSettings();
                }
            }
            catch (Exception e)
            {
                this.log.LogError(e);
            }

            return base.OnStart();
        }
    }
}
