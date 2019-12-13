// <copyright file="ProdConfiguration.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Utils
{
    using System.Collections.Generic;

    /// <summary>
    /// Utility to check whether a configuration is using production services
    /// </summary>
    public static class ProdConfiguration
    {
        /// <summary>
        /// List of production service names
        /// </summary>
        private static readonly List<string> ProductionServices = new List<string>()
        {
            // include here substrings that will identify production services in the connection string or service name for an Azure service
            "socialplusprod",
            "splusprod",
            "splus-prod",
            "push-sp-prod",
            "spdevbeihai",
            "sp-ppe",
            "spppe",
            "sp-ppe-beihai",
            "spppebeihai",
            "sp-prod-beihai",
            "spprodbeihai",
            "sp-mobisys",
            "spmobisys",
            "sp-api",
            "spapi"
        };

        /// <summary>
        /// Is the setting using a production service?
        /// </summary>
        /// <param name="setting">name of service or connection string</param>
        /// <returns>true if production</returns>
        public static bool IsProduction(string setting)
        {
            // empty strings are always false
            if (string.IsNullOrWhiteSpace(setting))
            {
                return false;
            }

            // go through each production service name
            foreach (string name in ProductionServices)
            {
                if (setting.Contains(name))
                {
                    return true;
                }
            }

            return false;
        }
    }
}