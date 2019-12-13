// <copyright file="BingWallpaperFetcher.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.WebRoleCommon.App_Start
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Newtonsoft.Json;
    using SocialPlus.Utils;

    /// <summary>
    /// Class that implements fetching the Bing Wall paper
    /// </summary>
    public class BingWallpaperFetcher
    {
        /// <summary>
        /// Url of where to fetch a little big of JSON from Bing that tells us where the wallpaper can be downloaded from
        /// </summary>
        private readonly Uri bingJSONUrl = new Uri(@"http://www.bing.com/HPImageArchive.aspx?format=js&idx=0&n=1&mkt=en-US");

        /// <summary>
        /// Relative file name to the wallpaper image to be loaded by the service
        /// </summary>
        private readonly string wallpaperRelativeFileName = @"~/Resources/wallpaper.jpg";

        /// <summary>
        /// Relative file name to a default wallpaper
        /// </summary>
        private readonly string defaultWallpaperRelativeFileName = @"~/Resources/default_wallpaper_1920x1080.jpg";

        /// <summary>
        /// Implements fetch the wallpaper once
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task FetchOneTime()
        {
            string localFile = System.Web.Hosting.HostingEnvironment.MapPath(this.wallpaperRelativeFileName);

            try
            {
                // Construct a request and go get the JSON snippet from Bing
                using (HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("GET"), this.bingJSONUrl))
                using (HttpClient client = new HttpClient())
                using (HttpResponseMessage response = await client.SendAsync(request))
                {
                    if (response != null && response.IsSuccessStatusCode)
                    {
                        // Url of wallpaper
                        string wallpaperUrl = @"http://bing.com" + this.GetImageUrl(await response.Content.ReadAsStringAsync().ConfigureAwait(false));

                        // Download the wallpaper to local file
                        await NetHttpHelper.DownloadFile(client, wallpaperUrl, localFile);

                        // Done.
                        return;
                    }
                }
            }
            catch
            {
                // When exception is fired, just fall through and copy the default background
            }

            string defaultFile = System.Web.Hosting.HostingEnvironment.MapPath(this.defaultWallpaperRelativeFileName);
            File.Copy(defaultFile, localFile);
        }

        /// <summary>
        /// Extracts the image url by looking for the property name url a JSON fragment
        /// </summary>
        /// <param name="json">input json fragment</param>
        /// <returns>the url as a string</returns>
        private string GetImageUrl(string json)
        {
            string propertyName = "url";

            using (var stringReader = new StringReader(json))
            using (var jsonReader = new JsonTextReader(stringReader))
            {
                while (jsonReader.Read())
                {
                    if (jsonReader.TokenType == JsonToken.PropertyName
                        && (string)jsonReader.Value == propertyName)
                    {
                        jsonReader.Read();

                        var serializer = new JsonSerializer();
                        return serializer.Deserialize<string>(jsonReader);
                    }
                }

                return null;
            }
        }
    }
}