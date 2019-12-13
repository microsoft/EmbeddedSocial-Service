// <copyright file="Email.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Email
{
    using System.Collections.Generic;

    /// <summary>
    /// Email for sending on behalf of Embedded Social using SendGrid
    /// </summary>
    public class Email : IEmail
    {
        /// <summary>
        /// Gets or sets the email addresses to send the email to
        /// </summary>
        public List<string> To { get; set; }

        /// <summary>
        /// Gets or sets the email address that the email is coming from
        /// </summary>
        public string FromAddress { get; set; } = "emsocial@microsoft.com";

        /// <summary>
        /// Gets or sets the name that the email is coming from
        /// </summary>
        public string FromName { get; set; } = "Microsoft Embedded Social";

        /// <summary>
        /// Gets or sets the subject of the email
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets the body of the email, in HTML
        /// </summary>
        public string HtmlBody { get; set; }

        /// <summary>
        /// Gets or sets the body of the email, in text
        /// </summary>
        public string TextBody { get; set; }

        /// <summary>
        /// Gets or sets the footer of the email, in HTML
        /// </summary>
        public string HtmlFooter { get; set; } =
            "<p><hr><i><small>" +
            "Microsoft respects your privacy. To learn more, please read our <a href=\"http://go.microsoft.com/fwlink/?LinkId=248681\">Privacy Statement</a>." +
            "<br><p>" +
            "Microsoft Corporation<br>" +
            "One Microsoft Way<br>" +
            "Redmond, WA, USA 98052" +
            "</small></i>";

        /// <summary>
        /// Gets or sets the footer of the email, in text
        /// </summary>
        public string TextFooter { get; set; } =
            "Microsoft respects your privacy. To learn more, please read our Privacy Statement : http://go.microsoft.com/fwlink/?LinkId=248681" +
            "\n" +
            "Microsoft Corporation\n" +
            "One Microsoft Way\n" +
            "Redmond, WA, USA 98052";

        /// <summary>
        /// Gets or sets the type of email, for reporting purposes
        /// </summary>
        public string Category { get; set; }
    }
}