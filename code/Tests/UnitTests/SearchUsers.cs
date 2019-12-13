// <copyright file="SearchUsers.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.UnitTests
{
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SocialPlus.Logging;
    using SocialPlus.Server.Search;
    using SocialPlus.Utils;

    /// <summary>
    /// Tests user search
    /// </summary>
    [TestClass]
    public class SearchUsers
    {
        /// <summary>
        /// Tests all user search functionality
        /// </summary>
        [TestMethod]
        public void AllSearchUsersTests()
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
            SocialPlus.Server.Search.SearchUsers searchUsers = new SocialPlus.Server.Search.SearchUsers(log, searchServiceName, searchServiceAdminKey);

            // clean the index
            searchUsers.DeleteIndex().Wait();
            searchUsers.CreateIndex().Wait();
            System.Threading.Thread.Sleep(1000); // there appears to be an Azure delay in indexing

            // insert some items
            List<UserDocument> users = new List<UserDocument>();
            users.Add(new UserDocument("f00_testXXX", "l00_testXXX", "app1", "u00_testXXX"));
            users.Add(new UserDocument("f01_testXXX", "l01_testXXX", "app1", "u01_testXXX"));
            users.Add(new UserDocument("f02_testXXX", "l02_testXXX", "app2", "u02_testXXX"));
            searchUsers.AddUsers(users).Wait();
            System.Threading.Thread.Sleep(1000); // there appears to be an Azure delay in indexing

            // basic search functionality
            List<UserDocumentResult> result1 = searchUsers.Search("l00_testXXX").Result;
            Assert.IsNotNull(result1);
            Assert.AreEqual(result1.Count, 1);
            Assert.AreEqual(result1[0].AppHandle, "app1");
            Assert.AreEqual(result1[0].UserHandle, "u00_testXXX");

            // substring search
            result1 = searchUsers.Search("f0*").Result;
            Assert.IsNotNull(result1);
            Assert.AreEqual(result1.Count, 3);
            List<string> userHandles = result1.ConvertAll<string>(o => o.UserHandle);
            Assert.IsTrue(userHandles.Contains("u00_testXXX"));
            Assert.IsTrue(userHandles.Contains("u01_testXXX"));
            Assert.IsTrue(userHandles.Contains("u02_testXXX"));

            // limit search functionality by app
            result1 = searchUsers.Search("f0*", "app1").Result;
            Assert.IsNotNull(result1);
            Assert.AreEqual(result1.Count, 2);
            userHandles = result1.ConvertAll<string>(o => o.UserHandle);
            Assert.IsTrue(userHandles.Contains("u00_testXXX"));
            Assert.IsTrue(userHandles.Contains("u01_testXXX"));

            // test updates
            UserDocument user = new UserDocument("d00_testXXX", "l00_testXXX", "app1", "u00_testXXX");
            searchUsers.AddUser(user).Wait();
            System.Threading.Thread.Sleep(1000); // there appears to be an Azure delay in indexing
            result1 = searchUsers.Search("d00*").Result;
            Assert.IsNotNull(result1);
            Assert.AreEqual(result1.Count, 1);
            Assert.AreEqual(result1[0].AppHandle, "app1");
            Assert.AreEqual(result1[0].UserHandle, "u00_testXXX");

            // search for limited set of records
            result1 = searchUsers.Search("l0*", null, 1, 2).Result;
            Assert.IsNotNull(result1);
            Assert.AreEqual(result1.Count, 2);
            userHandles = result1.ConvertAll<string>(o => o.UserHandle);
            Assert.IsTrue(userHandles.Contains("u00_testXXX") || userHandles.Contains("u01_testXXX") || userHandles.Contains("u02_testXXX"));
            result1 = searchUsers.Search("l0*", null, 1, 1).Result;
            Assert.IsNotNull(result1);
            Assert.AreEqual(result1.Count, 1);
            userHandles = result1.ConvertAll<string>(o => o.UserHandle);
            Assert.IsTrue(userHandles.Contains("u00_testXXX") || userHandles.Contains("u01_testXXX") || userHandles.Contains("u02_testXXX"));

            // search for non-existent record
            result1 = searchUsers.Search("AA11").Result;
            Assert.AreEqual(result1.Count, 0);

            // number of records
            long numRecords = searchUsers.GetNumberOfDocuments().Result;
            Assert.AreNotEqual(numRecords, -1);

            // number of bytes
            long numBytes = searchUsers.GetStorageUsage().Result;
            Assert.AreNotEqual(numBytes, -1);

            // delete
            searchUsers.DeleteUser("u00_testXXX", "app1").Wait();
            searchUsers.DeleteUser("u01_testXXX", "app1").Wait();
            searchUsers.DeleteUser("u02_testXXX", "app2").Wait();
            System.Threading.Thread.Sleep(1000); // there appears to be an Azure delay in indexing
            result1 = searchUsers.Search("*test*").Result;
            Assert.AreEqual(result1.Count, 0);
        }
    }
}