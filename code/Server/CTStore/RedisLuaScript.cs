//-----------------------------------------------------------------------
// <copyright file="RedisLuaScript.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <summary>
// Implements class RedisLuaScript.
// </summary>
//-----------------------------------------------------------------------

namespace Microsoft.CTStore
{
    using System.Collections.Generic;
    using StackExchange.Redis;

    /// <summary>
    /// <c>Redis Lua Script</c> class
    /// </summary>
    public class RedisLuaScript
    {
        /// <summary>
        /// Gets or sets the script
        /// </summary>
        public string Script { get; set; }

        /// <summary>
        /// Gets or sets list of conditions
        /// </summary>
        public List<string> Conditions { get; set; }

        /// <summary>
        /// Gets or sets list of actions
        /// </summary>
        public List<string> Actions { get; set; }

        /// <summary>
        /// Gets or sets <c>Redis</c> keys for the script
        /// </summary>
        public List<RedisKey> Keys { get; set; }

        /// <summary>
        /// Gets or sets script arguments
        /// </summary>
        public List<RedisValue> Values { get; set; }

        /// <summary>
        /// Gets or sets ETags returned on success
        /// </summary>
        public List<string> ETags { get; set; }

        /// <summary>
        /// Gets or sets mapping between error code and exceptions returned
        /// </summary>
        public Dictionary<int, List<OperationFailedException>> Exceptions { get; set; }
    }
}
