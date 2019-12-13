// <copyright file="AuthParserTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.UnitTests
{
    using System;
    using System.Collections.Generic;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Simple tests for AuthParser code
    /// </summary>
    [TestClass]
    public class AuthParserTests
    {
        /// <summary>
        /// Testing invalid authentication header parameters
        /// </summary>
        [TestMethod]
        public void AuthParserInvalidAuthHeaderParameters()
        {
            var managersContext = new ManagersContext();
            var commonAuthManager = new PrivateObject(managersContext.CommonAuthManager);
            string authParameter = null;
            Dictionary<string, string> authDictionary = null;
            object[] args = null;

            try
            {
                authParameter = (string)null;
                authDictionary = new Dictionary<string, string>();
                args = new object[2] { authParameter, authDictionary };
                commonAuthManager.Invoke("ParseAuthParameter", args);
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex.Message.Contains("Auth header parameter is null or empty."));
            }

            try
            {
                authParameter = string.Empty;
                authDictionary = new Dictionary<string, string>();
                args = new object[2] { authParameter, authDictionary };
                commonAuthManager.Invoke("ParseAuthParameter", args);
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex.Message.Contains("Auth header parameter is null or empty."));
            }

            try
            {
                authParameter = " ";
                authDictionary = new Dictionary<string, string>();
                args = new object[2] { authParameter, authDictionary };
                commonAuthManager.Invoke("ParseAuthParameter", args);
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex.Message.Contains("Auth header parameter is null or empty."));
            }

            try
            {
                authParameter = "3uvFaCx05pp";
                authDictionary = new Dictionary<string, string>();
                args = new object[2] { authParameter, authDictionary };
                commonAuthManager.Invoke("ParseAuthParameter", args);
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex.Message.Contains("Auth header parameter is malformed. It includes a string delimited by '|' but lacking a '=' separator."));
            }

            try
            {
                authParameter = "|";
                authDictionary = new Dictionary<string, string>();
                args = new object[2] { authParameter, authDictionary };
                commonAuthManager.Invoke("ParseAuthParameter", args);
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex.Message.Contains("Auth header parameter is null or empty."));
            }

            try
            {
                authParameter = "||";
                authDictionary = new Dictionary<string, string>();
                args = new object[2] { authParameter, authDictionary };
                commonAuthManager.Invoke("ParseAuthParameter", args);
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex.Message.Contains("Auth header parameter is null or empty."));
            }

            try
            {
                authParameter = "=||=";
                authDictionary = new Dictionary<string, string>();
                args = new object[2] { authParameter, authDictionary };
                commonAuthManager.Invoke("ParseAuthParameter", args);
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex.Message.Contains("Auth header parameter is malformed. It includes a string delimited by '|' but lacking a '=' separator."));
            }

            authParameter = "=|=";
            authDictionary = new Dictionary<string, string>();
            args = new object[2] { authParameter, authDictionary };
            commonAuthManager.Invoke("ParseAuthParameter", args);
            Assert.IsTrue(authDictionary.Count == 0);
        }

        /// <summary>
        /// Testing valid authentication header parameters
        /// </summary>
        [TestMethod]
        public void AuthParserValidAuthHeaderParameters()
        {
            var managersContext = new ManagersContext();
            var commonAuthManager = new PrivateObject(managersContext.CommonAuthManager);
            string authParameter = null;
            Dictionary<string, string> authDictionary = null;
            object[] args = null;

            authParameter = "ak=value";
            authDictionary = new Dictionary<string, string>();
            args = new object[2] { authParameter, authDictionary };
            commonAuthManager.Invoke("ParseAuthParameter", args);
            Assert.IsTrue(authDictionary.Count == 1);
            Assert.AreEqual("value", authDictionary["ak"]);

            authParameter = "ak=value|";
            authDictionary = new Dictionary<string, string>();
            args = new object[2] { authParameter, authDictionary };
            commonAuthManager.Invoke("ParseAuthParameter", args);
            Assert.IsTrue(authDictionary.Count == 1);
            Assert.AreEqual("value", authDictionary["ak"]);

            authParameter = "ak = value|";
            authDictionary = new Dictionary<string, string>();
            args = new object[2] { authParameter, authDictionary };
            commonAuthManager.Invoke("ParseAuthParameter", args);
            Assert.IsTrue(authDictionary.Count == 1);
            Assert.AreEqual("value", authDictionary["ak"]);

            authParameter = "ak = va=lue";
            authDictionary = new Dictionary<string, string>();
            args = new object[2] { authParameter, authDictionary };
            commonAuthManager.Invoke("ParseAuthParameter", args);
            Assert.IsTrue(authDictionary.Count == 1);
            Assert.AreEqual("va=lue", authDictionary["ak"]);

            authParameter = "ak=value|tk==value2";
            authDictionary = new Dictionary<string, string>();
            args = new object[2] { authParameter, authDictionary };
            commonAuthManager.Invoke("ParseAuthParameter", args);
            Assert.IsTrue(authDictionary.Count == 2);
            Assert.AreEqual("value", authDictionary["ak"]);
            Assert.AreEqual("=value2", authDictionary["tk"]);
        }
    }
}
