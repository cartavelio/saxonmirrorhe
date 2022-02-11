using System;
using java.lang;

namespace java.util.regex {
    public class Matcher {
        public bool matches() {
            return true;
        }

        public bool find() {
            return true;
        }
        
        public string replaceAll(string str) {
            return str;
        }

        public string group(int gp) {
            return "";
        }

        public int groupCount() {
            return 0;
        }

        public int start() {
            throw new NotImplementedException();
        }

        public int end() {
            throw new NotImplementedException();
        }
    }
}