// <copyright file="AADSettings.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Utils
{
    using System.Globalization;

    /// <summary>
    /// Encapsulating all the AAD settings into a data structure for simplicity
    /// </summary>
    public class AADSettings
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AADSettings"/> class.
        /// </summary>
        /// <param name="aadSettingsConnectionString">All AAD settings into a single string like a connection string</param>
        public AADSettings(string aadSettingsConnectionString)
        {
            string[] aadSettings = aadSettingsConnectionString.Split(';');
            this.TenantId = aadSettings[0];
            this.ClientId = aadSettings[1];
            this.AppUri = aadSettings[2];
            this.CertThumbprint = aadSettings[3];
        }

        /// <summary>
        /// Gets azure AD instance name. Should never change
        /// </summary>
        public string AADInstance
        {
            get
            {
                return "https://login.microsoftonline.com/{0}";
            }
        }

        ///
        /// <summary>
        /// Gets tenantId -- our tenant in AAD is typically Microsoft
        /// </summary>
        public string TenantId { get; internal set; }

        /// <summary>
        /// Gets client ID of client app
        /// </summary>
        public string ClientId { get; internal set; }

        /// <summary>
        /// Gets uRI of client app
        /// </summary>
        public string AppUri { get; internal set; }

        /// <summary>
        /// Gets certificate thumbprint
        /// </summary>
        public string CertThumbprint { get; internal set; }

        /// <summary>
        /// Gets the authority which is a combination of AAD instance and tenant id
        /// </summary>
        public string Authority
        {
            get
            {
                return string.Format(CultureInfo.InvariantCulture, this.AADInstance, this.TenantId);
            }
        }
    }
}
