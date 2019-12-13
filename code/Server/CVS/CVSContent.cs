// <copyright file="CVSContent.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.CVS
{
    using System;
    using System.Collections.Generic;

    using Newtonsoft.Json;

    /// <summary>
    /// Create a CVS content object for use in a CVS request
    /// </summary>
    public class CVSContent
    {
        /// <summary>
        /// json serializer settings
        /// </summary>
        private readonly JsonSerializerSettings serializerSettings = new JsonSerializerSettings()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Formatting = Formatting.None
        };

        /// <summary>
        /// Adds a text content item to the request
        /// </summary>
        /// <param name="text">Text to add</param>
        /// <param name="externalId">External id for item (optional)</param>
        public void AddText(string text, string externalId = null)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentNullException("text");
            }

            externalId = externalId ?? Guid.NewGuid().ToString();
            var now = DateTime.UtcNow;

            throw new NotImplementedException();
        }

        /// <summary>
        /// Adds an image content item to the request
        /// </summary>
        /// <param name="uri">The image uri</param>
        /// <param name="externalId">External id for item (optional)</param>
        public void AddImageUri(Uri uri, string externalId = null)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }

            if (uri.IsUnc || uri.IsFile || uri.IsLoopback)
            {
                throw new ArgumentException($"Uri value is not allowed, uri = {uri.ToString()}", "uri");
            }

            externalId = externalId ?? Guid.NewGuid().ToString();
            var now = DateTime.UtcNow;

            throw new NotImplementedException();
        }

        /// <summary>
        /// Converts all content items to be reviewed as part of this job into a json string
        /// </summary>
        /// <returns><see cref="IList{ContentItem}"/> as json string</returns>
        public string GetContentItemsAsJson()
        {
            throw new NotImplementedException();
        }
    }
}
