// <copyright file="CVSRequest.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.CVS
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// CVS request
    /// </summary>
    /// <remarks>
    /// A CVS request contains a list of content items to be reviewed.
    ///
    /// The Content Validation Service (CVS) is a content moderation provider.
    /// The API is documented at http://cvs-docs.azurewebsites.net/
    /// </remarks>
    public class CVSRequest : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CVSRequest"/> class
        /// </summary>
        /// <param name="serviceUri">CVS service uri</param>
        /// <param name="subscriptionKey">Subscription key</param>
        public CVSRequest(Uri serviceUri, string subscriptionKey)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Submits request for CVS to review all content items as an async Job
        /// </summary>
        /// <remarks>
        /// You may call this routine more than once (i.e. submit the same content items as multiple jobs).
        /// </remarks>
        /// <param name="content">content to submit for review</param>
        /// <param name="callbackUri">uri that CVS should use for its callback</param>
        /// <returns>Job id of CVS submission</returns>
        public async Task<string> SubmitAsyncJob(CVSContent content, Uri callbackUri)
        {
            if (callbackUri == null)
            {
                throw new ArgumentNullException("callbackUri");
            }

            // the callback url must use SSL, because CVS uses X509 certificate-based authentication on the callback
            if (callbackUri.IsUnc || callbackUri.IsFile || callbackUri.IsLoopback || callbackUri.Scheme != Uri.UriSchemeHttps)
            {
                throw new ArgumentException($"Uri value not allowed, callbackUri = {callbackUri.ToString()}", "callbackUri");
            }

            // avoid await warning
            await Task.Delay(0);
            throw new NotImplementedException();
        }

        /// <summary>
        /// Disposes <see cref="CVSRequest"/>
        /// </summary>
        public void Dispose()
        {
        }
    }
}
