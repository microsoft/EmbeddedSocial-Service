// <copyright file="ApiControllerExtensions.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.WebRoleCommon
{
    using System.Web.Http;

    /// <summary>
    /// Extensions to <c>ApiController</c>
    /// </summary>
    public static class ApiControllerExtensions
    {
        /// <summary>
        /// Return Unauthorized result with a message
        /// </summary>
        /// <param name="controller"><c>Api</c> controller</param>
        /// <param name="message">Message string</param>
        /// <returns>Unauthorized reason result</returns>
        public static UnauthorizedMessageResult Unauthorized(this ApiController controller, string message)
        {
            return new UnauthorizedMessageResult(message, controller.Request);
        }

        /// <summary>
        /// Return NotFound result with a message
        /// </summary>
        /// <param name="controller"><c>Api</c> controller</param>
        /// <param name="message">Message string</param>
        /// <returns>Not found result</returns>
        public static NotFoundMessageResult NotFound(this ApiController controller, string message)
        {
            return new NotFoundMessageResult(message, controller.Request);
        }

        /// <summary>
        /// Return NoContent result
        /// </summary>
        /// <param name="controller"><c>Api</c> controller</param>
        /// <returns>No content result</returns>
        public static NoContentResult NoContent(this ApiController controller)
        {
            return new NoContentResult(controller.Request);
        }

        /// <summary>
        /// Return NotImplemented result
        /// </summary>
        /// <param name="controller"><c>Api</c> controller</param>
        /// <param name="message">Message string</param>
        /// <returns>Not implemented result</returns>
        public static NotImplementedResult NotImplemented(this ApiController controller, string message)
        {
            return new NotImplementedResult(message, controller.Request);
        }

        /// <summary>
        /// Return Conflict result
        /// </summary>
        /// <param name="controller"><c>Api</c> controller</param>
        /// <param name="message">Message string</param>
        /// <returns>Conflict message result</returns>
        public static ConflictMessageResult Conflict(this ApiController controller, string message)
        {
            return new ConflictMessageResult(message, controller.Request);
        }

        /// <summary>
        /// Return Forbidden result
        /// </summary>
        /// <param name="controller"><c>Api</c> controller</param>
        /// <param name="message">Message string</param>
        /// <returns>Forbidden message result</returns>
        public static ForbiddenMessageResult Forbidden(this ApiController controller, string message)
        {
            return new ForbiddenMessageResult(message, controller.Request);
        }
    }
}