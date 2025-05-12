using System;
using SysCollGen = System.Collections.Generic;
using Saxon.Hej.trans.packages;

namespace Saxon.Ejava.util {
    public class StringTokenizer {
        private string[] tokens;
        private int i = 0;

        public StringTokenizer(string input, string delim) : this(input, delim, false) {

        }
        
        public StringTokenizer(string input, string delim = " \t\n\r\f", bool returnDelimiters = false) {
            if (returnDelimiters) {
                char[] delimChars = delim.ToCharArray();
                int start = 0;
                SysCollGen.List<string> result = new SysCollGen.List<string>();
                while (true) {
                    int index = input.IndexOfAny(delimChars, start);
                    if (index >= 0) {
                        if (index > start) {
                            result.Add(input.Substring(start, index - start));
                        }
                        result.Add(input.Substring(index, 1));
                        start = index+1;
                    }
                    else {
                        result.Add(input.Substring(start));
                        break;
                    }

                    if (start >= input.Length) {
                        break;
                    }
                }
                tokens = result.ToArray();
            }
            else {
                char[] delimChars = delim.ToCharArray();
                tokens = input.Split(delimChars, StringSplitOptions.RemoveEmptyEntries);
            }
            
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