using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.resource {
    public class EncodingDetector {
        public static string inferStreamEncoding(System.IO.Stream @is, Saxon.Hej.lib.Logger err) {
            Saxon.Hej.query.InputStreamMarker marker = new Saxon.Hej.query.InputStreamMarker(@is);
            marker.mark(100);
            byte[] start = new byte[100];
            int read = @is.Read(start, 0, 100);
            marker.reset();
            return inferEncoding(start, read, err);
        }
        private static string inferEncoding(byte[] start, int read, Saxon.Hej.lib.Logger logger) {
            bool debug = (logger != null);
            if ((read>=2)) {
                if (((ch(start[0]) == 0xFE) && (ch(start[1]) == 0xFF))) {
                    if (debug) {
                        logger.info("unparsed-text(): found UTF-16 byte order mark");
                    }
                    return "UTF-16";
                } else {
                    if (((ch(start[0]) == 0xFF) && (ch(start[1]) == 0xFE))) {
                        if (debug) {
                            logger.info("unparsed-text(): found UTF-16LE byte order mark");
                        }
                        return "UTF-16LE";
                    }
                }
            }
            if ((read>=3)) {
                if ((((ch(start[0]) == 0xEF) && (ch(start[1]) == 0xBB)) && (ch(start[2]) == 0xBF))) {
                    if (debug) {
                        logger.info("unparsed-text(): found UTF-8 byte order mark");
                    }
                    return "UTF-8";
                }
            }
            if ((read>=4)) {
                if ((((((ch(start[0]) == '<') && (ch(start[1]) == '?')) && (ch(start[2]) == 'x')) && (ch(start[3]) == 'm')) && (ch(start[4]) == 'l'))) {
                    if (debug) {
                        logger.info("unparsed-text(): found XML declaration");
                    }
                    Saxon.Ejava.lang.StringBuilder sb = new Saxon.Ejava.lang.StringBuilder(read);
                    for (int b = 0; (b<read); b++) {
                        sb.append((char)(start[b]));
                    }
                    string p = sb.ToString();
                    int v = p.IndexOf("encoding");
                    if ((v>=0)) {
                        v += 8;
                        while (((v<p.Length) && (" \n\r\t=\"'".IndexOf(p[v])>=0))) {
                            v++;
                        }
                        sb.setLength(0);
                        while ((((v<p.Length) && (p[v] != '"')) && (p[v] != '\''))) {
                            sb.append(p[v++]);
                        }
                        if (debug) {
                            logger.info(("unparsed-text(): encoding in XML declaration = "+sb.ToString()));
                        }
                        return sb.ToString();
                    }
                    if (debug) {
                        logger.info("unparsed-text(): no encoding found in XML declaration");
                    }
                }
            } else {
                if ((((((read>0) && (start[0] == 0)) && (start[2] == 0)) && (start[4] == 0)) && (start[6] == 0))) {
                    if (debug) {
                        logger.info("unparsed-text(): even-numbered bytes are zero, inferring UTF-16");
                    }
                    return "UTF-16";
                } else {
                    if ((((((read>1) && (start[1] == 0)) && (start[3] == 0)) && (start[5] == 0)) && (start[7] == 0))) {
                        if (debug) {
                            logger.info("unparsed-text(): odd-numbered bytes are zero, inferring UTF-16LE");
                        }
                        return "UTF-16LE";
                    }
                }
            }
            if (debug) {
                logger.info("unparsed-text(): assuming fallback encoding (UTF-8)");
            }
            return "UTF-8";
        }
        private static int ch(byte b) {
            return ((int)(b)&unchecked((int)0xff));
        }
    
    }
}