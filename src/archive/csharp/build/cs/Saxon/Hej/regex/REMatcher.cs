using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.regex {
    public class REMatcher {
        const int MAX_PAREN = 16;
        internal Saxon.Hej.regex.REProgram program;
        internal Saxon.Hej.str.UnicodeString search;
        internal Saxon.Hej.regex.History history = new Saxon.Hej.regex.History();
        internal int maxParen = MAX_PAREN;
        internal Saxon.Hej.regex.REMatcher.State _captureState = new Saxon.Hej.regex.REMatcher.State();
        internal int[] startBackref;
        internal int[] endBackref;
        internal Saxon.Hej.regex.Operation operation;
        internal bool anchoredMatch;
        public REMatcher(Saxon.Hej.regex.REProgram program)  {
            setProgram(program);
        }
        public void setProgram(Saxon.Hej.regex.REProgram program) {
            this.program = program;
            if (((program != null) && (program.maxParens != -1))) {
                this.operation = program.operation;
                this.maxParen = program.maxParens;
            } else {
                this.maxParen = MAX_PAREN;
            }
        }
        public Saxon.Hej.regex.REProgram getProgram() {
            return program;
        }
        public int getParenCount() {
            return _captureState.parenCount;
        }
        public Saxon.Hej.str.UnicodeString getParen(int which) {
            int start;
            if (((which<_captureState.parenCount) && ((start = getParenStart(which))>=0))) {
                return search.substring(start, getParenEnd(which));
            }
            return null;
        }
        public int getParenStart(int which) {
            if ((which<_captureState.startn.Length)) {
                return _captureState.startn[which];
            }
            return -1;
        }
        public int getParenEnd(int which) {
            if ((which<_captureState.endn.Length)) {
                return _captureState.endn[which];
            }
            return -1;
        }
        protected internal void setParenStart(int which, int i) {
            while ((which>(_captureState.startn.Length-1))) {
                int[] s2 = new int[(_captureState.startn.Length*2)];
                Saxon.Ejava.lang.System.arraycopy(_captureState.startn, 0, s2, 0, _captureState.startn.Length);
                Saxon.Ejava.util.Arrays.fill(s2, _captureState.startn.Length, s2.Length, -1);
                _captureState.startn = s2;
            }
            _captureState.startn[which] = i;
        }
        protected internal void setParenEnd(int which, int i) {
            while ((which>(_captureState.endn.Length-1))) {
                int[] e2 = new int[(_captureState.endn.Length*2)];
                Saxon.Ejava.lang.System.arraycopy(_captureState.endn, 0, e2, 0, _captureState.endn.Length);
                Saxon.Ejava.util.Arrays.fill(e2, _captureState.endn.Length, e2.Length, -1);
                _captureState.endn = e2;
            }
            _captureState.endn[which] = i;
        }
        protected internal void clearCapturedGroupsBeyond(int pos) {
            for (int i = 0; (i<_captureState.startn.Length); i++) {
                if ((_captureState.startn[i]>=pos)) {
                    _captureState.endn[i] = _captureState.startn[i];
                }
            }
            if ((startBackref != null)) {
                for (int i = 0; (i<startBackref.Length); i++) {
                    if ((startBackref[i]>=pos)) {
                        endBackref[i] = startBackref[i];
                    }
                }
            }
        }
        protected internal bool matchAt(int i, bool anchored) {
            _captureState.parenCount = 1;
            anchoredMatch = anchored;
            setParenStart(0, i);
            if ((((program.optimizationFlags&Saxon.Hej.regex.REProgram.OPT_HASBACKREFS)) != 0)) {
                startBackref = new int[maxParen];
                endBackref = new int[maxParen];
            }
            int idx;
            Saxon.Hej.z.IntIterator iter = operation.iterateMatches(this, i);
            if (iter.hasNext()) {
                idx = iter.next();
                setParenEnd(0, idx);
                return true;
            }
            _captureState.parenCount = 0;
            return false;
        }
        public bool isAnchoredMatch(Saxon.Hej.str.UnicodeString search) {
            this.search = search;
            return matchAt(0, true);
        }
        public bool match(Saxon.Hej.str.UnicodeString search, int i) {
            Saxon.Ejava.util.Objects.requireNonNull(search);
            this.search = search.tidy();
            _captureState = new Saxon.Hej.regex.REMatcher.State();
            if ((((program.optimizationFlags&Saxon.Hej.regex.REProgram.OPT_HASBOL)) == Saxon.Hej.regex.REProgram.OPT_HASBOL)) {
                if (!(program.flags.isMultiLine())) {
                    return (((i == 0) && checkPreconditions(i)) && matchAt(i, false));
                }
                int nl = i;
                if (matchAt(nl, false)) {
                    return true;
                }
                while (true) {
                    nl = ((int)(search.indexOf('\n', nl))+1);
                    if (((nl>=search.length()) || (nl<=0))) {
                        return false;
                    } else {
                        if (matchAt(nl, false)) {
                            return true;
                        }
                    }
                }
            }
            int actualLength = (search.length32()-i);
            if ((actualLength<program.minimumLength)) {
                return false;
            }
            if ((program.prefix == null)) {
                if ((program.initialCharClass != null)) {
                    Saxon.Hej.z.IntPredicateProxy pred = program.initialCharClass;
                    for (; !(((i>=search.length32()))); i++) {
                        if (pred.test(search.codePointAt(i))) {
                            if (matchAt(i, false)) {
                                return true;
                            }
                        }
                    }
                    return false;
                }
                if (!(checkPreconditions(i))) {
                    return false;
                }
                for (; !((((i-1)>=search.length32()))); i++) {
                    if (matchAt(i, false)) {
                        return true;
                    }
                }
                return false;
            } else {
                Saxon.Hej.str.UnicodeString prefix = program.prefix;
                int prefixLength = prefix.length32();
                bool ignoreCase = program.flags.isCaseIndependent();
                for (; !(((((i+prefixLength)-1)>=search.length()))); i++) {
                    bool prefixOK = true;
                    if (ignoreCase) {
                        for (int j = i, k = 0; (k<prefixLength); j++, k++) {
                            if (!(equalCaseBlind(search.codePointAt(j), prefix.codePointAt(k)))) {
                                prefixOK = false;
                                break ;
                            }
                        }
                    } else {
                        for (int j = i, k = 0; (k<prefixLength); j++, k++) {
                            if ((search.codePointAt(j) != prefix.codePointAt(k))) {
                                prefixOK = false;
                                break ;
                            }
                        }
                    }
                    if (prefixOK) {
                        if (matchAt(i, false)) {
                            return true;
                        }
                    }
                }
                return false;
            }
        }
        private bool checkPreconditions(int start) {
            foreach (Saxon.Hej.regex.RegexPrecondition condition in program.preconditions) {
                if ((condition.fixedPosition != -1)) {
                    bool match = condition.operation.iterateMatches(this, condition.fixedPosition).hasNext();
                    if (!(match)) {
                        return false;
                    }
                } else {
                    int i = start;
                    if ((i<condition.minPosition)) {
                        i = condition.minPosition;
                    }
                    bool found = false;
                    for (; !(((i>=search.length()))); i++) {
                        if (((((condition.fixedPosition == -1) || (condition.fixedPosition == i))) && condition.operation.iterateMatches(this, i).hasNext())) {
                            found = true;
                            break ;
                        }
                    }
                    if (!(found)) {
                        return false;
                    }
                }
            }
            return true;
        }
        public bool match(string search) {
            return match(Saxon.Hej.str.StringView.of(search).tidy(), 0);
        }
        public System.Collections.Generic.IList<Saxon.Hej.str.UnicodeString> split(Saxon.Hej.str.UnicodeString s) {
            System.Collections.Generic.IList<Saxon.Hej.str.UnicodeString> v = new System.Collections.Generic.List<Saxon.Hej.str.UnicodeString>();
            int pos = 0;
            int len = s.length32();
            while (((pos<len) && match(s, pos))) {
                int start = getParenStart(0);
                int newpos = getParenEnd(0);
                if ((newpos == pos)) {
                    v.Add(s.substring(pos, (start+1)));
                    newpos++;
                } else {
                    v.Add(s.substring(pos, start));
                }
                pos = newpos;
            }
            Saxon.Hej.str.UnicodeString remainder = s.substring(pos, len);
            v.Add(remainder);
            return v;
        }
        public Saxon.Hej.str.UnicodeString replace(Saxon.Hej.str.UnicodeString @in, Saxon.Hej.str.UnicodeString replacement) {
            Saxon.Hej.str.UnicodeString result = Saxon.Hej.str.EmptyUnicodeString.getInstance();
            int pos = 0;
            int len = @in.length32();
            bool firstMatch = true;
            bool simpleReplacement = false;
            while (((pos<len) && match(@in, pos))) {
                result = result.concat(@in.substring(pos, getParenStart(0)));
                if (firstMatch) {
                    simpleReplacement = program.flags.isLiteral();
                    firstMatch = false;
                }
                if (!(simpleReplacement)) {
                    int maxCapture = (program.maxParens-1);
                    simpleReplacement = true;
                    for (int i = 0; (i<replacement.length()); i++) {
                        int ch = replacement.codePointAt(i);
                        if ((ch == '\\')) {
                            simpleReplacement = false;
                            int index = ++i;
                            ch = replacement.codePointAt(index);
                            if (((ch == '\\') || (ch == '$'))) {
                                result = result.concat(Saxon.Hej.str.BMPString.of((""+(char)(ch))));
                            } else {
                                throw new Saxon.Hej.regex.RESyntaxException((("Invalid escape '"+ch)+"' in replacement string"));
                            }
                        } else {
                            if ((ch == '$')) {
                                simpleReplacement = false;
                                int index = ++i;
                                ch = replacement.codePointAt(index);
                                if (!((((ch>='0') && (ch<='9'))))) {
                                    throw new Saxon.Hej.regex.RESyntaxException("$ in replacement string must be followed by a digit");
                                }
                                int n = (ch-'0');
                                if ((maxCapture<=9)) {
                                    if ((maxCapture>=n)) {
                                        Saxon.Hej.str.UnicodeString captured = getParen(n);
                                        if ((captured != null)) {
                                            result = result.concat(captured);
                                        }
                                    }
                                } else {
                                    while (true) {
                                        if ((++i>=replacement.length())) {
                                            break ;
                                        }
                                        ch = replacement.codePointAt(i);
                                        if (((ch>='0') && (ch<='9'))) {
                                            int m = ((n*10)+((ch-'0')));
                                            if ((m>maxCapture)) {
                                                i--;
                                                break ;
                                            } else {
                                                n = m;
                                            }
                                        } else {
                                            i--;
                                            break ;
                                        }
                                    }
                                    Saxon.Hej.str.UnicodeString captured = getParen(n);
                                    if ((captured != null)) {
                                        result = result.concat(captured);
                                    }
                                }
                            } else {
                                result = result.concat(new Saxon.Hej.str.UnicodeChar(ch));
                            }
                        }
                    }
                } else {
                    result = result.concat(replacement);
                }
                int newpos = getParenEnd(0);
                if ((newpos == pos)) {
                    newpos++;
                }
                pos = newpos;
            }
            if (firstMatch) {
                return @in;
            }
            result = result.concat(@in.substring(pos, len));
            return result.economize();
        }
        public Saxon.Hej.str.UnicodeString replaceWith(Saxon.Hej.str.UnicodeString @in, System.Func<Saxon.Hej.str.UnicodeString,Saxon.Hej.str.UnicodeString> replacer) {
            Saxon.Hej.str.UnicodeBuilder sb = new Saxon.Hej.str.UnicodeBuilder();
            int pos = 0;
            int len = @in.length32();
            while (((pos<len) && match(@in, pos))) {
                for (long i = pos; (i<getParenStart(0)); i++) {
                    sb.append(@in.codePointAt(i));
                }
                Saxon.Hej.str.UnicodeString matchingSubstring = @in.substring(getParenStart(0), getParenEnd(0));
                Saxon.Hej.str.UnicodeString replacement = replacer(matchingSubstring);
                Saxon.Hej.z.IntIterator iter = replacement.codePoints();
                while (iter.hasNext()) {
                    sb.append(iter.next());
                }
                int newpos = getParenEnd(0);
                if ((newpos == pos)) {
                    newpos++;
                }
                pos = newpos;
            }
            for (int i = pos; (i<len); i++) {
                sb.append(@in.codePointAt(i));
            }
            return sb.toUnicodeString();
        }
        internal bool isNewline(int i) {
            return (search.codePointAt(i) == '\n');
        }
        internal bool equalCaseBlind(int c1, int c2) {
            if ((c1 == c2)) {
                return true;
            }
            foreach (int v in Saxon.Hej.regex.CaseVariants.getCaseVariants(c2)) {
                if ((c1 == v)) {
                    return true;
                }
            }
            return false;
        }
        public Saxon.Hej.regex.REMatcher.State captureState() {
            return new Saxon.Hej.regex.REMatcher.State(_captureState);
        }
        public void resetState(Saxon.Hej.regex.REMatcher.State state) {
            _captureState = new Saxon.Hej.regex.REMatcher.State(state);
        }
        public class State {
            internal int parenCount;
            internal int[] startn;
            internal int[] endn;
            public State()  {
                parenCount = 0;
                startn = new int[3];
                startn[0] = startn[1] = startn[2] = -1;
                endn = new int[3];
                endn[0] = endn[1] = endn[2] = -1;
            }
            public State(Saxon.Hej.regex.REMatcher.State s)  {
                parenCount = s.parenCount;
                startn = Saxon.Ejava.util.Arrays.copyOf(s.startn, s.startn.Length);
                endn = Saxon.Ejava.util.Arrays.copyOf(s.endn, s.endn.Length);
            }
        
        }
    
    }
}