// <copyright file="UserDocument.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Search
{
    /// <summary>
    /// a user "document" that will be inserted into the Azure Search index
    /// </summary>
    public class UserDocument : Document
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserDocument"/> class.
        /// </summary>
        /// <param name="firstName">user's first name</param>
        /// <param name="lastName">user's last name</param>
        /// <param name="appHandle">uniquely identifies the app</param>
        /// <param name="userHandle">uniquely identifies the user</param>
        public UserDocument(string firstName, string lastName, string appHandle, string userHandle)
        {
            this.Key = userHandle + appHandle;
            this.FirstName = firstName;
            this.LastName = lastName;
            this.AppHandle = appHandle;
            this.UserHandle = userHandle;
        }

        /// <summary>
        /// Gets or sets the unique identifier for this entry
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the user's first name
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the user's last name
        /// </summary>
        public string LastName { get; set; }

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
