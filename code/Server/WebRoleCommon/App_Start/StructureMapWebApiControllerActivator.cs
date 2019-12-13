// <copyright file="StructureMapWebApiControllerActivator.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.WebRoleCommon.App_Start
{
    using System;
    using System.Net.Http;
    using System.Web.Http.Controllers;
    using System.Web.Http.Dispatcher;

    using StructureMap;

    /// <summary>
    /// The StructureMapWebApiControllerActivator class provides a simpler way to use dependency injection (DI)
    /// with ASP.NET web api.  Instead of using the IDependencyResolver interface provided by web api,
    /// we use the IHttpControllerActivator extensibility mechanism to perform DI.
    /// </summary>
    public class StructureMapWebApiControllerActivator : IHttpControllerActivator
    {
        private readonly IContainer container;

        /// <summary>
        /// Initializes a new instance of the <see cref="StructureMapWebApiControllerActivator"/> class.
        /// </summary>
        /// <param name="container">The IoC container</param>
        public StructureMapWebApiControllerActivator(IContainer container)
        {
            this.container = container;
        }

        /// <summary>
        /// Creates an instance of the controller type using the IoC container.
        /// </summary>
        /// <param name="request">incoming http request</param>
        /// <param name="controllerDescriptor">descriptor for the controller to be invoked</param>
        /// <param name="controllerType">type of the controller to be invoked</param>
        /// <returns>An instance of the controller type</returns>
        public IHttpController Create(
            HttpRequestMessage request,
            HttpControllerDescriptor controllerDescriptor,
            Type controllerType)
        {
            var nested = this.container.GetNestedContainer();
            var instance = nested.GetInstance(controllerType) as IHttpController;
            request.RegisterForDispose(nested);
            return instance;
        }
    }
}