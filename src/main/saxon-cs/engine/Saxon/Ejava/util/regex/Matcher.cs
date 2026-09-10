using SysRegex = System.Text.RegularExpressions;


namespace Saxon.Ejava.util.regex {
    
    /// <summary>
    /// Emulates the API of the Java regex engine by delegating all calls to the .NET regex engine
    /// </summary>
    public class Matcher {

        private SysRegex.Regex regex;
        private SysRegex.Match latestMatch;
        private int position = 0;
        private string input;

        public Matcher(SysRegex.Regex regex, string input) {
            this.regex = regex;
            this.input = input;
        }
        public bool matches() {
            latestMatch = regex.Match(input, 0);
            return latestMatch.Success && latestMatch.Length == input.Length;
        }

        public bool find() {
            latestMatch = regex.Match(input, position);
            if (latestMatch.Success) {
                position = latestMatch.Index + latestMatch.Length;
            }
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
            return latestMatch.Index;
        }
        
        public int start(int i) {
            return latestMatch.Groups[i].Index;
        }

        public int end() {
            return latestMatch.Index + latestMatch.Length;
        }
        
        public int end(int i) {
            return latestMatch.Groups[i].Index + latestMatch.Groups[i].Length;
        }
    }
}