using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public abstract class UnparsedTextFunction : Saxon.Hej.functions.SystemFunction {
        public override int getSpecialProperties(Saxon.Hej.expr.Expression[] arguments) {
            int p = base.getSpecialProperties(arguments);
            if (getRetainedStaticContext().getConfiguration().getBooleanProperty(Saxon.Hej.lib.Feature<bool>.STABLE_UNPARSED_TEXT)) {
                return p;
            } else {
                return (p&~(Saxon.Hej.expr.StaticProperty.NO_NODES_NEWLY_CREATED));
            }
        }
        public static void readFile(System.Uri absoluteURI, string encoding, Saxon.Hej.str.UniStringConsumer output, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.Configuration config = context.getConfiguration();
            Saxon.Hej.z.IntPredicateProxy checker = config.getValidCharacterChecker();
            System.IO.TextReader reader;
            try {
                reader = /*Delegate.resolve()*/context.getController().getUnparsedTextURIResolver()(absoluteURI, encoding, config);
            } catch (Saxon.Hej.trans.XPathException err) {
                err.maybeSetErrorCode("FOUT1170");
                throw;
            }
            try {
                readFile(checker, reader, output);
            } catch (Saxon.Ejava.io.UnsupportedEncodingException encErr) {
                Saxon.Hej.trans.XPathException e = new Saxon.Hej.trans.XPathException(("Unknown encoding "+Saxon.Hej.trans.Err.wrap(encoding)), encErr);
                e.setErrorCode("FOUT1190");
                throw e;
            } catch (System.IO.IOException ioErr) {
                throw handleIOError(absoluteURI, ioErr, context);
            }
        }
        public static System.Uri getAbsoluteURI(string href, string baseURI, Saxon.Hej.expr.XPathContext context) {
            System.Uri absoluteURI;
            try {
                absoluteURI = Saxon.Hej.functions.ResolveURI.makeAbsolute(href, baseURI);
            } catch (System.UriFormatException err) {
                handleURISyntaxException(href, baseURI, err);
                return null;
            }
            if ((Saxon.Impl.Helpers.StringUtils.nullIfEmpty(absoluteURI.Fragment) != null)) {
                Saxon.Hej.trans.XPathException e = new Saxon.Hej.trans.XPathException("URI for unparsed-text() must not contain a fragment identifier");
                e.setErrorCode("FOUT1170");
                throw e;
            }
            Saxon.Hej.functions.EncodeForUri.checkPercentEncoding(absoluteURI.ToString());
            return absoluteURI;
        }
        private static void handleURISyntaxException(string href, string baseURI, System.UriFormatException err) {
            handleURISyntaxExceptionCSharp(href, baseURI, err);
        }
        private static void handleURISyntaxExceptionCSharp(string href, string @base, System.UriFormatException err) {
            Saxon.Hej.trans.XPathException e = new Saxon.Hej.trans.XPathException(((((("Failed to resolve "+href)+" against ")+@base)+": ")+err.Message));
            e.setErrorCode("FOUT1170");
            throw e;
        }
        public static Saxon.Hej.trans.XPathException handleIOError(System.Uri absoluteURI, System.IO.IOException ioErr, Saxon.Hej.expr.XPathContext context) {
            string message = "Failed to read input file";
            if (((absoluteURI != null) && !(ioErr.Message.Equals(absoluteURI.ToString())))) {
                message += (' '+absoluteURI.ToString());
            }
            message += ((" ("+ioErr.GetType().Name)+')');
            Saxon.Hej.trans.XPathException e = new Saxon.Hej.trans.XPathException(message, ioErr);
            string errorCode = getErrorCode(ioErr);
            e.setErrorCode(errorCode);
            return e;
        }
        private static string getErrorCode(System.IO.IOException ioErr) {
            return "FOUT1170";
        }
        public static Saxon.Hej.str.UnicodeString readFile(Saxon.Hej.z.IntPredicateProxy checker, System.IO.TextReader reader) {
            Saxon.Hej.str.UnicodeBuilder buffer = new Saxon.Hej.str.UnicodeBuilder();
            readFile(checker, reader, new Inner_AbstractUniStringConsumer_d1231e797(buffer));
            return buffer.toUnicodeString();
        }
        public static void readFile(Saxon.Hej.z.IntPredicateProxy checker, System.IO.TextReader reader, Saxon.Hej.str.UniStringConsumer output) {
            char[] buffer = new char[2048];
            bool first = true;
            int actual;
            int line = 1;
            int column = 1;
            int mask = 0;
            while (true) {
                try {
                    actual = reader.Read(buffer, 0, buffer.Length);
                } catch (/* substitute exception */ System.Text.DecoderFallbackException e) {
                    throw new System.IO.IOException(e.Message, e);
                }
                if (isEndOfFile(actual)) {
                    break ;
                }
                for (int c = 0; (c<actual); ) {
                    int ch32 = buffer[c++];
                    if ((ch32 == '\n')) {
                        line++;
                        column = 0;
                    }
                    column++;
                    mask |= ch32;
                    if (Saxon.Hej.serialize.charcode.UTF16CharacterSet.isHighSurrogate(ch32)) {
                        if ((c == actual)) {
                            char[] buffer2 = new char[2048];
                            int actual2 = reader.Read(buffer2, 0, 2048);
                            char[] buffer3 = new char[(actual+actual2)];
                            Saxon.Ejava.lang.System.arraycopy(buffer, 0, buffer3, 0, actual);
                            Saxon.Ejava.lang.System.arraycopy(buffer2, 0, buffer3, actual, actual2);
                            buffer = buffer3;
                            actual = (actual+actual2);
                        }
                        char low = buffer[c++];
                        ch32 = Saxon.Hej.serialize.charcode.UTF16CharacterSet.combinePair((char)(ch32), low);
                        mask |= ch32;
                    }
                    if (!(checker.test(ch32))) {
                        Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException((((((("The text file contains a character that is illegal in XML (line="+line)+" column=")+column)+" value=hex ")+Saxon.Impl.Helpers.Utils.intToHexString(ch32))+')'));
                        err.setErrorCode("FOUT1190");
                        throw err;
                    }
                }
                int start = 0;
                int len = actual;
                if (first) {
                    if ((buffer[0] == '\ufeff')) {
                        start = 1;
                        actual--;
                    }
                    first = false;
                }
                if ((mask<=unchecked((int)0xff))) {
                    output.accept(new Saxon.Hej.str.Twine8(buffer, start, actual));
                } else {
                    if ((mask<=unchecked((int)0xffff))) {
                        output.accept(new Saxon.Hej.str.Twine16(buffer, start, actual));
                    } else {
                        output.accept(Saxon.Hej.str.StringView.of(new string(buffer, start, len)));
                    }
                }
            }
            reader.Close();
        }
        private static bool isEndOfFile(int bytesRead) {
            return bytesRead == 0;
        }
    
        private class Inner_AbstractUniStringConsumer_d1231e797 : Saxon.Hej.str.AbstractUniStringConsumer {
            public Inner_AbstractUniStringConsumer_d1231e797(Saxon.Hej.str.UnicodeBuilder buffer) {
                this.buffer = buffer;
            }
            private Saxon.Hej.str.UnicodeBuilder buffer;
            public override  /* REPLACING public */ Saxon.Hej.str.UniStringConsumer accept(Saxon.Hej.str.UnicodeString chars) {
                return ((/*1430*/Saxon.Hej.str.UnicodeBuilder)/*extraParam*/buffer.accept(/*localparam*/chars));
            }
        }

    }
}