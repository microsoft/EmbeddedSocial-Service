// <copyright file="ConnectionStringProvider.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server
{
    using System.ComponentModel;
    using System.Threading.Tasks;

    using SocialPlus.Server.Blobs;
    using SocialPlus.Server.Queues;
    using SocialPlus.Server.Tables;
    using SocialPlus.Utils;

    /// <summary>
    /// Storage connection string provider
    /// </summary>
    public class ConnectionStringProvider : IConnectionStringProvider
    {
        /// <summary>
        /// Settings reader
        /// </summary>
        private readonly ISettingsReaderAsync settingsReader;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionStringProvider"/> class.
        /// </summary>
        /// <param name="settingsReader">settings reader</param>
        public ConnectionStringProvider(ISettingsReaderAsync settingsReader)
        {
            this.settingsReader = settingsReader;
        }

        /// <summary>
        /// Gets Azure storage connection string for tables
        /// </summary>
        /// <param name="azureStorageInstanceType">Azure storage instance type</param>
        /// <returns>Azure storage connection string</returns>
        public async Task<string> GetTablesAzureStorageConnectionString(SocialPlus.Server.Tables.AzureStorageInstanceType azureStorageInstanceType)
        {
            return await this.settingsReader.ReadValueAsync("AzureStorageConnectionString");
        }

        /// <summary>
        /// Gets Azure storage connection string for blobs
        /// </summary>
        /// <param name="azureStorageInstanceType">Azure storage instance type</param>
        /// <returns>Azure storage connection string</returns>
        public async Task<string> GetBlobsAzureStorageConnectionString(SocialPlus.Server.Blobs.AzureStorageInstanceType azureStorageInstanceType)
        {
            return await this.settingsReader.ReadValueAsync("AzureBlobStorageConnectionString");
        }

        /// <summary>
        /// Gets <c>Redis</c> connection string
        /// </summary>
        /// <param name="redisInstanceType"><c>Redis</c> instance type</param>
        /// <returns><c>Redis</c> connection string</returns>
        public async Task<string> GetRedisConnectionString(RedisInstanceType redisInstanceType)
        {
            switch (redisInstanceType)
            {
                case RedisInstanceType.Persistent:
                    return await this.settingsReader.ReadValueAsync("PersistentRedisConnectionString");
                case RedisInstanceType.Volatile:
                    return await this.settingsReader.ReadValueAsync("VolatileRedisConnectionString");
            }

            throw new InvalidEnumArgumentException("Redis instance type unsupported.");
        }

        /// <summary>
        /// Gets Azure CDN url
        /// </summary>
        /// <param name="azureCdnInstanceType">Azure CDN instance type</param>
        /// <returns>CDN url</returns>
        public async Task<string> GetAzureCdnUrl(AzureCdnInstanceType azureCdnInstanceType)
        {
            return await this.settingsReader.ReadValueAsync("CDNUrl");
        }

        /// <summary>
        /// Gets service bus connection string
        /// </summary>
        /// <param name="serviceBusInstanceType">Service bus instance type</param>
        /// <returns>service bus connection string</returns>
        public async Task<string> GetServiceBusConnectionString(ServiceBusInstanceType serviceBusInstanceType)
        {
            return await this.settingsReader.ReadValueAsync("ServiceBusConnectionString");
        }

        /// <summary>
        /// Gets push notifications connection string
        /// </summary>
        /// <param name="pushNotificationInstanceType">Push notification instance type</param>
        /// <returns>service push notification string</returns>
        public async Task<string> GetPushNotificationsConnectionString(PushNotificationInstanceType pushNotificationInstanceType)
        {
            return await this.settingsReader.ReadValueAsync("PushNotificationsConnectionString");
        }

        /// <summary>
        /// Gets search service name
        /// </summary>
        /// <param name="searchInstanceType">Search instance type</param>
        /// <returns>search instance name</returns>
        public async Task<string> GetSearchServiceName(SearchInstanceType searchInstanceType)
        {
            return await this.settingsReader.ReadValueAsync("SearchServiceName");
        }

        /// <summary>
        /// Gets search admin key
        /// </summary>
        /// <param name="searchInstanceType">Search instance type</param>
        /// <returns>search instance admin key</returns>
        public async Task<string> GetSearchServiceAdminKey(SearchInstanceType searchInstanceType)
        {
            return await this.settingsReader.ReadValueAsync("SearchServiceAdminKey");
        }

        /// <summary>
        /// Gets AVERT URL
        /// </summary>
        /// <param name="avertInstanceType">AVERT instance type</param>
        /// <returns>AVERT instance URL</returns>
        public async Task<string> GetAVERTUrl(AVERTInstanceType avertInstanceType)
        {
            return await this.settingsReader.ReadValueAsync("AVERTUrl");
        }

        /// <summary>
        /// Gets AVERT Key
        /// </summary>
        /// <param name="avertInstanceType">AVERT instance type</param>
        /// <returns>AVERT instance key</returns>
        public async Task<string> GetAVERTKey(AVERTInstanceType avertInstanceType)
        {
            return await this.settingsReader.ReadValueAsync("AVERTKey");
        }

        /// <summary>
        /// Gets AVERT X509 Certificate Thumbprint
        /// </summary>
        /// <param name="avertInstanceType">AVERT instance type</param>
        /// <returns>AVERT instance X509 certificate thumbprint</returns>
        public async Task<string> GetAVERTCertThumbprint(AVERTInstanceType avertInstanceType)
        {
            return await this.settingsReader.ReadValueAsync("AVERTCertThumbprint");
        }

        /// <summary>
        /// Gets the hashing key URL in the key vault
        /// </summary>
        /// <returns>hashing key URL</returns>
        public async Task<string> GetHashingKey()
        {
            return await this.settingsReader.ReadValueAsync("HashingKey");
        }

        /// <summary>
        /// Gets CVS key
        /// </summary>
        /// <param name="cvsInstanceType">CVS instance type</param>
        /// <returns>CVS key</returns>
        public async Task<string> GetCVSKey(CVSInstanceType cvsInstanceType)
        {
            return await this.settingsReader.ReadValueAsync("CVSKey");
        }

        /// <summary>
        /// Gets CVS URL
        /// </summary>
        /// <param name="cvsInstanceType">CVS instance type</param>
        /// <returns>CVS instance URL</returns>
        public async Task<string> GetCVSUrl(CVSInstanceType cvsInstanceType)
        {
            return await this.settingsReader.ReadValueAsync("CVSUrl");
        }

        /// <summary>
        /// Gets CVS X509 Certificate Thumbprint
        /// </summary>
        /// <param name="cvsInstanceType">CVS instance type</param>
        /// <returns>CVS instance X509 certificate thumbprint</returns>
        public async Task<string> GetCVSCertThumbprint(CVSInstanceType cvsInstanceType)
        {
            return await this.settingsReader.ReadValueAsync("CVSX509Thumbprint");
        }
    }
}
