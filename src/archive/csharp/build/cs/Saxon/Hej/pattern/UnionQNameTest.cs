using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.pattern {
    public class UnionQNameTest : Saxon.Hej.pattern.QNameTest {
        internal System.Collections.Generic.IList<Saxon.Hej.pattern.QNameTest> tests;
        public UnionQNameTest(System.Collections.Generic.IList<Saxon.Hej.pattern.QNameTest> tests)  {
            this.tests = new System.Collections.Generic.List<Saxon.Hej.pattern.QNameTest>(tests);
        }
        public bool matches(Saxon.Hej.om.StructuredQName qname) {
            foreach (Saxon.Hej.pattern.QNameTest test in tests) {
                if (test.matches(qname)) {
                    return true;
                }
            }
            return false;
        }
        public override string ToString() {
            bool started = false;
            Saxon.Ejava.lang.StringBuilder fsb = new Saxon.Ejava.lang.StringBuilder(256);
            foreach (Saxon.Hej.pattern.QNameTest qt in tests) {
                if (started) {
                    fsb.append("|");
                } else {
                    started = true;
                }
                fsb.append(qt.ToString());
            }
            return fsb.ToString();
        }
        public string exportQNameTest() {
            Saxon.Ejava.lang.StringBuilder builder = new Saxon.Ejava.lang.StringBuilder();
            bool first = true;
            foreach (Saxon.Hej.pattern.QNameTest test in tests) {
                if (!(first)) {
                    builder.append(" ");
                } else {
                    first = false;
                }
                builder.append(test.exportQNameTest());
            }
            return builder.ToString();
        }
        public string generateJavaScriptNameTest(int targetVersion) {
            Saxon.Ejava.lang.StringBuilder fsb = new Saxon.Ejava.lang.StringBuilder(256);
            bool started = false;
            foreach (Saxon.Hej.pattern.QNameTest qt in tests) {
                if (started) {
                    fsb.append("||");
                } else {
                    started = true;
                }
                string test = qt.generateJavaScriptNameTest(targetVersion);
                fsb.append((("("+test)+")"));
            }
            return fsb.ToString();
        }
    
    }
}