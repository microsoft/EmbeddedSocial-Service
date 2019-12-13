// <copyright file="BlobTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.EndToEndTests
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Microsoft.Rest;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SocialPlus.Client;
    using SocialPlus.Client.Models;
    using SocialPlus.TestUtils;

    /// <summary>
    /// Tests posting and fetching of blobs
    /// </summary>
    [TestClass]
    public class BlobTests
    {
        /// <summary>
        /// Tests uploading and fetching a blob (an audio .mid file)
        /// </summary>
        /// <returns>a task</returns>
        [TestMethod]
        public async Task BlobTest_PostAndGet()
        {
            // Set up initial login
            SocialPlusClient client = new SocialPlusClient(TestConstants.ServerApiBaseUrl);
            string firstName = "Image";
            string lastName = "Consumer";
            string bio = "I like to download images";
            PostUserResponse postUserResponse = await TestUtilities.DoLogin(client, firstName, lastName, bio);
            string auth = AuthHelper.CreateSocialPlusAuth(postUserResponse.SessionToken);

            try
            {
                var blobUri = new Uri("https://upload.wikimedia.org/wikipedia/commons/5/51/Just_major_triad_on_C.mid");

                // fetch the blob
                using (var httpClient = new HttpClient())
                {
                    using (var responseMessage = await httpClient.GetAsync(blobUri))
                    {
                        HttpOperationResponse<PostBlobResponse> postBlobResponse;
                        long originalBlobSize = 0;
                        long newBlobSize = 0;

                        if (!responseMessage.IsSuccessStatusCode)
                        {
                            // test fails if we cannot download the blob
                            Assert.Fail("Cannot download blob");
                        }

                        using (Stream responseStream = await responseMessage.Content.ReadAsStreamAsync())
                        {
                            MemoryStream memoryStream1 = responseStream as MemoryStream;
                            originalBlobSize = memoryStream1.Length;

                            // submit the POST request
                            postBlobResponse = await client.Blobs.PostBlobWithHttpMessagesAsync(authorization: auth, blob: responseStream);
                        }

                        Assert.IsNotNull(postBlobResponse);
                        Assert.IsTrue(postBlobResponse.Response.IsSuccessStatusCode);
                        Assert.IsNotNull(postBlobResponse.Body);
                        Assert.IsFalse(string.IsNullOrWhiteSpace(postBlobResponse.Body.BlobHandle));

                        // if the post is successful, then download the blob
                        var blobHandle = postBlobResponse.Body.BlobHandle;
                        var getBlobResponse = await client.Blobs.GetBlobWithHttpMessagesAsync(authorization: auth, blobHandle: blobHandle);
                        Assert.IsNotNull(getBlobResponse);
                        Assert.IsTrue(getBlobResponse.Response.IsSuccessStatusCode);

                        using (var memoryStream2 = new MemoryStream())
                        {
                            getBlobResponse.Body.CopyTo(memoryStream2);
                            newBlobSize = memoryStream2.Length;
                        }

                        // check that size of blob downloaded from Social Plus matches the size
                        Assert.AreEqual(originalBlobSize, newBlobSize);
                    }
                }
            }
            finally
            {
                // delete the user
                HttpOperationResponse<object> deleteUserOperationResponse = await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);
            }
        }
    }
}
