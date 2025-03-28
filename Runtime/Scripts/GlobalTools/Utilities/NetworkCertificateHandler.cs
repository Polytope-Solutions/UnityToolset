using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.Networking;

namespace PolytopeSolutions.Toolset.Networking {
    public class NetworkCertificateHandler : CertificateHandler {
        //// Encoded RSAPublicKey
        //private static string PUB_KEY = "SOMEKEY";

        protected override bool ValidateCertificate(byte[] certificateData) {
            return true;
            //X509Certificate2 certificate = new X509Certificate2(certificateData);
            //string publicKey = certificate.GetPublicKeyString();
            //return publicKey.Equals(PUB_KEY);
        }

    }
}
