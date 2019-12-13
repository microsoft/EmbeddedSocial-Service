// <copyright file="TestConstants.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus
{
    using System;
    using System.Configuration;

    using SocialPlus.Utils;

    /// <summary>
    /// Configuration constants used for tests
    /// </summary>
    public static class TestConstants
    {
        /// <summary>
        /// amount of time (in milliseconds) to wait after a write before issuing a query that depends upon it.
        /// </summary>
        public const int ServiceBusShortDelay = 2000;

        /// <summary>
        /// amount of time (in milliseconds) to wait after a write before issuing a query that depends upon it.
        /// </summary>
        public const int ServiceBusMediumDelay = 10000;

        /// <summary>
        /// amount of time (in milliseconds) to wait after a write before issuing a query that depends upon it.
        /// </summary>
        public const int ServiceBusLongDelay = 60000;

        /// <summary>
        /// amount of time to wait for search updates to be visible
        /// </summary>
        public const int SearchDelay = 8000;

        /// <summary>
        /// amount of time to wait for image resize operations to finish and be visible
        /// </summary>
        public const int ImageResizeDelay = 20000;

        /// <summary>
        /// Instance ID
        /// </summary>
        public const string InstanceId = "SocialPlusTestInstanceId";

        /// <summary>
        /// String used to name the setting of the Azure AD client
        /// </summary>
        public static readonly string EmbeddedSocialClientIdSetting = "AADEmbeddedSocialClientId";

        /// <summary>
        /// String used to name the setting of the SocialPlus's cert thumbprint
        /// </summary>
        public static readonly string SocialPlusCertThumbprint = "SocialPlusCertThumbprint";

        /// <summary>
        /// String used to name the setting of the URL to access keyvault
        /// </summary>
        public static readonly string SocialPlusVaultUrlSetting = "KeyVaultUri";

        /// <summary>
        /// the following AAD settings are for the Embedded Social Test Client 1 AAD application
        /// </summary>
        public static readonly AADSettings ValidSPClient = new AADSettings(ConfigurationManager.AppSettings["AADEmbeddedSocialTestClient1Settings"]);

        /// <summary>
        /// Name of environment (e.g., sp-dev-stefan)
        /// </summary>
        public static readonly string EnvironmentName;

        /// <summary>
        /// Name of service (e.g., localhost or sp-dev-stefan)
        /// </summary>
        public static readonly string ServiceName;

        /// <summary>
        /// .config file name
        /// </summary>
        public static readonly string ConfigFileName;

        /// <summary>
        /// DNS name of the environment
        /// </summary>
        public static readonly string DnsName;

        /// <summary>
        /// AppKey to use for the environment
        /// </summary>
        public static readonly string AppKey;

        /// <summary>
        /// E-mail address where e-mail about failed tests should be sent
        /// </summary>
        public static readonly string FailedTestsEmail;

        /// <summary>
        /// Initializes static members of the <see cref="TestConstants"/> class.
        ///
        /// Note that our CleanAndRecreateEnv.ps1 script parses this file to extract the AppKey.
        /// So, if you change the format below, remember to update that script.
        /// </summary>
        static TestConstants()
        {
#if SP_API
            EnvironmentName = "sp-api";
            ServiceName = EnvironmentName;
            ConfigFileName = EnvironmentName + ".config";
            AppKey = "2e3e7c55-69d5-44e3-bd17-1edc4040d2d5";
            FailedTestsEmail = "SocialPlusOps@microsoft.com";
            DnsName = "api.embeddedsocial.microsoft.com";
#endif
#if SP_API_LOCALHOST
            EnvironmentName = "sp-api";
            ServiceName = "localhost";
            ConfigFileName = EnvironmentName + ".config";
            AppKey = "2e3e7c55-69d5-44e3-bd17-1edc4040d2d5";
            FailedTestsEmail = "SocialPlusOps@microsoft.com";
#endif
#if SP_PPE
            EnvironmentName = "sp-ppe";
            ServiceName = EnvironmentName;
            ConfigFileName = EnvironmentName + ".config";
            AppKey = "6ac7fbfb-e0e1-4c74-bded-b6db984abf50";
            FailedTestsEmail = "SocialPlusOps@microsoft.com";
            DnsName = "ppe.embeddedsocial.microsoft.com";
#endif
#if SP_PPE_LOCALHOST
            EnvironmentName = "sp-ppe";
            ServiceName = "localhost";
            ConfigFileName = EnvironmentName + ".config";
            AppKey = "6ac7fbfb-e0e1-4c74-bded-b6db984abf50";
            FailedTestsEmail = "SocialPlusOps@microsoft.com";
#endif
#if SP_PPE_BEIHAI
            EnvironmentName = "sp-ppe-beihai";
            ServiceName = EnvironmentName;
            ConfigFileName = EnvironmentName + ".config";
            AppKey = "9eedc166-e9ea-4ce3-986f-891a5f0793af";
            FailedTestsEmail = "SocialPlusOps@microsoft.com";
            DnsName = "ppe-beihai.embeddedsocial.microsoft.com";
#endif
#if SP_PPE_BEIHAI_LOCALHOST
            EnvironmentName = "sp-ppe-beihai";
            ServiceName = "localhost";
            ConfigFileName = EnvironmentName + ".config";
            AppKey = "9eedc166-e9ea-4ce3-986f-891a5f0793af";
            FailedTestsEmail = "SocialPlusOps@microsoft.com";
#endif
#if SP_DEV_TEST1
            EnvironmentName = "sp-dev-test1";
            ServiceName = EnvironmentName;
            ConfigFileName = EnvironmentName + ".config";
            AppKey = "f450d15f-e0ac-4a6b-b693-ae38a1b6821c";
            FailedTestsEmail = "SocialPlusOps@microsoft.com";
            DnsName = "dev-test1.embeddedsocial.microsoft.com";
#endif
#if SP_DEV_TEST1_LOCALHOST
            EnvironmentName = "sp-dev-test1";
            ServiceName = "localhost";
            ConfigFileName = EnvironmentName + ".config";
            AppKey = "f450d15f-e0ac-4a6b-b693-ae38a1b6821c";
            FailedTestsEmail = "SocialPlusOps@microsoft.com";
#endif
#if SP_DEV_VSO
            EnvironmentName = "sp-dev-vso";
            ServiceName = EnvironmentName;
            ConfigFileName = EnvironmentName + ".config";
            AppKey = "100a3262-3c0b-4b1d-ba1b-29c2d037cfbf";
            FailedTestsEmail = "SocialPlusOps@microsoft.com";
            DnsName = "dev-vso.embeddedsocial.microsoft.com";
#endif
#if SP_DEV_VSO_LOCALHOST
            EnvironmentName = "sp-dev-vso";
            ServiceName = "localhost";
            ConfigFileName = EnvironmentName + ".config";
            AppKey = "100a3262-3c0b-4b1d-ba1b-29c2d037cfbf";
            FailedTestsEmail = "SocialPlusOps@microsoft.com";
#endif
#if SP_DEV_ALEC
            EnvironmentName = "sp-dev-alec";
            ServiceName = EnvironmentName;
            ConfigFileName = EnvironmentName + ".config";
            AppKey = "92b8551b-0b0a-44e8-9082-6ff080ea6616";
            FailedTestsEmail = "alecw@microsoft.com";
            DnsName = "dev-alec.embeddedsocial.microsoft.com";
#endif
#if SP_DEV_ALEC_LOCALHOST
            EnvironmentName = "sp-dev-alec";
            ServiceName = "localhost";
            ConfigFileName = EnvironmentName + ".config";
            AppKey = "92b8551b-0b0a-44e8-9082-6ff080ea6616";
            FailedTestsEmail = "alecw@microsoft.com";
#endif
#if SP_DEV_STEFAN
            EnvironmentName = "sp-dev-stefan";
            ServiceName = EnvironmentName;
            ConfigFileName = EnvironmentName + ".config";
            AppKey = "b7688014-902d-4091-96c7-dbbe4055f31b";
            FailedTestsEmail = "ssaroiu@microsoft.com";
            DnsName = "dev-stefan.embeddedsocial.microsoft.com";
#endif
#if SP_DEV_STEFAN_LOCALHOST
            EnvironmentName = "sp-dev-stefan";
            ServiceName = "localhost";
            ConfigFileName = EnvironmentName + ".config";
            AppKey = "b7688014-902d-4091-96c7-dbbe4055f31b";
            FailedTestsEmail = "ssaroiu@microsoft.com";
#endif
#if SP_DEV_EDUARDO
            EnvironmentName = "sp-dev-eduardo";
            ServiceName = EnvironmentName;
            ConfigFileName = EnvironmentName + ".config";
            AppKey = "23e0425d-7ee8-4e24-b81a-199c7870db68";
            FailedTestsEmail = "cuervo@microsoft.com";
            DnsName = "dev-eduardo.embeddedsocial.microsoft.com";
#endif
#if SP_DEV_EDUARDO_LOCALHOST
            EnvironmentName = "sp-dev-eduardo";
            ServiceName = "localhost";
            ConfigFileName = EnvironmentName + ".config";
            AppKey = "23e0425d-7ee8-4e24-b81a-199c7870db68";
            FailedTestsEmail = "cuervo@microsoft.com";
#endif
#if SP_DEV_LANDON
            // application = test-landon, appHandle = 3xgHX4IvCIk, developerId = 4f211154-f0d8-45e4-b434-14f6fbc7229e
            EnvironmentName = "sp-dev-landon";
            ServiceName = EnvironmentName;
            ConfigFileName = EnvironmentName + ".config";
            AppKey = "efc94deb-7753-40c6-8a1c-abc7ddf31bf5";
            FailedTestsEmail = "t-lacox@microsoft.com";
            DnsName = "dev-landon.embeddedsocial.microsoft.com";
#endif
#if SP_DEV_LANDON_LOCALHOST
            // application = test-landon, appHandle = 3xgHX4IvCIk, developerId = 4f211154-f0d8-45e4-b434-14f6fbc7229e
            EnvironmentName = "sp-dev-landon";
            ServiceName = "localhost";
            ConfigFileName = EnvironmentName + ".config";
            AppKey = "efc94deb-7753-40c6-8a1c-abc7ddf31bf5";
            FailedTestsEmail = "t-lacox@microsoft.com";
#endif
#if SP_DEV_SHARAD
            EnvironmentName = "sp-dev-sharad";
            ServiceName = EnvironmentName;
            ConfigFileName = EnvironmentName + ".config";
            AppKey = "2fa43f8a-9767-40ad-9914-d9a007f27fd7";
            FailedTestsEmail = "sagarwal@microsoft.com";
            DnsName = "dev-sharad.embeddedsocial.microsoft.com";
#endif
#if SP_DEV_SHARAD_LOCALHOST
            EnvironmentName = "sp-dev-sharad";
            ServiceName = "localhost";
            ConfigFileName = EnvironmentName + ".config";
            AppKey = "2fa43f8a-9767-40ad-9914-d9a007f27fd7";
            FailedTestsEmail = "sagarwal@microsoft.com";
#endif
#if SP_PROD_BEIHAI
            EnvironmentName = "sp-prod-beihai";
            ServiceName = EnvironmentName;
            ConfigFileName = EnvironmentName + ".config";
            AppKey = "2aed3c01-85e5-4510-b55f-5f891e33942e";
            FailedTestsEmail = "SocialPlusOps@microsoft.com";
            DnsName = "prod-beihai.embeddedsocial.microsoft.com";
#endif
#if SP_PROD_BEIHAI_LOCALHOST
            EnvironmentName = "sp-prod-beihai";
            ServiceName = "localhost";
            ConfigFileName = EnvironmentName + ".config";
            AppKey = "2aed3c01-85e5-4510-b55f-5f891e33942e";
            FailedTestsEmail = "SocialPlusOps@microsoft.com";
#endif
#if SP_TEST_PERF
            EnvironmentName = "sp-test-perf";
            ServiceName = EnvironmentName;
            ConfigFileName = EnvironmentName + ".config";
            AppKey = "5743cd5f-28bc-4ce9-86e6-9987be8e8b75";
            FailedTestsEmail = "SocialPlusOps@microsoft.com";
            DnsName = "test-perf.embeddedsocial.microsoft.com";
#endif
#if SP_TEST_PERF_LOCALHOST
            EnvironmentName = "sp-test-perf";
            ServiceName = "localhost";
            ConfigFileName = EnvironmentName + ".config";
            AppKey = "5743cd5f-28bc-4ce9-86e6-9987be8e8b75";
            FailedTestsEmail = "SocialPlusOps@microsoft.com";
#endif
        }

        /// <summary>
        /// Gets the URI name for the service
        /// </summary>
        public static Uri ServerApiBaseUrl
        {
            get
            {
                // If DNS name present, use SSL and DNS name in the URI
                if (!string.IsNullOrEmpty(DnsName))
                {
                    return new Uri("https://" + DnsName + "/");
                }

                // Otherwise, always use http.
                if (ServiceName != "localhost")
                {
                    return new Uri("http://" + ServiceName + ".cloudapp.net/");
                }

                return new Uri("http://localhost:1324/");
            }
        }
    }
}
