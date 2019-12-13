// <copyright file="CustomThrottlingHandler.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.WebRoleCommon.RateLimiting
{
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;

    using SocialPlus.Logging;
    using SocialPlus.Utils;
    using WebApiThrottle;

    /// <summary>
    /// Custom implementation of the throttling handler that ensures that the rate limiting policy can update periodically.
    /// Our throttling policy uses appKeys -- each appKey gets a rate limit.
    /// In its constructor, this throttling handler sets a static throttling policy where each appKey gets a fixed rate limit at first. The constructor
    /// also sets a timer that fires every 24 hours and calls the method RefreshingThottlePolicy. This method's role is to update the throttling policy.
    /// Currently, the implementation of this callback makes no updates (TODO: this is temporary).
    /// </summary>
    public class CustomThrottlingHandler : ThrottlingHandler
    {
        /// <summary>
        /// Log
        /// </summary>
        private readonly ILog log;

        /// <summary>
        /// This alert function is called when the validation parameters fail to self-refresh.
        /// </summary>
        private readonly Action<string, Exception> alert;

        /// <summary>
        /// Self-refreshing variable for updating the throttle policy
        /// </summary>
        private readonly SelfRefreshingVar<IPolicyRepository> srvThrottlePolicy;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomThrottlingHandler"/> class.
        /// </summary>
        /// <param name="log">Log</param>
        /// <param name="syncSettingsReader">synchronous settings reader</param>
        public CustomThrottlingHandler(ILog log, ISettingsReader syncSettingsReader)
            : base()
        {
            this.log = log;
            this.alert = (string msg, Exception ex) => this.log.LogError(msg, ex);

            // Read the rate limit from the configuration
            long rateLimitPerMinute = Convert.ToInt64(syncSettingsReader.ReadValue("RateLimitPerMinute"));

            // The policy reads the threshold from the settings reader
            ThrottlePolicy throttlePolicyOnStartup = new ThrottlePolicy(perMinute: rateLimitPerMinute) { ClientThrottling = true };

            // Bug fixes <-- WebApiThrottle has a bug. Setting these to null tells WebApiThrottle that we do not do ip-based nor endpoint-based
            // rate limiting. We avoid the bug this way.
            throttlePolicyOnStartup.IpRules = null;
            throttlePolicyOnStartup.EndpointRules = null;

            // Assign the static throttle policy on startup. Throttle on clients (meaning app keys).
            this.Policy = throttlePolicyOnStartup;

            this.Repository = new CacheRepository();
            this.Logger = new TracingThrottleLogger(this.log);
            this.PolicyRepository = new PolicyCacheRepository();

            // Set the throttle policy to update every 24 hours
            this.srvThrottlePolicy = new SelfRefreshingVar<IPolicyRepository>(this.PolicyRepository, TimeUtils.TwentyFourHours, this.RefreshingThottlePolicy, this.alert);
        }

        /// <summary>
        /// Gets app key header name
        /// </summary>
        public static string AppKeyHeader
        {
            get { return "appkey"; }
        }

        /// <summary>
        /// This method is called periodically to refresh the thottle policy.
        /// </summary>
        /// <remarks>
        /// Currently, this method does no updates to the policy. However, in the future, we'll update the throttle policy dynamically. These will require
        /// making async calls to Table Storage.
        /// </remarks>
        /// <returns>a task</returns>
        public Task<IPolicyRepository> RefreshingThottlePolicy()
        {
            // get policy object from cache
            var policy = this.PolicyRepository.FirstOrDefault(ThrottleManager.GetPolicyKey());

            // here where we will update the policy

            // apply policy updates
            ThrottleManager.UpdatePolicy(policy, this.PolicyRepository);

            return Task.FromResult(this.PolicyRepository);
        }

        /// <summary>
        /// Extracts a key (an identity) of the incoming request for rate limiting purposes. Currently, we use the appKey as an identified for
        /// each request -- our rate limiting policies operate on app keys. If no appKey present, the request will be classified as "anon".
        /// </summary>
        /// <param name="request">incoming request</param>
        /// <returns>the request identity for rate limiting purposes</returns>
        protected override RequestIdentity SetIndentity(HttpRequestMessage request)
        {
            return new RequestIdentity()
            {
                ClientKey = request.Headers.Contains(AppKeyHeader) ? request.Headers.GetValues(AppKeyHeader).First() : "anon"
            };
        }
    }
}
