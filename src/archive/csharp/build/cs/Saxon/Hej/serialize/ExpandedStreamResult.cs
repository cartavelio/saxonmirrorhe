using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.serialize {
    public class ExpandedStreamResult {
        private readonly Saxon.Hej.Configuration config;
        private Saxon.Ejava.util.Properties outputProperties;
        private readonly string systemId;
        private System.IO.TextWriter writer;
        private System.IO.Stream outputStream;
        private Saxon.Hej.serialize.charcode.CharacterSet characterSet;
        private string encoding;
        public ExpandedStreamResult(Saxon.Hej.Configuration config, Saxon.Ejavax.xml.transform.stream.StreamResult result, Saxon.Ejava.util.Properties outputProperties)  {
            this.config = config;
            this.systemId = result.getSystemId();
            this.writer = result.getWriter();
            this.outputStream = result.getOutputStream();
            this.outputProperties = outputProperties;
            this.encoding = outputProperties.getProperty(Saxon.Ejavax.xml.transform.OutputKeys.ENCODING);
            if ((encoding == null)) {
                encoding = "UTF8";
            } else {
                if (Saxon.Impl.Helpers.StringUtils.stringEqualsIgnoreCase(encoding, "UTF-8")) {
                    encoding = "UTF8";
                } else {
                    if (Saxon.Impl.Helpers.StringUtils.stringEqualsIgnoreCase(encoding, "UTF-16")) {
                        encoding = "UTF16";
                    }
                }
            }
            if ((characterSet == null)) {
                characterSet = config.getCharacterSetFactory().getCharacterSet(encoding);
            }
            string byteOrderMark = outputProperties.getProperty(Saxon.Hej.lib.SaxonOutputKeys.BYTE_ORDER_MARK);
            if (("no".Equals(byteOrderMark) && "UTF16".Equals(encoding))) {
                encoding = "UTF-16BE";
            } else {
                if (!(((characterSet is Saxon.Hej.serialize.charcode.UTF8CharacterSet)))) {
                    encoding = characterSet.getCanonicalName();
                }
            }
        }
        public Saxon.Hej.str.UnicodeWriter obtainUnicodeWriter() {
            if ((writer != null)) {
                return new Saxon.Hej.str.UnicodeWriterToWriter(writer);
            } else {
                System.IO.Stream os = obtainOutputStream();
                return makeUnicodeWriterFromOutputStream(os);
            }
        }
        protected internal System.IO.Stream obtainOutputStream() {
            if ((outputStream != null)) {
                return outputStream;
            }
            string uriString = systemId;
            if ((uriString == null)) {
                throw new Saxon.Hej.trans.XPathException("Result has no system ID, writer, or output stream defined");
            }
            try {
                Saxon.Ejava.io.File file = makeWritableOutputFile(uriString);
                outputStream = new System.IO.FileStream(file.getPath(), System.IO.FileMode.Create);
            } catch (Exception fnf) when (fnf is Saxon.Ejava.io.FileNotFoundException || fnf is System.UriFormatException || fnf is Saxon.Ejava.lang.IllegalArgumentException) {
                throw new Saxon.Hej.trans.XPathException(fnf);
            }
            return outputStream;
        }
        public static Saxon.Ejava.io.File makeWritableOutputFile(string uriString) {
            System.Uri uri = new System.Uri(uriString, System.UriKind.RelativeOrAbsolute);
            if (!(uri.IsAbsoluteUri)) {
                try {
                    uri = new Saxon.Ejava.io.File(uriString).getAbsoluteFile().toURI();
                } catch (System.Exception e) {
                }
            }
            Saxon.Ejava.io.File file = new Saxon.Ejava.io.File(uri);
            try {
                if (("file".Equals(uri.Scheme) && !(file.exists()))) {
                    Saxon.Ejava.io.File directory = file.getParentFile();
                    if (((directory != null) && !(directory.exists()))) {
                        directory.mkdirs();
                    }
                    file.createNewFile();
                }
                if (file.isDirectory()) {
                    throw new Saxon.Hej.trans.XPathException(("Cannot write to a directory: "+uriString), Saxon.Hej.trans.SaxonErrorCode.SXRD0004);
                }
                if (!(file.canWrite())) {
                    throw new Saxon.Hej.trans.XPathException(("Cannot write to URI "+uriString), Saxon.Hej.trans.SaxonErrorCode.SXRD0004);
                }
            } catch (System.IO.IOException err) {
                throw new Saxon.Hej.trans.XPathException(("Failed to create output file "+uri), err);
            }
            return file;
        }
        public bool usesWriter() {
            return true;
        }
        public void setWriter(System.IO.TextWriter writer) {
            this.writer = writer;
            if (((writer is System.IO.StreamWriter) && (outputProperties != null))) {
                string enc = ((System.IO.StreamWriter)(writer)).Encoding.EncodingName;
                outputProperties.setProperty(Saxon.Ejavax.xml.transform.OutputKeys.ENCODING, enc);
                characterSet = config.getCharacterSetFactory().getCharacterSet(outputProperties);
            }
        }
        public System.IO.TextWriter getWriter() {
            return writer;
        }
        private System.IO.TextWriter makeWriterFromOutputStream(System.IO.Stream stream) {
            outputStream = stream;
            try {
                System.Text.Encoding javaEncoding;
                if ((Saxon.Impl.Helpers.StringUtils.stringEqualsIgnoreCase(encoding, "iso-646") || Saxon.Impl.Helpers.StringUtils.stringEqualsIgnoreCase(encoding, "iso646"))) {
                    javaEncoding = System.Text.Encoding.ASCII;
                } else {
                    javaEncoding = Saxon.Impl.Helpers.StringUtils.EncodingForName(encoding);
                }
                if (Saxon.Impl.Helpers.StringUtils.stringEqualsIgnoreCase(encoding, "UTF8")) {
                    writer = new Saxon.Hej.serialize.UTF8Writer(outputStream);
                } else {
                    writer = new Saxon.Ejava.io.BufferedWriter(new System.IO.StreamWriter(outputStream, javaEncoding));
                }
                return writer;
            } catch (System.Exception err) {
                if (Saxon.Impl.Helpers.StringUtils.stringEqualsIgnoreCase(encoding, "UTF8")) {
                    throw new Saxon.Hej.trans.XPathException("Failed to create a UTF8 output writer");
                }
                throw new Saxon.Hej.trans.XPathException((("Encoding "+encoding)+" is not supported"), "SESU0007");
            }
        }
        private Saxon.Hej.str.UnicodeWriter makeUnicodeWriterFromOutputStream(System.IO.Stream stream) {
            outputStream = stream;
            try {
                if (Saxon.Impl.Helpers.StringUtils.stringEqualsIgnoreCase(encoding, "UTF8")) {
                    return new Saxon.Hej.serialize.UTF8Writer(outputStream);
                } else {
                    System.IO.TextWriter writer = makeWriterFromOutputStream(stream);
                    return new Saxon.Hej.str.UnicodeWriterToWriter(writer);
                }
            } catch (System.Exception err) {
                if (Saxon.Impl.Helpers.StringUtils.stringEqualsIgnoreCase(encoding, "UTF8")) {
                    throw new Saxon.Hej.trans.XPathException("Failed to create a UTF8 output writer");
                }
                throw new Saxon.Hej.trans.XPathException((("Encoding "+encoding)+" is not supported"), "SESU0007");
            }
        }
        public System.IO.Stream getOutputStream() {
            return outputStream;
        }
        public Saxon.Hej.serialize.charcode.CharacterSet getCharacterSet() {
            return characterSet;
        }
    
    }
}