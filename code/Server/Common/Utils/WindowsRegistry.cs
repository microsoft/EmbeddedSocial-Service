// <copyright file="WindowsRegistry.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Utils
{
    using Microsoft.Win32;
    using SocialPlus.Logging;

    /// <summary>
    /// Server utils class that manipulates registry settings for the WebRole and WorkerRole servers.
    /// </summary>
    public class WindowsRegistry
    {
        /// <summary>
        /// Log
        /// </summary>
        private readonly ILog log;

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowsRegistry"/> class
        /// </summary>
        /// <param name="log">log</param>
        public WindowsRegistry(ILog log)
        {
            this.log = log;
        }

        /// <summary>
        /// Use the registry to modify TCP port reuse behavior: set the maximum port number for ephemeral ports to 65534,
        /// and set the TCP timed wait delay to 30 seconds.
        /// </summary>
        public void ConfigureLocalTcpSettings()
        {
            RegistryKey rkbase = null;
            rkbase = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
            using (RegistryKey tcpParamsKey = rkbase.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters", true))
            {
                if (tcpParamsKey != null)
                {
                    tcpParamsKey.SetValue("MaxUserPort", 65534, RegistryValueKind.DWord);
                    int maxUserPort = (int)tcpParamsKey.GetValue("MaxUserPort", 0);

                    tcpParamsKey.SetValue("TcpTimedWaitDelay", 30, RegistryValueKind.DWord);
                    int tcpTimedWaitDelay = (int)tcpParamsKey.GetValue("TcpTimedWaitDelay", 0);

                    this.log.LogInformation(string.Format("Set Registry values for MaxUserPort = {0}, TcpTimedWaitDelay = {1}", maxUserPort, tcpTimedWaitDelay));
                }
            }
        }
    }
}
