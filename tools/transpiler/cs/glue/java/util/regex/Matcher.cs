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
        
        public bool find(int start) {
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
        
        public int start(int i) {
            throw new NotImplementedException();
        }

        public int end() {
            throw new NotImplementedException();
        }
        
        public int end(int i) {
            throw new NotImplementedException();
        }
    }
}