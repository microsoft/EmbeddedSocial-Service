// <copyright file="UserDocumentResult.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Search
{
    /// <summary>
    /// search results from searching on users
    /// </summary>
    public class UserDocumentResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserDocumentResult"/> class.
        /// </summary>
        /// <param name="appHandle">uniquely identifies the app</param>
        /// <param name="userHandle">uniquely identifies the user</param>
        public UserDocumentResult(string appHandle, string userHandle)
        {
            this.AppHandle = appHandle;
            this.UserHandle = userHandle;
        }

        /// <summary>
        /// Gets or sets the unique identifier for the app
        /// </summary>
        public string AppHandle { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the user
        /// </summary>
        public string UserHandle { get; set; }
    }
}
