// <copyright file="AddByteStreamToBlobCalls.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.SwaggerConfig
{
    using System.Collections.Generic;
    using System.Web.Http.Description;
    using Swashbuckle.Swagger;

    /// <summary>
    /// Filter that adds image or octet stream as input or output for API calls that handle images or blobs
    /// </summary>
    public class AddByteStreamToBlobCalls : IOperationFilter
    {
        /// <summary>
        /// List of image MIME types that we support.
        /// We can add other formats to this list if we verify that they work
        /// with all parts of our service, including resizing and CDN.
        /// </summary>
        private static string[] imageMimeTypes = new[] { "image/gif", "image/jpeg", "image/png" };

        /// <summary>
        /// List of blob MIME types that we support.
        /// We can add other formats to this list if we verify that they work
        /// with all parts of our service.
        /// </summary>
        private static string[] blobMimeTypes = new[] { "application/octet-stream" };

        /// <summary>
        /// Applies a change to a given Swagger API operation
        /// </summary>
        /// <param name="operation">API operation being changed</param>
        /// <param name="schemaRegistry">not used</param>
        /// <param name="apiDescription">API description that provides filter descriptions</param>
        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            // handle image gets
            if (apiDescription.FriendlyId() == "Images_GetImage")
            {
                operation.produces = imageMimeTypes;
                operation.responses["200"].schema =
                    new Schema
                    {
                        type = "file",
                        description = "MIME encoded contents of the image"
                    };
            }

            // handle image posts
            if (apiDescription.FriendlyId() == "Images_PostImage")
            {
                // add acceptable image mime types
                foreach (string imageMimeType in imageMimeTypes)
                {
                    operation.consumes.Add(imageMimeType);
                }

                Parameter image =
                    new Parameter()
                    {
                        name = "image",
                        @in = "body",
                        required = true,
                        schema = new Schema
                        {
                            type = "file",
                            format = "file"
                        },
                        description = "MIME encoded contents of the image"
                    };
                if (operation.parameters == null)
                {
                    operation.parameters = new List<Parameter>();
                }

                operation.parameters.Add(image);
            }

            // handle blob gets
            if (apiDescription.FriendlyId() == "Blobs_GetBlob")
            {
                operation.produces = blobMimeTypes;
                operation.responses["200"].schema = new Schema { type = "file", description = "MIME encoded contents of the blob" };
            }

            // handle blob posts
            if (apiDescription.FriendlyId() == "Blobs_PostBlob")
            {
                // add acceptable blob mime types
                foreach (string blobMimeType in blobMimeTypes)
                {
                    operation.consumes.Add(blobMimeType);
                }

                Parameter blob =
                    new Parameter()
                    {
                        name = "blob",
                        @in = "body",
                        required = true,
                        schema = new Schema
                        {
                            type = "file",
                            format = "file"
                        },
                        description = "MIME encoded contents of the blob"
                    };
                if (operation.parameters == null)
                {
                    operation.parameters = new List<Parameter>();
                }

                operation.parameters.Add(blob);
            }
        }
    }
}