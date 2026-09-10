using System;
using net.sf.saxon.trans.packages;

namespace java.util {
    public class StringTokenizer {
        private string[] tokens;
        private int i = 0;

        public StringTokenizer(string input, string delim) {
            // TODO:
            
        }

        public int countTokens() {
            return tokens.Length;
        }

        public bool hasMoreTokens() {
            return i < tokens.Length;
        }

        public string nextToken() {
            return tokens[i++];
        }
    }
}