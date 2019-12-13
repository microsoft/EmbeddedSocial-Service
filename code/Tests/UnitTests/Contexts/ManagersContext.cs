// <copyright file="ManagersContext.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.UnitTests
{
    using SocialPlus.OAuth;
    using SocialPlus.Server.Managers;
    using SocialPlus.Server.Metrics;
    using SocialPlus.Server.Queues;

    /// <summary>
    /// Class encapsulating the context needed for managers
    /// </summary>
    public class ManagersContext : StoresContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ManagersContext"/> class.
        /// </summary>
        public ManagersContext()
            : base()
        {
            // for the queue manager
            int serviceBusBatchIntervalMs = int.Parse(this.FileSettingsReader.ReadValue("ServiceBusBatchIntervalMs"));

            // Initialize the queues
            this.QueueManager = new QueueManager(this.ConnectionStringProvider, serviceBusBatchIntervalMs);
            this.FanoutActivitiesQueue = new FanoutActivitiesQueue(this.QueueManager);
            this.FanoutTopicsQueue = new FanoutTopicsQueue(this.QueueManager);
            this.FollowingImportsQueue = new FollowingImportsQueue(this.QueueManager);
            this.ModerationQueue = new ModerationQueue(this.QueueManager);
            this.RelationshipsQueue = new RelationshipsQueue(this.QueueManager);
            this.ResizeImagesQueue = new ResizeImagesQueue(this.QueueManager);
            this.SearchQueue = new SearchQueue(this.QueueManager);

            // Initializes the managers
            this.AppsManager = new AppsManager(this.AppsStore);
            this.PopularUsersManager = new PopularUsersManager(this.UsersStore);
            this.ViewsManager = new ViewsManager(
                this.Log,
                this.AppsStore,
                this.UsersStore,
                this.UserRelationshipsStore,
                this.TopicsStore,
                this.TopicRelationshipsStore,
                this.CommentsStore,
                this.RepliesStore,
                this.LikesStore,
                this.PinsStore,
                this.BlobsStore);
            this.PushNotificationsManager = new PushNotificationsManager(
                this.Log,
                this.PushRegistrationStore,
                this.AppsStore,
                this.ViewsManager,
                this.ConnectionStringProvider);
            this.UsersManager = new UsersManager(
                this.UsersStore,
                this.PushNotificationsManager,
                this.PopularUsersManager,
                this.SearchQueue);
            this.AADAuthManager = new AADAuthManager(this.Log, this.AppsManager, this.UsersManager);
            this.SessionTokenManager = new SessionTokenManager(this.KeyVault, this.ConnectionStringProvider);
            this.SocialPlusAuthManager = new SocialPlusAuthManager(
                this.Log,
                this.AppsManager,
                this.UsersManager,
                this.SessionTokenManager);
            this.ActivitiesManager = new ActivitiesManager(
                this.ActivitiesStore,
                this.UserRelationshipsStore,
                this.TopicRelationshipsStore);
            this.AnonAuthManager = new AnonAuthManager(this.Log, this.AppsManager, this.UsersManager);
            this.MSAAuthManager = new OAuthManager(this.Log, this.AppsManager, this.UsersManager, IdentityProviders.Microsoft);
            this.FBAuthManager = new OAuthManager(this.Log, this.AppsManager, this.UsersManager, IdentityProviders.Facebook);
            this.GAuthManager = new OAuthManager(this.Log, this.AppsManager, this.UsersManager, IdentityProviders.Google);
            this.TAuthManager = new OAuthManager(this.Log, this.AppsManager, this.UsersManager, IdentityProviders.Twitter);
            this.AuthManager = new CompositeAuthManager(
                this.AADAuthManager,
                this.SocialPlusAuthManager,
                this.AnonAuthManager,
                this.MSAAuthManager,
                this.FBAuthManager,
                this.GAuthManager,
                this.TAuthManager);
            this.CommonAuthManager = new CommonAuthManager(this.Log, this.AppsManager, this.UsersManager);
            this.NotificationsManager = new NotificationsManager(this.NotificationsStore, this.PushNotificationsManager);
            this.CommentsManager = new CommentsManager(this.CommentsStore, this.FanoutActivitiesQueue, this.NotificationsManager);
            this.RepliesManager = new RepliesManager(this.RepliesStore, this.FanoutActivitiesQueue, this.NotificationsManager);
            this.IdentitiesManager = new IdentitiesManager();
            this.PopularTopicsManager = new PopularTopicsManager(this.TopicsStore, this.UsersStore, this.AppsStore);
            this.RelationshipsManager = new RelationshipsManager(
                this.Log,
                this.UserRelationshipsStore,
                this.TopicRelationshipsStore,
                this.RelationshipsQueue,
                this.FanoutActivitiesQueue,
                this.FollowingImportsQueue,
                this.PopularUsersManager,
                this.NotificationsManager);
            this.SearchManager = new SearchManager(this.Log, this.ConnectionStringProvider);
            this.TopicsManager = new TopicsManager(
                this.TopicsStore,
                this.UserRelationshipsStore,
                this.FanoutTopicsQueue,
                this.SearchQueue,
                this.PopularTopicsManager);
            this.TopicNamesManager = new TopicNamesManager(this.TopicNamesStore);
            this.BlobsManager = new BlobsManager(this.Log, this.BlobsStore, this.BlobsMetadataStore, this.ResizeImagesQueue);
            this.CVSModerationManager = new CVSModerationManager(
                this.Log,
                this.UsersManager,
                this.CVSTransactionStore,
                this.ModerationStore,
                this.ModerationQueue,
                this.BlobsManager,
                this.TopicsManager,
                this.CommentsManager,
                this.RepliesManager,
                this.AppsManager,
                this.ConnectionStringProvider);

            // Metrics managers
            this.ApplicationMetrics = new LogApplicationMetrics(this.Log);
        }

        /// <summary>
        /// Gets queue manager
        /// </summary>
        public QueueManager QueueManager { get; private set; }

        /// <summary>
        /// Gets fanout activities queue for comments
        /// </summary>
        public FanoutActivitiesQueue FanoutActivitiesQueue { get; private set; }

        /// <summary>
        /// Gets fanout topics queue
        /// </summary>
        public FanoutTopicsQueue FanoutTopicsQueue { get; private set; }

        /// <summary>
        /// Gets following imports queue
        /// </summary>
        public FollowingImportsQueue FollowingImportsQueue { get; private set; }

        /// <summary>
        /// Gets moderation queue
        /// </summary>
        public ModerationQueue ModerationQueue { get; private set; }

        /// <summary>
        /// Gets relationships queue
        /// </summary>
        public RelationshipsQueue RelationshipsQueue { get; private set; }

        /// <summary>
        /// Gets resize images queue
        /// </summary>
        public ResizeImagesQueue ResizeImagesQueue { get; private set; }

        /// <summary>
        /// Gets search queue
        /// </summary>
        public SearchQueue SearchQueue { get; private set; }

        /// <summary>
        /// Gets AAD auth manager
        /// </summary>
        public AADAuthManager AADAuthManager { get; private set; }

        /// <summary>
        /// Gets activities manager
        /// </summary>
        public ActivitiesManager ActivitiesManager { get; private set; }

        /// <summary>
        /// Gets Anon auth manager
        /// </summary>
        public AnonAuthManager AnonAuthManager { get; private set; }

        /// <summary>
        /// Gets apps manager
        /// </summary>
        public AppsManager AppsManager { get; private set; }

        /// <summary>
        /// Gets auth manager
        /// </summary>
        public CompositeAuthManager AuthManager { get; private set; }

        /// <summary>
        /// Gets blobs manager
        /// </summary>
        public BlobsManager BlobsManager { get; private set; }

        /// <summary>
        /// Gets comments manager
        /// </summary>
        public CommentsManager CommentsManager { get; private set; }

        /// <summary>
        /// Gets CVS moderation manager
        /// </summary>
        public CVSModerationManager CVSModerationManager { get; private set; }

        /// <summary>
        /// Gets Facebook auth manager
        /// </summary>
        public OAuthManager FBAuthManager { get; private set; }

        /// <summary>
        /// Gets Google auth manager
        /// </summary>
        public OAuthManager GAuthManager { get; private set; }

        /// <summary>
        /// Gets identities manager
        /// </summary>
        public IdentitiesManager IdentitiesManager { get; private set; }

        /// <summary>
        /// Gets MSA auth manager
        /// </summary>
        public OAuthManager MSAAuthManager { get; private set; }

        /// <summary>
        /// Gets Common auth manager
        /// </summary>
        public CommonAuthManager CommonAuthManager { get; private set; }

        /// <summary>
        /// Gets notifications manager for comments
        /// </summary>
        public NotificationsManager NotificationsManager { get; private set; }

        /// <summary>
        /// Gets popular topics manager
        /// </summary>
        public PopularTopicsManager PopularTopicsManager { get; private set; }

        /// <summary>
        /// Gets popular users manager
        /// </summary>
        public PopularUsersManager PopularUsersManager { get; private set; }

        /// <summary>
        /// Gets push notification manager
        /// </summary>
        public PushNotificationsManager PushNotificationsManager { get; private set; }

        /// <summary>
        /// Gets relationships manager
        /// </summary>
        public RelationshipsManager RelationshipsManager { get; private set; }

        /// <summary>
        /// Gets replies manager
        /// </summary>
        public RepliesManager RepliesManager { get; private set; }

        /// <summary>
        /// Gets search manager
        /// </summary>
        public SearchManager SearchManager { get; private set; }

        /// <summary>
        /// Gets session token manager
        /// </summary>
        public SessionTokenManager SessionTokenManager { get; private set; }

        /// <summary>
        /// Gets SocialPlus auth manager
        /// </summary>
        public SocialPlusAuthManager SocialPlusAuthManager { get; private set; }

        /// <summary>
        /// Gets Twitter auth manager
        /// </summary>
        public OAuthManager TAuthManager { get; private set; }

        /// <summary>
        /// Gets topics manager
        /// </summary>
        public TopicsManager TopicsManager { get; private set; }

        /// <summary>
        /// Gets topic names manager
        /// </summary>
        public TopicNamesManager TopicNamesManager { get; private set; }

        /// <summary>
        /// Gets users manager
        /// </summary>
        public UsersManager UsersManager { get; private set; }

        /// <summary>
        /// Gets views manager
        /// </summary>
        public ViewsManager ViewsManager { get; private set; }

        /// <summary>
        /// Gets application metrics
        /// </summary>
        public LogApplicationMetrics ApplicationMetrics { get; private set; }
    }
}
