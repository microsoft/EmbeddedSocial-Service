// <copyright file="StoresContext.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.UnitTests
{
    using System;
    using System.Threading.Tasks;

    using SocialPlus.Server.Blobs;
    using SocialPlus.Server.Managers;
    using SocialPlus.Server.Tables;

    /// <summary>
    /// Class encapsulating the context needed for stores
    /// </summary>
    public class StoresContext : ConnectionStringContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StoresContext"/> class.
        /// </summary>
        public StoresContext()
            : base()
        {
            // Initialize CBStore
            this.CBStoreManager = new CBStoreManager(this.ConnectionStringProvider);

            // Initialize CTStore
            bool tableInit = false;
            Exception exception = null;
            try
            {
                // initialize CTStore after the global exception logger
                this.CTStoreManager = new CTStoreManager(this.ConnectionStringProvider);

                // use Task.Run to ensure that the async Initialize routine runs on a threadpool thread
                Task<bool> task = Task<bool>.Run(() => this.CTStoreManager.Initialize());

                // task.Result blocks until the result is ready
                tableInit = task.Result;
            }
            catch (Exception e)
            {
                exception = e;
            }

            if (tableInit == false)
            {
                string errorMessage = "CTstore version number does not match the expected version number." + Environment.NewLine +
                    "If your tables are empty, then you probably forgot to provision storage." + Environment.NewLine +
                    "If not, then you need to convert the data format and update the storage version number.";
                throw new Exception(errorMessage, exception);
            }

            // Initialize the stores
            this.ActivitiesStore = new ActivitiesStore(this.CTStoreManager);
            this.AppsStore = new AppsStore(this.CTStoreManager);
            this.BlobsStore = new BlobsStore(this.CBStoreManager);
            this.BlobsMetadataStore = new BlobsMetadataStore(this.CTStoreManager);
            this.CommentsStore = new CommentsStore(this.CTStoreManager);
            this.CVSTransactionStore = new CVSTransactionStore(this.CTStoreManager);
            this.LikesStore = new LikesStore(this.CTStoreManager);
            this.ModerationStore = new ModerationStore(this.CTStoreManager);
            this.NotificationsStore = new NotificationsStore(this.Log, this.CTStoreManager);
            this.PinsStore = new PinsStore(this.CTStoreManager);
            this.PushRegistrationStore = new PushRegistrationsStore(this.CTStoreManager);
            this.RepliesStore = new RepliesStore(this.CTStoreManager);
            this.TopicNamesStore = new TopicNamesStore(this.CTStoreManager);
            this.TopicRelationshipsStore = new TopicRelationshipsStore(this.CTStoreManager);
            this.TopicsStore = new TopicsStore(this.CTStoreManager);
            this.UserRelationshipsStore = new UserRelationshipsStore(this.CTStoreManager);
            this.UsersStore = new UsersStore(this.CTStoreManager);
        }

        /// <summary>
        /// Gets CB store manager
        /// </summary>
        public CBStoreManager CBStoreManager { get; private set; }

        /// <summary>
        /// Gets CT store manager
        /// </summary>
        public CTStoreManager CTStoreManager { get; private set; }

        /// <summary>
        /// Gets activities store
        /// </summary>
        public ActivitiesStore ActivitiesStore { get; private set; }

        /// <summary>
        /// Gets apps store
        /// </summary>
        public AppsStore AppsStore { get; private set; }

        /// <summary>
        /// Gets blobs store
        /// </summary>
        public BlobsStore BlobsStore { get; private set; }

        /// <summary>
        /// Gets blobs metadata store
        /// </summary>
        public BlobsMetadataStore BlobsMetadataStore { get; private set; }

        /// <summary>
        /// Gets comments store
        /// </summary>
        public CommentsStore CommentsStore { get; private set; }

        /// <summary>
        /// Gets CVS transaction store
        /// </summary>
        public CVSTransactionStore CVSTransactionStore { get; private set; }

        /// <summary>
        /// Gets likes store
        /// </summary>
        public LikesStore LikesStore { get; private set; }

        /// <summary>
        /// Gets moderation store
        /// </summary>
        public ModerationStore ModerationStore { get; private set; }

        /// <summary>
        /// Gets notifications store
        /// </summary>
        public NotificationsStore NotificationsStore { get; private set; }

        /// <summary>
        /// Gets pins store
        /// </summary>
        public PinsStore PinsStore { get; private set; }

        /// <summary>
        /// Gets push registration store
        /// </summary>
        public PushRegistrationsStore PushRegistrationStore { get; private set; }

        /// <summary>
        /// Gets replies store
        /// </summary>
        public RepliesStore RepliesStore { get; private set; }

        /// <summary>
        /// Gets topic relationships store
        /// </summary>
        public TopicRelationshipsStore TopicRelationshipsStore { get; private set; }

        /// <summary>
        /// Gets topic names store
        /// </summary>
        public TopicNamesStore TopicNamesStore { get; private set; }

        /// <summary>
        /// Gets topics store
        /// </summary>
        public TopicsStore TopicsStore { get; private set; }

        /// <summary>
        /// Gets user relationships store
        /// </summary>
        public UserRelationshipsStore UserRelationshipsStore { get; private set; }

        /// <summary>
        /// Gets users store
        /// </summary>
        public UsersStore UsersStore { get; private set; }
    }
}
