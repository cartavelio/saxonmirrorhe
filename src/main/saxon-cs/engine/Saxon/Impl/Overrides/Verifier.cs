using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using NSec.Cryptography;
using Saxon.Hej.trans;
using PublicKey = NSec.Cryptography.PublicKey;

namespace Saxon.Impl.Overrides {
    public class Verifier : Saxon.Eej.config.Verifier {

        internal Verifier()  {
        }
        
        protected internal override void loadLicense(Saxon.Hej.Configuration config) {
            string dir = System.Environment.GetEnvironmentVariable("SAXON_LICENSE_DIR");
            string path = dir + "/saxon-license.lic";
            loadLicense(path);
        }

        protected internal override bool verify(string data, string signature) {
            byte[] binaryData = Encoding.UTF8.GetBytes(data);
            // byte[] digest = new SHA1Managed().ComputeHash(binaryData);
            DSA dsa = DSA.Create();
            dsa.ImportFromPem(publicKeyPem.AsSpan());
            // DSAParameters parameters = dsa.ExportParameters(false);
            // BigInteger G = new BigInteger(parameters.G, true, true);
            // BigInteger P = new BigInteger(parameters.P, true, true);
            // BigInteger Q = new BigInteger(parameters.Q, true, true);
            // BigInteger Y = new BigInteger(parameters.Y, true, true);
            // Console.WriteLine(parameters);
            
            var decodedSig = decodeSignature(signature);
            var valid = dsa.VerifyData(binaryData, decodedSig, HashAlgorithmName.SHA1);
            return valid;
        }

        protected internal override bool verify2(string data, string signature)
        {
            var algorithm = SignatureAlgorithm.Ed25519;
            var publicKeyBytes = Convert.FromBase64String(ed25519Pem);
            var signatureBytes = decodeSignature(signature);
            var dataBytes = Encoding.UTF8.GetBytes(data);
            var publicKey = PublicKey.Import(algorithm, publicKeyBytes, KeyBlobFormat.PkixPublicKey);
            var valid = algorithm.Verify(publicKey, dataBytes, signatureBytes);
            return valid;
        }

        // The public key in PEM format
        private static string publicKeyPem =
            "-----BEGIN PUBLIC KEY-----\n" +
            "MIIBtzCCASwGByqGSM44BAEwggEfAoGBAP1/U4EddRIpUt9KnC7s5Of2EbdSPO9E\n" +
            "AMMeP4C2USZpRV1AIlH7WT2NWPq/xfW6MPbLm1Vs14E7gB00b/JmYLdrmVClpJ+f\n" +
            "6AR7ECLCT7up1/63xhv4O1fnxqimFQ8E+4P208UewwI1VBNaFpEy9nXzrith1yrv\n" +
            "8iIDGZ3RSAHHAhUAl2BQjxUjC8yykrmCouuEC/BYHPUCgYEA9+GghdabPd7LvKtc\n" +
            "NrhXuXmUr7v6OuqC+VdMCz0HgmdRWVeOutRZT+ZxBxCBgLRJFnEj6EwoFhO3zwky\n" +
            "jMim4TwWeotUfI0o4KOuHiuzpnWRbqN/C/ohNWLx+2J6ASQ7zKTxvqhRkImog9/h\n" +
            "WuWfBpKLZl6Ae1UlZAFMO/7PSSoDgYQAAoGAbKBn5Gl1Si+Y0Ill01q0ElKXT2n8\n" +
            "0mAVLPtY+U2LlW1qGpHMIToREHMB3TfIWzg92kCewGfSqMS+ZlECCmmv1TNjAXLW\n" +
            "+W+ShmfXcF1G5W02To6WcAKn2GS6ywIiX1KycbofZSLZj/gpnGJzskvEEgLIhXiX\n" +
            "5GtA/XYkqcsMyeg=\n" +
            "-----END PUBLIC KEY-----";
        
        // The public ED25519 key
        // To convert the hexbinary form in Java to the PEM form needed here:
        // $ echo 302A...6030 | xxd -r -p - | openssl ec -inform der -pubin -pubout
        private static string ed25519Pem = "MCowBQYDK2VwAyEA0o9qpFUZdj2CigQ0ybybWmZl7lYwPBA+0qSZ6EynYDA=";
        
        // Take the signature as it appears in the license file, and convert it to the
        // form expected by the .NET verification API. We first convert from hex to binary.
        // then treat the binary as a sequence of two long integers encoded in ASN.1 DER
        // format. Rather than import a DER decoding library for the purpose, we hand-parse
        // the ASN.1. We're expecting the following:
        // 
        // 30 xx - signals a sequence of length xx bytes
        //    02 ii - signals an integer of length ii (typically ii=20)
        //    first integer (a sequence of ii bytes, unsigned big-endian)
        //    02 jj - signals an integer of length jj (typically jj=20)
        //    second integer (a sequence of jj bytes, unsigned big-endian)
        //
        // The format of the output is a 40-byte array containing the concatenation of the
        // two 20-byte integers, in big-endian format
        private static byte[] decodeSignature(string signatureHex) {
            byte[] sigDER = convertHexToBinary(signatureHex);
            expect(sigDER[0] == 0x30);
            int len0 = sigDER[1];
            expect(sigDER[2] == 0x02);
            int len1 = sigDER[3];
            expect(sigDER[4 + len1] == 0x02);
            int len2 = sigDER[5 + len1];
            expect(len1 + len2 + 4 == len0);
            byte[] decodedSig = new byte[40];
            int start1 = 4;
            int size1 = len1;
            int dest1 = 0;
            if (len1 > 20) {
                start1 += (len1 - 20);
                size1 = 20;
            }

            if (len1 < 20) {
                dest1 = (20 - len1);
            }

            Array.Copy(sigDER, start1, decodedSig, dest1, size1);
            int start2 = 6 + len1;
            int size2 = len2;
            int dest2 = 20;
            if (len2 > 20) {
                start2 += (len2 - 20);
                size2 = 20;
            }

            if (len2 < 20) {
                dest2 += (20 - len2);
            }

            Array.Copy(sigDER, start2, decodedSig, dest2, size2);
            return decodedSig;
        }

        private static void expect(bool assertion) {
            if (!assertion) {
                throw new LicenseException("Invalid license signature", LicenseException.INVALID);
            }
        } 
    }
    
}

// Copyright (c) 2021 - 2023 Saxonica Limited
