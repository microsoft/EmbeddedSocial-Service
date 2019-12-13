// <copyright file="AuthFilterHelpers.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.SwaggerConfig
{
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Web.Http;
    using System.Web.Http.Description;
    using System.Web.Http.Filters;

    using SocialPlus.Server.Filters;

    /// <summary>
    /// Helper code for handling auth filters in Swagger.
    /// </summary>
    public static class AuthFilterHelpers
    {
        /// <summary>
        /// Processes a list of filters and removes auth filters applied prior to OverrideAuthenticationAttribute.
        /// Some actions inside of the controllers are labelled with [OverrideAuthentication]. This class ensures that
        /// Swagger does not show any authentication filters defined prior to this attribute definition. In particular,
        /// it will not show the Authorization field for the API calls corresponding to these actions.
        /// </summary>
        /// <remarks>
        /// When OverrideAuthenticationAttribute is applied to a controller, all prior authentication filters must be skipped.
        /// However, subsequent authentication filters can still be applied to the controller.
        /// This method will accordingly remove overridden authentication filters from a collection of FilterInfo.
        /// This method is used for Swagger only.
        /// </remarks>
        /// <param name="filterList">list of filters for an operation</param>
        /// <returns>cleaned up list of filters</returns>
        public static Collection<FilterInfo> RemoveAuthenticationFilters(Collection<FilterInfo> filterList)
        {
            // skip empty lists
            if (filterList == null || filterList.Count < 1)
            {
                return filterList;
            }

            // does the OverrideAuthenticationAttribute exist in this list?
            bool overrideFilterExists = false;
            int indexOfOverrideFilter = -1;
            for (int i = 0; i < filterList.Count; i++)
            {
                if (filterList[i].Instance is OverrideAuthenticationAttribute)
                {
                    overrideFilterExists = true;
                    indexOfOverrideFilter = i;

                    // cannot skip the loop in case the attribute has been applied multiple times
                    // so need to find the last one
                }
            }

            // If no overrirde filter found, or the override filter is the very first one (no prior filters found)
            if (!overrideFilterExists || indexOfOverrideFilter == 0)
            {
                return filterList;
            }

            // remove all prior authentication filters
            for (int i = 0; i < indexOfOverrideFilter; i++)
            {
                if (filterList[i].Instance is IAuthenticationFilter)
                {
                    filterList.RemoveAt(i);

                    // go back one step since the collection is now shorter
                    i--;
                    indexOfOverrideFilter--;
                }
            }

            return filterList;
        }

        /// <summary>
        /// This method returns true if auth filters are applied to this API call. False, otherwise.
        /// </summary>
        /// <param name="apiDescription">Swagger API description</param>
        /// <returns>whether auth filters are applied to this API call</returns>
        public static bool IsAuthFilterApplied(ApiDescription apiDescription)
        {
            var filterPipeline = apiDescription.ActionDescriptor.GetFilterPipeline();
            filterPipeline = AuthFilterHelpers.RemoveAuthenticationFilters(filterPipeline);
            return filterPipeline.Select(filterInfo => filterInfo.Instance).Any(filter => filter is AuthenticationFilter);
        }
    }
}
