using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.query {
    public class QueryReader {
        private QueryReader()  {
        }
        public static string readSourceQuery(Saxon.Ejavax.xml.transform.stream.StreamSource ss, Saxon.Hej.z.IntPredicateProxy charChecker) {
            string queryText;
            if ((ss.getInputStream() != null)) {
                System.IO.Stream @is = ss.getInputStream();
                @is = Saxon.Hej.query.InputStreamMarker.ensureMarkSupported(@is);
                string encoding = readEncoding(@is);
                queryText = readInputStream(@is, encoding, charChecker);
            } else {
                if ((ss.getReader() != null)) {
                    queryText = readQueryFromReader(ss.getReader(), charChecker);
                } else {
                    throw new Saxon.Hej.trans.XPathException("Module URI Resolver must supply either an InputStream or a Reader");
                }
            }
            return queryText.ToString();
        }
        public static string readEncoding(System.IO.Stream @is) {
            try {
                Saxon.Hej.query.InputStreamMarker marker = new Saxon.Hej.query.InputStreamMarker(@is);
                if (!(marker.isMarkSupported())) {
                    throw new Saxon.Ejava.lang.IllegalArgumentException("InputStream must have markSupported() = true");
                }
                marker.mark(100);
                byte[] start = new byte[100];
                int read = @is.Read(start, 0, 100);
                if ((read == -1)) {
                    throw new Saxon.Hej.trans.XPathException("Query source file is empty");
                }
                marker.reset();
                return inferEncoding(start, read);
            } catch (System.IO.IOException e) {
                throw new Saxon.Hej.trans.XPathException("Failed to read query source file", e);
            }
        }
        public static string readInputStream(System.IO.Stream @is, string encoding, Saxon.Hej.z.IntPredicateProxy nameChecker) {
            if ((encoding == null)) {
                @is = Saxon.Hej.query.InputStreamMarker.ensureMarkSupported(@is);
                encoding = readEncoding(@is);
            }
            try {
                System.IO.TextReader reader = new System.IO.StreamReader(@is, System.Text.Encoding.GetEncoding(encoding));
                return readQueryFromReader(reader, nameChecker);
            } catch (Saxon.Ejava.io.UnsupportedEncodingException encErr) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(("Unknown encoding "+Saxon.Hej.trans.Err.wrap(encoding)), encErr);
                err.setErrorCode("XQST0087");
                throw err;
            }
        }
        private static string readQueryFromReader(System.IO.TextReader reader, Saxon.Hej.z.IntPredicateProxy charChecker) {
            try {
                Saxon.Hej.str.UnicodeString content = Saxon.Hej.functions.UnparsedTextFunction.readFile(charChecker, reader);
                return content.ToString();
            } catch (Saxon.Hej.trans.XPathException err) {
                err.setErrorCode("XPST0003");
                err.setIsStaticError(true);
                throw;
            } catch (System.IO.IOException ioErr) {
                throw new Saxon.Hej.trans.XPathException("Failed to read supplied query file", ioErr);
            }
        }
        private static string inferEncoding(byte[] start, int read) {
            if ((read>=2)) {
                if (((ch(start[0]) == 0xFE) && (ch(start[1]) == 0xFF))) {
                    return "UTF-16";
                } else {
                    if (((ch(start[0]) == 0xFF) && (ch(start[1]) == 0xFE))) {
                        return "UTF-16LE";
                    }
                }
            }
            if ((read>=3)) {
                if ((((ch(start[0]) == 0xEF) && (ch(start[1]) == 0xBB)) && (ch(start[2]) == 0xBF))) {
                    return "UTF-8";
                }
            }
            if ((((((read>=8) && (start[0] == 0)) && (start[2] == 0)) && (start[4] == 0)) && (start[6] == 0))) {
                return "UTF-16";
            }
            if ((((((read>=8) && (start[1] == 0)) && (start[3] == 0)) && (start[5] == 0)) && (start[7] == 0))) {
                return "UTF-16LE";
            }
            int i = 0;
            string tok = readToken(start, i, read);
            if (Saxon.Hej.value.Whitespace.trim(tok).Equals("xquery")) {
                i += tok.Length;
            } else {
                return "UTF-8";
            }
            tok = readToken(start, i, read);
            if (Saxon.Hej.value.Whitespace.trim(tok).Equals("encoding")) {
                i += tok.Length;
            } else {
                if (Saxon.Hej.value.Whitespace.trim(tok).Equals("version")) {
                    i += tok.Length;
                } else {
                    return "UTF-8";
                }
                tok = readToken(start, i, read);
                i += tok.Length;
                tok = readToken(start, i, read);
                if (Saxon.Hej.value.Whitespace.trim(tok).Equals("encoding")) {
                    i += tok.Length;
                } else {
                    return "UTF-8";
                }
            }
            tok = Saxon.Hej.value.Whitespace.trim(readToken(start, i, read));
            if (((tok.StartsWith("\"") && tok.EndsWith("\"")) && (tok.Length>2))) {
                return tok.Substring(1, (tok.Length-1) - 1);
            } else {
                if (((tok.StartsWith("'") && tok.EndsWith("'")) && (tok.Length>2))) {
                    return tok.Substring(1, (tok.Length-1) - 1);
                } else {
                    throw new Saxon.Hej.trans.XPathException((("Unrecognized encoding "+Saxon.Hej.trans.Err.wrap(tok))+" in query prolog"));
                }
            }
        }
        private static string readToken(byte[] @in, int i, int len) {
            int p = i;
            while (((p<len) && (" \n\r\t".IndexOf(ch(@in[p]))>=0))) {
                p++;
            }
            if ((ch(@in[p]) == '"')) {
                p++;
                while (((p<len) && (ch(@in[p]) != '"'))) {
                    p++;
                }
            } else {
                if ((ch(@in[p]) == '\'')) {
                    p++;
                    while (((p<len) && (ch(@in[p]) != '\''))) {
                        p++;
                    }
                } else {
                    while (((p<len) && (" \n\r\t".IndexOf(ch(@in[p]))<0))) {
                        p++;
                    }
                }
            }
            if ((p>=len)) {
                return fromAsciiByteArray(@in, i, (len-i));
            }
            Saxon.Ejava.lang.StringBuilder sb = new Saxon.Ejava.lang.StringBuilder(((p-i)+1));
            for (int c = i; (c<=p); c++) {
                sb.append((char)(ch(@in[c])));
            }
            return sb.ToString();
        }
        private static string fromAsciiByteArray(byte[] bytes, int start, int length) {
            return System.Text.Encoding.ASCII.GetString(bytes, start, length);
        }
        private static char ch(byte b) {
            return (char)(((b&unchecked((int)0xff))));
        }
    
    }
}