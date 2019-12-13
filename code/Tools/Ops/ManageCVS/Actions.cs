// <copyright file="Actions.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Tools.ManageCVS
{
    using System;
    using System.Threading.Tasks;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using SocialPlus.Server.CVS;

    /// <summary>
    /// Implements manual control of CVS jobs
    /// </summary>
    public class Actions
    {
        /// <summary>
        /// Json serializer settings
        /// </summary>
        private JsonSerializerSettings jsonSettings = new JsonSerializerSettings()
        {
            Formatting = Formatting.Indented,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        /// <summary>
        /// query the results for an existing CVS job
        /// </summary>
        /// <param name="cvsUrl">cvs url</param>
        /// <param name="cvsKey">cvs key</param>
        /// <param name="jobId">id of job to query</param>
        /// <returns>query job task</returns>
        public async Task QueryJob(string cvsUrl, string cvsKey, string jobId)
        {
            // avoid await warning
            await Task.Delay(0);
            throw new NotImplementedException();
        }
    }
}
