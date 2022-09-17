#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using System;
using System.Collections.Generic;

namespace Org.BouncyCastle.Crypto.Tls
{
	/// <summary>
	///     A temporary class to use LegacyTlsAuthentication
	/// </summary>
	public sealed class LegacyTlsClient : DefaultTlsClient
    {
        private readonly IClientCredentialsProvider credProvider;
        private readonly Uri TargetUri;
        private readonly ICertificateVerifyer verifyer;

        public LegacyTlsClient(Uri targetUri, ICertificateVerifyer verifyer, IClientCredentialsProvider prov,
            List<string> hostNames)
        {
            TargetUri = targetUri;
            this.verifyer = verifyer;
            credProvider = prov;
            HostNames = hostNames;
        }

        public override TlsAuthentication GetAuthentication()
        {
            return new LegacyTlsAuthentication(TargetUri, verifyer, credProvider);
        }
    }
}

#endif