using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.serialize {
    public class HTMLTagHashSet {
        internal string[] strings;
        internal int size;
        public HTMLTagHashSet(int size)  {
            strings = new string[size];
            this.size = size;
        }
        public void add(string s) {
            int hash = (((hashCode(s)&0x7fffffff))%size);
            while (true) {
                if ((strings[hash] == null)) {
                    strings[hash] = s;
                    return ;
                }
                if (Saxon.Impl.Helpers.StringUtils.stringEqualsIgnoreCase(strings[hash], s)) {
                    return ;
                }
                hash = (((hash+1))%size);
            }
        }
        public bool contains(string s) {
            int hash = (((hashCode(s)&0x7fffffff))%size);
            while (true) {
                if ((strings[hash] == null)) {
                    return false;
                }
                if (Saxon.Impl.Helpers.StringUtils.stringEqualsIgnoreCase(strings[hash], s)) {
                    return true;
                }
                hash = (((hash+1))%size);
            }
        }
        private int hashCode(string s) {
            int hash = 0;
            int limit = s.Length;
            if ((limit>24)) {
                limit = 24;
            }
            for (int i = 0; (i<limit); i++) {
                hash = (((hash<<1))+((s[i]&unchecked((int)0xdf))));
            }
            return hash;
        }
    
    }
}