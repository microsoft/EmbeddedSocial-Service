// <copyright file="SearchTopics.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace SocialPlus.UnitTests
{
    using System;
    using System.Collections.Generic;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SocialPlus.Logging;
    using SocialPlus.Server.Search;
    using SocialPlus.Utils;

    /// <summary>
    /// Test all topic search functionality
    /// </summary>
    [TestClass]
    public class SearchTopics
    {
        /// <summary>
        /// tests basic topic search functionality
        /// </summary>
        [TestMethod]
        public void BasicSearchTopicsTests()
        {
            // get the connection strings
            ISettingsReader sr = new AppSettingsReader();
            string searchServiceName = sr.ReadValue("SearchServiceName");
            string searchServiceAdminKey = sr.ReadValue("SearchServiceAdminKey");

            // TO do this test successfully, we need to DELETE THE INDEX!!!
            // THIS TEST IS DESTRUCTIVE
            // make sure this is not production
            Assert.IsFalse(ProdConfiguration.IsProduction(searchServiceName));

            var log = new Log(LogDestination.Debug, Log.DefaultCategoryName);

            // instantiate the search interface
            SocialPlus.Server.Search.SearchTopics searchTopics = new SocialPlus.Server.Search.SearchTopics(log, searchServiceName, searchServiceAdminKey);

            // clean the index
            searchTopics.DeleteIndex().Wait();
            searchTopics.CreateIndex().Wait();
            System.Threading.Thread.Sleep(1000); // there appears to be an Azure delay in indexing

            // test empty topic tags
            List<TopicDocument> topics = new List<TopicDocument>();
            TopicDocument topicEmptyTags = new TopicDocument("topicEmptyTags", null, "hello", null, "freshpaint", "John", 1, DateTime.Now);
            topics.Add(topicEmptyTags);
            searchTopics.AddTopics(topics).Wait();
            System.Threading.Thread.Sleep(1000); // there appears to be an Azure delay in indexing
            List<TopicDocumentResult> resultEmptyTags = searchTopics.Search(topicEmptyTags.TopicText, searchTopicTags: false, searchTopicText: true).Result;
            Assert.IsNotNull(resultEmptyTags);
            Assert.AreEqual(resultEmptyTags.Count, 1);
            Assert.AreEqual(resultEmptyTags[0].TopicHandle, topicEmptyTags.TopicHandle);

            // cleanup
            topics.Clear();
            searchTopics.DeleteTopic(topicEmptyTags.TopicHandle).Wait();

            // insert some items
            TopicDocument topic1 = new TopicDocument("topic1", "title1212", "text1", new List<string>() { "tag1", "tag2" }, "app1", "user1", 95, DateTime.Now.AddDays(-1));
            TopicDocument topic2 = new TopicDocument("topic2", "title2", "title1212", new List<string>() { topic1.TopicTags[0], "tag3" }, "app2", "user2", 90, DateTime.Now);
            topics.Add(topic1);
            topics.Add(topic2);
            searchTopics.AddTopics(topics).Wait();
            System.Threading.Thread.Sleep(1000); // there appears to be an Azure delay in indexing

            // basic search functionality and ordering
            List<TopicDocumentResult> result1 = searchTopics.Search(topic1.TopicTags[0]).Result;
            Assert.IsNotNull(result1);
            Assert.AreEqual(result1.Count, 2);
            Assert.AreEqual(result1[0].TopicHandle, topic2.TopicHandle);
            Assert.AreEqual(result1[1].TopicHandle, topic1.TopicHandle);

            // test search on other fields
            result1 = searchTopics.Search("titl*", null, null, false, true, false).Result;
            Assert.IsNotNull(result1);
            Assert.AreEqual(result1.Count, 2);
            List<string> topicHandles = result1.ConvertAll<string>(o => o.TopicHandle);
            Assert.IsTrue(topicHandles.Contains(topic1.TopicHandle));
            Assert.IsTrue(topicHandles.Contains(topic2.TopicHandle));

            result1 = searchTopics.Search("title2", null, null, false, true, false).Result;
            Assert.IsNotNull(result1);
            Assert.AreEqual(result1.Count, 1);
            Assert.AreEqual(result1[0].TopicHandle, topic2.TopicHandle);

            result1 = searchTopics.Search("tag*", null, null, false, true, false).Result;
            Assert.IsNotNull(result1);
            Assert.AreEqual(result1.Count, 0);

            result1 = searchTopics.Search("title1212", null, null, false, false, true).Result;
            Assert.IsNotNull(result1);
            Assert.AreEqual(result1.Count, 1);
            Assert.AreEqual(result1[0].TopicHandle, topic2.TopicHandle);

            result1 = searchTopics.Search("title1212", null, null, false, true, true).Result;
            Assert.IsNotNull(result1);
            Assert.AreEqual(result1.Count, 2);
            topicHandles = result1.ConvertAll<string>(o => o.TopicHandle);
            Assert.IsTrue(topicHandles.Contains(topic1.TopicHandle));
            Assert.IsTrue(topicHandles.Contains(topic2.TopicHandle));

            // test search for a particular user's topics
            result1 = searchTopics.Search("tag*", null, topic2.UserHandle).Result;
            Assert.IsNotNull(result1);
            Assert.AreEqual(result1.Count, 1);
            Assert.AreEqual(result1[0].TopicHandle, topic2.TopicHandle);

            // test search for a particular user and particular apps topics
            result1 = searchTopics.Search("tag*", topic2.AppHandle, topic2.UserHandle).Result;
            Assert.IsNotNull(result1);
            Assert.AreEqual(result1.Count, 1);
            Assert.AreEqual(result1[0].TopicHandle, topic2.TopicHandle);

            result1 = searchTopics.Search("tag*", topic1.AppHandle, topic2.UserHandle).Result;
            Assert.IsNotNull(result1);
            Assert.AreEqual(result1.Count, 0);

            // test updates
            topic2.SearchWeight = 80;
            topics.Clear();
            topics.Add(topic2);
            searchTopics.AddTopics(topics).Wait();
            System.Threading.Thread.Sleep(1000); // there appears to be an Azure delay in indexing
            result1 = searchTopics.Search(topic1.TopicTags[0]).Result;
            Assert.IsNotNull(result1);
            Assert.AreEqual(result1.Count, 2);
            Assert.AreEqual(result1[0].TopicHandle, topic1.TopicHandle);
            Assert.AreEqual(result1[1].TopicHandle, topic2.TopicHandle);

            // search with apps
            result1 = searchTopics.Search(topic1.TopicTags[0], topic2.AppHandle).Result;
            Assert.IsNotNull(result1);
            Assert.AreEqual(result1.Count, 1);
            Assert.AreEqual(result1[0].TopicHandle, topic2.TopicHandle);

            // search for limited set of records
            result1 = searchTopics.Search(topic1.TopicTags[0], null, null, true, false, false, 0, 1).Result;
            Assert.IsNotNull(result1);
            Assert.AreEqual(result1.Count, 1);
            Assert.AreEqual(result1[0].TopicHandle, topic1.TopicHandle);

            // search for non-existent record
            result1 = searchTopics.Search(new Random().Next().ToString()).Result;
            Assert.AreEqual(result1.Count, 0);

            // delete
            searchTopics.DeleteTopic(topic1.TopicHandle).Wait();
            System.Threading.Thread.Sleep(1000); // there appears to be an Azure delay in indexing
            result1 = searchTopics.Search(topic1.TopicTags[0]).Result;
            Assert.IsNotNull(result1);
            Assert.AreEqual(result1.Count, 1);
            Assert.AreEqual(result1[0].TopicHandle, topic2.TopicHandle);

            // number of records
            long numRecords = searchTopics.GetNumberOfDocuments().Result;
            Assert.AreNotEqual(numRecords, -1);

            // number of bytes
            long numBytes = searchTopics.GetStorageUsage().Result;
            Assert.AreNotEqual(numBytes, -1);

            // reset
            searchTopics.DeleteIndex().Wait();
            searchTopics.CreateIndex().Wait();
        }

        /// <summary>
        /// tests advanced topic search functionality
        /// </summary>
        [TestMethod]
        public void AdvancedSearchTopicsTests()
        {
            // get the connection strings
            AppSettingsReader sr = new AppSettingsReader();
            string searchServiceName = sr.ReadValue("SearchServiceName");
            string searchServiceAdminKey = sr.ReadValue("SearchServiceAdminKey");

            // TO do this test successfully, we need to DELETE THE INDEX!!!
            // THIS TEST IS DESTRUCTIVE
            // make sure this is not production
            Assert.IsFalse(ProdConfiguration.IsProduction(searchServiceName));

            var log = new Log(LogDestination.Debug, Log.DefaultCategoryName);

            // instantiate the search interface
            SocialPlus.Server.Search.SearchTopics searchTopics = new SocialPlus.Server.Search.SearchTopics(log, searchServiceName, searchServiceAdminKey);

            // clean the index
            searchTopics.DeleteIndex().Wait();
            searchTopics.CreateIndex().Wait();
            System.Threading.Thread.Sleep(1000); // there appears to be an Azure delay in indexing

            // insert some items
            List<TopicDocument> topics = new List<TopicDocument>();
            TopicDocument topic1 = new TopicDocument("topic1", "title1", "text1", new List<string>() { "jane", "canada" }, "app1", "user1", 1, DateTime.Now.AddDays(-21));
            TopicDocument topic2 = new TopicDocument("topic2", "title2", "text2", new List<string>() { topic1.TopicTags[1], "britain" }, topic1.AppHandle, "user2", 1, DateTime.Now.AddDays(-21));
            TopicDocument topic3 = new TopicDocument("topic3", "title3", "text3", new List<string>() { topic1.TopicTags[0] }, topic1.AppHandle, "user3", 1, DateTime.Now.AddDays(-21));
            TopicDocument topic4 = new TopicDocument("topic4", "title4", "text4", new List<string>() { "california", "!!hai!!" }, "app2", "user4", 1, DateTime.Now);
            TopicDocument topic5 = new TopicDocument("topic5", "title5", "text5", new List<string>() { topic4.TopicTags[0] }, topic4.AppHandle, "user5", 1, DateTime.Now);
            TopicDocument topic6 = new TopicDocument("topic6", "title6", "text6", new List<string>() { topic4.TopicTags[1] }, topic4.AppHandle, "user6", 1, DateTime.Now);
            TopicDocument topic7 = new TopicDocument("topic7", "title7", "text7", new List<string>() { topic4.TopicTags[1], "cantilever" }, topic4.AppHandle, "user7", 1, DateTime.Now);
            topics.Add(topic1);
            topics.Add(topic2);
            topics.Add(topic3);
            topics.Add(topic4);
            topics.Add(topic5);
            topics.Add(topic6);
            topics.Add(topic7);
            searchTopics.AddTopics(topics).Wait();
            System.Threading.Thread.Sleep(1000); // there appears to be an Azure delay in indexing

            // test trending topics
            TimeSpan month = new TimeSpan(31, 0, 0, 0);
            TimeSpan week = new TimeSpan(7, 0, 0, 0);
            List<Tuple<string, double>> trendingTopics = searchTopics.GetTrendingTopicTags(month).Result;
            Assert.AreEqual(trendingTopics.Count, 6);
            Assert.AreEqual(trendingTopics[0].Item1, topic4.TopicTags[1]);
            Assert.AreEqual(trendingTopics[0].Item2, 3);
            Assert.AreEqual(trendingTopics[1].Item2, 2);
            Assert.AreEqual(trendingTopics[2].Item2, 2);
            Assert.AreEqual(trendingTopics[3].Item2, 2);
            Assert.AreEqual(trendingTopics[4].Item2, 1);
            Assert.AreEqual(trendingTopics[5].Item2, 1);

            // test trending topics filtered by app
            trendingTopics = searchTopics.GetTrendingTopicTags(month, topic1.AppHandle).Result;
            Assert.AreEqual(trendingTopics.Count, 3);
            Assert.AreEqual(trendingTopics[0].Item2, 2);
            Assert.AreEqual(trendingTopics[1].Item2, 2);
            Assert.AreEqual(trendingTopics[2].Item1, topic2.TopicTags[1]);
            Assert.AreEqual(trendingTopics[2].Item2, 1);

            // test trending topics for past week
            trendingTopics = searchTopics.GetTrendingTopicTags(week).Result;
            Assert.AreEqual(trendingTopics.Count, 3);
            Assert.AreEqual(trendingTopics[0].Item1, topic4.TopicTags[1]);
            Assert.AreEqual(trendingTopics[0].Item2, 3);
            Assert.AreEqual(trendingTopics[1].Item1, topic4.TopicTags[0]);
            Assert.AreEqual(trendingTopics[1].Item2, 2);
            Assert.AreEqual(trendingTopics[2].Item1, topic7.TopicTags[1]);
            Assert.AreEqual(trendingTopics[2].Item2, 1);

            // test autocomplete
            List<string> autocomplete = new List<string>();
            autocomplete = searchTopics.AutoCompleteTopicTag("bri").Result;
            Assert.AreEqual(autocomplete.Count, 2);
            Assert.IsTrue(autocomplete.Contains(topic1.TopicTags[0]));
            Assert.IsTrue(autocomplete.Contains(topic2.TopicTags[1]));

            // test autocomplete non fuzzy search
            autocomplete = searchTopics.AutoCompleteTopicTag("can").Result;
            Assert.AreEqual(autocomplete.Count, 2);
            Assert.IsTrue(autocomplete.Contains(topic1.TopicTags[1]));
            Assert.IsTrue(autocomplete.Contains(topic7.TopicTags[1]));

            // test autocomplete fuzzy search
            autocomplete = searchTopics.AutoCompleteTopicTag("can", null, true).Result;
            Assert.AreEqual(autocomplete.Count, 3);
            Assert.IsTrue(autocomplete.Contains(topic1.TopicTags[1]));
            Assert.IsTrue(autocomplete.Contains(topic4.TopicTags[0]));
            Assert.IsTrue(autocomplete.Contains(topic7.TopicTags[1]));

            // test autocomplete fuzzy search with app filter
            autocomplete = searchTopics.AutoCompleteTopicTag("can", topic4.AppHandle, true).Result;
            Assert.AreEqual(autocomplete.Count, 2);
            Assert.IsTrue(autocomplete.Contains(topic4.TopicTags[0]));
            Assert.IsTrue(autocomplete.Contains(topic7.TopicTags[1]));

            // test autocomplete with symbols
            autocomplete = searchTopics.AutoCompleteTopicTag("!!h").Result;
            Assert.AreEqual(autocomplete.Count, 1);
            Assert.AreEqual(autocomplete[0], topic4.TopicTags[1]);

            // reset
            searchTopics.DeleteIndex().Wait();
            searchTopics.CreateIndex().Wait();
        }
    }
}
