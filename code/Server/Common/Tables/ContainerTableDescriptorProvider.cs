// <copyright file="ContainerTableDescriptorProvider.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Tables
{
    using System;
    using System.Collections.Generic;

    using Microsoft.CTStore;

    /// <summary>
    /// Container and table descriptor provider -- All container and table descriptors are programmed here
    /// </summary>
    public static class ContainerTableDescriptorProvider
    {
        /// <summary>
        /// Container descriptors dictionary
        /// </summary>
        private static Dictionary<ContainerIdentifier, ContainerDescriptor> containers = new Dictionary<ContainerIdentifier, ContainerDescriptor>();

        /// <summary>
        /// Combined container and table initials
        /// </summary>
        private static HashSet<string> tableFullInitials = new HashSet<string>();

        /// <summary>
        /// Initializes static members of the <see cref="ContainerTableDescriptorProvider"/> class.
        /// </summary>
        static ContainerTableDescriptorProvider()
        {
            Initalize();
        }

        /// <summary>
        /// Gets container descriptors dictionary
        /// </summary>
        public static Dictionary<ContainerIdentifier, ContainerDescriptor> Containers
        {
            get
            {
                return containers;
            }
        }

        /// <summary>
        /// Initialize table descriptors
        /// </summary>
        private static void Initalize()
        {
            ContainerDescriptor topics = new ContainerDescriptor()
            {
                ContainerName = ContainerIdentifier.Topics.ToString(),
                ContainerInitial = "T",
                AzureStorageInstanceType = AzureStorageInstanceType.Default,
                RedisInstanceType = RedisInstanceType.Volatile,
                Tables = new Dictionary<TableIdentifier, TableDescriptor>()
                    {
                        {
                            TableIdentifier.TopicsObject, new TableDescriptor()
                            {
                                TableType = TableType.Object,
                                TableName = "Object",
                                TableInitial = "O",
                                StorageMode = StorageMode.Default
                            }
                        }
                    }
            };
            ContainerDescriptor comments = new ContainerDescriptor()
            {
                ContainerName = ContainerIdentifier.Comments.ToString(),
                ContainerInitial = "C",
                AzureStorageInstanceType = AzureStorageInstanceType.Default,
                RedisInstanceType = RedisInstanceType.Volatile,
                Tables = new Dictionary<TableIdentifier, TableDescriptor>()
                    {
                        {
                            TableIdentifier.CommentsObject, new TableDescriptor()
                            {
                                TableType = TableType.Object,
                                TableName = "Object",
                                TableInitial = "O",
                                StorageMode = StorageMode.Default
                            }
                        }
                    }
            };
            ContainerDescriptor replies = new ContainerDescriptor()
            {
                ContainerName = ContainerIdentifier.Replies.ToString(),
                ContainerInitial = "R",
                AzureStorageInstanceType = AzureStorageInstanceType.Default,
                RedisInstanceType = RedisInstanceType.Volatile,
                Tables = new Dictionary<TableIdentifier, TableDescriptor>()
                    {
                        {
                            TableIdentifier.RepliesObject, new TableDescriptor()
                            {
                                TableType = TableType.Object,
                                TableName = "Object",
                                TableInitial = "O",
                                StorageMode = StorageMode.Default
                            }
                        }
                    }
            };
            ContainerDescriptor recentTopics = new ContainerDescriptor()
            {
                ContainerName = ContainerIdentifier.RecentTopics.ToString(),
                ContainerInitial = "RT",
                AzureStorageInstanceType = AzureStorageInstanceType.Default,
                RedisInstanceType = RedisInstanceType.Volatile,
                Tables = new Dictionary<TableIdentifier, TableDescriptor>()
                    {
                        {
                            TableIdentifier.RecentTopicsFeed, new TableDescriptor()
                            {
                                TableType = TableType.Feed,
                                TableName = "Feed",
                                TableInitial = "F",
                                StorageMode = StorageMode.Default
                            }
                        }
                    }
            };
            ContainerDescriptor userTopics = new ContainerDescriptor()
            {
                ContainerName = ContainerIdentifier.UserTopics.ToString(),
                ContainerInitial = "UT",
                AzureStorageInstanceType = AzureStorageInstanceType.Default,
                RedisInstanceType = RedisInstanceType.Volatile,
                Tables = new Dictionary<TableIdentifier, TableDescriptor>()
                    {
                        {
                            TableIdentifier.UserTopicsFeed, new TableDescriptor()
                            {
                                TableType = TableType.Feed,
                                TableName = "Feed",
                                TableInitial = "F",
                                StorageMode = StorageMode.Default
                            }
                        },
                        {
                            TableIdentifier.UserTopicsCount, new TableDescriptor()
                            {
                                TableType = TableType.Count,
                                TableName = "Count",
                                TableInitial = "Z",
                                StorageMode = StorageMode.Default
                            }
                        }
                    }
            };
            ContainerDescriptor followingTopics = new ContainerDescriptor()
            {
                ContainerName = ContainerIdentifier.FollowingTopics.ToString(),
                ContainerInitial = "FT",
                AzureStorageInstanceType = AzureStorageInstanceType.Default,
                RedisInstanceType = RedisInstanceType.Volatile,
                Tables = new Dictionary<TableIdentifier, TableDescriptor>()
                    {
                        {
                            TableIdentifier.FollowingTopicsFeed, new TableDescriptor()
                            {
                                TableType = TableType.Feed,
                                TableName = "Feed",
                                TableInitial = "F",
                                StorageMode = StorageMode.Default
                            }
                        }
                    }
            };
            ContainerDescriptor featuredTopics = new ContainerDescriptor()
            {
                ContainerName = ContainerIdentifier.FeaturedTopics.ToString(),
                ContainerInitial = "FeT",
                AzureStorageInstanceType = AzureStorageInstanceType.Default,
                RedisInstanceType = RedisInstanceType.Volatile,
                Tables = new Dictionary<TableIdentifier, TableDescriptor>()
                    {
                        {
                            TableIdentifier.FeaturedTopicsFeed, new TableDescriptor()
                            {
                                TableType = TableType.Feed,
                                TableName = "Feed",
                                TableInitial = "F",
                                StorageMode = StorageMode.Default
                            }
                        }
                    }
            };
            ContainerDescriptor popularTopics = new ContainerDescriptor()
            {
                ContainerName = ContainerIdentifier.PopularTopics.ToString(),
                ContainerInitial = "PT",
                AzureStorageInstanceType = AzureStorageInstanceType.Default,
                RedisInstanceType = RedisInstanceType.Persistent,
                Tables = new Dictionary<TableIdentifier, TableDescriptor>()
                    {
                        {
                            TableIdentifier.PopularTopicsFeed, new TableDescriptor()
                            {
                                TableType = TableType.RankFeed,
                                TableName = "Feed",
                                TableInitial = "F",
                                StorageMode = StorageMode.CacheOnly,
                                MaxFeedSizeInCache = 400
                            }
                        },
                        {
                            TableIdentifier.PopularTopicsExpirationsFeed, new TableDescriptor()
                            {
                                TableType = TableType.RankFeed,
                                TableName = "ExpirationsFeed",
                                TableInitial = "EF",
                                StorageMode = StorageMode.CacheOnly,
                                MaxFeedSizeInCache = int.MaxValue
                            }
                        }
                    }
            };
            ContainerDescriptor popularUserTopics = new ContainerDescriptor()
            {
                ContainerName = ContainerIdentifier.PopularUserTopics.ToString(),
                ContainerInitial = "PUT",
                AzureStorageInstanceType = AzureStorageInstanceType.Default,
                RedisInstanceType = RedisInstanceType.Persistent,
                Tables = new Dictionary<TableIdentifier, TableDescriptor>()
                    {
                        {
                            TableIdentifier.PopularUserTopicsFeed, new TableDescriptor()
                            {
                                TableType = TableType.RankFeed,
                                TableName = "Feed",
                                TableInitial = "F",
                                StorageMode = StorageMode.CacheOnly,
                                MaxFeedSizeInCache = 400
                            }
                        }
                    }
            };
            ContainerDescriptor popularUsers = new ContainerDescriptor()
            {
                ContainerName = ContainerIdentifier.PopularUsers.ToString(),
                ContainerInitial = "PU",
                AzureStorageInstanceType = AzureStorageInstanceType.Default,
                RedisInstanceType = RedisInstanceType.Persistent,
                Tables = new Dictionary<TableIdentifier, TableDescriptor>()
                    {
                        {
                            TableIdentifier.PopularUsersFeed, new TableDescriptor()
                            {
                                TableType = TableType.RankFeed,
                                TableName = "Feed",
                                TableInitial = "F",
                                StorageMode = StorageMode.CacheOnly,
                                MaxFeedSizeInCache = 400
                            }
                        }
                    }
            };
            ContainerDescriptor users = new ContainerDescriptor()
            {
                ContainerName = ContainerIdentifier.Users.ToString(),
                ContainerInitial = "U",
                AzureStorageInstanceType = AzureStorageInstanceType.Default,
                RedisInstanceType = RedisInstanceType.Volatile,
                Tables = new Dictionary<TableIdentifier, TableDescriptor>()
                    {
                        {
                            TableIdentifier.UserProfilesObject, new TableDescriptor()
                            {
                                TableType = TableType.Object,
                                TableName = "ProfilesObject",
                                TableInitial = "PO",
                                StorageMode = StorageMode.Default
                            }
                        },
                        {
                            TableIdentifier.UserAppsFeed, new TableDescriptor()
                            {
                                TableType = TableType.Feed,
                                TableName = "AppsFeed",
                                TableInitial = "AF",
                                StorageMode = StorageMode.Default
                            }
                        },
                        {
                            TableIdentifier.UserLinkedAccountsFeed, new TableDescriptor()
                            {
                                TableType = TableType.Feed,
                                TableName = "LinkedAccountsFeed",
                                TableInitial = "LAF",
                                StorageMode = StorageMode.Default
                            }
                        }
                    }
            };
            ContainerDescriptor linkedAccounts = new ContainerDescriptor()
            {
                ContainerName = ContainerIdentifier.LinkedAccounts.ToString(),
                ContainerInitial = "LA",
                AzureStorageInstanceType = AzureStorageInstanceType.Default,
                RedisInstanceType = RedisInstanceType.Volatile,
                Tables = new Dictionary<TableIdentifier, TableDescriptor>()
                    {
                        {
                            TableIdentifier.LinkedAccountsIndex, new TableDescriptor()
                            {
                                TableType = TableType.Object,
                                TableName = "Index",
                                TableInitial = "I",
                                StorageMode = StorageMode.Default
                            }
                        }
                    }
            };
            ContainerDescriptor apps = new ContainerDescriptor()
            {
                ContainerName = ContainerIdentifier.Apps.ToString(),
                ContainerInitial = "A",
                AzureStorageInstanceType = AzureStorageInstanceType.Default,
                RedisInstanceType = RedisInstanceType.Volatile,
                Tables = new Dictionary<TableIdentifier, TableDescriptor>()
                    {
                        {
                            TableIdentifier.AppProfilesObject, new TableDescriptor()
                            {
                                TableType = TableType.Object,
                                TableName = "ProfilesObject",
                                TableInitial = "PO",
                                StorageMode = StorageMode.Default
                            }
                        },
                        {
                            TableIdentifier.AppValidationConfigurationsObject, new TableDescriptor()
                            {
                                TableType = TableType.Object,
                                TableName = "ValidationConfigsObject",
                                TableInitial = "VO",
                                StorageMode = StorageMode.Default
                            }
                        },
                        {
                            TableIdentifier.AppIdentityProviderCredentialsObject, new TableDescriptor()
                            {
                                TableType = TableType.Object,
                                TableName = "IdentityCredentialsObject",
                                TableInitial = "IO",
                                StorageMode = StorageMode.Default
                            }
                        },
                        {
                            TableIdentifier.AppPushNotificationsConfigurationsObject, new TableDescriptor()
                            {
                                TableType = TableType.Object,
                                TableName = "PushNotificationsConfigsObject",
                                TableInitial = "NO",
                                StorageMode = StorageMode.Default
                            }
                        },
                        {
                            TableIdentifier.AppKeysFeed, new TableDescriptor()
                            {
                                TableType = TableType.Feed,
                                TableName = "KeysFeed",
                                TableInitial = "KF",
                                StorageMode = StorageMode.Default
                            }
                        }
                    }
            };
            ContainerDescriptor appKeys = new ContainerDescriptor()
            {
                ContainerName = ContainerIdentifier.AppKeys.ToString(),
                ContainerInitial = "AK",
                AzureStorageInstanceType = AzureStorageInstanceType.Default,
                RedisInstanceType = RedisInstanceType.Volatile,
                Tables = new Dictionary<TableIdentifier, TableDescriptor>()
                    {
                        {
                            TableIdentifier.AppKeysIndex, new TableDescriptor()
                            {
                                TableType = TableType.Object,
                                TableName = "Index",
                                TableInitial = "I",
                                StorageMode = StorageMode.Default
                            }
                        },
                        {
                            TableIdentifier.ClientNamesFeed, new TableDescriptor()
                            {
                                TableType = TableType.Feed,
                                TableName = "ClientNamesFeed",
                                TableInitial = "CF",
                                StorageMode = StorageMode.Default
                            }
                        }
                    }
            };
            ContainerDescriptor clientConfigs = new ContainerDescriptor()
            {
                ContainerName = ContainerIdentifier.ClientConfigs.ToString(),
                ContainerInitial = "CC",
                AzureStorageInstanceType = AzureStorageInstanceType.Default,
                RedisInstanceType = RedisInstanceType.Volatile,
                Tables = new Dictionary<TableIdentifier, TableDescriptor>()
                    {
                        {
                            TableIdentifier.ClientConfigsObject, new TableDescriptor()
                            {
                                TableType = TableType.Object,
                                TableName = "Object",
                                TableInitial = "O",
                                StorageMode = StorageMode.Default
                            }
                        }
                    }
            };
            ContainerDescriptor developerApps = new ContainerDescriptor()
            {
                ContainerName = ContainerIdentifier.DeveloperApps.ToString(),
                ContainerInitial = "DA",
                AzureStorageInstanceType = AzureStorageInstanceType.Default,
                RedisInstanceType = RedisInstanceType.Volatile,
                Tables = new Dictionary<TableIdentifier, TableDescriptor>()
                    {
                        {
                            TableIdentifier.DeveloperAppsFeed, new TableDescriptor()
                            {
                                TableType = TableType.Feed,
                                TableName = "Feed",
                                TableInitial = "F",
                                StorageMode = StorageMode.Default
                            }
                        }
                    }
            };
            ContainerDescriptor allApps = new ContainerDescriptor()
            {
                ContainerName = ContainerIdentifier.AllApps.ToString(),
                ContainerInitial = "AA",
                AzureStorageInstanceType = AzureStorageInstanceType.Default,
                RedisInstanceType = RedisInstanceType.Volatile,
                Tables = new Dictionary<TableIdentifier, TableDescriptor>()
                    {
                        {
                            TableIdentifier.AllAppsFeed, new TableDescriptor()
                            {
                                TableType = TableType.Feed,
                                TableName = "Feed",
                                TableInitial = "F",
                                StorageMode = StorageMode.Default
                            }
                        }
                    }
            };
            ContainerDescriptor likes = new ContainerDescriptor()
            {
                ContainerName = ContainerIdentifier.Likes.ToString(),
                ContainerInitial = "L",
                AzureStorageInstanceType = AzureStorageInstanceType.Default,
                RedisInstanceType = RedisInstanceType.Volatile,
                Tables = new Dictionary<TableIdentifier, TableDescriptor>()
                    {
                        {
                            TableIdentifier.LikesLookup, new TableDescriptor()
                            {
                                TableType = TableType.Object,
                                TableName = "Lookup",
                                TableInitial = "L",
                                StorageMode = StorageMode.Default
                            }
                        },
                        {
                            TableIdentifier.LikesFeed, new TableDescriptor()
                            {
                                TableType = TableType.Feed,
                                TableName = "Feed",
                                TableInitial = "F",
                                StorageMode = StorageMode.Default
                            }
                        },
                        {
                            TableIdentifier.LikesCount, new TableDescriptor()
                            {
                                TableType = TableType.Count,
                                TableName = "Count",
                                TableInitial = "Z",
                                StorageMode = StorageMode.Default
                            }
                        }
                    }
            };
            ContainerDescriptor pins = new ContainerDescriptor()
            {
                ContainerName = ContainerIdentifier.Pins.ToString(),
                ContainerInitial = "P",
                AzureStorageInstanceType = AzureStorageInstanceType.Default,
                RedisInstanceType = RedisInstanceType.Volatile,
                Tables = new Dictionary<TableIdentifier, TableDescriptor>()
                    {
                        {
                            TableIdentifier.PinsLookup, new TableDescriptor()
                            {
                                TableType = TableType.Object,
                                TableName = "Lookup",
                                TableInitial = "L",
                                StorageMode = StorageMode.Default
                            }
                        },
                        {
                            TableIdentifier.PinsFeed, new TableDescriptor()
                            {
                                TableType = TableType.Feed,
                                TableName = "Feed",
                                TableInitial = "F",
                                StorageMode = StorageMode.Default
                            }
                        },
                        {
                            TableIdentifier.PinsCount, new TableDescriptor()
                            {
                                TableType = TableType.Count,
                                TableName = "Count",
                                TableInitial = "Z",
                                StorageMode = StorageMode.Default
                            }
                        }
                    }
            };
            ContainerDescriptor userFollowers = new ContainerDescriptor()
            {
                ContainerName = ContainerIdentifier.UserFollowers.ToString(),
                ContainerInitial = "F",
                AzureStorageInstanceType = AzureStorageInstanceType.Default,
                RedisInstanceType = RedisInstanceType.Volatile,
                Tables = new Dictionary<TableIdentifier, TableDescriptor>()
                    {
                        {
                            TableIdentifier.FollowersLookup, new TableDescriptor()
                            {
                                TableType = TableType.Object,
                                TableName = "Lookup",
                                TableInitial = "L",
                                StorageMode = StorageMode.Default
                            }
                        },
                        {
                            TableIdentifier.FollowersFeed, new TableDescriptor()
                            {
                                TableType = TableType.Feed,
                                TableName = "Feed",
                                TableInitial = "F",
                                StorageMode = StorageMode.Default
                            }
                        },
                        {
                            TableIdentifier.FollowersCount, new TableDescriptor()
                            {
                                TableType = TableType.Count,
                                TableName = "Count",
                                TableInitial = "Z",
                                StorageMode = StorageMode.Default
                            }
                        }
                    }
            };
            ContainerDescriptor userFollowing = new ContainerDescriptor()
            {
                ContainerName = ContainerIdentifier.UserFollowing.ToString(),
                ContainerInitial = "G",
                AzureStorageInstanceType = AzureStorageInstanceType.Default,
                RedisInstanceType = RedisInstanceType.Volatile,
                Tables = new Dictionary<TableIdentifier, TableDescriptor>()
                    {
                        {
                            TableIdentifier.FollowingLookup, new TableDescriptor()
                            {
                                TableType = TableType.Object,
                                TableName = "Lookup",
                                TableInitial = "L",
                                StorageMode = StorageMode.Default
                            }
                        },
                        {
                            TableIdentifier.FollowingFeed, new TableDescriptor()
                            {
                                TableType = TableType.Feed,
                                TableName = "Feed",
                                TableInitial = "F",
                                StorageMode = StorageMode.Default
                            }
                        },
                        {
                            TableIdentifier.FollowingCount, new TableDescriptor()
                            {
                                TableType = TableType.Count,
                                TableName = "Count",
                                TableInitial = "Z",
                                StorageMode = StorageMode.Default
                            }
                        }
                    }
            };
            ContainerDescriptor topicComments = new ContainerDescriptor()
            {
                ContainerName = ContainerIdentifier.TopicComments.ToString(),
                ContainerInitial = "TC",
                AzureStorageInstanceType = AzureStorageInstanceType.Default,
                RedisInstanceType = RedisInstanceType.Volatile,
                Tables = new Dictionary<TableIdentifier, TableDescriptor>()
                    {
                        {
                            TableIdentifier.TopicCommentsFeed, new TableDescriptor()
                            {
                                TableType = TableType.Feed,
                                TableName = "Feed",
                                TableInitial = "F",
                                StorageMode = StorageMode.Default
                            }
                        },
                        {
                            TableIdentifier.TopicCommentsCount, new TableDescriptor()
                            {
                                TableType = TableType.Count,
                                TableName = "Count",
                                TableInitial = "Z",
                                StorageMode = StorageMode.Default
                            }
                        }
                    }
            };
            ContainerDescriptor commentReplies = new ContainerDescriptor()
            {
                ContainerName = ContainerIdentifier.CommentReplies.ToString(),
                ContainerInitial = "CR",
                AzureStorageInstanceType = AzureStorageInstanceType.Default,
                RedisInstanceType = RedisInstanceType.Volatile,
                Tables = new Dictionary<TableIdentifier, TableDescriptor>()
                    {
                        {
                            TableIdentifier.CommentRepliesFeed, new TableDescriptor()
                            {
                                TableType = TableType.Feed,
                                TableName = "Feed",
                                TableInitial = "F",
                                StorageMode = StorageMode.Default
                            }
                        },
                        {
                            TableIdentifier.CommentRepliesCount, new TableDescriptor()
                            {
                                TableType = TableType.Count,
                                TableName = "Count",
                                TableInitial = "Z",
                                StorageMode = StorageMode.Default
                            }
                        }
                    }
            };
            ContainerDescriptor notifications = new ContainerDescriptor()
            {
                ContainerName = ContainerIdentifier.Notifications.ToString(),
                ContainerInitial = "N",
                AzureStorageInstanceType = AzureStorageInstanceType.Default,
                RedisInstanceType = RedisInstanceType.Volatile,
                Tables = new Dictionary<TableIdentifier, TableDescriptor>()
                    {
                        {
                            TableIdentifier.NotificationsFeed, new TableDescriptor()
                            {
                                TableType = TableType.Feed,
                                TableName = "Feed",
                                TableInitial = "F",
                                StorageMode = StorageMode.Default
                            }
                        },
                        {
                            TableIdentifier.NotificationsCount, new TableDescriptor()
                            {
                                TableType = TableType.Count,
                                TableName = "Count",
                                TableInitial = "Z",
                                StorageMode = StorageMode.Default
                            }
                        },
                        {
                            TableIdentifier.NotificationsStatus, new TableDescriptor()
                            {
                                TableType = TableType.Object,
                                TableName = "Status",
                                TableInitial = "S",
                                StorageMode = StorageMode.Default
                            }
                        }
                    }
            };
            ContainerDescriptor followingActivities = new ContainerDescriptor()
            {
                ContainerName = ContainerIdentifier.FollowingActivities.ToString(),
                ContainerInitial = "FA",
                AzureStorageInstanceType = AzureStorageInstanceType.Default,
                RedisInstanceType = RedisInstanceType.Volatile,
                Tables = new Dictionary<TableIdentifier, TableDescriptor>()
                    {
                        {
                            TableIdentifier.FollowingActivitiesFeed, new TableDescriptor()
                            {
                                TableType = TableType.Feed,
                                TableName = "Feed",
                                TableInitial = "F",
                                StorageMode = StorageMode.Default
                            }
                        }
                    }
            };
            ContainerDescriptor blobs = new ContainerDescriptor()
            {
                ContainerName = ContainerIdentifier.Blobs.ToString(),
                ContainerInitial = "B",
                AzureStorageInstanceType = AzureStorageInstanceType.Default,
                Tables = new Dictionary<TableIdentifier, TableDescriptor>()
                    {
                        {
                            TableIdentifier.BlobsMetadata, new TableDescriptor()
                            {
                                TableType = TableType.Object,
                                TableName = "Object",
                                TableInitial = "O",
                                StorageMode = StorageMode.PersistentOnly
                            }
                        }
                    }
            };
            ContainerDescriptor images = new ContainerDescriptor()
            {
                ContainerName = ContainerIdentifier.Images.ToString(),
                ContainerInitial = "I",
                AzureStorageInstanceType = AzureStorageInstanceType.Default,
                Tables = new Dictionary<TableIdentifier, TableDescriptor>()
                    {
                        {
                            TableIdentifier.ImagesMetadata, new TableDescriptor()
                            {
                                TableType = TableType.Object,
                                TableName = "Object",
                                TableInitial = "O",
                                StorageMode = StorageMode.PersistentOnly
                            }
                        }
                    }
            };
            ContainerDescriptor pushRegistrations = new ContainerDescriptor()
            {
                ContainerName = ContainerIdentifier.PushRegistrations.ToString(),
                ContainerInitial = "PR",
                AzureStorageInstanceType = AzureStorageInstanceType.Default,
                RedisInstanceType = RedisInstanceType.Volatile,
                Tables = new Dictionary<TableIdentifier, TableDescriptor>()
                    {
                        {
                            TableIdentifier.PushRegistrationsFeed, new TableDescriptor()
                            {
                                TableType = TableType.Feed,
                                TableName = "Feed",
                                TableInitial = "F",
                                StorageMode = StorageMode.Default
                            }
                        }
                    }
            };
            ContainerDescriptor appAdmins = new ContainerDescriptor()
            {
                ContainerName = ContainerIdentifier.AppAdmins.ToString(),
                ContainerInitial = "AD",
                AzureStorageInstanceType = AzureStorageInstanceType.Default,
                RedisInstanceType = RedisInstanceType.Volatile,
                Tables = new Dictionary<TableIdentifier, TableDescriptor>()
                    {
                        {
                            TableIdentifier.AppAdminsObject, new TableDescriptor()
                            {
                                TableType = TableType.Object,
                                TableName = "Object",
                                TableInitial = "O",
                                StorageMode = StorageMode.Default
                            }
                        }
                    }
            };
            ContainerDescriptor topicNames = new ContainerDescriptor()
            {
                ContainerName = ContainerIdentifier.TopicNames.ToString(),
                ContainerInitial = "TN",
                AzureStorageInstanceType = AzureStorageInstanceType.Default,
                RedisInstanceType = RedisInstanceType.Volatile,
                Tables = new Dictionary<TableIdentifier, TableDescriptor>()
                    {
                        {
                            TableIdentifier.TopicNamesObject, new TableDescriptor()
                            {
                                TableType = TableType.Object,
                                TableName = "Object",
                                TableInitial = "O",
                                StorageMode = StorageMode.Default
                            }
                        }
                    }
            };
            ContainerDescriptor contentReports = new ContainerDescriptor()
            {
                ContainerName = ContainerIdentifier.ContentReports.ToString(),
                ContainerInitial = "CRt",
                AzureStorageInstanceType = AzureStorageInstanceType.Default,
                RedisInstanceType = RedisInstanceType.Volatile,
                Tables = new Dictionary<TableIdentifier, TableDescriptor>()
                    {
                        {
                            TableIdentifier.ContentReportsLookup, new TableDescriptor()
                            {
                                TableType = TableType.Object,
                                TableName = "Lookup",
                                TableInitial = "L",
                                StorageMode = StorageMode.PersistentOnly
                            }
                        },
                        {
                            TableIdentifier.ContentReportsLookupUniquenessByReportingUser, new TableDescriptor()
                            {
                                TableType = TableType.Object,
                                TableName = "LookupUniqueness",
                                TableInitial = "LU",
                                StorageMode = StorageMode.PersistentOnly
                            }
                        },
                        {
                            TableIdentifier.ContentReportsRecentFeedByApp, new TableDescriptor()
                            {
                                TableType = TableType.Feed,
                                TableName = "FeedByApp",
                                TableInitial = "FA",
                                StorageMode = StorageMode.PersistentOnly
                            }
                        },
                        {
                            TableIdentifier.ContentReportsRecentFeedByContent, new TableDescriptor()
                            {
                                TableType = TableType.Feed,
                                TableName = "FeedByContent",
                                TableInitial = "FC",
                                StorageMode = StorageMode.PersistentOnly
                            }
                        },
                        {
                            TableIdentifier.ContentReportsRecentFeedByContentUser, new TableDescriptor()
                            {
                                TableType = TableType.Feed,
                                TableName = "FeedByContentUser",
                                TableInitial = "FCU",
                                StorageMode = StorageMode.PersistentOnly
                            }
                        },
                        {
                            TableIdentifier.ContentReportsRecentFeedByReportingUser, new TableDescriptor()
                            {
                                TableType = TableType.Feed,
                                TableName = "FeedByReportingUser",
                                TableInitial = "FRU",
                                StorageMode = StorageMode.PersistentOnly
                            }
                        },
                        {
                            TableIdentifier.ContentReportsCountByContent, new TableDescriptor()
                            {
                                TableType = TableType.Count,
                                TableName = "CountByContent",
                                TableInitial = "ZC",
                                StorageMode = StorageMode.PersistentOnly
                            }
                        },
                        {
                            TableIdentifier.ContentReportsCountByContentUser, new TableDescriptor()
                            {
                                TableType = TableType.Count,
                                TableName = "CountByContentUser",
                                TableInitial = "ZCU",
                                StorageMode = StorageMode.PersistentOnly
                            }
                        },
                        {
                            TableIdentifier.ContentReportsCountByReportingUser, new TableDescriptor()
                            {
                                TableType = TableType.Count,
                                TableName = "CountByReportingUser",
                                TableInitial = "ZRU",
                                StorageMode = StorageMode.PersistentOnly
                            }
                        },
                    }
            };
            ContainerDescriptor userReports = new ContainerDescriptor()
            {
                ContainerName = ContainerIdentifier.UserReports.ToString(),
                ContainerInitial = "URt",
                AzureStorageInstanceType = AzureStorageInstanceType.Default,
                RedisInstanceType = RedisInstanceType.Volatile,
                Tables = new Dictionary<TableIdentifier, TableDescriptor>()
                    {
                        {
                            TableIdentifier.UserReportsLookup, new TableDescriptor()
                            {
                                TableType = TableType.Object,
                                TableName = "Lookup",
                                TableInitial = "L",
                                StorageMode = StorageMode.PersistentOnly
                            }
                        },
                        {
                            TableIdentifier.UserReportsLookupUniquenessByReportingUser, new TableDescriptor()
                            {
                                TableType = TableType.Object,
                                TableName = "LookupUniqueness",
                                TableInitial = "LU",
                                StorageMode = StorageMode.PersistentOnly
                            }
                        },
                        {
                            TableIdentifier.UserReportsRecentFeedByApp, new TableDescriptor()
                            {
                                TableType = TableType.Feed,
                                TableName = "FeedByApp",
                                TableInitial = "FA",
                                StorageMode = StorageMode.PersistentOnly
                            }
                        },
                        {
                            TableIdentifier.UserReportsRecentFeedByReportedUser, new TableDescriptor()
                            {
                                TableType = TableType.Feed,
                                TableName = "FeedByReportedUser",
                                TableInitial = "FRDU",
                                StorageMode = StorageMode.PersistentOnly
                            }
                        },
                        {
                            TableIdentifier.UserReportsRecentFeedByReportingUser, new TableDescriptor()
                            {
                                TableType = TableType.Feed,
                                TableName = "FeedByReportingUser",
                                TableInitial = "FRNU",
                                StorageMode = StorageMode.PersistentOnly
                            }
                        },
                        {
                            TableIdentifier.UserReportsCountByReportedUser, new TableDescriptor()
                            {
                                TableType = TableType.Count,
                                TableName = "CountByReportedUser",
                                TableInitial = "ZRDU",
                                StorageMode = StorageMode.PersistentOnly
                            }
                        },
                        {
                            TableIdentifier.UserReportsCountByReportingUser, new TableDescriptor()
                            {
                                TableType = TableType.Count,
                                TableName = "CountByReportingUser",
                                TableInitial = "ZRNU",
                                StorageMode = StorageMode.PersistentOnly
                            }
                        },
                    }
            };
            ContainerDescriptor avert = new ContainerDescriptor()
            {
                ContainerName = ContainerIdentifier.AVERT.ToString(),
                ContainerInitial = "AV",
                AzureStorageInstanceType = AzureStorageInstanceType.Default,
                RedisInstanceType = RedisInstanceType.Volatile,
                Tables = new Dictionary<TableIdentifier, TableDescriptor>()
                    {
                        {
                            TableIdentifier.AVERTLookup, new TableDescriptor()
                            {
                                TableType = TableType.Object,
                                TableName = "Lookup",
                                TableInitial = "L",
                                StorageMode = StorageMode.PersistentOnly
                            }
                        },
                    }
            };
            ContainerDescriptor topicFollowers = new ContainerDescriptor()
            {
                ContainerName = ContainerIdentifier.TopicFollowers.ToString(),
                ContainerInitial = "TF",
                AzureStorageInstanceType = AzureStorageInstanceType.Default,
                RedisInstanceType = RedisInstanceType.Volatile,
                Tables = new Dictionary<TableIdentifier, TableDescriptor>()
                    {
                        {
                            TableIdentifier.TopicFollowersLookup, new TableDescriptor()
                            {
                                TableType = TableType.Object,
                                TableName = "Lookup",
                                TableInitial = "L",
                                StorageMode = StorageMode.Default
                            }
                        },
                        {
                            TableIdentifier.TopicFollowersFeed, new TableDescriptor()
                            {
                                TableType = TableType.Feed,
                                TableName = "Feed",
                                TableInitial = "F",
                                StorageMode = StorageMode.Default
                            }
                        },
                        {
                            TableIdentifier.TopicFollowersCount, new TableDescriptor()
                            {
                                TableType = TableType.Count,
                                TableName = "Count",
                                TableInitial = "Z",
                                StorageMode = StorageMode.Default
                            }
                        }
                    }
            };
            ContainerDescriptor topicFollowing = new ContainerDescriptor()
            {
                ContainerName = ContainerIdentifier.TopicFollowing.ToString(),
                ContainerInitial = "TG",
                AzureStorageInstanceType = AzureStorageInstanceType.Default,
                RedisInstanceType = RedisInstanceType.Volatile,
                Tables = new Dictionary<TableIdentifier, TableDescriptor>()
                    {
                        {
                            TableIdentifier.TopicFollowingLookup, new TableDescriptor()
                            {
                                TableType = TableType.Object,
                                TableName = "Lookup",
                                TableInitial = "L",
                                StorageMode = StorageMode.Default
                            }
                        },
                        {
                            TableIdentifier.TopicFollowingFeed, new TableDescriptor()
                            {
                                TableType = TableType.Feed,
                                TableName = "Feed",
                                TableInitial = "F",
                                StorageMode = StorageMode.Default
                            }
                        },
                        {
                            TableIdentifier.TopicFollowingCount, new TableDescriptor()
                            {
                                TableType = TableType.Count,
                                TableName = "Count",
                                TableInitial = "Z",
                                StorageMode = StorageMode.Default
                            }
                        }
                    }
            };
            ContainerDescriptor moderation = new ContainerDescriptor()
            {
                ContainerName = ContainerIdentifier.Moderation.ToString(),
                ContainerInitial = "Md",
                AzureStorageInstanceType = AzureStorageInstanceType.Default,
                RedisInstanceType = RedisInstanceType.Volatile,
                Tables = new Dictionary<TableIdentifier, TableDescriptor>()
                    {
                        {
                            TableIdentifier.ModerationObject, new TableDescriptor()
                            {
                                TableType = TableType.Object,
                                TableName = "Object",
                                TableInitial = "O",
                                StorageMode = StorageMode.Default
                            }
                        }
                    }
            };
            ContainerDescriptor cvs = new ContainerDescriptor()
            {
                ContainerName = ContainerIdentifier.CVS.ToString(),
                ContainerInitial = "CV",
                AzureStorageInstanceType = AzureStorageInstanceType.Default,
                RedisInstanceType = RedisInstanceType.Volatile,
                Tables = new Dictionary<TableIdentifier, TableDescriptor>()
                    {
                        {
                            TableIdentifier.CVSLookup, new TableDescriptor()
                            {
                                TableType = TableType.Object,
                                TableName = "Lookup",
                                TableInitial = "L",
                                StorageMode = StorageMode.Default
                            }
                        }
                    }
            };

            // list of ContainerInitials that are in use:
            // T, C, R, RT, UT, FT, FeT, PT, PUT, PU, U, LA, A, AK, CC, DA, AA, L, P, F, G, TC, CR, N, FA, B, I, PR, AD, TN, CRt, URt, AV, TF, TG, Md, CV
            Add(ContainerIdentifier.Topics, topics);
            Add(ContainerIdentifier.Comments, comments);
            Add(ContainerIdentifier.Replies, replies);
            Add(ContainerIdentifier.RecentTopics, recentTopics);
            Add(ContainerIdentifier.UserTopics, userTopics);
            Add(ContainerIdentifier.FollowingTopics, followingTopics);
            Add(ContainerIdentifier.FeaturedTopics, featuredTopics);
            Add(ContainerIdentifier.PopularTopics, popularTopics);
            Add(ContainerIdentifier.PopularUserTopics, popularUserTopics);
            Add(ContainerIdentifier.PopularUsers, popularUsers);
            Add(ContainerIdentifier.Users, users);
            Add(ContainerIdentifier.LinkedAccounts, linkedAccounts);
            Add(ContainerIdentifier.Apps, apps);
            Add(ContainerIdentifier.AppKeys, appKeys);
            Add(ContainerIdentifier.ClientConfigs, clientConfigs);
            Add(ContainerIdentifier.DeveloperApps, developerApps);
            Add(ContainerIdentifier.AllApps, allApps);
            Add(ContainerIdentifier.Likes, likes);
            Add(ContainerIdentifier.Pins, pins);
            Add(ContainerIdentifier.UserFollowers, userFollowers);
            Add(ContainerIdentifier.UserFollowing, userFollowing);
            Add(ContainerIdentifier.TopicComments, topicComments);
            Add(ContainerIdentifier.CommentReplies, commentReplies);
            Add(ContainerIdentifier.Notifications, notifications);
            Add(ContainerIdentifier.FollowingActivities, followingActivities);
            Add(ContainerIdentifier.Blobs, blobs);
            Add(ContainerIdentifier.Images, images);
            Add(ContainerIdentifier.PushRegistrations, pushRegistrations);
            Add(ContainerIdentifier.AppAdmins, appAdmins);
            Add(ContainerIdentifier.TopicNames, topicNames);
            Add(ContainerIdentifier.ContentReports, contentReports);
            Add(ContainerIdentifier.UserReports, userReports);
            Add(ContainerIdentifier.AVERT, avert);
            Add(ContainerIdentifier.TopicFollowers, topicFollowers);
            Add(ContainerIdentifier.TopicFollowing, topicFollowing);
            Add(ContainerIdentifier.Moderation, moderation);
            Add(ContainerIdentifier.CVS, cvs);
        }

        /// <summary>
        /// Add container descriptor for container identifier
        /// </summary>
        /// <param name="containerIdentifier">Container identifier</param>
        /// <param name="containerDescriptor">Container descriptor</param>
        private static void Add(ContainerIdentifier containerIdentifier, ContainerDescriptor containerDescriptor)
        {
            foreach (var tableIdentifier in containerDescriptor.Tables.Keys)
            {
                TableDescriptor tableDescriptor = containerDescriptor.Tables[tableIdentifier];
                string tableFullInitial = containerDescriptor.ContainerInitial + tableDescriptor.TableInitial;
                if (!tableFullInitials.Contains(tableFullInitial))
                {
                    tableFullInitials.Add(tableFullInitial);
                }
                else
                {
                    throw new Exception("Table initials conflict");
                }
            }

            containers.Add(containerIdentifier, containerDescriptor);
        }
    }
}
