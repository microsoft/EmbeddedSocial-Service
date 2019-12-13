// <copyright file="SendEmail.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.UnitTests
{
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SocialPlus.Logging;
    using SocialPlus.Server.Email;
    using SocialPlus.TestUtils;

    /// <summary>
    /// Tests email sending functionality
    /// </summary>
    [TestClass]
    public class SendEmail
    {
        /// <summary>
        /// Sends a single email. Check your inbox to see if it worked
        /// </summary>
        [TestMethod]
        public void SendOneEmail()
        {
            var log = new Log(LogDestination.Debug, Log.DefaultCategoryName);
            string configFilePath = ConfigurationManager.AppSettings["ConfigRelativePath"] + Path.DirectorySeparatorChar + TestConstants.ConfigFileName;
            string sendGridKey = TestUtilities.GetSendGridKey(configFilePath);

            // setup the email to send
            Email email = new Email();
            email.To = new List<string>();
            email.To.Add(TestConstants.FailedTestsEmail);
            email.Subject = "testing SendGrid";
            email.HtmlBody = "<html><H1>not much here</H1></html>";
            email.TextBody = "not much here";
            email.Category = "UnitTest";

            // send the email
            SendGridEmail emailSender = new SendGridEmail(log, sendGridKey);
            emailSender.SendEmail(email).Wait();
        }
    }
}