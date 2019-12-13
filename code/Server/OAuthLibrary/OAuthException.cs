// <copyright file="OAuthException.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.OAuth
{
    using System;
    using System.Reflection;
    using System.Resources;

    /// <summary>
    /// Implement possible exceptions thrown by our <c>auth</c> library
    /// </summary>
    public class OAuthException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OAuthException"/> class
        /// </summary>
        /// <param name="message">error string</param>
        public OAuthException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OAuthException"/> class
        /// </summary>
        /// <param name="message">error string</param>
        /// <param name="innerEx">inner exception</param>
        public OAuthException(string message, Exception innerEx)
            : base(message, innerEx)
        {
        }

        /// <summary>
        /// Converts the exception into string format
        /// </summary>
        /// <returns>The exception in string format</returns>
        public override string ToString()
        {
            string result = null;
            if (!string.IsNullOrEmpty(this.Message))
            {
                // Create a resource manager to retrieve resources.
                ResourceManager rm = new ResourceManager("OAuthErrors", Assembly.GetExecutingAssembly());

                result = "Name = " + this.Message + "\n" + "Value = " + rm.GetString(this.Message);
            }

            if (this.InnerException != null)
            {
                if (!string.IsNullOrEmpty(result))
                {
                    result += "\n";
                }

                result += "InnerException" + this.InnerException.ToString();
            }

            return result;
        }

        /// <summary>
        /// Converts the exception into <c>Json</c> format
        /// </summary>
        /// <returns>The exception in <c>json</c> format</returns>
        public string ToJson()
        {
            string result = "{";
            if (!string.IsNullOrEmpty(this.Message))
            {
                // Create a resource manager to retrieve resources.
                ResourceManager rm = new ResourceManager("OAuthErrors", Assembly.GetExecutingAssembly());

                result = "\"Name\": \"" + this.Message + "\"," + "\"Value\": \"" + rm.GetString(this.Message) + "\"";
            }

            if (this.InnerException != null)
            {
                if (!string.IsNullOrEmpty(result))
                {
                    result += ",";
                }

                result += "\"InnerException\": \"" + this.InnerException.ToString() + "\"";
            }

            result += "}";

            return result;
        }
    }
}
