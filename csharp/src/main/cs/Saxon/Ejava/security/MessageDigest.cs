using System.Security.Cryptography;
using Saxon.Hej.trans;

namespace Saxon.Ejava.security {

    // Emulation of java.security.MessageDigest, sufficient to support the needs of the fn:hash function in 4.0
    public class MessageDigest {

        public static MessageDigest getInstance(string algorithm) {
            return new MessageDigest(algorithm);
        }

        private string algorithm;

        public MessageDigest(string algorithm) {
            this.algorithm = algorithm;
        }

        public byte[] digest(byte[] input) {
            switch (algorithm) {
                case "MD5":
                    return MD5.Create().ComputeHash(input);
                case "SHA-1":
                    return SHA1.Create().ComputeHash(input);
                case "SHA-256":
                    return SHA256.Create().ComputeHash(input);
                default:
                    throw new UncheckedXPathException(new XPathException("Unknown hash algorithm: " + algorithm,
                        "FOHA0001"));
            }
        }
    }
}