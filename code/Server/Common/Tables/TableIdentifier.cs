// <copyright file="TableIdentifier.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Tables
{
    /// <summary>
    /// Table identifiers
    /// </summary>
    /// <remarks>
    /// The suffix of a table name encodes the purpose of the table:
    /// Object: describes an entity (e.g., TopicEntity, ReplyEntity, etc...). It is a CTStore object table.
    /// Lookup: used to lookup a type of handles by another type of handles (e.g., retrieve userHandle by relationshipHandle). It is a CTStore object table.
    /// Count:  used to store counters related to an entity. It is a CTStore count table.
    /// Feed:   used to store a feed (aka multiple rows) of entities. It is a CTStore feed table.
    /// </remarks>
    public enum TableIdentifier
    {
        /// <summary>
        /// Topics table
        /// </summary>
        TopicsObject,

        /// <summary>
        /// Comments table
        /// </summary>
        CommentsObject,

        /// <summary>
        /// Replies table
        /// </summary>
        RepliesObject,

        /// <summary>
        /// Likes lookup table
        /// </summary>
        LikesLookup,

        /// <summary>
        /// Likes feed table
        /// </summary>
        LikesFeed,

        /// <summary>
        /// Likes count table
        /// </summary>
        LikesCount,

        /// <summary>
        /// Pins lookup table
        /// </summary>
        PinsLookup,

        /// <summary>
        /// Pins feed table
        /// </summary>
        PinsFeed,

        /// <summary>
        /// Pins count table
        /// </summary>
        PinsCount,

        /// <summary>
        /// User followers lookup table
        /// </summary>
        FollowersLookup,

        /// <summary>
        /// User followers feed table
        /// </summary>
        FollowersFeed,

        /// <summary>
        /// User followers count table
        /// </summary>
        FollowersCount,

        /// <summary>
        /// User following lookup table
        /// </summary>
        FollowingLookup,

        /// <summary>
        /// User following feed table
        /// </summary>
        FollowingFeed,

        /// <summary>
        /// User following count table
        /// </summary>
        FollowingCount,

        /// <summary>
        /// Topic comments feed table
        /// </summary>
        TopicCommentsFeed,

        /// <summary>
        /// Topic comments count table
        /// </summary>
        TopicCommentsCount,

        /// <summary>
        /// Comment replies feed table
        /// </summary>
        CommentRepliesFeed,

        /// <summary>
        /// Comment replies count table
        /// </summary>
        CommentRepliesCount,

        /// <summary>
        /// User topics feed table
        /// </summary>
        UserTopicsFeed,

        /// <summary>
        /// User topic count table
        /// </summary>
        UserTopicsCount,

        /// <summary>
        /// Popular user topics feed table
        /// </summary>
        PopularUserTopicsFeed,

        /// <summary>
        /// Recent topics feed table
        /// </summary>
        RecentTopicsFeed,

        /// <summary>
        /// Featured topics feed table
        /// </summary>
        FeaturedTopicsFeed,

        /// <summary>
        /// Following topics feed table
        /// </summary>
        FollowingTopicsFeed,

        /// <summary>
        /// Popular topics feed table
        /// </summary>
        PopularTopicsFeed,

        /// <summary>
        /// Popular topics expirations feed table
        /// </summary>
        PopularTopicsExpirationsFeed,

        /// <summary>
        /// Notifications feed table
        /// </summary>
        NotificationsFeed,

        /// <summary>
        /// Notifications count table
        /// </summary>
        NotificationsCount,

        /// <summary>
        /// Notifications status
        /// </summary>
        NotificationsStatus,

        /// <summary>
        /// Following activities feed table
        /// </summary>
        FollowingActivitiesFeed,

        /// <summary>
        /// Content reports table to lookup details of a report
        /// </summary>
        ContentReportsLookup,

        /// <summary>
        /// Content reports table to lookup whether the same reporting user has complained about this content before
        /// </summary>
        ContentReportsLookupUniquenessByReportingUser,

        /// <summary>
        /// Content reports feed table by the app that the content lives in
        /// </summary>
        ContentReportsRecentFeedByApp,

        /// <summary>
        /// Content reports feed table by the content
        /// </summary>
        ContentReportsRecentFeedByContent,

        /// <summary>
        /// Content reports feed table by the user that created content
        /// </summary>
        ContentReportsRecentFeedByContentUser,

        /// <summary>
        /// Content reports feed table by the user that reported content
        /// </summary>
        ContentReportsRecentFeedByReportingUser,

        /// <summary>
        /// Content reports count per content
        /// </summary>
        ContentReportsCountByContent,

        /// <summary>
        /// Content reports count per user that created content
        /// </summary>
        ContentReportsCountByContentUser,

        /// <summary>
        /// Content reports count per user that reported content
        /// </summary>
        ContentReportsCountByReportingUser,

        /// <summary>
        /// User reports table to lookup details of a report
        /// </summary>
        UserReportsLookup,

        /// <summary>
        /// User reports table to lookup whether the same reporting user has complained about this user before
        /// </summary>
        UserReportsLookupUniquenessByReportingUser,

        /// <summary>
        /// User reports feed table by the app that the user lives in
        /// </summary>
        UserReportsRecentFeedByApp,

        /// <summary>
        /// User reports feed table by the user that is being reported on
        /// </summary>
        UserReportsRecentFeedByReportedUser,

        /// <summary>
        /// User reports feed table by the user that has reported other users
        /// </summary>
        UserReportsRecentFeedByReportingUser,

        /// <summary>
        /// User reports count per user that is being reported on
        /// </summary>
        UserReportsCountByReportedUser,

        /// <summary>
        /// User reports count per user that reported other users
        /// </summary>
        UserReportsCountByReportingUser,

        /// <summary>
        /// AVERT calls and responses lookup
        /// </summary>
        AVERTLookup,

        /// <summary>
        /// User profiles table
        /// </summary>
        UserProfilesObject,

        /// <summary>
        /// User linked accounts feed table
        /// </summary>
        UserLinkedAccountsFeed,

        /// <summary>
        /// User apps feed table
        /// </summary>
        UserAppsFeed,

        /// <summary>
        /// Popular users feed table
        /// </summary>
        PopularUsersFeed,

        /// <summary>
        /// Linked accounts index table
        /// </summary>
        LinkedAccountsIndex,

        /// <summary>
        /// App profiles table
        /// </summary>
        AppProfilesObject,

        /// <summary>
        /// App validation configuration table
        /// </summary>
        AppValidationConfigurationsObject,

        /// <summary>
        /// App identity credentials table
        /// </summary>
        AppIdentityProviderCredentialsObject,

        /// <summary>
        /// App push notifications configuration table
        /// </summary>
        AppPushNotificationsConfigurationsObject,

        /// <summary>
        /// App keys feed table
        /// </summary>
        AppKeysFeed,

        /// <summary>
        /// App keys index table
        /// </summary>
        AppKeysIndex,

        /// <summary>
        /// Client names feed table
        /// </summary>
        ClientNamesFeed,

        /// <summary>
        /// Client configs object table
        /// </summary>
        ClientConfigsObject,

        /// <summary>
        /// Developer apps feed table
        /// </summary>
        DeveloperAppsFeed,

        /// <summary>
        /// Apps feed table
        /// </summary>
        AllAppsFeed,

        /// <summary>
        /// Blobs metadata table
        /// </summary>
        BlobsMetadata,

        /// <summary>
        /// Images metadata table
        /// </summary>
        ImagesMetadata,

        /// <summary>
        /// Push registrations feed table
        /// </summary>
        PushRegistrationsFeed,

        /// <summary>
        /// Topic names table
        /// </summary>
        TopicNamesObject,

        /// <summary>
        /// App admins table
        /// </summary>
        AppAdminsObject,

        /// <summary>
        /// Store version table
        /// </summary>
        StoreVersionObject,

        /// <summary>
        /// Topic followers lookup table
        /// </summary>
        TopicFollowersLookup,

        /// <summary>
        /// Topic followers feed table
        /// </summary>
        TopicFollowersFeed,

        /// <summary>
        /// Topic followers count table
        /// </summary>
        TopicFollowersCount,

        /// <summary>
        /// Topic following lookup table
        /// </summary>
        TopicFollowingLookup,

        /// <summary>
        /// Topic following feed table
        /// </summary>
        TopicFollowingFeed,

        /// <summary>
        /// Topic following count table
        /// </summary>
        TopicFollowingCount,

        /// <summary>
        /// CVS requests and responses lookup
        /// </summary>
        CVSLookup,

        /// <summary>
        /// Moderation table
        /// </summary>
        ModerationObject
    }
}
