using System;
using SysRegex = System.Text.RegularExpressions;
using java.lang;

namespace java.util.regex {
    
    /*
     * Emulates the API of the Java regex engine by delegating all calls to the .NET regex engine
     */
    public class Matcher {

        private SysRegex.Regex regex;
        private SysRegex.Match latestMatch;
        private string input;

        public Matcher(SysRegex.Regex regex, string input) {
            this.regex = regex;
            this.input = input;
        }
        public bool matches() {
            latestMatch = regex.Match(input);
            return latestMatch.Success;
        }

        public bool find() {
            latestMatch = regex.Match(input);
            return latestMatch.Success;
        }
        
        public bool find(int start) {
            latestMatch = regex.Match(input, start);
            return latestMatch.Success;
        }
        
        public string replaceAll(string str) {
            return regex.Replace(input, str) ;
        }

        public string group(int gp) {
            return latestMatch.Groups[gp].Value;
        }

        public int groupCount() {
            return latestMatch.Groups.Count;
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