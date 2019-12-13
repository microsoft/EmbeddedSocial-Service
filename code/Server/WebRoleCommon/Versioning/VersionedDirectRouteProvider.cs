//-----------------------------------------------------------------------
// <copyright file="VersionedDirectRouteProvider.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <summary>
// Implements a <c>DirectRouteProvider</c> that supports specifying the supported version numbers for a controller
// class by using a <c>VersionRange</c> attribute.
// </summary>
//-----------------------------------------------------------------------

namespace SocialPlus.Server.WebRoleCommon.Versioning
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Web.Http.Controllers;
    using System.Web.Http.Routing;

    /// <summary>
    /// Specialized version of the <c>DefaultDirectRouteProvider</c> that supports an attribute which specifies
    /// which versions are implemented by each controller class.
    /// This class should be derived inside of a WebRole and named VersionedDirectRouteProvider
    /// </summary>
    public class VersionedDirectRouteProvider : DefaultDirectRouteProvider
    {
        private readonly IServiceVersionInfo serviceVersionInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionedDirectRouteProvider"/> class.
        /// </summary>
        /// <param name="serviceVersionInfo">service version</param>
        public VersionedDirectRouteProvider(IServiceVersionInfo serviceVersionInfo)
        {
            this.serviceVersionInfo = serviceVersionInfo;
        }

        /// <summary>
        /// Override of <c>GetActionDirectRoutes</c> which extracts the version range and creates route entries
        /// for each version.
        /// </summary>
        /// <param name="actionDescriptor">action descriptor</param>
        /// <param name="factories">a list of factories</param>
        /// <param name="constraintResolver">constraint resolver</param>
        /// <returns>a list of route entries</returns>
        protected override IReadOnlyList<RouteEntry> GetActionDirectRoutes(
            HttpActionDescriptor actionDescriptor,
            IReadOnlyList<IDirectRouteFactory> factories,
            IInlineConstraintResolver constraintResolver)
        {
            List<RouteEntry> entries = new List<RouteEntry>();

            // extract the version range associate with the controller
            List<string> validVersions = this.GetVersionListForController(actionDescriptor);
            if (validVersions != null)
            {
                foreach (string vers in validVersions)
                {
                    string prefix = vers + "/" + this.GetRoutePrefix(actionDescriptor.ControllerDescriptor);

                    var actions = new HttpActionDescriptor[] { actionDescriptor };

                    foreach (IDirectRouteFactory factory in factories)
                    {
                        RouteEntry entry = CreateActionRouteEntry(prefix, factory, actions, constraintResolver);
                        entries.Add(entry);
                    }
                }
            }
            else
            {
                string prefix = this.GetRoutePrefix(actionDescriptor.ControllerDescriptor);

                var actions = new HttpActionDescriptor[] { actionDescriptor };

                foreach (IDirectRouteFactory factory in factories)
                {
                    RouteEntry entry = CreateActionRouteEntry(prefix, factory, actions, constraintResolver);
                    entries.Add(entry);
                }
            }

            return entries;
        }

        /// <summary>
        /// Uses the factory to create a route entry for the specific actions
        /// </summary>
        /// <param name="prefix">the route prefix</param>
        /// <param name="factory">the factory used to create a route entry</param>
        /// <param name="actions">the actions</param>
        /// <param name="constraintResolver">constraint resolver</param>
        /// <returns>the route entry</returns>
        private static RouteEntry CreateActionRouteEntry(
            string prefix,
            IDirectRouteFactory factory,
            IReadOnlyCollection<HttpActionDescriptor> actions,
            IInlineConstraintResolver constraintResolver)
        {
            Contract.Assert(factory != null);

            DirectRouteFactoryContext context = new DirectRouteFactoryContext(prefix, actions, constraintResolver, true);
            RouteEntry entry = factory.CreateRoute(context);
            if (entry == null)
            {
                throw new InvalidOperationException(string.Format(
                    "Method {0}.{1} must not return null.",
                    typeof(IDirectRouteFactory).Name,
                    "CreateRoute"));
            }

            ValidateRouteEntry(entry);
            return entry;
        }

        /// <summary>
        /// validates the route entry
        /// </summary>
        /// <param name="entry">route entry</param>
        private static void ValidateRouteEntry(RouteEntry entry)
        {
            Contract.Assert(entry != null);
            IHttpRoute route = entry.Route;
            Contract.Assert(route != null);

            // implement validation on the target actions
            HttpActionDescriptor[] targetActions = null;

            if (route.DataTokens != null)
            {
                object result;
                route.DataTokens.TryGetValue("actions", out result);
                targetActions = (HttpActionDescriptor[])result;
            }

            if (targetActions == null || targetActions.Length == 0)
            {
                throw new InvalidOperationException("The route does not have any associated action descriptors. Routing requires that each direct route map to a non-empty set of actions.");
            }

            if (route.Handler != null)
            {
                throw new InvalidOperationException("Direct routing does not support per-route message handlers.");
            }
        }

        /// <summary>
        /// For each controller action, this method gets the list of supported versions
        /// </summary>
        /// <param name="action">the action descriptor</param>
        /// <returns>a list of strings, one for each supported version</returns>
        private List<string> GetVersionListForController(HttpActionDescriptor action)
        {
            HttpControllerDescriptor desc = action.ControllerDescriptor;
            Type controllerType = desc.ControllerType;
            BaseVersionRangeAttribute vr = null;

            Contract.Assert(controllerType.IsClass);

            var method = controllerType.GetMethod(action.ActionName);
            var attributes = Attribute.GetCustomAttributes(method);
            foreach (var a in attributes)
            {
                if (a is BaseVersionRangeAttribute)
                {
                    if (vr == null)
                    {
                        vr = (BaseVersionRangeAttribute)a;
                    }
                    else
                    {
                        throw new Exception("Multiple Version Ranges specified on a method");
                    }
                }
            }

            if (vr != null)
            {
                var versions = vr.EnumerateVersions(this.serviceVersionInfo);
                return versions;
            }
            else
            {
                return null;
            }
        }
    }
}