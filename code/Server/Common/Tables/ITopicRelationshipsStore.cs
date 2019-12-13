// <copyright file="ITopicRelationshipsStore.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Tables
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using SocialPlus.Models;
    using SocialPlus.Server.Entities;

    /// <summary>
    /// Topic relationships store interface
    /// </summary>
    public interface ITopicRelationshipsStore
    {
        /// <summary>
        /// Update topic follower relationship.
        /// Follower user : someone who follows the topic.
        /// </summary>
        /// <param name="storageConsistencyMode">Consistency mode</param>
        /// <param name="relationshipHandle">Relationship handle</param>
        /// <param name="topicHandle">topic handle</param>
        /// <param name="relationshipUserHandle">Relationship user handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="topicRelationshipStatus">Topic relationship status</param>
        /// <param name="lastUpdatedTime">Last updated time</param>
        /// <param name="readTopicRelationshipLookupEntity">Read topic relationship lookup entity</param>
        /// <returns>Update follower relationship task</returns>
        Task UpdateTopicFollowerRelationship(
            StorageConsistencyMode storageConsistencyMode,
            string relationshipHandle,
            string topicHandle,
            string relationshipUserHandle,
            string appHandle,
            TopicRelationshipStatus topicRelationshipStatus,
            DateTime lastUpdatedTime,
            ITopicRelationshipLookupEntity readTopicRelationshipLookupEntity);

        /// <summary>
        /// Update topic following relationship.
        /// Following topic : a topic that i am following.
        /// </summary>
        /// <param name="storageConsistencyMode">Consistency mode</param>
        /// <param name="relationshipHandle">Relationship handle</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="relationshipTopicHandle">Relationship topic handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="topicRelationshipStatus">Topic relationship status</param>
        /// <param name="lastUpdatedTime">Last updated time</param>
        /// <param name="readTopicRelationshipLookupEntity">Read topic relationship lookup entity</param>
        /// <returns>Update following relationship task</returns>
        Task UpdateTopicFollowingRelationship(
            StorageConsistencyMode storageConsistencyMode,
            string relationshipHandle,
            string userHandle,
            string relationshipTopicHandle,
            string appHandle,
            TopicRelationshipStatus topicRelationshipStatus,
            DateTime lastUpdatedTime,
            ITopicRelationshipLookupEntity readTopicRelationshipLookupEntity);

        /// <summary>
        /// Query topic follower relationship.
        /// Follower user : someone who follows the topic.
        /// </summary>
        /// <param name="topicHandle">Topic handle</param>
        /// <param name="relationshipUserHandle">Relationship user handle</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>User relationship lookup entity</returns>
        Task<ITopicRelationshipLookupEntity> QueryTopicFollowerRelationship(
            string topicHandle,
            string relationshipUserHandle,
            string appHandle);

        /// <summary>
        /// Query topic following relationship.
        /// Following topic : a topic that i am following.
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="relationshipTopicHandle">Relationship topic handle</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>User relationship lookup entity</returns>
        Task<ITopicRelationshipLookupEntity> QueryTopicFollowingRelationship(
            string userHandle,
            string relationshipTopicHandle,
            string appHandle);

        /// <summary>
        /// Query topic followers in an app.
        /// Follower users : users who follow the topic.
        /// </summary>
        /// <param name="topicHandle">Topic handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>List of user relationship feed entities</returns>
        Task<IList<IUserRelationshipFeedEntity>> QueryTopicFollowers(
            string topicHandle,
            string appHandle,
            string cursor,
            int limit);

        /// <summary>
        /// Query topic following in an app.
        /// Following topics : topics that i am following.
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>List of topic relationship feed entities</returns>
        Task<IList<ITopicRelationshipFeedEntity>> QueryTopicFollowing(
            string userHandle,
            string appHandle,
            string cursor,
            int limit);

        /// <summary>
        /// Query count of topic followers in an app.
        /// This is the number of users who follow a given topic in an app.
        /// </summary>
        /// <param name="topicHandle">Topic handle</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>Topic followers count</returns>
        Task<long?> QueryTopicFollowersCount(string topicHandle, string appHandle);

        /// <summary>
        /// Query count of topic following in an app.
        /// This is the number of topics a user is following in an app.
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>Topic following count</returns>
        Task<long?> QueryTopicFollowingCount(string userHandle, string appHandle);
    }
}
