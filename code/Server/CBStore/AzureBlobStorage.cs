// <copyright file="AzureBlobStorage.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.CBStore
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;

    /// <summary>
    /// Azure blob Storage as persistent store
    /// </summary>
    public class AzureBlobStorage : IPersistentStore
    {
        /// <summary>
        /// Connection string to Azure blob storage
        /// </summary>
        private string connectionString;

        /// <summary>
        /// Cloud storage account
        /// </summary>
        private CloudStorageAccount cloudStorageAccount;

        /// <summary>
        /// Default blob request options
        /// </summary>
        private BlobRequestOptions blobRequestOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureBlobStorage"/> class
        /// </summary>
        /// <param name="connectionString">Azure blob storage connection string</param>
        public AzureBlobStorage(string connectionString)
        {
            this.connectionString = connectionString;
        }

        /// <summary>
        /// Gets or sets connection string to Azure blob storage
        /// </summary>
        public string ConnectionString
        {
            get
            {
                return this.connectionString;
            }

            set
            {
                this.connectionString = value;
            }
        }

        /// <summary>
        /// Gets cloud storage account
        /// </summary>
        public CloudStorageAccount CloudStorageAccount
        {
            get
            {
                if (this.cloudStorageAccount == null)
                {
                    this.cloudStorageAccount = CloudStorageAccount.Parse(this.ConnectionString);
                }

                return this.cloudStorageAccount;
            }
        }

        /// <summary>
        /// Gets or sets blob request options
        /// </summary>
        public BlobRequestOptions BlobRequestOptions
        {
            get
            {
                return this.blobRequestOptions;
            }

            set
            {
                this.blobRequestOptions = value;
            }
        }

        /// <summary>
        /// Create container
        /// </summary>
        /// <param name="containerName">Container name</param>
        /// <returns>True on success</returns>
        public async Task<bool> CreateContainerAsync(string containerName)
        {
            CloudBlobClient blobClient = this.CloudStorageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);
            return await container.CreateIfNotExistsAsync(BlobContainerPublicAccessType.Blob, this.blobRequestOptions, null);
        }

        /// <summary>
        /// Delete container
        /// </summary>
        /// <param name="containerName">Container name</param>
        /// <returns>True on success</returns>
        public async Task<bool> DeleteContainerAsync(string containerName)
        {
            CloudBlobClient blobClient = this.CloudStorageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);
            return await container.DeleteIfExistsAsync(null, this.blobRequestOptions, null);
        }

        /// <summary>
        /// Create blob async
        /// </summary>
        /// <param name="containerName">Container name</param>
        /// <param name="blobName">Blob name</param>
        /// <param name="stream">Blob stream</param>
        /// <param name="contentType">Content type</param>
        /// <param name="cacheTTL">Cache time to live</param>
        /// <returns>Create blob task</returns>
        public async Task CreateBlobAsync(string containerName, string blobName, Stream stream, string contentType, TimeSpan cacheTTL)
        {
            CloudBlobClient blobClient = this.CloudStorageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);
            CloudBlockBlob blob = container.GetBlockBlobReference(blobName);
            blob.Properties.ContentType = contentType;
            blob.Properties.CacheControl = "public, max-age=" + cacheTTL.TotalSeconds.ToString();
            await blob.UploadFromStreamAsync(stream, null, this.blobRequestOptions, null);
        }

        /// <summary>
        /// Delete blob async
        /// </summary>
        /// <param name="containerName">Container name</param>
        /// <param name="blobName">Blob name</param>
        /// <returns>Delete blob task</returns>
        public async Task DeleteBlobAsync(string containerName, string blobName)
        {
            CloudBlobClient blobClient = this.CloudStorageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);
            CloudBlockBlob blob = container.GetBlockBlobReference(blobName);
            await blob.DeleteAsync(DeleteSnapshotsOption.None, null, this.blobRequestOptions, null);
        }

        /// <summary>
        /// Query blob async
        /// </summary>
        /// <param name="containerName">Container name</param>
        /// <param name="blobName">Blob name</param>
        /// <returns>Blob stream</returns>
        public async Task<Blob> QueryBlobAsync(string containerName, string blobName)
        {
            CloudBlobClient blobClient = this.CloudStorageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobName);

            Blob blob = new Blob();
            MemoryStream stream = new MemoryStream();
            await blockBlob.DownloadToStreamAsync(stream, null, this.blobRequestOptions, null);
            blob.Stream = stream;
            blob.ContentType = blockBlob.Properties.ContentType;
            return blob;
        }

        /// <summary>
        /// Check if container exits
        /// </summary>
        /// <param name="containerName">Container name</param>
        /// <returns>A value indicating whether container exists</returns>
        public async Task<bool> ContainerExists(string containerName)
        {
            CloudBlobClient blobClient = this.CloudStorageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);
            return await container.ExistsAsync(this.blobRequestOptions, null);
        }

        /// <summary>
        /// Check if blob exits
        /// </summary>
        /// <param name="containerName">Container name</param>
        /// <param name="blobName">Blob name</param>
        /// <returns>A value indicating whether blob exists</returns>
        public async Task<bool> BlobExists(string containerName, string blobName)
        {
            CloudBlobClient blobClient = this.CloudStorageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);
            CloudBlockBlob blob = container.GetBlockBlobReference(blobName);
            return await blob.ExistsAsync(this.blobRequestOptions, null);
        }

        /// <summary>
        /// Query persistent url
        /// </summary>
        /// <param name="containerName">Container name</param>
        /// <param name="blobName">Blob name</param>
        /// <returns>Persistent url</returns>
        public Uri QueryPersistentUrl(string containerName, string blobName)
        {
            throw new NotImplementedException();
        }
    }
}
