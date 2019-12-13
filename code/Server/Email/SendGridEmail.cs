// <copyright file="SendGridEmail.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Email
{
    using System;
    using System.Reflection;
    using System.Threading.Tasks;

    using SendGrid;
    using SendGrid.Helpers.Mail;
    using SocialPlus.Logging;
    using SocialPlus.Utils;

    /// <summary>
    /// Send Email using the SendGrid service
    /// </summary>
    public class SendGridEmail : ISendEmail
    {
        /// <summary>
        /// The key that allows us to send e-mail with SendGrid
        /// </summary>
        private readonly string sendGridKey = null;

        private readonly ILog log;

        /// <summary>
        /// Initializes a new instance of the <see cref="SendGridEmail"/> class.
        /// </summary>
        /// <param name="log">log</param>
        /// <param name="sendGridKey">SendGrid key</param>
        public SendGridEmail(ILog log, string sendGridKey)
        {
            this.log = log;
            this.sendGridKey = sendGridKey;
        }

        /// <summary>
        /// Send an email
        /// </summary>
        /// <param name="email">contents of the email to send</param>
        /// <returns>send email task</returns>
        public async Task SendEmail(IEmail email)
        {
            // check the email addresses
            if (email.To == null || email.To.Count < 1)
            {
                this.log.LogException("got empty email address list");
            }

            foreach (string to in email.To)
            {
                if (!EmailAddressChecker.IsValidEmail(to))
                {
                    this.log.LogException("got bad email address: " + to);
                }
            }

            // create a new message
            SendGridMessage myMessage = new SendGridMessage();

            // assign the from address
            myMessage.From = new EmailAddress(email.FromAddress, email.FromName);

            // assign the recipients
            foreach (string emailAddress in email.To)
            {
                myMessage.AddTo(emailAddress);
            }

            // assign the subject
            myMessage.Subject = email.Subject;

            // assign the body of the email
            myMessage.HtmlContent = email.HtmlBody;
            myMessage.PlainTextContent = email.TextBody;

            // assign the footer of the email
            myMessage.SetFooterSetting(true, email.HtmlFooter, email.TextFooter);

            // email reporting features
            myMessage.SetClickTracking(true, false); // weekly stats of which links users opened in our emails
            myMessage.SetOpenTracking(true, string.Empty); // weekly stats of how many of our emails were opened

            // set email category. All e-mails must have category in place.
            string category = null;
            PropertyInfo[] properties = email.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo property in properties)
            {
                if (property.Name == "Category")
                {
                    object categoryPropObj = property.GetValue(email, null);
                    if (categoryPropObj == null)
                    {
                        this.log.LogException(new ArgumentException("Each email message must have a category. Please set one."));
                    }

                    category = categoryPropObj.ToString();
                }
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                myMessage.AddCategory(category); // email stats will be bucketed by reportingCategory
            }

            // send it
            SendGridClient client = new SendGridClient(this.sendGridKey);
            this.log.LogInformation("Sending email from:" + email.FromAddress + ", to:" + string.Join(",", email.To.ToArray()) + " subject:" + email.Subject);
            Response response = await client.SendEmailAsync(myMessage);
            if (response.StatusCode != System.Net.HttpStatusCode.Accepted && response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                this.log.LogException(new Exception("Got " + response.StatusCode.ToString() + " status code from Sendgrid"));
            }
        }
    }
}