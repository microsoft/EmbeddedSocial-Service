// <copyright file="IHandleGenerator.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Utils
{
    /// <summary>
    /// Interface for generating handles for SocialPlus. A handle must have two properties:
    /// 1. Uniqueness: no two handles must collide.
    /// 2. Reverse chronologically and lexigraphically ordered:
    ///     if handle h2 is generated later in time than handle h1, h1 > h2 in lexicographical order
    /// </summary>
    public interface IHandleGenerator
    {
        /// <summary>
        /// Generates short handle
        /// </summary>
        /// <returns>Time ordered handle</returns>
        string GenerateShortHandle();

        /// <summary>
        /// Generates large time handle
        /// </summary>
        /// <returns>Large time ordered handle</returns>
        string GenerateLongHandle();
    }
}
