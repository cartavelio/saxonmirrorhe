using System;
using System.IO.Compression;
using System.Text;
using Saxon.Ejava.lang;
using Saxon.Hej.resource;

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
        
        public static string gzipInput(System.IO.Stream stream) {
            var decompressor = new GZipStream(stream, CompressionMode.Decompress);
            var reader = new System.IO.StreamReader(decompressor);
            return reader.ReadToEnd();
        }

        public static bool CatalogFileExists(Saxon.Ejavax.xml.transform.Source sourceInput)
        {
            // Trying to determine if the input has successfully connected to a URI.
            if (sourceInput.GetType() == typeof(TypedStreamSource))
            {
                var tsource = (TypedStreamSource) sourceInput;
                if (tsource.getInputStream() == null)
                {
                    return false;
                }
                // We aren't going to use this stream, so close it.
                tsource.getInputStream().Close();
            }
            return true;
        }
        
    }
}

// Copyright (c) 2021 - 2023 Saxonica Limited