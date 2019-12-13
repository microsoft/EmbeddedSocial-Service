// <copyright file="ImageSizesConfiguration.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Managers
{
    using System.Collections.Generic;

    using SocialPlus.Models;

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