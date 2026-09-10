using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.regex {
    public class REFlags {
        private bool caseIndependent;
        private bool multiLine;
        private bool singleLine;
        private bool allowWhitespace;
        private bool literal;
        private bool xpath20;
        private bool xpath30;
        private bool xsd11;
        private bool debug;
        private bool allowUnknownBlockNames = false;
        public REFlags(string flags, string language)  {
            if (language.Equals("XSD10")) {
            } else {
                if (language.Contains("XSD11")) {
                    allowUnknownBlockNames = !(language.Contains("XP"));
                    xsd11 = true;
                }
            }
            if (language.Contains("XP20")) {
                xpath20 = true;
            } else {
                if ((language.Contains("XP30") || language.Contains("XP31"))) {
                    xpath20 = true;
                    xpath30 = true;
                }
            }
            int semi = flags.IndexOf(';');
            int endStd = ((semi>=0) ? semi : flags.Length);
            for (int i = 0; (i<endStd); i++) {
                char c = flags[i];
                switch (c) {
                  case 'i':
                    caseIndependent = true;
                    break ;
                  case 'm':
                    multiLine = true;
                    break ;
                  case 's':
                    singleLine = true;
                    break ;
                  case 'q':
                    literal = true;
                    if (!(xpath30)) {
                        throw new Saxon.Hej.regex.RESyntaxException("'q' flag requires XPath 3.0 to be enabled");
                    }
                    break ;
                  case 'x':
                    allowWhitespace = true;
                    break ;
                  default:
                    throw new Saxon.Hej.regex.RESyntaxException((("Unrecognized flag '"+c)+"'"));
                }
            }
            for (int i = (semi+1); (i<flags.Length); i++) {
                char c = flags[i];
                switch (c) {
                  case 'g':
                    debug = true;
                    break ;
                  case 'k':
                    allowUnknownBlockNames = true;
                    break ;
                  case 'K':
                    allowUnknownBlockNames = false;
                    break ;
                }
            }
        }
        public bool isCaseIndependent() {
            return caseIndependent;
        }
        public bool isMultiLine() {
            return multiLine;
        }
        public bool isSingleLine() {
            return singleLine;
        }
        public bool isAllowWhitespace() {
            return allowWhitespace;
        }
        public bool isLiteral() {
            return literal;
        }
        public bool isAllowsXPath20Extensions() {
            return xpath20;
        }
        public bool isAllowsXPath30Extensions() {
            return xpath30;
        }
        public bool isAllowsXSD11Syntax() {
            return xsd11;
        }
        public void setDebug(bool debug) {
            this.debug = debug;
        }
        public bool isDebug() {
            return debug;
        }
        public void setAllowUnknownBlockNames(bool allow) {
            this.allowUnknownBlockNames = allow;
        }
        public bool isAllowUnknownBlockNames() {
            return allowUnknownBlockNames;
        }
    
    }
}