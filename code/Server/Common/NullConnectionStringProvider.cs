// <copyright file="NullConnectionStringProvider.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server
{
    using System.Threading.Tasks;

    using SocialPlus.Server.Blobs;
    using SocialPlus.Server.Queues;
    using SocialPlus.Server.Tables;

    /// <summary>
    /// Storage connection string provider that returns null on all its methods.
    /// Used by projects that do not implement StructureMap.
    /// </summary>
    public class NullConnectionStringProvider : IConnectionStringProvider
    {
        /// <summary>
        /// Gets Azure storage connection string for tables. This method returns null.
        /// </summary>
        /// <param name="azureStorageInstanceType">Azure storage instance type</param>
        /// <returns>returns null</returns>
        public Task<string> GetTablesAzureStorageConnectionString(SocialPlus.Server.Tables.AzureStorageInstanceType azureStorageInstanceType)
        {
            return null;
        }

        /// <summary>
        /// Gets Azure storage connection string for blobs. This method returns null.
        /// </summary>
        /// <param name="azureStorageInstanceType">Azure storage instance type</param>
        /// <returns>returns null</returns>
        public Task<string> GetBlobsAzureStorageConnectionString(SocialPlus.Server.Blobs.AzureStorageInstanceType azureStorageInstanceType)
        {
            return null;
        }

        /// <summary>
        /// Gets <c>Redis</c> connection string. This method returns null.
        /// </summary>
        /// <param name="redisInstanceType"><c>Redis</c> instance type</param>
        /// <returns>returns null</returns>
        public Task<string> GetRedisConnectionString(RedisInstanceType redisInstanceType)
        {
            return null;
        }

        /// <summary>
        /// Gets Azure CDN url. This method returns null.
        /// </summary>
        /// <param name="azureCdnInstanceType">Azure CDN instance type</param>
        /// <returns>returns null</returns>
        public Task<string> GetAzureCdnUrl(AzureCdnInstanceType azureCdnInstanceType)
        {
            return null;
        }

        /// <summary>
        /// Gets service bus connection string. This method returns null.
        /// </summary>
        /// <param name="serviceBusInstanceType">Service bus instance type</param>
        /// <returns>returns null</returns>
        public Task<string> GetServiceBusConnectionString(ServiceBusInstanceType serviceBusInstanceType)
        {
            return null;
        }

        /// <summary>
        /// Gets push notifications connection string
        /// </summary>
        /// <param name="pushNotificationInstanceType">Push notification instance type</param>
        /// <returns>returns null</returns>
        public Task<string> GetPushNotificationsConnectionString(PushNotificationInstanceType pushNotificationInstanceType)
        {
            return null;
        }

        /// <summary>
        /// Gets search service name
        /// </summary>
        /// <param name="searchInstanceType">Search instance type</param>
        /// <returns>returns null</returns>
        public Task<string> GetSearchServiceName(SearchInstanceType searchInstanceType)
        {
            return null;
        }

        /// <summary>
        /// Gets search admin key
        /// </summary>
        /// <param name="searchInstanceType">Search instance type</param>
        /// <returns>returns null</returns>
        public Task<string> GetSearchServiceAdminKey(SearchInstanceType searchInstanceType)
        {
            return null;
        }

        /// <summary>
        /// Gets AVERT URL
        /// </summary>
        /// <param name="avertInstanceType">AVERT instance type</param>
        /// <returns>returns null</returns>
        public Task<string> GetAVERTUrl(AVERTInstanceType avertInstanceType)
        {
            return null;
        }

        /// <summary>
        /// Gets AVERT Key
        /// </summary>
        /// <param name="avertInstanceType">AVERT instance type</param>
        /// <returns>returns null</returns>
        public Task<string> GetAVERTKey(AVERTInstanceType avertInstanceType)
        {
            return null;
        }

        /// <summary>
        /// Gets AVERT X509 certificate thumbprint
        /// </summary>
        /// <param name="avertInstanceType">AVERT instance type</param>
        /// <returns>returns null</returns>
        public Task<string> GetAVERTCertThumbprint(AVERTInstanceType avertInstanceType)
        {
            return null;
        }

        /// <summary>
        /// Gets the hashing key URL in the key vault
        /// </summary>
        /// <returns>hashing key URL</returns>
        public Task<string> GetHashingKey()
        {
            return null;
        }

        /// <summary>
        /// Gets CVS key
        /// </summary>
        /// <param name="cvsInstanceType">CVS instance type</param>
        /// <returns>CVS key</returns>
        public Task<string> GetCVSKey(CVSInstanceType cvsInstanceType)
        {
            return null;
        }

        /// <summary>
        /// Gets CVS URL
        /// </summary>
        /// <param name="cvsInstanceType">CVS instance type</param>
        /// <returns>CVS instance URL</returns>
        public Task<string> GetCVSUrl(CVSInstanceType cvsInstanceType)
        {
            return null;
        }

        /// <summary>
        /// Gets CVS X509 Certificate Thumbprint
        /// </summary>
        /// <param name="cvsInstanceType">CVS instance type</param>
        /// <returns>CVS instance X509 certificate thumbprint</returns>
        public Task<string> GetCVSCertThumbprint(CVSInstanceType cvsInstanceType)
        {
            return null;
        }
    }
}
