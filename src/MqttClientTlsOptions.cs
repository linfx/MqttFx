using System;
using System.Collections.Generic;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace nMqtt
{
    public class MqttClientTlsOptions
    {
        public bool UseTls { get; set; }

        public bool IgnoreCertificateRevocationErrors { get; set; }

        public bool IgnoreCertificateChainErrors { get; set; }

        public bool AllowUntrustedCertificates { get; set; }

        public List<byte[]> Certificates { get; set; }

        public Func<X509Certificate, X509Chain, SslPolicyErrors, MqttClientOptions, bool> CertificateValidationCallback { get; set; }
    }
}