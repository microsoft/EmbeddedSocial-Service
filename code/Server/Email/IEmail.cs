// <copyright file="IEmail.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Email
{
    using System.Collections.Generic;

    /// <summary>
    /// Email (for sending)
    /// </summary>
    public interface IEmail
    {
        /// <summary>
        /// Gets or sets the email addresses to send the email to
        /// </summary>
        List<string> To { get; set; }

        /// <summary>
        /// Gets or sets the email address that the email is coming from
        /// </summary>
        string FromAddress { get; set; }

        /// <summary>
        /// Gets or sets the name that the email is coming from
        /// </summary>
        string FromName { get; set; }

        /// <summary>
        /// Gets or sets the subject of the email
        /// </summary>
        string Subject { get; set; }

        /// <summary>
        /// Gets or sets the body of the email, in HTML
        /// </summary>
        string HtmlBody { get; set; }

        /// <summary>
        /// Gets or sets the body of the email, in text
        /// </summary>
        string TextBody { get; set; }

        /// <summary>
        /// Gets or sets the footer of the email, in HTML
        /// </summary>
        string HtmlFooter { get; set; }

        /// <summary>
        /// Gets or sets the footer of the email, in text
        /// </summary>
        string TextFooter { get; set; }
    }
}