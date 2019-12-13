// <copyright file="NetHttpHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Utils
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Net.Http;
    using System.Threading.Tasks;

    /// <summary>
    /// Utilities for System.Net.Http
    /// </summary>
    public static class NetHttpHelper
    {
        /// <summary>
        /// Downloads a file from a URL
        /// </summary>
        /// <param name="client">http client</param>
        /// <param name="whereFrom">URL where file can be found</param>
        /// <param name="whereTo">string corresponding to location</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task DownloadFile(HttpClient client, string whereFrom, string whereTo)
        {
            if (client == null || whereFrom == null || whereTo == null)
            {
                new ArgumentNullException(string.Format("Input parameters are null. HttpClient: {0}, whereFrom: {1}, whereTo: {2}", client, whereFrom, whereTo));
            }

            // Send request
            HttpResponseMessage response = await client.GetAsync(whereFrom);

            // Check that response was successful or throw exception
            response.EnsureSuccessStatusCode();

            // Read content into buffer
            await response.Content.LoadIntoBufferAsync();

            // The content can now be read multiple times using any ReadAs* extension method
            await ReadAsFileAsync(response.Content, whereTo, true);
        }

        /// <summary>
        /// Provides support for reading content and storing it directly to a local file
        /// </summary>
        /// <param name="content">Http content</param>
        /// <param name="filename">local file</param>
        /// <param name="overwrite">flag whether to overwrite the file</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static Task ReadAsFileAsync(HttpContent content, string filename, bool overwrite)
        {
            string pathname = Path.GetFullPath(filename);
            if (!overwrite && File.Exists(filename))
            {
                throw new InvalidOperationException(string.Format("File {0} already exists.", pathname));
            }

            FileStream fileStream = null;
            try
            {
                fileStream = new FileStream(pathname, FileMode.Create, FileAccess.Write, FileShare.None);
                return content.CopyToAsync(fileStream).ContinueWith(
                    (copyTask) =>
                    {
                        fileStream.Close();
                    });
            }
            catch
            {
                if (fileStream != null)
                {
                    fileStream.Close();
                }

                throw;
            }
        }
    }
}
