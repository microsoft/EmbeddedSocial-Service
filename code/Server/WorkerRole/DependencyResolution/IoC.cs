// <copyright file="IoC.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.DependencyResolution
{
    using StructureMap;

    /// <summary>
    /// Inversion of control for dependency resolution
    /// </summary>
    public static class IoC
    {
        /// <summary>
        /// Initialize method
        /// </summary>
        /// <returns>Container interface</returns>
        public static IContainer Initialize()
        {
            return new Container(c => c.AddRegistry<DefaultRegistry>());
        }
    }
}
