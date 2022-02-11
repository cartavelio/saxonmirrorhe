using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.serialize.charcode {
    public sealed class UTF8CharacterSet : Saxon.Hej.serialize.charcode.CharacterSet {
        private static readonly Saxon.Hej.serialize.charcode.UTF8CharacterSet theInstance = new Saxon.Hej.serialize.charcode.UTF8CharacterSet();
        private UTF8CharacterSet()  {
        }
        public static Saxon.Hej.serialize.charcode.UTF8CharacterSet getInstance() {
            return theInstance;
        }
        public bool inCharset(int c) {
            return true;
        }
        public string getCanonicalName() {
            return "UTF-8";
        }
        public static int getUTF8Encoding(char @in, char in2, byte[] @out) {
            int i = @in;
            if ((i<=0x7f)) {
                @out[0] = (byte)(i);
                return 1;
            } else {
                if ((i<=0x7ff)) {
                    @out[0] = (byte)(((unchecked((int)0xc0)|((((@in>>6))&0x1f)))));
                    @out[1] = (byte)(((unchecked((int)0x80)|((@in&0x3f)))));
                    return 2;
                } else {
                    if (((i>=unchecked((int)0xd800)) && (i<=unchecked((int)0xdbff)))) {
                        int j = in2;
                        if (!((((j>=unchecked((int)0xdc00)) && (j<=unchecked((int)0xdfff)))))) {
                            throw new Saxon.Ejava.lang.IllegalArgumentException((((("Malformed Unicode Surrogate Pair ("+i)+',')+j)+')'));
                        }
                        byte xxxxxx = (byte)(((j&0x3f)));
                        byte yyyyyy = (byte)(((((((i&0x03))<<4))|((((j>>6))&0x0f)))));
                        byte zzzz = (byte)(((((i>>2))&0x0f)));
                        byte uuuuu = (byte)(((((((i>>6))&0x0f))+1)));
                        @out[0] = (byte)(((unchecked((int)0xf0)|((((uuuuu>>2))&0x07)))));
                        @out[1] = (byte)((((unchecked((int)0x80)|((((uuuuu&0x03))<<4)))|zzzz)));
                        @out[2] = (byte)(((unchecked((int)0x80)|yyyyyy)));
                        @out[3] = (byte)(((unchecked((int)0x80)|xxxxxx)));
                        return 4;
                    } else {
                        if (((i>=unchecked((int)0xdc00)) && (i<=unchecked((int)0xdfff)))) {
                            return 0;
                        } else {
                            @out[0] = (byte)(((unchecked((int)0xe0)|((((@in>>12))&0x0f)))));
                            @out[1] = (byte)(((unchecked((int)0x80)|((((@in>>6))&0x3f)))));
                            @out[2] = (byte)(((unchecked((int)0x80)|((@in&0x3f)))));
                            return 3;
                        }
                    }
                }
            }
        }
        public static byte[] encode(Saxon.Hej.z.IntIterator codePoints) {
            System.IO.MemoryStream baos = new System.IO.MemoryStream();
            byte[] cpBytes = new byte[6];
            while (codePoints.hasNext()) {
                int cp = codePoints.next();
                if ((cp<0)) {
                    throw new Saxon.Ejava.lang.IllegalStateException("No negative code point allowed");
                } else {
                    if ((cp<unchecked((int)0x80))) {
                        baos.WriteByte((byte)(byte)(cp));
                    } else {
                        int bi = 0;
                        int lastPrefix = 0xC0;
                        int lastMask = 0x1F;
                        for (; ; ) {
                            int b = (unchecked((int)0x80)|((cp&0x3F)));
                            cpBytes[bi] = (byte)(b);
                            ++bi;
                            cp >>= 6;
                            if ((((cp&~(lastMask))) == 0)) {
                                cpBytes[bi] = (byte)(((lastPrefix|cp)));
                                ++bi;
                                break ;
                            }
                            lastPrefix = (unchecked((int)0x80)|((lastPrefix>>1)));
                            lastMask >>= 1;
                        }
                        while ((bi>0)) {
                            --bi;
                            baos.WriteByte((byte)cpBytes[bi]);
                        }
                    }
                }
            }
            return baos.ToArray();
        }
        public static int decodeUTF8(byte[] @in, int used) {
            int bottom = 0;
            for (int i = 1; (i<used); i++) {
                if ((((@in[i]&unchecked((int)0xc0))) != unchecked((int)0x80))) {
                    throw new Saxon.Ejava.lang.IllegalArgumentException((("Byte "+((i+1)))+" in UTF-8 sequence has wrong top bits"));
                }
                bottom = (((bottom<<6))+((@in[i]&0x3f)));
            }
            if ((((@in[0]&unchecked((int)0x80))) == 0)) {
                if ((used == 1)) {
                    return @in[0];
                } else {
                    throw new Saxon.Ejava.lang.IllegalArgumentException("UTF8 single byte expected");
                }
            } else {
                if ((((@in[0]&unchecked((int)0xe0))) == unchecked((int)0xc0))) {
                    if ((used != 2)) {
                        throw new Saxon.Ejava.lang.IllegalArgumentException("UTF8 sequence of two bytes expected");
                    }
                    return (((((@in[0]&0x1f))<<6))+bottom);
                } else {
                    if ((((@in[0]&unchecked((int)0xf0))) == unchecked((int)0xe0))) {
                        if ((used != 3)) {
                            throw new Saxon.Ejava.lang.IllegalArgumentException("UTF8 sequence of three bytes expected");
                        }
                        return (((((@in[0]&0x0f))<<12))+bottom);
                    } else {
                        if ((((@in[0]&unchecked((int)0xf8))) == unchecked((int)0xf8))) {
                            if ((used != 4)) {
                                throw new Saxon.Ejava.lang.IllegalArgumentException("UTF8 sequence of four bytes expected");
                            }
                            return (((((@in[0]&0x07))<<24))+bottom);
                        } else {
                            throw new Saxon.Ejava.lang.IllegalArgumentException("UTF8 invalid first byte");
                        }
                    }
                }
            }
        }
    
    }
}