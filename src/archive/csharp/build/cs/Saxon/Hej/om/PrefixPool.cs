using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.om {
    public class PrefixPool {
        internal string[] prefixes = new string[8];
        internal int used = 0;
        internal System.Collections.Generic.IDictionary<string,int> index = null;
        public PrefixPool()  {
            prefixes[0] = "";
            used = 1;
        }
        public int obtainPrefixCode(string prefix) {
            if (prefix.Length==0) {
                return 0;
            }
            if (((index == null) && (used>8))) {
                makeIndex();
            }
            if ((index != null)) {
                int existing = Saxon.Impl.Helpers.MapUtils.GetOrDefault(index, prefix, -1);
                if ((existing != -1)) {
                    return existing;
                }
            } else {
                for (int i = 0; (i<used); i++) {
                    if (prefixes[i].Equals(prefix)) {
                        return i;
                    }
                }
            }
            int code = used++;
            if ((used>=prefixes.Length)) {
                prefixes = Saxon.Ejava.util.Arrays.copyOf(prefixes, (used*2));
            }
            prefixes[code] = prefix;
            if ((index != null)) {
                Saxon.Impl.Helpers.MapUtils.Put(index, prefix, code);
            }
            return code;
        }
        private void makeIndex() {
            index = new System.Collections.Generic.Dictionary<string, int>(used);
            for (int i = 0; (i<used); i++) {
                Saxon.Impl.Helpers.MapUtils.Put(index, prefixes[i], i);
            }
        }
        public string getPrefix(int code) {
            if ((code<used)) {
                return prefixes[code];
            }
            throw new Saxon.Ejava.lang.IllegalArgumentException(("Unknown prefix code "+code));
        }
        public void condense() {
            prefixes = Saxon.Ejava.util.Arrays.copyOf(prefixes, used);
            index = null;
        }
    
    }
}