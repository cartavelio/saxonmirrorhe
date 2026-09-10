using System.Security.Cryptography;
using System.Text;

#nullable enable 

namespace Saxon.Impl.Helpers {
    public class DigestMaker
    {
        private string? hexDigest = null;
        private readonly SHA256 _digest;

        public DigestMaker()
        {
            _digest = SHA256.Create();
        }

        public void update(int value)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value.ToString());
            _digest.TransformBlock(bytes, 0, bytes.Length, bytes, 0);
        }

        public void update(string value)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            _digest.TransformBlock(bytes, 0, bytes.Length, bytes, 0);
        }

        public string getDigest()
        {
            if (hexDigest == null)
            {
                byte[] bytes = Encoding.UTF8.GetBytes("");
                _digest.TransformFinalBlock(bytes, 0, 0);
                bytes = _digest.Hash!;
                StringBuilder sb = new StringBuilder();
                foreach (var b in bytes)
                {
                    sb.AppendFormat("{0:x2}", b);
                }

                hexDigest = sb.ToString();
            }

            return hexDigest;
        }
    }
}
// Copyright (c) 2024 Saxonica Limited
