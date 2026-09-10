using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.regex {
    public class ARegexIterator : Saxon.Hej.regex.RegexIterator, Saxon.Hej.expr.LastPositionFinder {
        private readonly Saxon.Hej.str.UnicodeString theString;
        private readonly Saxon.Hej.str.UnicodeString _regex;
        private readonly Saxon.Hej.regex.REMatcher _matcher;
        private Saxon.Hej.str.UnicodeString current;
        private Saxon.Hej.str.UnicodeString nextSubstring;
        private int prevEnd = 0;
        private Saxon.Hej.z.IntToIntHashMap nestingTable = null;
        private bool skip = false;
        public ARegexIterator(Saxon.Hej.str.UnicodeString str, Saxon.Hej.str.UnicodeString regex, Saxon.Hej.regex.REMatcher matcher)  {
            Saxon.Ejava.util.Objects.requireNonNull(str);
            Saxon.Ejava.util.Objects.requireNonNull(regex);
            Saxon.Ejava.util.Objects.requireNonNull(matcher);
            theString = str;
            this._regex = regex;
            this._matcher = matcher;
            nextSubstring = null;
        }
        public bool supportsGetLength() {
            return true;
        }
        public int getLength() {
            Saxon.Hej.regex.ARegexIterator another = new Saxon.Hej.regex.ARegexIterator(theString, _regex, new Saxon.Hej.regex.REMatcher(_matcher.getProgram()));
            int n = 0;
            while ((((/*1430*/Saxon.Hej.value.StringValue)another.next()) != null)) {
                n++;
            }
            return n;
        }
        public Saxon.Hej.om.Item next() {
            try {
                if (((nextSubstring == null) && (prevEnd>=0))) {
                    int searchStart = prevEnd;
                    if (skip) {
                        searchStart++;
                        if ((searchStart>=theString.length())) {
                            if ((prevEnd<theString.length())) {
                                current = theString.substring(prevEnd);
                                nextSubstring = null;
                            } else {
                                current = null;
                                prevEnd = -1;
                                return null;
                            }
                        }
                    }
                    if (_matcher.match(theString, searchStart)) {
                        int start = _matcher.getParenStart(0);
                        int end = _matcher.getParenEnd(0);
                        skip = (start == end);
                        if ((prevEnd == start)) {
                            nextSubstring = null;
                            current = theString.substring(start, end);
                            prevEnd = end;
                        } else {
                            current = theString.substring(prevEnd, start);
                            nextSubstring = theString.substring(start, end);
                        }
                    } else {
                        if ((prevEnd<theString.length())) {
                            current = theString.substring(prevEnd);
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
                        prevEnd = _matcher.getParenEnd(0);
                    } else {
                        current = null;
                        return null;
                    }
                }
                return currentStringValue();
            } catch (System.StackOverflowException e) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException.StackOverflow("Stack overflow (excessive recursion) during regular expression evaluation", Saxon.Hej.trans.SaxonErrorCode.SXRE0001, Saxon.Hej.expr.parser.Loc.NONE);
                throw new Saxon.Hej.trans.UncheckedXPathException(err);
            }
        }
        private Saxon.Hej.value.StringValue currentStringValue() {
            return new Saxon.Hej.value.StringValue(current);
        }
        public bool isMatching() {
            return ((nextSubstring == null) && (prevEnd>=0));
        }
        public Saxon.Hej.str.UnicodeString getRegexGroup(int number) {
            if (!(isMatching())) {
                return null;
            }
            if (((number>=_matcher.getParenCount()) || (number<0))) {
                return Saxon.Hej.str.EmptyUnicodeString.getInstance();
            }
            Saxon.Hej.str.UnicodeString us = _matcher.getParen(number);
            return (((us == null) ? Saxon.Hej.str.EmptyUnicodeString.getInstance() : us));
        }
        public int getNumberOfGroups() {
            return _matcher.getParenCount();
        }
        public void processMatchingSubstring(Saxon.Hej.regex.RegexMatchHandler action) {
            int c = (_matcher.getParenCount()-1);
            if ((c == 0)) {
                action.characters(current);
            } else {
                Saxon.Hej.z.IntHashMap<System.Collections.Generic.IList<int>> actions = new Saxon.Hej.z.IntHashMap<System.Collections.Generic.IList<int>>(c);
                for (int i = 1; (i<=c); i++) {
                    int start = (_matcher.getParenStart(i)-_matcher.getParenStart(0));
                    if ((start != -1)) {
                        int end = (_matcher.getParenEnd(i)-_matcher.getParenStart(0));
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
                                nestingTable = computeNestingTable(_regex);
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
                for (int i = 0; (i<(current.length()+1)); i++) {
                    System.Collections.Generic.IList<int> events = actions.get(i);
                    if ((events != null)) {
                        if (!(buff.isEmpty())) {
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
                    if ((i<current.length())) {
                        buff.append(current.codePointAt(i));
                    }
                }
                if (!(buff.isEmpty())) {
                    action.characters(buff.toUnicodeString());
                }
            }
        }
        public static Saxon.Hej.z.IntToIntHashMap computeNestingTable(Saxon.Hej.str.UnicodeString regex) {
            Saxon.Hej.z.IntToIntHashMap nestingTable = new Saxon.Hej.z.IntToIntHashMap(16);
            int[] stack = new int[regex.length32()];
            int tos = 0;
            bool[] captureStack = new bool[regex.length32()];
            int captureTos = 0;
            int group = 1;
            int inBrackets = 0;
            stack[tos++] = 0;
            for (int i = 0; (i<regex.length()); i++) {
                int ch = regex.codePointAt(i);
                if ((ch == '\\')) {
                    i++;
                } else {
                    if ((ch == '[')) {
                        inBrackets++;
                    } else {
                        if ((ch == ']')) {
                            inBrackets--;
                        } else {
                            if (((ch == '(') && (inBrackets == 0))) {
                                bool capture = (regex.codePointAt((i+1)) != '?');
                                captureStack[captureTos++] = capture;
                                if (capture) {
                                    nestingTable.put(group, stack[(tos-1)]);
                                    stack[tos++] = group++;
                                }
                            } else {
                                if (((ch == ')') && (inBrackets == 0))) {
                                    bool capture = captureStack[--captureTos];
                                    if (capture) {
                                        tos--;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return nestingTable;
        }
    
        /* Expanding default method close from interface SequenceIterator */
        public virtual void close() {
        }

    }
}