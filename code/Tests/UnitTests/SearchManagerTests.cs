// <copyright file="SearchManagerTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace SocialPlus.UnitTests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SocialPlus.Server.Managers;

    /// <summary>
    /// Test SearchManager functionality
    /// </summary>
    [TestClass]
    public class SearchManagerTests
    {
        /// <summary>
        /// Tests the ContainsOnlyHashtags method
        /// </summary>
        [TestMethod]
        public void ContainsOnlyHashtagsTests()
        {
            // instantiate a dummy search manager
            SearchManager searchManager = new SearchManager(null, null);
            PrivateObject searchManagerPrivate = new PrivateObject(searchManager);

            // test functionality
            Assert.IsFalse((bool)searchManagerPrivate.Invoke("ContainsOnlyHashtags", string.Empty));
            Assert.IsFalse((bool)searchManagerPrivate.Invoke("ContainsOnlyHashtags", "    "));
            Assert.IsTrue((bool)searchManagerPrivate.Invoke("ContainsOnlyHashtags", "#hello"));
            Assert.IsTrue((bool)searchManagerPrivate.Invoke("ContainsOnlyHashtags", "#hello #bye"));
            Assert.IsFalse((bool)searchManagerPrivate.Invoke("ContainsOnlyHashtags", "hello"));
            Assert.IsFalse((bool)searchManagerPrivate.Invoke("ContainsOnlyHashtags", "#hello goodbye"));
            Assert.IsFalse((bool)searchManagerPrivate.Invoke("ContainsOnlyHashtags", "hello #goodbye"));
            Assert.IsFalse((bool)searchManagerPrivate.Invoke("ContainsOnlyHashtags", " # hello "));
            Assert.IsFalse((bool)searchManagerPrivate.Invoke("ContainsOnlyHashtags", "# #hello"));
            Assert.IsTrue((bool)searchManagerPrivate.Invoke("ContainsOnlyHashtags", " #hello "));
        }
    }
}