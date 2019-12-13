// <copyright file="ImageTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.EndToEndTests
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Drawing;
    using System.IO;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Microsoft.Rest;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SocialPlus.Client;
    using SocialPlus.Client.Models;
    using SocialPlus.TestUtils;
    using SocialPlus.Utils;

    /// <summary>
    /// Tests the posting and resizing of different types of images
    /// </summary>
    [TestClass]
    public class ImageTests
    {
        /// <summary>
        /// posts the given image to Social Plus
        /// </summary>
        /// <param name="imageUri">URL to an image</param>
        /// <param name="imageType">user profile photo or topic content image</param>
        /// <param name="client">social plus client library</param>
        /// <param name="bearerToken">bearer token from logged on user</param>
        /// <returns>task that returns the response from server</returns>
        public static async Task<HttpOperationResponse<PostImageResponse>> AddImage(Uri imageUri, ImageType imageType, SocialPlusClient client, string bearerToken)
        {
            // get the image from the internets
            if (imageUri.Scheme == Uri.UriSchemeHttp || imageUri.Scheme == Uri.UriSchemeHttps)
            {
                using (var httpClient = new HttpClient())
                {
                    using (var responseMessage = await httpClient.GetAsync(imageUri))
                    {
                        if (!responseMessage.IsSuccessStatusCode)
                        {
                            return null;
                        }

                        using (Stream responseStream = await responseMessage.Content.ReadAsStreamAsync())
                        {
                            // submit the POST request
                            HttpOperationResponse<PostImageResponse> postImageResponse = await client.Images.PostImageWithHttpMessagesAsync(imageType: imageType, authorization: bearerToken, image: responseStream);
                            return postImageResponse;
                        }
                    }
                }
            }

            // get the image from the local disk
            else if (imageUri.Scheme == Uri.UriSchemeFile)
            {
                Image image = Image.FromFile(imageUri.LocalPath);
                using (MemoryStream imageMemoryStream = new MemoryStream())
                {
                    image.Save(imageMemoryStream, System.Drawing.Imaging.ImageFormat.Jpeg);
                    imageMemoryStream.Position = 0;

                    // submit the POST request
                    HttpOperationResponse<PostImageResponse> postImageResponse = await client.Images.PostImageWithHttpMessagesAsync(imageType: imageType, authorization: bearerToken, image: imageMemoryStream);
                    return postImageResponse;
                }
            }

            // not supported
            else
            {
                throw new NotImplementedException("only http, https, and file URIs are supported");
            }
        }

        /// <summary>
        /// Tests resizing of topic image
        /// </summary>
        /// <returns>a task that executes this test</returns>
        [TestMethod]
        public async Task TopicImage()
        {
            await TestImage(new Uri("https://upload.wikimedia.org/wikipedia/commons/thumb/a/a0/Bryan_Cranston_by_Gage_Skidmore_2.jpg/1024px-Bryan_Cranston_by_Gage_Skidmore_2.jpg"), ImageType.ContentBlob);
        }

        /// <summary>
        /// Tests resizing of user profile photo
        /// </summary>
        /// <returns>a task that executes this test</returns>
        [TestMethod]
        public async Task UserImage()
        {
            await TestImage(new Uri("https://upload.wikimedia.org/wikipedia/commons/thumb/a/a0/Bryan_Cranston_by_Gage_Skidmore_2.jpg/1024px-Bryan_Cranston_by_Gage_Skidmore_2.jpg"), ImageType.UserPhoto);
        }

        /// <summary>
        /// Tests resizing of app icon
        /// </summary>
        /// <returns>a task that executes this test</returns>
        [TestMethod]
        public async Task AppIcon()
        {
            await TestImage(new Uri("https://upload.wikimedia.org/wikipedia/commons/thumb/a/a0/Bryan_Cranston_by_Gage_Skidmore_2.jpg/1024px-Bryan_Cranston_by_Gage_Skidmore_2.jpg"), ImageType.AppIcon);
        }

        /// <summary>
        /// Tests resizing a tiny image on the Internet that is less than 2KB big
        /// </summary>
        /// <returns>a task that executes this test</returns>
        [TestMethod]
        public async Task TinyImage()
        {
            await TestImage(new Uri("https://www.wikipedia.org/portal/wikipedia.org/assets/img/Wikipedia_wordmark@1.5x.png"), ImageType.ContentBlob);
        }

        /// <summary>
        /// Tests posting a local image file that is under 50KB
        /// </summary>
        /// <returns>a task that executes this test</returns>
        [TestMethod]
        [DeploymentItem("Resources\\veronica-lake.jpg", "Resources")]
        public async Task LocalSmallImage()
        {
            string localImagePath = Path.GetFullPath("Resources\\veronica-lake.jpg");
            Assert.IsTrue(File.Exists(localImagePath));
            await TestImage(new Uri(localImagePath), ImageType.ContentBlob);
        }

        /// <summary>
        /// Tests posting a local image file that is over 200KB
        /// </summary>
        /// <returns>a task that executes this test</returns>
        [TestMethod]
        [DeploymentItem("Resources\\1024px-Bryan_Cranston_by_Gage_Skidmore_2.jpg", "Resources")]
        public async Task LocalMediumImage()
        {
            string localImagePath = Path.GetFullPath("Resources\\1024px-Bryan_Cranston_by_Gage_Skidmore_2.jpg");
            Assert.IsTrue(File.Exists(localImagePath));
            await TestImage(new Uri(localImagePath), ImageType.ContentBlob);
        }

        /// <summary>
        /// Tests whether the service allows the very first request in a new connection
        /// to have a large request body (>200KB image). A customer reported
        /// a HTTP 413 error in this situation.
        /// </summary>
        /// <returns>a task that executes this test</returns>
        [TestMethod]
        public async Task LargePostOnFirstRequest()
        {
            // Set up initial login with one client
            SocialPlusClient client1 = new SocialPlusClient(TestConstants.ServerApiBaseUrl);
            string firstName = "Image";
            string lastName = "Consumer";
            string bio = "I like to download images";
            PostUserResponse postUserResponse = await TestUtilities.DoLogin(client1, firstName, lastName, bio);
            string auth = AuthHelper.CreateSocialPlusAuth(postUserResponse.SessionToken);

            // add image to server using another client
            Uri imageUri = new Uri("https://upload.wikimedia.org/wikipedia/commons/thumb/a/a0/Bryan_Cranston_by_Gage_Skidmore_2.jpg/1024px-Bryan_Cranston_by_Gage_Skidmore_2.jpg");
            SocialPlusClient client2 = new SocialPlusClient(TestConstants.ServerApiBaseUrl);
            HttpOperationResponse<PostImageResponse> postImageResponse = await AddImage(imageUri, ImageType.ContentBlob, client2, auth);

            // there is no delete image API call, so cannot cleanup the image from the server

            // delete the user
            HttpOperationResponse<object> deleteUserOperationResponse = await client1.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);

            // test responses
            Assert.IsNotNull(postImageResponse);
            Assert.IsTrue(postImageResponse.Response.IsSuccessStatusCode);
            Assert.IsTrue(deleteUserOperationResponse.Response.IsSuccessStatusCode);
        }

    /// <summary>
    /// Returns a URL to access an image on the CDN
    /// </summary>
    /// <param name="blobHandle">unique handle to the image</param>
    /// <returns>URL to get the image</returns>
    private static Uri FormImageUriOnCDN(string blobHandle)
        {
            var sr = new FileSettingsReader(ConfigurationManager.AppSettings["ConfigRelativePath"] + Path.DirectorySeparatorChar + TestConstants.ConfigFileName);
            var cdnUrl = sr.ReadValue("CDNUrl");
            return new Uri(cdnUrl + "images/" + blobHandle);
        }

        /// <summary>
        /// Given a URL, checks if it exists and is an image
        /// </summary>
        /// <param name="imageUri">URL to an image</param>
        /// <returns>true if it exists and is an image</returns>
        private static async Task<bool> DoesImageExist(Uri imageUri)
        {
            // get the image
            using (var client = new HttpClient())
            {
                using (var responseMessage = await client.GetAsync(imageUri))
                {
                    if (!responseMessage.IsSuccessStatusCode)
                    {
                        return false;
                    }

                    using (Stream responseStream = await responseMessage.Content.ReadAsStreamAsync())
                    {
                        var contentType = responseMessage.Content.Headers.ContentType.ToString();
                        if (!contentType.StartsWith("image", true, System.Globalization.CultureInfo.InvariantCulture))
                        {
                            return false;
                        }

                        return true;
                    }
                }
            }
        }

        /// <summary>
        /// Fetches an image and returns the size
        /// </summary>
        /// <param name="imageUri">image to fetch</param>
        /// <returns>task that returns the size of the image</returns>
        private static async Task<Size> GetSizeOfImage(Uri imageUri)
        {
            // get the image from the internets
            if (imageUri.Scheme == Uri.UriSchemeHttp || imageUri.Scheme == Uri.UriSchemeHttps)
            {
                using (var client = new HttpClient())
                {
                    using (var responseMessage = await client.GetAsync(imageUri))
                    {
                        responseMessage.EnsureSuccessStatusCode();
                        using (Stream responseStream = await responseMessage.Content.ReadAsStreamAsync())
                        {
                            var contentType = responseMessage.Content.Headers.ContentType.ToString();
                            Assert.IsTrue(contentType.StartsWith("image", true, System.Globalization.CultureInfo.InvariantCulture));
                            return GetSizeOfImage(responseStream);
                        }
                    }
                }
            }

            // get the image from the local disk
            else if (imageUri.Scheme == Uri.UriSchemeFile)
            {
                using (MemoryStream imageMemoryStream = new MemoryStream())
                using (FileStream imageFileStream = new FileStream(imageUri.LocalPath, FileMode.Open, FileAccess.Read))
                {
                    await imageFileStream.CopyToAsync(imageMemoryStream);
                    return GetSizeOfImage(imageMemoryStream);
                }
            }

            // not supported
            else
            {
                throw new NotImplementedException("only http, https, and file URIs are supported");
            }
        }

        /// <summary>
        /// Given an image stream, will return the size of the image
        /// </summary>
        /// <param name="image">Stream that represents the image</param>
        /// <returns>size of the image</returns>
        private static Size GetSizeOfImage(Stream image)
        {
            // convert to image
            Image receivedImage = Image.FromStream(image);

            // return the size
            return receivedImage.Size;
        }

        /// <summary>
        /// calculates the expected size of an image
        /// </summary>
        /// <param name="newWidth">target width</param>
        /// <param name="originalSize">original image size</param>
        /// <returns>expected image size</returns>
        private static Size ExpectedImageSize(int newWidth, Size originalSize)
        {
            // if original image is too small, just use that
            if (originalSize.Width <= newWidth)
            {
                return originalSize;
            }

            // maintain aspect ratio
            Size newSize = default(Size);
            newSize.Width = newWidth;
            double newHeight = (newWidth * originalSize.Height) / originalSize.Width;
            newSize.Height = Convert.ToInt32(Math.Round(newHeight));

            return newSize;
        }

        /// <summary>
        /// Tests the add of an image and resizing of an image
        /// </summary>
        /// <param name="imageUri">URL to the image to test</param>
        /// <param name="imageType">type of image</param>
        /// <returns>task of testing the image</returns>
        private static async Task TestImage(Uri imageUri, ImageType imageType)
        {
            // Set up initial login
            SocialPlusClient client = new SocialPlusClient(TestConstants.ServerApiBaseUrl);
            string firstName = "Image";
            string lastName = "Consumer";
            string bio = "I like to download images";
            PostUserResponse postUserResponse = await TestUtilities.DoLogin(client, firstName, lastName, bio);
            string auth = AuthHelper.CreateSocialPlusAuth(postUserResponse.SessionToken);

            // get size of original image
            Size originalSize = await GetSizeOfImage(imageUri);

            // add image to server
            HttpOperationResponse<PostImageResponse> postImageResponse = await AddImage(imageUri, imageType, client, auth);

            // if add image was a success, grab the handle, the image, and delete the image and user to cleanup
            string imageHandle = string.Empty;
            HttpOperationResponse<Stream> getImageResponse = null;
            Size getImageResponseSize = default(Size);
            bool cdnImageExists = false;
            bool cdnSmallerImagesExist = false;
            bool cdnSmallerImageSizesCorrect = false;
            if (postImageResponse != null && postImageResponse.Response.IsSuccessStatusCode)
            {
                // get the image handle
                imageHandle = postImageResponse.Body.BlobHandle;

                // get the image from the social plus server and pull out its size
                getImageResponse = await client.Images.GetImageWithHttpMessagesAsync(blobHandle: imageHandle, authorization: auth);
                if (getImageResponse.Response.IsSuccessStatusCode)
                {
                    getImageResponseSize = GetSizeOfImage(getImageResponse.Body);
                }

                // get the image from the CDN
                Uri cdnImageUri = FormImageUriOnCDN(imageHandle);
                cdnImageExists = await DoesImageExist(cdnImageUri);

                // wait for image resizer worker role to finish
                System.Threading.Thread.Sleep(TestConstants.ImageResizeDelay);

                // do smaller images exist on CDN?
                cdnSmallerImagesExist = true;
                cdnSmallerImageSizesCorrect = true;
                foreach (ImageSize size in ImageSizesConfiguration.Sizes[imageType])
                {
                    Uri smallImageUri = FormImageUriOnCDN(imageHandle + size.Id);
                    bool exists = await DoesImageExist(smallImageUri);
                    if (!exists)
                    {
                        cdnSmallerImagesExist = false;
                    }

                    // is it the right size?
                    Size expectedSize = ExpectedImageSize(size.Width, originalSize);
                    Size newSize = await GetSizeOfImage(smallImageUri);
                    if (expectedSize != newSize)
                    {
                        cdnSmallerImageSizesCorrect = false;
                    }
                }

                // there is no delete image API call, so cannot cleanup the image from the server
            }

            // delete the user
            HttpOperationResponse<object> deleteUserOperationResponse = await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);

            // test responses
            Assert.IsNotNull(postImageResponse);
            Assert.IsTrue(postImageResponse.Response.IsSuccessStatusCode);
            Assert.IsTrue(getImageResponse.Response.IsSuccessStatusCode);
            Assert.AreEqual(originalSize, getImageResponseSize);
            Assert.IsTrue(cdnImageExists);
            Assert.IsTrue(cdnSmallerImagesExist);
            Assert.IsTrue(cdnSmallerImageSizesCorrect);
            Assert.IsTrue(deleteUserOperationResponse.Response.IsSuccessStatusCode);
        }

        /// <summary>
        /// defines a particular image size
        /// </summary>
        public struct ImageSize
        {
            /// <summary>
            /// One character symbol for this image size
            /// </summary>
            public char Id;

            /// <summary>
            /// Width of the image in pixels
            /// </summary>
            public int Width;
        }

        /// <summary>
        /// Lists the different image sizes for each type of image
        /// </summary>
        public static class ImageSizesConfiguration
        {
            /// <summary>
            /// Tiny image size
            /// </summary>
            public static readonly ImageSize Tiny = new ImageSize() { Id = 'd', Width = 25 };

            /// <summary>
            /// Small image size
            /// </summary>
            public static readonly ImageSize Small = new ImageSize() { Id = 'h', Width = 50 };

            /// <summary>
            /// Medium image size
            /// </summary>
            public static readonly ImageSize Medium = new ImageSize() { Id = 'l', Width = 100 };

            /// <summary>
            /// Large image size
            /// </summary>
            public static readonly ImageSize Large = new ImageSize() { Id = 'p', Width = 250 };

            /// <summary>
            /// Huge image size
            /// </summary>
            public static readonly ImageSize Huge = new ImageSize() { Id = 't', Width = 500 };

            /// <summary>
            /// Gigantic image size
            /// </summary>
            public static readonly ImageSize Gigantic = new ImageSize() { Id = 'x', Width = 1000 };

            /// <summary>
            /// Image sizes for different types of images
            /// </summary>
            public static readonly Dictionary<ImageType, List<ImageSize>> Sizes = new Dictionary<ImageType, List<ImageSize>>()
            {
                { ImageType.UserPhoto, new List<ImageSize>() { Tiny, Small, Medium, Large, Huge, Gigantic } },
                { ImageType.ContentBlob, new List<ImageSize>() { Tiny, Small, Medium, Large, Huge, Gigantic } },
                { ImageType.AppIcon, new List<ImageSize>() { Medium } }
            };
        }
    }
}
