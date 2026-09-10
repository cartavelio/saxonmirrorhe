using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.serialize.charcode {
    public class UTF16CharacterSet : Saxon.Hej.serialize.charcode.CharacterSet {
        private static readonly Saxon.Hej.serialize.charcode.UTF16CharacterSet theInstance = new Saxon.Hej.serialize.charcode.UTF16CharacterSet();
        private UTF16CharacterSet()  {
        }
        public static Saxon.Hej.serialize.charcode.UTF16CharacterSet getInstance() {
            return theInstance;
        }
        public bool inCharset(int c) {
            return true;
        }
        public string getCanonicalName() {
            return "UTF-16";
        }
        public const int NONBMP_MIN = 0x10000;
        public const int NONBMP_MAX = 0x10FFFF;
        public static readonly char SURROGATE1_MIN = (char)(0xD800);
        public static readonly char SURROGATE1_MAX = (char)(0xDBFF);
        public static readonly char SURROGATE2_MIN = (char)(0xDC00);
        public static readonly char SURROGATE2_MAX = (char)(0xDFFF);
        public static int combinePair(char high, char low) {
            return (((((high-SURROGATE1_MIN))*0x400)+((low-SURROGATE2_MIN)))+NONBMP_MIN);
        }
        public static char highSurrogate(int ch) {
            return (char)(((((((ch-NONBMP_MIN))>>10))+SURROGATE1_MIN)));
        }
        public static char lowSurrogate(int ch) {
            return (char)(((((((ch-NONBMP_MIN))&0x3FF))+SURROGATE2_MIN)));
        }
        public static bool isSurrogate(int c) {
            return (((c&0xF800)) == 0xD800);
        }
        public static bool isHighSurrogate(int ch) {
            return (((SURROGATE1_MIN<=ch) && (ch<=SURROGATE1_MAX)));
        }
        public static bool isLowSurrogate(int ch) {
            return (((SURROGATE2_MIN<=ch) && (ch<=SURROGATE2_MAX)));
        }
        public static int firstInvalidChar(Saxon.Hej.z.IntIterator iter, Saxon.Hej.z.IntPredicateProxy predicate) {
            while (iter.hasNext()) {
                int ch32 = iter.next();
                if (!(predicate.test(ch32))) {
                    return ch32;
                }
            }
            return -1;
        }
    
    }
}