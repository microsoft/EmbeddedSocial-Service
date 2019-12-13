// <copyright file="AzureKeyVaultClientFactory.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Reflection;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;
    using Moq;
    using SocialPlus.Server.KVLibrary;
    using SocialPlus.Utils;

    /// <summary>
    /// Generates IKeyVaultClient objects for unit testing KVLibrary
    /// </summary>
    public class AzureKeyVaultClientFactory
    {
        /// <summary>
        /// String used to name the setting of the Azure AD client
        /// </summary>
        private static readonly string EmbeddedSocialClientIdSetting = "AADEmbeddedSocialClientId";

        /// <summary>
        /// String used to name the setting of the Azure AD client secret
        /// </summary>
        private static readonly string SocialPlusCertThumbprint = "SocialPlusCertThumbprint";

        /// <summary>
        /// List of mocked Azure KV clients
        /// </summary>
        private readonly List<Mock<AzureKeyVaultClient>> listOfAzureKeyVaultClients = new List<Mock<AzureKeyVaultClient>>();

        /// <summary>
        /// Input list of methods to exceptions
        /// </summary>
        private readonly Dictionary<MethodInfo, List<Exception>> methodToExceptionList = new Dictionary<MethodInfo, List<Exception>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureKeyVaultClientFactory"/> class.
        /// Creates a whole bunch of mocked Azure key vault clients
        /// </summary>
        public AzureKeyVaultClientFactory()
        {
            var fileSettingsReader = new FileSettingsReader(ConfigurationManager.AppSettings["ConfigRelativePath"] + Path.DirectorySeparatorChar + TestConstants.ConfigFileName);
            var clientId = fileSettingsReader.ReadValue(EmbeddedSocialClientIdSetting);
            var certThumbprint = fileSettingsReader.ReadValue(SocialPlusCertThumbprint);
            var storeLocation = StoreLocation.CurrentUser;
            var cert = new CertificateHelper(certThumbprint, clientId, storeLocation);

            Mock<AzureKeyVaultClient> mock1 = new Mock<AzureKeyVaultClient>(cert);
            mock1.CallBase = true;
            mock1.Setup(c => c.GetKeyAsync(It.IsNotNull<string>(), It.IsAny<CancellationToken>())).ThrowsAsync(new ArgumentNullException());

            Mock<AzureKeyVaultClient> mock2 = new Mock<AzureKeyVaultClient>(cert);
            mock2.CallBase = true;
            mock2.Setup(c => c.GetSecretAsync(It.IsNotNull<string>(), It.IsAny<CancellationToken>())).ThrowsAsync(new ArgumentNullException());

            Mock<AzureKeyVaultClient> mock3 = new Mock<AzureKeyVaultClient>(cert);
            mock3.CallBase = true;
            mock3.Setup(c => c.SignAsync(It.IsNotNull<string>(), It.IsNotNull<string>(), It.IsNotNull<string>(), It.IsNotNull<string>(), It.IsNotNull<byte[]>(), It.IsNotNull<CancellationToken>()));

            Mock<AzureKeyVaultClient> mock4 = new Mock<AzureKeyVaultClient>(cert);
            mock4.CallBase = true;

            this.methodToExceptionList[typeof(AzureKeyVaultClient).GetMethod("GetKeyAsync")] = new List<Exception> { new ArgumentNullException() };
            this.methodToExceptionList[typeof(AzureKeyVaultClient).GetMethod("GetSecretAsync")] = new List<Exception> { new ArgumentNullException() };
            this.methodToExceptionList[typeof(AzureKeyVaultClient).GetMethod("SignAsync")] = new List<Exception> { new ArgumentNullException() };

            this.listOfAzureKeyVaultClients.Add(this.MakeBlankMock(cert));
            foreach (Mock<AzureKeyVaultClient> mock in this.MakeMocks(cert))
            {
                this.listOfAzureKeyVaultClients.Add(mock);
            }
        }

        /// <summary>
        /// Generates IKeyVaultClient objects using Moq for unit testing KVLibrary
        /// </summary>
        /// <returns>The IKeyVaultClient objects</returns>
        public IEnumerable<IKeyVaultClient> CreateKVClient()
        {
            foreach (var client in this.listOfAzureKeyVaultClients)
            {
                yield return client.Object;
            }
        }

        /// <summary>
        /// Makes a mock with the same behavior as the implemented object, no changes
        /// </summary>
        /// <param name="cert">CertificateHelper for building the Mock</param>
        /// <returns>A Mock with the same behavior as the base object</returns>
        private Mock<AzureKeyVaultClient> MakeBlankMock(CertificateHelper cert)
        {
            Mock<AzureKeyVaultClient> mock = new Mock<AzureKeyVaultClient>(cert);
            mock.CallBase = true;

            return mock;
        }

        /// <summary>
        /// Takes the methodToExceptionList and makes mocks
        /// </summary>
        /// <param name="cert">CertificateHelper for building the Mock</param>
        /// <returns>Yields Mocks for each item in the methodToExceptionList</returns>
        private IEnumerable<Mock<AzureKeyVaultClient>> MakeMocks(CertificateHelper cert)
        {
            foreach (MethodInfo method in this.methodToExceptionList.Keys)
            {
                foreach (Exception e in this.methodToExceptionList[method])
                {
                    Mock<AzureKeyVaultClient> mock = new Mock<AzureKeyVaultClient>(cert);
                    mock.CallBase = true;

                    if (method.Name == "GetKeyAsync")
                    {
                        mock.Setup(c => c.GetKeyAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ThrowsAsync(e);
                    }
                    else if (method.Name == "GetSecretAsync")
                    {
                        mock.Setup(c => c.GetSecretAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ThrowsAsync(e);
                    }
                    else if (method.Name == "SignAsync")
                    {
                        mock.Setup(c => c.SignAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<CancellationToken>())).ThrowsAsync(e);
                    }
                    else
                    {
                        Console.WriteLine("Unknown method name: " + method.Name);
                    }

                    yield return mock;
                }
            }
        }
    }
}
