using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.str {
    public sealed class EmptyUnicodeString : Saxon.Hej.str.UnicodeString {
        private readonly static Saxon.Hej.str.EmptyUnicodeString INSTANCE = new Saxon.Hej.str.EmptyUnicodeString();
        public static Saxon.Hej.str.EmptyUnicodeString getInstance() {
            return INSTANCE;
        }
        private EmptyUnicodeString()  {
        }
        public override long length() {
            return 0;
        }
        public override int length32() {
            return 0;
        }
        public override Saxon.Hej.str.UnicodeString concat(Saxon.Hej.str.UnicodeString other) {
            return other;
        }
        internal override void copy8bit(byte[] target, int offset) {
        }
        internal override void copy16bit(char[] target, int offset) {
        }
        internal override void copy24bit(byte[] target, int offset) {
        }
        public override Saxon.Hej.str.UnicodeString substring(long start, long end) {
            checkSubstringBounds(start, end);
            return this;
        }
        public override int codePointAt(long index) {
            throw new System.IndexOutOfRangeException();
        }
        public override long indexOf(int code, long from) {
            return -1;
        }
        public override long indexOf(Saxon.Hej.str.UnicodeString other, long from) {
            return -1;
        }
        public override bool isEmpty() {
            return true;
        }
        public override int getWidth() {
            return 0;
        }
        public override Saxon.Hej.z.IntIterator codePoints() {
            return Saxon.Hej.expr.sort.EmptyIntIterator.getInstance();
        }
        public override int GetHashCode() {
            return 0;
        }
        public override bool Equals(System.Object o) {
            if ((o is Saxon.Hej.str.UnicodeString)) {
                return ((Saxon.Hej.str.UnicodeString)(o)).isEmpty();
            }
            return false;
        }
        public override int compareTo(Saxon.Hej.str.UnicodeString other) {
            return (other.isEmpty() ? 0 : -1);
        }
        public override string ToString() {
            return "";
        }
        public override long indexWhere(Saxon.Ejava.util.function.IntPredicate predicate, long from) {
            return -1;
        }
        public string details() {
            return "empty string";
        }
    
    }
}