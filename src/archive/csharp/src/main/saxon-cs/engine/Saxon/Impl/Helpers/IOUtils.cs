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
                textEncoding = Encoding.GetEncoding(encoding);
            }

            return new System.IO.StreamWriter(stream, textEncoding);
        }
    }
}

// Copyright (c) 2021 Saxonica Limited