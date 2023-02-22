using System;
using SysRegex = System.Text.RegularExpressions;
using java.lang;

namespace java.util.regex {
    public class Matcher {

        private SysRegex.Regex regex;
        private string input;

        public Matcher(SysRegex.Regex regex, string input) {
            this.regex = regex;
            this.input = input;
        }
        public bool matches() {
            return regex.Match(input).Success;
        }

        public bool find() {
            return regex.Match(input).Success;
        }
        
        public bool find(int start) {
            return regex.Match(input, start).Success;
        }
        
        public string replaceAll(string str) {
            return regex.Replace(input, str) ;
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