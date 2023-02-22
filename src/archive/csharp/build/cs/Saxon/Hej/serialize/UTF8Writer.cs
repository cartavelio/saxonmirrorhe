using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.serialize {
    public sealed class UTF8Writer : System.IO.TextWriter, Saxon.Hej.str.UnicodeWriter {
    // Injecting C# code
        public override System.Text.Encoding Encoding { get { return System.Text.Encoding.UTF8; } }
    // End of injected C# code
        private const int MIN_BUF_LEN = 32;
        private const int DEFAULT_BUF_LEN = 4000;
        const int SURR1_FIRST = 0xD800;
        const int SURR1_LAST = 0xDBFF;
        const int SURR2_FIRST = 0xDC00;
        const int SURR2_LAST = 0xDFFF;
        private System.IO.Stream _out;
        private byte[] _outBuffer;
        readonly private int _outBufferLast;
        private int _outPtr;
        internal int _surrogate = 0;
        public UTF8Writer(System.IO.Stream @out) : this (@out, DEFAULT_BUF_LEN) {
        }
        public UTF8Writer(System.IO.Stream @out, int bufferLength)  {
            if ((bufferLength<MIN_BUF_LEN)) {
                bufferLength = MIN_BUF_LEN;
            }
            _out = @out;
            _outBuffer = new byte[bufferLength];
            _outBufferLast = (bufferLength-4);
            _outPtr = 0;
        }
        public void close() {
            if ((_out != null)) {
                _flushBuffer();
                _outBuffer = null;
                _out.Close();
                _out = null;
                if ((_surrogate != 0)) {
                    int code = _surrogate;
                    _surrogate = 0;
                    throwIllegal(code);
                }
            }
        }
        public void flush() {
            _flushBuffer();
            _out.Flush();
        }
        public void write(char[] cbuf) {
            write(cbuf, 0, cbuf.Length);
        }
        public void write(char[] cbuf, int off, int len) {
            System.Diagnostics.Debug.Assert(((off+len)<=cbuf.Length));
            if ((len<2)) {
                if ((len == 1)) {
                    write(cbuf[off]);
                }
                return ;
            }
            if ((_surrogate>0)) {
                char second = cbuf[off++];
                --len;
                write(_convertSurrogate(second));
            }
            int outPtr = _outPtr;
            byte[] outBuf = _outBuffer;
            int outBufLast = _outBufferLast;
            len += off;
            while ((off<len)) {
                if ((outPtr>=outBufLast)) {
                    _out.Write(outBuf, 0, outPtr);
                    outPtr = 0;
                }
                int c = cbuf[off++];
                if ((c<unchecked((int)0x80))) {
                    outBuf[outPtr++] = (byte)(c);
                    int maxInCount = ((len-off));
                    int maxOutCount = ((outBufLast-outPtr));
                    if ((maxInCount>maxOutCount)) {
                        maxInCount = maxOutCount;
                    }
                    maxInCount += off;
                    bool continueOuter = false;
                    while (true) {
                        if ((off>=maxInCount)) {
                            continueOuter = true;
                            break ;
                        }
                        c = cbuf[off++];
                        if ((c>=unchecked((int)0x80))) {
                            break ;
                        }
                        outBuf[outPtr++] = (byte)(c);
                    }
                    if (continueOuter) {
                        continue ;
                    }
                }
                if ((c<unchecked((int)0x800))) {
                    outBuf[outPtr++] = (byte)(((unchecked((int)0xc0)|((c>>6)))));
                    outBuf[outPtr++] = (byte)(((unchecked((int)0x80)|((c&0x3f)))));
                } else {
                    if (((c<SURR1_FIRST) || (c>SURR2_LAST))) {
                        outBuf[outPtr++] = (byte)(((unchecked((int)0xe0)|((c>>12)))));
                        outBuf[outPtr++] = (byte)(((unchecked((int)0x80)|((((c>>6))&0x3f)))));
                        outBuf[outPtr++] = (byte)(((unchecked((int)0x80)|((c&0x3f)))));
                        continue ;
                    }
                    if ((c>SURR1_LAST)) {
                        _outPtr = outPtr;
                        throwIllegal(c);
                    }
                    _surrogate = c;
                    if ((off>=len)) {
                        break ;
                    }
                    c = _convertSurrogate(cbuf[off++]);
                    if ((c>0x10FFFF)) {
                        _outPtr = outPtr;
                        throwIllegal(c);
                    }
                    outBuf[outPtr++] = (byte)(((unchecked((int)0xf0)|((c>>18)))));
                    outBuf[outPtr++] = (byte)(((unchecked((int)0x80)|((((c>>12))&0x3f)))));
                    outBuf[outPtr++] = (byte)(((unchecked((int)0x80)|((((c>>6))&0x3f)))));
                    outBuf[outPtr++] = (byte)(((unchecked((int)0x80)|((c&0x3f)))));
                }
            }
            _outPtr = outPtr;
        }
        public void writeLatin1(byte[] bytes, int off, int len) {
            System.Diagnostics.Debug.Assert(((off+len)<=bytes.Length));
            int outPtr = _outPtr;
            byte[] outBuf = _outBuffer;
            int outBufLast = _outBufferLast;
            len += off;
            while ((off<len)) {
                if ((outPtr>=outBufLast)) {
                    _out.Write(outBuf, 0, outPtr);
                    outPtr = 0;
                }
                int c = (bytes[off++]&unchecked((int)0xff));
                if ((c<unchecked((int)0x80))) {
                    outBuf[outPtr++] = (byte)(c);
                    int maxInCount = ((len-off));
                    int maxOutCount = ((outBufLast-outPtr));
                    if ((maxInCount>maxOutCount)) {
                        maxInCount = maxOutCount;
                    }
                    maxInCount += off;
                    bool continueOuter = false;
                    while (true) {
                        if ((off>=maxInCount)) {
                            continueOuter = true;
                            break ;
                        }
                        c = (bytes[off++]&unchecked((int)0xff));
                        if ((c>=unchecked((int)0x80))) {
                            break ;
                        }
                        outBuf[outPtr++] = (byte)(c);
                    }
                    if (continueOuter) {
                        continue ;
                    }
                }
                outBuf[outPtr++] = (byte)(((unchecked((int)0xc0)|((c>>6)))));
                outBuf[outPtr++] = (byte)(((unchecked((int)0x80)|((c&0x3f)))));
            }
            _outPtr = outPtr;
        }
        public void writeAscii(byte[] content) {
            writeAscii(content, 0, content.Length);
        }
        public void writeAscii(byte[] chars, int off, int len) {
            int outPtr = _outPtr;
            byte[] outBuf = _outBuffer;
            int outBufLast = _outBufferLast;
            while ((len>0)) {
                if ((outPtr>=outBufLast)) {
                    _out.Write(outBuf, 0, outPtr);
                    outPtr = 0;
                }
                int available = (outBufLast-outPtr);
                int count = System.Math.Min(len, available);
                Saxon.Ejava.lang.System.arraycopy(chars, off, outBuf, outPtr, count);
                outPtr += count;
                off += count;
                len -= count;
            }
            _outPtr = outPtr;
        }
        public void writeRepeatedAscii(byte ch, int repeat) {
            int outPtr = _outPtr;
            byte[] outBuf = _outBuffer;
            int outBufLast = _outBufferLast;
            while ((repeat>0)) {
                if ((outPtr>=outBufLast)) {
                    _out.Write(outBuf, 0, outPtr);
                    outPtr = 0;
                }
                int available = (outBufLast-outPtr);
                int count = System.Math.Min(repeat, available);
                Saxon.Ejava.util.Arrays.fill(outBuf, outPtr, (outPtr+count), ch);
                outPtr += count;
                repeat -= count;
            }
            _outPtr = outPtr;
        }
        public void writeCodePoint(int codepoint) {
            write(codepoint);
        }
        public void write(int c) {
            if ((_surrogate>0)) {
                c = _convertSurrogate(c);
            } else {
                if (((c>=SURR1_FIRST) && (c<=SURR2_LAST))) {
                    if ((c>SURR1_LAST)) {
                        throwIllegal(c);
                    }
                    _surrogate = c;
                    return ;
                }
            }
            if ((_outPtr>=_outBufferLast)) {
                _flushBuffer();
            }
            if ((c<unchecked((int)0x80))) {
                _outBuffer[_outPtr++] = (byte)(c);
            } else {
                int ptr = _outPtr;
                if ((c<unchecked((int)0x800))) {
                    _outBuffer[ptr++] = (byte)(((unchecked((int)0xc0)|((c>>6)))));
                    _outBuffer[ptr++] = (byte)(((unchecked((int)0x80)|((c&0x3f)))));
                } else {
                    if ((c<=0xFFFF)) {
                        _outBuffer[ptr++] = (byte)(((unchecked((int)0xe0)|((c>>12)))));
                        _outBuffer[ptr++] = (byte)(((unchecked((int)0x80)|((((c>>6))&0x3f)))));
                        _outBuffer[ptr++] = (byte)(((unchecked((int)0x80)|((c&0x3f)))));
                    } else {
                        if ((c>0x10FFFF)) {
                            throwIllegal(c);
                        }
                        _outBuffer[ptr++] = (byte)(((unchecked((int)0xf0)|((c>>18)))));
                        _outBuffer[ptr++] = (byte)(((unchecked((int)0x80)|((((c>>12))&0x3f)))));
                        _outBuffer[ptr++] = (byte)(((unchecked((int)0x80)|((((c>>6))&0x3f)))));
                        _outBuffer[ptr++] = (byte)(((unchecked((int)0x80)|((c&0x3f)))));
                    }
                }
                _outPtr = ptr;
            }
        }
        public void write(Saxon.Hej.str.UnicodeString chars) {
            if (((chars is Saxon.Hej.str.StringView) || (chars is Saxon.Hej.str.BMPString))) {
                write(chars.ToString());
            } else {
                if ((chars is Saxon.Hej.str.UnicodeChar)) {
                    writeCodePoint(((Saxon.Hej.str.UnicodeChar)(chars)).getCodepoint());
                } else {
                    if ((chars is Saxon.Hej.str.ZenoString)) {
                        ((Saxon.Hej.str.ZenoString)(chars)).writeSegments(this);
                    } else {
                        if ((chars.getWidth()<=8)) {
                            if ((chars is Saxon.Hej.str.Twine8)) {
                                int width = chars.getWidth();
                                if ((width == 7)) {
                                    writeAscii(((Saxon.Hej.str.Twine8)(chars)).getByteArray(), 0, chars.length32());
                                } else {
                                    if ((width == 8)) {
                                        writeLatin1(((Saxon.Hej.str.Twine8)(chars)).getByteArray(), 0, chars.length32());
                                    }
                                }
                            } else {
                                if ((chars is Saxon.Hej.str.Slice8)) {
                                    Saxon.Hej.str.Slice8 s8 = (Saxon.Hej.str.Slice8)(chars);
                                    int width = chars.getWidth();
                                    if ((width == 7)) {
                                        writeAscii(s8.getByteArray(), s8.getStart(), (s8.getEnd()-s8.getStart()));
                                    } else {
                                        if ((width == 8)) {
                                            writeLatin1(s8.getByteArray(), s8.getStart(), (s8.getEnd()-s8.getStart()));
                                        }
                                    }
                                } else {
                                    if ((chars is Saxon.Hej.str.WhitespaceString)) {
                                        ((Saxon.Hej.str.WhitespaceString)(chars)).write(this);
                                    } else {
                                        Saxon.Hej.z.IntIterator iter = chars.codePoints();
                                        while (iter.hasNext()) {
                                            write(iter.next());
                                        }
                                    }
                                }
                            }
                        } else {
                            if ((chars.getWidth() == 16)) {
                                if ((chars is Saxon.Hej.str.Twine16)) {
                                    write(((Saxon.Hej.str.Twine16)(chars)).getCharArray());
                                } else {
                                    if ((chars is Saxon.Hej.str.Slice16)) {
                                        Saxon.Hej.str.Slice16 s16 = (Saxon.Hej.str.Slice16)(chars);
                                        write(s16.getCharArray(), s16.getStart(), (s16.getEnd()-s16.getStart()));
                                    }
                                }
                            } else {
                                Saxon.Hej.z.IntIterator iter = chars.codePoints();
                                while (iter.hasNext()) {
                                    write(iter.next());
                                }
                            }
                        }
                    }
                }
            }
        }
        public void write(string str) {
            write(str, 0, str.Length);
        }
        public void write(string str, int off, int len) {
            if ((len<2)) {
                if ((len == 1)) {
                    write(str[off]);
                }
                return ;
            }
            if ((_surrogate>0)) {
                char second = str[off++];
                --len;
                write(_convertSurrogate(second));
            }
            int outPtr = _outPtr;
            byte[] outBuf = _outBuffer;
            int outBufLast = _outBufferLast;
            len += off;
            while ((off<len)) {
                if ((outPtr>=outBufLast)) {
                    _out.Write(outBuf, 0, outPtr);
                    outPtr = 0;
                }
                int c = str[off++];
                if ((c<unchecked((int)0x80))) {
                    outBuf[outPtr++] = (byte)(c);
                    int maxInCount = ((len-off));
                    int maxOutCount = ((outBufLast-outPtr));
                    if ((maxInCount>maxOutCount)) {
                        maxInCount = maxOutCount;
                    }
                    maxInCount += off;
                    bool continueOuter = false;
                    while (true) {
                        if ((off>=maxInCount)) {
                            continueOuter = true;
                            break ;
                        }
                        c = str[off++];
                        if ((c>=unchecked((int)0x80))) {
                            break ;
                        }
                        outBuf[outPtr++] = (byte)(c);
                    }
                    if (continueOuter) {
                        continue ;
                    }
                }
                if ((c<unchecked((int)0x800))) {
                    outBuf[outPtr++] = (byte)(((unchecked((int)0xc0)|((c>>6)))));
                    outBuf[outPtr++] = (byte)(((unchecked((int)0x80)|((c&0x3f)))));
                } else {
                    if (((c<SURR1_FIRST) || (c>SURR2_LAST))) {
                        outBuf[outPtr++] = (byte)(((unchecked((int)0xe0)|((c>>12)))));
                        outBuf[outPtr++] = (byte)(((unchecked((int)0x80)|((((c>>6))&0x3f)))));
                        outBuf[outPtr++] = (byte)(((unchecked((int)0x80)|((c&0x3f)))));
                        continue ;
                    }
                    if ((c>SURR1_LAST)) {
                        _outPtr = outPtr;
                        throwIllegal(c);
                    }
                    _surrogate = c;
                    if ((off>=len)) {
                        break ;
                    }
                    c = _convertSurrogate(str[off++]);
                    if ((c>0x10FFFF)) {
                        _outPtr = outPtr;
                        throwIllegal(c);
                    }
                    outBuf[outPtr++] = (byte)(((unchecked((int)0xf0)|((c>>18)))));
                    outBuf[outPtr++] = (byte)(((unchecked((int)0x80)|((((c>>12))&0x3f)))));
                    outBuf[outPtr++] = (byte)(((unchecked((int)0x80)|((((c>>6))&0x3f)))));
                    outBuf[outPtr++] = (byte)(((unchecked((int)0x80)|((c&0x3f)))));
                }
            }
            _outPtr = outPtr;
        }
        private void _flushBuffer() {
            if (((_outPtr>0) && (_outBuffer != null))) {
                _out.Write(_outBuffer, 0, _outPtr);
                _outPtr = 0;
            }
        }
        private int _convertSurrogate(int secondPart) {
            int firstPart = _surrogate;
            _surrogate = 0;
            if (((secondPart<SURR2_FIRST) || (secondPart>SURR2_LAST))) {
                throw new System.IO.IOException((((("Broken surrogate pair: first char 0x"+Saxon.Impl.Helpers.Utils.intToHexString(firstPart))+", second 0x")+Saxon.Impl.Helpers.Utils.intToHexString(secondPart))+"; illegal combination"));
            }
            return ((0x10000+((((firstPart-SURR1_FIRST))<<10)))+((secondPart-SURR2_FIRST)));
        }
        private void throwIllegal(int code) {
            if ((code>0x10FFFF)) {
                throw new System.IO.IOException((("Illegal character point (0x"+Saxon.Impl.Helpers.Utils.intToHexString(code))+") to output; max is 0x10FFFF as per RFC 3629"));
            }
            if ((code>=SURR1_FIRST)) {
                if ((code<=SURR1_LAST)) {
                    throw new System.IO.IOException((("Unmatched first part of surrogate pair (0x"+Saxon.Impl.Helpers.Utils.intToHexString(code))+")"));
                }
                throw new System.IO.IOException((("Unmatched second part of surrogate pair (0x"+Saxon.Impl.Helpers.Utils.intToHexString(code))+")"));
            }
            throw new System.IO.IOException((("Illegal character point (0x"+Saxon.Impl.Helpers.Utils.intToHexString(code))+") to output"));
        }
    
    }
}