// <copyright file="AddAuthToHeader.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.SwaggerConfig
{
    using System.Collections.Generic;
    using System.Web.Http.Description;

    using Swashbuckle.Swagger;

    /// <summary>
    /// Filter that adds the Authorization field to API headers in Swagger. The format of the Authorization field is "Scheme CredentialsList".
    /// The format must be:
    /// - SocialPlus TK=SessionToken
    /// - Anon AK=AppKey
    /// - AADS2S AK=AppKey|[UH=UserHandle]|TK=AADToken
    /// - Facebook AK=AppKey|TK=AccessToken
    /// - Microsoft AK=AppKey|TK=AccessToken
    /// - Google AK=AppKey|TK=AccessToken
    /// - Twitter AK=AppKey,RT=RequestToken,TK=AccessToken
    /// </summary>
    public class AddAuthToHeader : IOperationFilter
    {
        /// <summary>
        /// Applies a change to a given Swagger API operation. This change is to add the Authorization field to an API call
        /// </summary>
        /// <param name="operation">API operation being changed</param>
        /// <param name="schemaRegistry">not used</param>
        /// <param name="apiDescription">API description that provides filter descriptions</param>
        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            // Some API calls do not need an Authorization field. The actions corresponding to these API calls
            // are labeled with an OverrideAuthenticationAttribute. In such a case, we should return
            if (!AuthFilterHelpers.IsAuthFilterApplied(apiDescription))
            {
                return;
            }

            // define the bearer auth header parameter
            Parameter authHeader = new Parameter();
            authHeader.type = "string";
            authHeader.@in = "header";
            authHeader.name = "Authorization";

            // fill in the description.
            authHeader.description = "Format is: \"Scheme CredentialsList\". Possible values are:";
            authHeader.description += "\n\n";
            authHeader.description += "- Anon AK=AppKey";
            authHeader.description += "\n\n";
            authHeader.description += "- SocialPlus TK=SessionToken";
            authHeader.description += "\n\n";
            authHeader.description += "- Facebook AK=AppKey|TK=AccessToken";
            authHeader.description += "\n\n";
            authHeader.description += "- Google AK=AppKey|TK=AccessToken";
            authHeader.description += "\n\n";
            authHeader.description += "- Twitter AK=AppKey|RT=RequestToken|TK=AccessToken";
            authHeader.description += "\n\n";
            authHeader.description += "- Microsoft AK=AppKey|TK=AccessToken";
            authHeader.description += "\n\n";
            authHeader.description += "- AADS2S AK=AppKey|[UH=UserHandle]|TK=AADToken";

            authHeader.required = true;

            // insert the new parameters in the operation's parameters
            if (operation.parameters == null)
            {
                operation.parameters = new List<Parameter>();
            }

            // add the auth parameter
            operation.parameters.Add(authHeader);
        }
    }
}