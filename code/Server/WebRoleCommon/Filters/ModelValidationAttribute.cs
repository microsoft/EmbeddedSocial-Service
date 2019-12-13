// <copyright file="ModelValidationAttribute.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.WebRoleCommon.Filters
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http.Controllers;
    using System.Web.Http.Filters;

    /// <summary>
    /// This filter ensures that whenever a model does not meet an attribute, an HTTP.BadRequest is returned
    /// to the client. Whenever an attribute check fails, ModelState becomes invalid. ModelState is checked below whether valid or not.
    /// However, ModelState's default value is to be valid. A corner case occurs when a controller is called with an
    /// empty request. In this case, checking just the ModelState is insufficient.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public sealed class ModelValidationAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// Action parameters that should not be null. This dictionary acts as a cache across requests.
        /// </summary>
        private static readonly ConcurrentDictionary<HttpActionDescriptor, IList<string>> NotNullActionParameters =
            new ConcurrentDictionary<HttpActionDescriptor, IList<string>>();

        /// <summary>
        /// Occurs before the action method is invoked.
        /// </summary>
        /// <param name="actionContext">The action context</param>
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            // An HTTP context can contain one or more HTTP actions. In general however, this code is invoked
            // on an incoming HTTP request. Some of these actions can have null arguments, meaning the request itself
            // is null (this would probably constitute a bug). However, on a null argument, the ModelState is valid, and
            // this code will return OK incorrectly. Check for this corner-case

            // For each action in the HTTP context (this is just one single HTTP request typically), check that the action does in fact exist.
            // To do that, we need to extract the action parameters and check they're not null.
            var notNullActionParameters = GetNotNullActionParameters(actionContext);
            foreach (var notNullActionParameter in notNullActionParameters)
            {
                // Check the parameter is not null (in other words, checks request is not null).
                object value;
                if (!actionContext.ActionArguments.TryGetValue(notNullActionParameter, out value) || value == null)
                {
                    actionContext.ModelState.AddModelError(notNullActionParameter, "Action \"" + notNullActionParameter + "\" was not specified.");
                }
            }

            // If an attribute check fails, the ModelState becomes invalid.
            if (actionContext.ModelState.IsValid == false)
            {
                actionContext.Response = actionContext.Request.CreateErrorResponse(HttpStatusCode.BadRequest, actionContext.ModelState);
            }
        }

        /// <summary>
        /// Get all action parameters that should not be null
        /// </summary>
        /// <param name="actionContext">The action context</param>
        /// <returns>Action parameters that should not be null</returns>
        private static IList<string> GetNotNullActionParameters(HttpActionContext actionContext)
        {
            var result = NotNullActionParameters.GetOrAdd(
                actionContext.ActionDescriptor,
                descriptor => descriptor.GetParameters()
                    .Where(p => !p.IsOptional && p.DefaultValue == null &&
                        !p.ParameterType.IsValueType &&
                        p.ParameterType != typeof(string))
                        .Select(p => p.ParameterName)
                        .ToList());

            return result;
        }
    }
}