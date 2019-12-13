// <copyright file="X509CertificateChecker.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Utils
{
    using System;
    using System.Security.Cryptography.X509Certificates;

    /// <summary>
    /// Utility class to check the validity of a X509 Certificate
    /// </summary>
    public static class X509CertificateChecker
    {
        /// <summary>
        /// Validate the X509 client certificate in an HTTP request.
        /// Method call completes successfully if client cert is valid, throws exception otherwise.
        /// </summary>
        /// <param name="thumbprint">expected X509 thumbprint</param>
        /// <param name="certificate">X509 certificate from an HTTP request</param>
        public static void ValidateX509Certificate(string thumbprint, X509Certificate2 certificate)
        {
            string certThumbprint = string.Empty;
            string certSubject = string.Empty;
            string certIssuer = string.Empty;
            string certSignatureAlg = string.Empty;
            string certIssueDate = string.Empty;
            string certExpiryDate = string.Empty;

            // check the X509 client certificate
            if (certificate == null)
            {
                throw new ArgumentNullException("certificate");
            }

            // Extract attributes from certificate
            // if any of these values cannot be extracted, an exception will be thrown
            certSubject = certificate.Subject;
            certIssuer = certificate.Issuer;
            certThumbprint = certificate.Thumbprint;
            certSignatureAlg = certificate.SignatureAlgorithm.FriendlyName;
            certIssueDate = certificate.NotBefore.ToShortDateString() + " " + certificate.NotBefore.ToShortTimeString();
            certExpiryDate = certificate.NotAfter.ToShortDateString() + " " + certificate.NotAfter.ToShortTimeString();

            // We will only accept the certificate as a valid certificate if all the conditions below are met:
            // 1. The certificate is not expired and is active for the current time.
            // 2. The certificate is not self signed
            // 3. The thumbprint of the certificate matches the input thumbprint
            // 4. The certificate is trusted

            // 1. Check time validity of certificate
            if (DateTime.Compare(DateTime.Now, certificate.NotBefore) < 0 || DateTime.Compare(DateTime.Now, certificate.NotAfter) > 0)
            {
                throw new Exception("Certificate is not valid for the current time. NotBefore= " + certificate.NotBefore + " , NotAfter= " + certificate.NotAfter);
            }

            // 2. Check that the certificate is not self signed
            if (certificate.Subject == certificate.Issuer)
            {
                throw new Exception("Certificate is self signed");
            }

            // 3. Check thumprint of certificate
            if (string.Compare(certificate.Thumbprint.Trim().ToUpper(), thumbprint.Trim().ToUpper()) != 0)
            {
                throw new Exception("Certificate thumbprint " + certificate.Thumbprint + " did not match expected thumbprint " + thumbprint);
            }

            // 4. Test if the certificate chains to a Trusted Root Authority
            X509Chain certChain = new X509Chain();
            certChain.Build(certificate);
            foreach (X509ChainElement chElement in certChain.ChainElements)
            {
                if (!chElement.Certificate.Verify())
                {
                    throw new Exception("Certificate chain element is not trusted: " + chElement.Certificate);
                }
            }

            // success. terminate without an exception.
        }
    }
}