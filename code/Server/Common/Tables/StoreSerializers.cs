// <copyright file="StoreSerializers.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Tables
{
    using System;

    using SocialPlus.Server.Entities;

    /// <summary>
    /// Serialization helper routines for dealing with RankFeeds in CTStore
    /// </summary>
    public static class StoreSerializers
    {
        private static readonly char KeySeparator = ':';

        /// <summary>
        /// Performs partial serialization of a topic rank feed entity.
        /// This routine only serializes the items specific to a topic rank feed entity -- none of the base class
        /// feed entity fields are serialized.
        /// </summary>
        /// <param name="topic">topic rank feed entity to serialize</param>
        /// <returns>a serialized topic rank feed entity</returns>
        public static string MinimalTopicRankFeedEntitySerialize(TopicRankFeedEntity topic)
        {
            if (topic == null)
            {
                return null;
            }

            if (topic.TopicHandle != null && topic.TopicHandle.Contains(":"))
            {
                throw new InvalidOperationException("Error serialzing " + topic.TopicHandle + " because the topic handle contains the field separator character");
            }

            if (topic.UserHandle != null && topic.UserHandle.Contains(":"))
            {
                throw new InvalidOperationException("Error serialzing " + topic.UserHandle + " because the user handle contains the field separator character");
            }

            if (topic.AppHandle != null && topic.AppHandle.Contains(":"))
            {
                throw new InvalidOperationException("Error serialzing " + topic.AppHandle + " because the app handle contains the field separator character");
            }

            return topic.TopicHandle + KeySeparator + topic.UserHandle + KeySeparator + topic.AppHandle;
        }

        /// <summary>
        /// Performs partial desserialization of a topic rank feed entity.
        /// </summary>
        /// <param name="serializedItem">string to deserialize</param>
        /// <returns>a topic rank feed entity</returns>
        public static TopicRankFeedEntity MinimalTopicRankFeedEntityDeserialize(string serializedItem)
        {
            if (serializedItem == null)
            {
                return null;
            }

            var elements = serializedItem.Split(KeySeparator);
            if (elements.Length == 3)
            {
                return new TopicRankFeedEntity { TopicHandle = elements[0], UserHandle = elements[1], AppHandle = elements[2] };
            }
            else
            {
                throw new InvalidOperationException("Error Deserialzing " + serializedItem + " into a TopicRankFeedEntity");
            }
        }

        /// <summary>
        /// Performs partial serialization of a user rank feed entity.
        /// This routine only serializes the items specific to a user rank feed entity -- none of the base class
        /// feed entity fields are serialized.
        /// </summary>
        /// <param name="user">user rank feed entity to serialize</param>
        /// <returns>a serialized user rank feed entity</returns>
        public static string MinimalUserRankFeedEntitySerialize(UserRankFeedEntity user)
        {
            if (user == null)
            {
                return null;
            }

            if (user.UserHandle != null && user.UserHandle.Contains(":"))
            {
                throw new InvalidOperationException("Error serialzing " + user.UserHandle + " because the user handle contains the field separator character");
            }

            if (user.AppHandle != null && user.AppHandle.Contains(":"))
            {
                throw new InvalidOperationException("Error serialzing " + user.AppHandle + " because the app handle contains the field separator character");
            }

            return user.UserHandle + KeySeparator + user.AppHandle;
        }

        /// <summary>
        /// Performs partial desserialization of a user rank feed entity.
        /// </summary>
        /// <param name="serializedItem">string to deserialize</param>
        /// <returns>a user rank feed entity</returns>
        public static UserRankFeedEntity MinimalUserRankFeedEntityDeserialize(string serializedItem)
        {
            if (serializedItem == null)
            {
                return null;
            }

            var elements = serializedItem.Split(KeySeparator);
            if (elements.Length == 2)
            {
                return new UserRankFeedEntity { UserHandle = elements[0], AppHandle = elements[1] };
            }
            else
            {
                throw new InvalidOperationException("Error Deserialzing " + serializedItem + " into a UserRankFeedEntity");
            }
        }
    }
}
