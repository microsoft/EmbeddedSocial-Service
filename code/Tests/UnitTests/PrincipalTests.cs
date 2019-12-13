// <copyright file="PrincipalTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.UnitTests
{
    using System;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SocialPlus.Logging;
    using SocialPlus.Models;
    using SocialPlus.Server.Principal;

    /// <summary>
    /// Unit tests for UserPrincipal and AppPrincipal
    /// </summary>
    [TestClass]
    public class PrincipalTests
    {
        /// <summary>
        /// Serialized and deserialize user principal
        /// </summary>
        [TestMethod]
        public void SerializeDeserializeUserPrincipal()
        {
            var log = new Log(LogDestination.Debug, Log.DefaultCategoryName);
            UserPrincipal userPrincipal = new UserPrincipal(log, "genericUserHandle", IdentityProviderType.AADS2S, "genericUserHandle");
            string serializedUserPrincipal = userPrincipal.ToString();
            UserPrincipal deserializedUserPrincipal = new UserPrincipal(log, serializedUserPrincipal, IdentityProviderType.AADS2S, "genericUserHandle");
            Assert.AreEqual(userPrincipal.UserHandle, deserializedUserPrincipal.UserHandle);
        }

        /// <summary>
        /// Serialized and deserialize app principal
        /// </summary>
        [TestMethod]
        public void SerializeDeserializeAppPrincipal()
        {
            AppPrincipal appPrincipal = new AppPrincipal("genericAppHandle", "genericAppKey");
            string serializedAppPrincipal = appPrincipal.ToString();
            AppPrincipal deserializedAppPrincipal = new AppPrincipal(serializedAppPrincipal);
            Assert.AreEqual(appPrincipal.AppHandle, deserializedAppPrincipal.AppHandle);
            Assert.AreEqual(appPrincipal.AppKey, deserializedAppPrincipal.AppKey);
        }

        /// <summary>
        /// Tests that initializing appPrincipal from a malformed serialized appPrincipal fails
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SerializedAppPrincipalMustContainNewlineChar()
        {
            string maformedSerializedAppPrincipal = "genericStringThatContainsNoNewline";
            AppPrincipal appPrincipal = new AppPrincipal(maformedSerializedAppPrincipal);
        }
    }
}
