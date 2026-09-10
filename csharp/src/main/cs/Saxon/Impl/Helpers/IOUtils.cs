using System;
using System.Text;
using Saxon.Ejava.lang;

namespace Saxon.Impl.Helpers {
    public class IOUtils {
        public static System.IO.StreamWriter newOutputStreamWriter(System.IO.Stream stream, string encoding) {
            Encoding textEncoding;
            if (encoding.Equals("utf-8", StringComparison.OrdinalIgnoreCase)) {
                textEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
            }
            else {
                try {
                    textEncoding = Encoding.GetEncoding(encoding);
                }
                catch (ArgumentException e) {
                    throw new Saxon.Ejava.io.UnsupportedEncodingException();
                }
            }

            return new System.IO.StreamWriter(stream, textEncoding);
        }

        // Read all bytes from an input stream
        public static byte[] readAllBytes(System.IO.Stream stream) {
            byte[] bytes;
            using (var binaryReader = new System.IO.BinaryReader(stream))
            {
                bytes = binaryReader.ReadBytes((int)stream.Length);
            }
            return bytes;
        }
    }
}

// Copyright (c) 2021 - 2023 Saxonica Limited