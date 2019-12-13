// <copyright file="ReportAbuse.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.AVERT
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Interface to the cloud service that handles abuse reporting
    /// </summary>
    public class ReportAbuse
    {
        /// <summary>
        /// Number of times to retry a submission before giving up
        /// </summary>
        private readonly int retryCount = 3;

        /// <summary>
        /// Email address that will receive a summary of results for each request
        /// </summary>
        private readonly string callbackEmailAddress;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportAbuse"/> class.
        /// </summary>
        /// <param name="avertUri">URI to the service</param>
        /// <param name="avertKey">APIM key for accessing the service</param>
        /// <param name="emailAddress">Email address that will receive a summary of results for each request</param>
        public ReportAbuse(Uri avertUri, string avertKey, string emailAddress = "")
        {
            // set the email address
            this.callbackEmailAddress = emailAddress;

            // initialize the service client
            throw new NotImplementedException();
        }

        /// <summary>
        /// Checks if a callback response indicates that there is mature content
        /// </summary>
        /// <param name="response">callback response</param>
        /// <returns>true if content is for mature audiences</returns>
        public bool IsContentMature(string response)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Checks if a callback response indicates that the content is not allowed on Embedded Social
        /// </summary>
        /// <param name="response">callback response</param>
        /// <returns>true if content is not allowed on Embedded Social</returns>
        public bool IsContentNotAllowed(string response)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates and submits a request for reviewing an abuse report
        /// </summary>
        /// <remarks>
        /// Note that there are several fields in a request that we do not fill in with
        /// real values because collecting them could be considered PII collection.
        /// Most of those fields are not required in a request. However,
        /// isoCountry and isoLanguage are two that are required, and so for now, we will allow
        /// default values of USA and English if not specified.
        /// </remarks>
        /// <param name="reason">reason for the report</param>
        /// <param name="reportTime">when the report was made by the user</param>
        /// <param name="callbackUri">URI that the sevice will callback with the result</param>
        /// <param name="text1">reported text1</param>
        /// <param name="text2">reported text2</param>
        /// <param name="text3">reported text3</param>
        /// <param name="imageUri">reported image</param>
        /// <param name="isoCountry">3 letter ISO country that the reporter is in</param>
        /// <param name="isoLanguage">3 letter ISO language that the reporter is using</param>
        /// <returns>response to submitting the request</returns>
        public async Task<string> SubmitReviewRequest(
            SocialPlus.Models.ReportReason reason,
            DateTime reportTime,
            Uri callbackUri,
            string text1 = null,
            string text2 = null,
            string text3 = null,
            Uri imageUri = null,
            string isoCountry = "USA",
            string isoLanguage = "eng")
        {
            // check that the country is valid
            if (string.IsNullOrWhiteSpace(isoCountry))
            {
                throw new ArgumentNullException("isoCountry");
            }

            if (isoCountry.Length != 3)
            {
                throw new ArgumentException("invalid country code: " + isoCountry, "isoCountry");
            }

            // check that the language is valid
            if (string.IsNullOrWhiteSpace(isoLanguage))
            {
                throw new ArgumentNullException("isoLanguage");
            }

            if (isoLanguage.Length != 3)
            {
                throw new ArgumentException("invalid language code: " + isoLanguage, "isoLanguage");
            }

            // check that the report time is valid
            if (reportTime == null)
            {
                throw new ArgumentNullException("reportTime");
            }

            // check that the callback URI is valid
            if (callbackUri == null)
            {
                throw new ArgumentNullException("callbackUri");
            }

            // AVERT will call us back from public Azure. So the URI has to be publically visible.
            // The callback will have an X509 client certificate which we must check and this can only be done over HTTPS.
            if (callbackUri.IsFile || callbackUri.IsLoopback || callbackUri.IsUnc || callbackUri.Scheme != Uri.UriSchemeHttps)
            {
                throw new ArgumentException("callbackUri");
            }

            // check that some content has been provided
            if (string.IsNullOrWhiteSpace(text1) && string.IsNullOrWhiteSpace(text2) && string.IsNullOrWhiteSpace(text3) && imageUri == null)
            {
                throw new InvalidOperationException("image and all three pieces of text are null");
            }

            // avoid await warning
            await Task.Delay(0);
            throw new NotImplementedException();
        }
    }
}