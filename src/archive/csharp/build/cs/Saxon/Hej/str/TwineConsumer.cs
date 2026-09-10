using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.str {
    public interface TwineConsumer {
        void append8(byte[] bytes, int start, int len);
        void append16(char[] chars, int start, int len);
        void append24(byte[] bytes, int start, int len);
        void append(Saxon.Hej.z.IntIterator codePoints);
    
    }
}