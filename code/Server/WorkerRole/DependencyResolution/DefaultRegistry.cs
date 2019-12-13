// <copyright file="DefaultRegistry.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.DependencyResolution
{
    using System;
    using System.Security.Cryptography.X509Certificates;

    using Microsoft.WindowsAzure.ServiceRuntime;
    using SocialPlus.Logging;
    using SocialPlus.Server.Blobs;
    using SocialPlus.Server.KVLibrary;
    using SocialPlus.Server.Managers;
    using SocialPlus.Server.Queues;
    using SocialPlus.Server.Tables;
    using SocialPlus.Utils;
    using StructureMap.Configuration.DSL;
    using StructureMap.Graph;

    /// <summary>
    /// Default registry for dependency resolution
    /// </summary>
    public class DefaultRegistry : Registry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultRegistry"/> class
        /// </summary>
        public DefaultRegistry()
        {
            bool enableAzureSettingsReaderTracing = false;
            ISettingsReader settingsReader = new AzureSettingsReader(enableAzureSettingsReaderTracing);
            string clientID = settingsReader.ReadValue("AADEmbeddedSocialClientId");
            string vaultUrl = settingsReader.ReadValue("KeyVaultUri");
            string certThumbPrint = settingsReader.ReadValue("SocialPlusCertThumbprint");
            StoreLocation storeLocation = StoreLocation.LocalMachine;
            int serviceBusBatchIntervalMs = int.Parse(settingsReader.ReadValue("ServiceBusBatchIntervalMs"));
            enableAzureSettingsReaderTracing = bool.Parse(settingsReader.ReadValue("EnableAzureSettingsReaderTracing"));

            this.Scan(
                scan =>
                {
                    scan.AssembliesFromPath(Environment.CurrentDirectory);
                    scan.LookForRegistries();
                    scan.WithDefaultConventions();
                });

            // initialize the ILog to use debug output in an emulated role, and event source output in Azure
            Log log;
            if (RoleEnvironment.IsEmulated)
            {
                log = new Log(LogDestination.Debug, Log.DefaultCategoryName);
            }
            else
            {
                log = new Log(LogDestination.EventSource, Log.DefaultCategoryName);
            }

            this.For<ILog>().Singleton().Use(log);

            // This SettingsReader does not use KV; it's used to read regular (not protected) cloud configuration settings
            this.For<ISettingsReader>().Singleton().Use<AzureSettingsReader>().Ctor<bool>("isTracingEnabled").Is(enableAzureSettingsReaderTracing);

            // Initialize KV, asynchronous SettingsReader, ConnectionStringProvider, CTStore, CBStore, QueueManagers
            this.For<ICertificateHelper>().Singleton().Use<CertificateHelper>().Ctor<string>("certThumbprint").Is(certThumbPrint)
                .Ctor<string>("clientID").Is(clientID)
                .Ctor<StoreLocation>("storeLocation").Is(storeLocation);
            this.For<IKeyVaultClient>().Singleton().Use<AzureKeyVaultClient>();
            this.For<IKV>().Singleton().Use<KV>().Ctor<string>("clientID").Is(clientID)
                .Ctor<string>("vaultUrl").Is(vaultUrl)
                .Ctor<string>("certThumbprint").Is(certThumbPrint)
                .Ctor<StoreLocation>("storeLocation").Is(storeLocation);
            this.For<ISettingsReaderAsync>().Singleton().Use<KVSettingsReader>();
            this.For<IConnectionStringProvider>().Singleton().Use<ConnectionStringProvider>();
            this.For<ICTStoreManager>().Singleton().Use<CTStoreManager>();
            this.For<ICBStoreManager>().Singleton().Use<CBStoreManager>();
            this.For<IQueueManager>().Singleton().Use<QueueManager>().Ctor<int>("batchIntervalMs").Is(serviceBusBatchIntervalMs);

            this.For<ISessionTokenManager>().Use<SessionTokenManager>();
            this.For<IAppsManager>().Use<AppsManager>();
            this.For<IIdentitiesManager>().Singleton().Use<IdentitiesManager>();
            this.For<IUsersManager>().Use<UsersManager>();
            this.For<IAppsManager>().Use<AppsManager>();
            this.For<ITopicsManager>().Use<TopicsManager>();
            this.For<IViewsManager>().Use<ViewsManager>();
            this.For<IRelationshipsManager>().Use<RelationshipsManager>();
            this.For<ILikesManager>().Use<LikesManager>();
            this.For<IPinsManager>().Use<PinsManager>();
            this.For<ICommentsManager>().Use<CommentsManager>();
            this.For<IRepliesManager>().Use<RepliesManager>();
            this.For<IActivitiesManager>().Use<ActivitiesManager>();
            this.For<INotificationsManager>().Use<NotificationsManager>();
            this.For<ISearchManager>().Singleton().Use<SearchManager>();
            this.For<IBlobsManager>().Use<BlobsManager>();
            this.For<IPushNotificationsManager>().Use<PushNotificationsManager>();
            this.For<IPopularTopicsManager>().Use<PopularTopicsManager>();
            this.For<IPopularUsersManager>().Use<PopularUsersManager>();
            this.For<ITopicNamesManager>().Use<TopicNamesManager>();
            this.For<IReportsManager>().Use<ReportsManager>();

            this.For<IHandleGenerator>().Use<HandleGenerator>();

            this.For<ITopicsStore>().Use<TopicsStore>();
            this.For<IUsersStore>().Use<UsersStore>();
            this.For<IAppsStore>().Use<AppsStore>();
            this.For<ILikesStore>().Use<LikesStore>();
            this.For<IPinsStore>().Use<PinsStore>();
            this.For<IUserRelationshipsStore>().Use<UserRelationshipsStore>();
            this.For<ITopicRelationshipsStore>().Use<TopicRelationshipsStore>();
            this.For<ICommentsStore>().Use<CommentsStore>();
            this.For<IRepliesStore>().Use<RepliesStore>();
            this.For<INotificationsStore>().Use<NotificationsStore>();
            this.For<IActivitiesStore>().Use<ActivitiesStore>();
            this.For<IBlobsMetadataStore>().Use<BlobsMetadataStore>();
            this.For<IPushRegistrationsStore>().Use<PushRegistrationsStore>();
            this.For<IBlobsStore>().Use<BlobsStore>();
            this.For<ITopicNamesStore>().Use<TopicNamesStore>();
            this.For<IContentReportsStore>().Use<ContentReportsStore>();
            this.For<IUserReportsStore>().Use<UserReportsStore>();
            this.For<IAVERTStore>().Use<AVERTStore>();

            this.For<IFanoutTopicsQueue>().Use<FanoutTopicsQueue>();
            this.For<IFanoutActivitiesQueue>().Use<FanoutActivitiesQueue>();
            this.For<IFollowingImportsQueue>().Use<FollowingImportsQueue>();
            this.For<IResizeImagesQueue>().Use<ResizeImagesQueue>();
            this.For<ILikesQueue>().Use<LikesQueue>();
            this.For<IRelationshipsQueue>().Use<RelationshipsQueue>();
            this.For<IReportsQueue>().Use<ReportsQueue>();
            this.For<ISearchQueue>().Use<SearchQueue>();
        }
    }
}
