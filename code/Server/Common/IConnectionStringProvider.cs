// <copyright file="IConnectionStringProvider.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server
{
    using System.Threading.Tasks;

    using SocialPlus.Server.Blobs;
    using SocialPlus.Server.Queues;
    using SocialPlus.Server.Tables;

    /// <summary>
    /// interface of connection string provider
    /// </summary>
    public interface IConnectionStringProvider
    {
        /// <summary>
        /// Gets Azure storage connection string for tables
        /// </summary>
        /// <param name="azureStorageInstanceType">Azure storage instance type</param>
        /// <returns>Azure storage connection string</returns>
        Task<string> GetTablesAzureStorageConnectionString(SocialPlus.Server.Tables.AzureStorageInstanceType azureStorageInstanceType);

        /// <summary>
        /// Gets Azure storage connection string for blobs
        /// </summary>
        /// <param name="azureStorageInstanceType">Azure storage instance type</param>
        /// <returns>Azure storage connection string</returns>
        Task<string> GetBlobsAzureStorageConnectionString(SocialPlus.Server.Blobs.AzureStorageInstanceType azureStorageInstanceType);

        /// <summary>
        /// Gets <c>Redis</c> connection string
        /// </summary>
        /// <param name="redisInstanceType"><c>Redis</c> instance type</param>
        /// <returns><c>Redis</c> connection string</returns>
        Task<string> GetRedisConnectionString(RedisInstanceType redisInstanceType);

        /// <summary>
        /// Gets Azure CDN URL
        /// </summary>
        /// <param name="azureCdnInstanceType">Azure CDN instance type</param>
        /// <returns>CDN URL</returns>
        Task<string> GetAzureCdnUrl(AzureCdnInstanceType azureCdnInstanceType);

        /// <summary>
        /// Gets service bus connection string
        /// </summary>
        /// <param name="serviceBusInstanceType">Service bus instance type</param>
        /// <returns>service bus connection string</returns>
        Task<string> GetServiceBusConnectionString(ServiceBusInstanceType serviceBusInstanceType);

        /// <summary>
        /// Gets push notifications connection string
        /// </summary>
        /// <param name="pushNotificationInstanceType">Push notification instance type</param>
        /// <returns>service push notification string</returns>
        Task<string> GetPushNotificationsConnectionString(PushNotificationInstanceType pushNotificationInstanceType);

        /// <summary>
        /// Gets search service name
        /// </summary>
        /// <param name="searchInstanceType">Search instance type</param>
        /// <returns>search instance name</returns>
        Task<string> GetSearchServiceName(SearchInstanceType searchInstanceType);

        /// <summary>
        /// Gets search admin key
        /// </summary>
        /// <param name="searchInstanceType">Search instance type</param>
        /// <returns>search instance admin key</returns>
        Task<string> GetSearchServiceAdminKey(SearchInstanceType searchInstanceType);

        /// <summary>
        /// Gets AVERT URL
        /// </summary>
        /// <param name="avertInstanceType">AVERT instance type</param>
        /// <returns>AVERT instance URL</returns>
        Task<string> GetAVERTUrl(AVERTInstanceType avertInstanceType);

        /// <summary>
        /// Gets AVERT Key
        /// </summary>
        /// <param name="avertInstanceType">AVERT instance type</param>
        /// <returns>AVERT instance key</returns>
        Task<string> GetAVERTKey(AVERTInstanceType avertInstanceType);

        /// <summary>
        /// Gets AVERT X509 Certificate Thumbprint
        /// </summary>
        /// <param name="avertInstanceType">AVERT instance type</param>
        /// <returns>AVERT instance X509 certificate thumbprint</returns>
        Task<string> GetAVERTCertThumbprint(AVERTInstanceType avertInstanceType);

        /// <summary>
        /// Gets the hashing key URL in the key vault
        /// </summary>
        /// <returns>hashing key URL</returns>
        Task<string> GetHashingKey();

        /// <summary>
        /// Gets CVS key
        /// </summary>
        /// <param name="cvsInstanceType">CVS instance type</param>
        /// <returns>CVS key</returns>
        Task<string> GetCVSKey(CVSInstanceType cvsInstanceType);

        /// <summary>
        /// Gets CVS URL
        /// </summary>
        /// <param name="cvsInstanceType">CVS instance type</param>
        /// <returns>CVS instance URL</returns>
        Task<string> GetCVSUrl(CVSInstanceType cvsInstanceType);

        /// <summary>
        /// Gets CVS X509 Certificate Thumbprint
        /// </summary>
        /// <param name="cvsInstanceType">CVS instance type</param>
        /// <returns>CVS instance X509 certificate thumbprint</returns>
        Task<string> GetCVSCertThumbprint(CVSInstanceType cvsInstanceType);
    }
}
