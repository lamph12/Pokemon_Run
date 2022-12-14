#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using System;

namespace Org.BouncyCastle.Crypto.Tls
{
    public interface IClientCredentialsProvider
    {
        TlsCredentials GetClientCredentials(TlsContext context, CertificateRequest certificateRequest);
    }

    /// <summary>
    ///     A temporary class to wrap old CertificateVerifyer stuff for new TlsAuthentication.
    /// </summary>
    public class LegacyTlsAuthentication : TlsAuthentication
    {
        protected IClientCredentialsProvider credProvider;
        protected Uri TargetUri;
        protected ICertificateVerifyer verifyer;

        public LegacyTlsAuthentication(Uri targetUri, ICertificateVerifyer verifyer, IClientCredentialsProvider prov)
        {
            TargetUri = targetUri;
            this.verifyer = verifyer;
            credProvider = prov;
        }

        public virtual void NotifyServerCertificate(Certificate serverCertificate)
        {
            if (!verifyer.IsValid(TargetUri, serverCertificate.GetCertificateList()))
                throw new TlsFatalAlert(AlertDescription.user_canceled);
        }

        public virtual TlsCredentials GetClientCredentials(TlsContext context, CertificateRequest certificateRequest)
        {
            return credProvider == null ? null : credProvider.GetClientCredentials(context, certificateRequest);
        }
    }
}

#endif