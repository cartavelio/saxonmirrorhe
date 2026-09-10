using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.regex {
    public class JRegexIterator : Saxon.Hej.regex.RegexIterator, Saxon.Hej.expr.LastPositionFinder {
        private readonly string theString;
        private readonly Saxon.Ejava.util.regex.Pattern pattern;
        private readonly Saxon.Ejava.util.regex.Matcher matcher;
        private string current;
        private string nextSubstring;
        private int prevEnd = 0;
        private Saxon.Hej.z.IntToIntHashMap nestingTable = null;
        public JRegexIterator(string str, Saxon.Ejava.util.regex.Pattern pattern)  {
            theString = str;
            this.pattern = pattern;
            matcher = pattern.matcher(str);
            nextSubstring = null;
        }
        public bool supportsGetLength() {
            return true;
        }
        public int getLength() {
            Saxon.Hej.regex.JRegexIterator another = new Saxon.Hej.regex.JRegexIterator(theString, pattern);
            return Saxon.Hej.functions.Count.steppingCount(another);
        }
        public Saxon.Hej.om.Item next() {
            if (((nextSubstring == null) && (prevEnd>=0))) {
                if (matcher.find()) {
                    int start = matcher.start();
                    int end = matcher.end();
                    if ((prevEnd == start)) {
                        nextSubstring = null;
                        current = theString.Substring(start, end - start);
                        prevEnd = end;
                    } else {
                        current = theString.Substring(prevEnd, start - prevEnd);
                        nextSubstring = theString.Substring(start, end - start);
                    }
                } else {
                    if ((prevEnd<theString.Length)) {
                        current = theString.Substring(prevEnd);
                        nextSubstring = null;
                    } else {
                        current = null;
                        prevEnd = -1;
                        return null;
                    }
                    prevEnd = -1;
                }
            } else {
                if ((prevEnd>=0)) {
                    current = nextSubstring;
                    nextSubstring = null;
                    prevEnd = matcher.end();
                } else {
                    current = null;
                    return null;
                }
            }
            return Saxon.Hej.value.StringValue.makeStringValue(current);
        }
        public bool isMatching() {
            return ((nextSubstring == null) && (prevEnd>=0));
        }
        public Saxon.Hej.str.UnicodeString getRegexGroup(int number) {
            if (!(isMatching())) {
                return null;
            }
            if (((number>matcher.groupCount()) || (number<0))) {
                return Saxon.Hej.str.EmptyUnicodeString.getInstance();
            }
            string s = matcher.group(number);
            if ((s == null)) {
                return Saxon.Hej.str.EmptyUnicodeString.getInstance();
            }
            return Saxon.Hej.str.StringView.of(s).tidy();
        }
        public int getNumberOfGroups() {
            return matcher.groupCount();
        }
        public void processMatchingSubstring(Saxon.Hej.regex.RegexMatchHandler action) {
            int c = matcher.groupCount();
            if ((c == 0)) {
                action.characters(Saxon.Hej.str.StringView.of(current));
            } else {
                Saxon.Hej.z.IntHashMap<System.Collections.Generic.IList<int>> actions = new Saxon.Hej.z.IntHashMap<System.Collections.Generic.IList<int>>(c);
                for (int i = 1; (i<=c); i++) {
                    int start = (matcher.start(i)-matcher.start());
                    if ((start != -1)) {
                        int end = (matcher.end(i)-matcher.start());
                        if ((start<end)) {
                            System.Collections.Generic.IList<int> s = actions.get(start);
                            if ((s == null)) {
                                s = new System.Collections.Generic.List<int>(4);
                                actions.put(start, s);
                            }
                            s.Add(i);
                            System.Collections.Generic.IList<int> e = actions.get(end);
                            if ((e == null)) {
                                e = new System.Collections.Generic.List<int>(4);
                                actions.put(end, e);
                            }
                            e.Insert(0, -(i));
                        } else {
                            if ((nestingTable == null)) {
                                computeNestingTable();
                            }
                            int parentGroup = nestingTable.get(i);
                            System.Collections.Generic.IList<int> s = actions.get(start);
                            if ((s == null)) {
                                s = new System.Collections.Generic.List<int>(4);
                                actions.put(start, s);
                                s.Add(i);
                                s.Add(-(i));
                            } else {
                                int pos = s.Count;
                                for (int e = 0; (e<s.Count); e++) {
                                    if ((s[e] == -(parentGroup))) {
                                        pos = e;
                                        break ;
                                    }
                                }
                                s.Insert(pos, -(i));
                                s.Insert(pos, i);
                            }
                        }
                    }
                }
                Saxon.Hej.str.UnicodeBuilder buff = new Saxon.Hej.str.UnicodeBuilder();
                for (int i = 0; (i<(current.Length+1)); i++) {
                    System.Collections.Generic.IList<int> events = actions.get(i);
                    if ((events != null)) {
                        if ((buff.length()>0)) {
                            action.characters(buff.toUnicodeString());
                            buff.clear();
                        }
                        foreach (int group in events) {
                            if ((group>0)) {
                                action.onGroupStart(group);
                            } else {
                                action.onGroupEnd(-(group));
                            }
                        }
                    }
                    if ((i<current.Length)) {
                        buff.append(current[i]);
                    }
                }
                if ((buff.length()>0)) {
                    action.characters(buff.toUnicodeString());
                }
            }
        }
        private void computeNestingTable() {
            nestingTable = new Saxon.Hej.z.IntToIntHashMap(16);
            string s = pattern.pattern();
            int[] stack = new int[s.Length];
            int tos = 0;
            int group = 1;
            int inBrackets = 0;
            stack[tos++] = 0;
            for (int i = 0; (i<s.Length); i++) {
                char ch = s[i];
                if ((ch == '\'')) {
                    i++;
                } else {
                    if ((ch == '[')) {
                        inBrackets++;
                    } else {
                        if ((ch == ']')) {
                            inBrackets--;
                        } else {
                            if ((((ch == '(') && (s[(i+1)] != '?')) && (inBrackets == 0))) {
                                nestingTable.put(group, stack[(tos-1)]);
                                stack[tos++] = group++;
                            } else {
                                if (((ch == ')') && (inBrackets == 0))) {
                                    tos--;
                                }
                            }
                        }
                    }
                }
            }
        }
    
        /* Expanding default method close from interface SequenceIterator */
        public virtual void close() {
        }

    }
}