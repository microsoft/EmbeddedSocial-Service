// <copyright file="ISendEmail.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Email
{
    using System.Threading.Tasks;

    /// <summary>
    /// Send Email
    /// </summary>
    public interface ISendEmail
    {
        /// <summary>
        /// Send an email
        /// </summary>
        /// <param name="email">Contents of the email to send</param>
        /// <returns>send email task</returns>
        Task SendEmail(IEmail email);
    }
}