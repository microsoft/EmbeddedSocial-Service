// <copyright file="CVSResponse.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.CVS
{
    using System;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using SocialPlus.Models;
    using SocialPlus.Server.AVERT;

    /// <summary>
    /// CVS Response
    /// </summary>
    /// <remarks>
    /// Process a response from CVS
    ///
    /// The Content Validation Service (CVS) is a content moderation provider.
    /// The API is documented at http://cvs-docs.azurewebsites.net/
    /// </remarks>
    public class CVSResponse
    {
        private readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings()
        {
            Formatting = Formatting.Indented,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="CVSResponse"/> class
        /// </summary>
        /// <param name="responseJson">Response from CVS in a json object</param>
        /// <remarks>
        /// The response json should come directly from the body of the CVS callback
        /// </remarks>
        public CVSResponse(JToken responseJson)
        {
            if (responseJson == null)
            {
                throw new ArgumentNullException("responseJson");
            }

            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets a value indicating whether the job ran successfully
        /// </summary>
        public bool HasCompletedSuccessfully
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Gets a value indicating whether the job failed
        /// </summary>
        public bool HasFailed
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Gets the review status
        /// </summary>
        /// <remarks>
        /// The CVS job determination can return multiple policy violations. This method consolidates them into a single ReviewStatus,
        /// which represents one of three outcomes: banned, mature, or clean. We implement consolidation by returning the highest
        /// violation status we find among all policies.
        /// </remarks>
        /// <returns>review status</returns>
        public ReviewStatus GetReviewStatus()
        {
            if (this.HasFailed)
            {
                throw new InvalidOperationException("CVS moderation job failed. Cannot get review status");
            }

            var notAllowedPolicyCodes = new NotAllowedPolicyCodes();
            var maturePolicyCodes = new MaturePolicyCodes();
            var knownPolicyCodes = new KnownPolicyCodes();

            throw new NotImplementedException();
        }
    }
}
