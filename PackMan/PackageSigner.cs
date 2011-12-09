using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace PackMan
{
    public class PackageSigner
    {
        public static void Sign(string cn, string outputFileName)
        {
            var myStore = OpenCertificateStore();
            var certificate = FindSingleMatchingCertificate(myStore, cn);
            var cryptoServiceProvider = (RSACryptoServiceProvider) certificate.PrivateKey;
            var signature = ComputeSignature(outputFileName, cryptoServiceProvider);
            WriteSignature(outputFileName, signature);
        }

        public static bool VerifySignature(string cn, string packageFileName)
        {
            var myStore = OpenCertificateStore();
            var certificate = FindSingleMatchingCertificate(myStore, cn);
            var cryptoServiceProvider = (RSACryptoServiceProvider)certificate.PrivateKey;
            var signature = ComputeSignature(outputFileName, cryptoServiceProvider);
            WriteSignature(outputFileName, signature);
        }

        private static byte[] ComputeSignature(string outputFileName, RSACryptoServiceProvider cryptoServiceProvider)
        {
            byte[] signature;
            using( var packageFileStream = new FileStream(outputFileName, FileMode.Open))
            {
                signature = cryptoServiceProvider.SignData(packageFileStream, new SHA1CryptoServiceProvider());
            }
            return signature;
        }

        private static X509Store OpenCertificateStore()
        {
            var myStore = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            myStore.Open(OpenFlags.ReadOnly);
            return myStore;
        }

        private static X509Certificate2 FindSingleMatchingCertificate(X509Store myStore, string cn)
        {
            var matching = myStore.Certificates.Find(X509FindType.FindBySubjectName, cn, false);
            if (matching.Count == 0)
            {
                throw new PackManException(String.Format("Certificate for CN={0} not found in current user's store",cn));
            }
            if (matching.Count > 1)
            {
                throw new PackManException(String.Format("More than one match found for certificate with CN={0} in current user's store.",cn));
            }
            return matching[0];
        }

        private static void WriteSignature(string outputFileName, byte[] signature)
        {
            var signatureFileName = Path.GetFileNameWithoutExtension(outputFileName) + ".signature";
            using (var signatureFileStream = new FileStream(signatureFileName,FileMode.Create))
            {
                signatureFileStream.Write(signature, 0, signature.Length);
                signatureFileStream.Flush();
            }
        }

        private static byte[] ReadSignature(string packageFileName)
        {
            var signatureFileName = Path.GetFileNameWithoutExtension(packageFileName) + ".signature";
            return File.ReadAllBytes(signatureFileName);
        }
    }
}