using Sys = System;
namespace Saxon.Ejava.text {
    public class CollationKey : Saxon.Ejava.lang.Comparable<CollationKey> {
        public CollationKey(string s) {
            
        }
        public virtual int compareTo(CollationKey other) {
            throw new System.NotImplementedException();
        }

        public virtual byte[] toByteArray() {
            throw new System.NotImplementedException();
        }
    }
}