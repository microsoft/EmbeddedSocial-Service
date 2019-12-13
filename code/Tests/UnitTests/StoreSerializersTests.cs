// <copyright file="StoreSerializersTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.UnitTests
{
    using System;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SocialPlus.Server.Entities;
    using SocialPlus.Server.Tables;

    /// <summary>
    /// Tests the functionality of the StoreSerializers class in SocialPlus.Server.Tables
    /// </summary>
    [TestClass]
    public class StoreSerializersTests
    {
        /// <summary>
        /// tests the MinimalTopicRankFeedEntitySerialize and MinimalUserRankFeedEntitySerialize routines
        /// </summary>
        [TestMethod]
        public void TestSerialize()
        {
            // test of MinimalTopicRankFeedEntitySerialize

            // first, check the normal case
            var separator = ":";
            var entity = new TopicRankFeedEntity() { TopicHandle = "baz", UserHandle = "bar", AppHandle = "foo" };
            var result = StoreSerializers.MinimalTopicRankFeedEntitySerialize(entity);
            Assert.AreEqual("baz" + separator + "bar" + separator + "foo", result);

            // next, try a null input
            result = StoreSerializers.MinimalTopicRankFeedEntitySerialize(null);
            Assert.AreEqual(null, result);

            // next, try null values
            entity = new TopicRankFeedEntity();
            result = StoreSerializers.MinimalTopicRankFeedEntitySerialize(entity);
            Assert.AreEqual(separator + separator, result);

            // next, try inserting a handle with a ":"
            entity = new TopicRankFeedEntity() { TopicHandle = "ba:z", UserHandle = "bar", AppHandle = "foo" };
            try
            {
                result = StoreSerializers.MinimalTopicRankFeedEntitySerialize(entity);
                Assert.Fail("Serialize routine was expected to throw an exception");
            }
            catch (Exception e)
            {
                // check that the operation throws the expected exception
                Assert.AreEqual(e.GetType(), typeof(InvalidOperationException));
            }

            // test of MinimalUserRankFeedEntitySerialize

            // first, test the normal case
            var entity2 = new UserRankFeedEntity() { UserHandle = "bar", AppHandle = "foo" };
            result = StoreSerializers.MinimalUserRankFeedEntitySerialize(entity2);
            Assert.AreEqual("bar" + separator + "foo", result);

            // next, try a null input
            result = StoreSerializers.MinimalUserRankFeedEntitySerialize(null);
            Assert.AreEqual(null, result);

            // next, try null values
            entity2 = new UserRankFeedEntity();
            result = StoreSerializers.MinimalUserRankFeedEntitySerialize(entity2);
            Assert.AreEqual(separator, result);

            // next, try inserting a handle with a ":"
            entity2 = new UserRankFeedEntity() { UserHandle = "bar:", AppHandle = "foo" };
            try
            {
                result = StoreSerializers.MinimalUserRankFeedEntitySerialize(entity2);
                Assert.Fail("Serialize routine was expected to throw an exception");
            }
            catch (Exception e)
            {
                // check that the operation throws the expected exception
                Assert.AreEqual(e.GetType(), typeof(InvalidOperationException));
            }
        }

        /// <summary>
        /// tests the MinimalTopicRankFeedEntityDeserialize and MinimalUserRankFeedEntityDeserialize routines
        /// </summary>
        [TestMethod]
        public void TestDeserialize()
        {
            // test of MinimalTopicRankFeedEntityDeserialize

            // first, check the normal case
            var separator = ":";
            string input = "abc" + separator + "def" + separator + "foobar";
            var result = StoreSerializers.MinimalTopicRankFeedEntityDeserialize(input);
            Assert.AreEqual("abc", result.TopicHandle);
            Assert.AreEqual("def", result.UserHandle);
            Assert.AreEqual("foobar", result.AppHandle);

            // check the null case
            input = null;
            result = StoreSerializers.MinimalTopicRankFeedEntityDeserialize(input);
            Assert.AreEqual(null, result);

            // check the null string case
            input = separator + separator;
            result = StoreSerializers.MinimalTopicRankFeedEntityDeserialize(input);
            Assert.AreEqual(string.Empty, result.TopicHandle);
            Assert.AreEqual(string.Empty, result.UserHandle);
            Assert.AreEqual(string.Empty, result.AppHandle);

            // check a case that must never happen
            input = separator;
            try
            {
                result = StoreSerializers.MinimalTopicRankFeedEntityDeserialize(input);
                Assert.Fail("Deserialize routine was expected to throw an exception");
            }
            catch (Exception e)
            {
                // check that the operation throws the expected exception
                Assert.AreEqual(e.GetType(), typeof(InvalidOperationException));
            }

            // test of MinimalUserRankFeedEntityDeserialize

            // first, check the normal case
            input = "abc383" + separator + "def888";
            var result2 = StoreSerializers.MinimalUserRankFeedEntityDeserialize(input);
            Assert.AreEqual("abc383", result2.UserHandle);
            Assert.AreEqual("def888", result2.AppHandle);

            // check the null case
            input = null;
            result2 = StoreSerializers.MinimalUserRankFeedEntityDeserialize(input);
            Assert.AreEqual(null, result2);

            // check the null string case
            input = separator;
            result2 = StoreSerializers.MinimalUserRankFeedEntityDeserialize(input);
            Assert.AreEqual(string.Empty, result2.UserHandle);
            Assert.AreEqual(string.Empty, result2.AppHandle);

            // check a case that must never happen
            input = separator + separator;
            try
            {
                result2 = StoreSerializers.MinimalUserRankFeedEntityDeserialize(input);
                Assert.Fail("Deserialize routine was expected to throw an exception");
            }
            catch (Exception e)
            {
                // check that the operation throws the expected exception
                Assert.AreEqual(e.GetType(), typeof(InvalidOperationException));
            }
        }
    }
}
