using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.regex {
    public class RECompiler {
        internal Saxon.Hej.str.UnicodeString pattern;
        internal int len;
        internal int idx;
        internal int capturingOpenParenCount;
        const int NODE_NORMAL = 0;
        const int NODE_TOPLEVEL = 2;
        internal int bracketMin;
        internal int bracketMax;
        internal bool isXPath = true;
        internal bool isXPath30 = true;
        internal bool isXSD11 = false;
        internal Saxon.Hej.z.IntHashSet captures = new Saxon.Hej.z.IntHashSet();
        internal bool hasBackReferences = false;
        internal Saxon.Hej.regex.REFlags reFlags;
        internal System.Collections.Generic.IList<string> warnings;
        private const bool TRACING = false;
        public RECompiler()  {
        }
        public void setFlags(Saxon.Hej.regex.REFlags flags) {
            this.reFlags = flags;
            isXPath = flags.isAllowsXPath20Extensions();
            isXPath30 = flags.isAllowsXPath30Extensions();
            isXSD11 = flags.isAllowsXSD11Syntax();
        }
        private void warning(string s) {
            if ((warnings == null)) {
                warnings = new System.Collections.Generic.List<string>(4);
            }
            warnings.Add(s);
        }
        public System.Collections.Generic.IList<string> getWarnings() {
            if ((warnings == null)) {
                return new System.Collections.Generic.List<string>();
            } else {
                return warnings;
            }
        }
        internal void internalError() {
            throw new Saxon.Ejava.lang.AssertionError("Internal error!");
        }
        internal void syntaxError(string s) {
            throw new Saxon.Hej.regex.RESyntaxException(s, idx);
        }
        static Saxon.Hej.regex.Operation trace(Saxon.Hej.regex.Operation @base) {
            if ((TRACING && !(((@base is Saxon.Hej.regex.OpTrace))))) {
                return new Saxon.Hej.regex.OpTrace(@base);
            } else {
                return @base;
            }
        }
        internal void bracket() {
            if (((idx>=len) || (pattern.codePointAt(idx++) != '{'))) {
                internalError();
            }
            if (((idx>=len) || !(isAsciiDigit(pattern.codePointAt(idx))))) {
                syntaxError("Expected digit");
            }
            Saxon.Ejava.lang.StringBuilder number = new Saxon.Ejava.lang.StringBuilder(16);
            while (((idx<len) && isAsciiDigit(pattern.codePointAt(idx)))) {
                number.appendCodePoint(pattern.codePointAt(idx++));
            }
            try {
                bracketMin = System.Int32.Parse(number.ToString());
            } catch (Saxon.Ejava.lang.NumberFormatException e) {
                syntaxError("Expected valid number");
            }
            if ((idx>=len)) {
                syntaxError("Expected comma or right bracket");
            }
            if ((pattern.codePointAt(idx) == '}')) {
                idx++;
                bracketMax = bracketMin;
                return ;
            }
            if (((idx>=len) || (pattern.codePointAt(idx++) != ','))) {
                syntaxError("Expected comma");
            }
            if ((idx>=len)) {
                syntaxError("Expected comma or right bracket");
            }
            if ((pattern.codePointAt(idx) == '}')) {
                idx++;
                bracketMax = System.Int32.MaxValue;
                return ;
            }
            if (((idx>=len) || !(isAsciiDigit(pattern.codePointAt(idx))))) {
                syntaxError("Expected digit");
            }
            number.setLength(0);
            while (((idx<len) && isAsciiDigit(pattern.codePointAt(idx)))) {
                number.appendCodePoint(pattern.codePointAt(idx++));
            }
            try {
                bracketMax = System.Int32.Parse(number.ToString());
            } catch (Saxon.Ejava.lang.NumberFormatException e) {
                syntaxError("Expected valid number");
            }
            if ((bracketMax<bracketMin)) {
                syntaxError("Bad range");
            }
            if (((idx>=len) || (pattern.codePointAt(idx++) != '}'))) {
                syntaxError("Missing close brace");
            }
        }
        private static bool isAsciiDigit(int ch) {
            return ((ch>='0') && (ch<='9'));
        }
        internal Saxon.Hej.regex.charclass.CharacterClass escape(bool inSquareBrackets) {
            if ((pattern.codePointAt(idx) != '\\')) {
                internalError();
            }
            if (((idx+1) == len)) {
                syntaxError("Escape terminates string");
            }
            idx += 2;
            int escapeChar = pattern.codePointAt((idx-1));
            switch (escapeChar) {
              case 'n':
                return new Saxon.Hej.regex.charclass.SingletonCharacterClass('\n');
              case 'r':
                return new Saxon.Hej.regex.charclass.SingletonCharacterClass('\r');
              case 't':
                return new Saxon.Hej.regex.charclass.SingletonCharacterClass('\t');
              case '\\':
              case '|':
              case '.':
              case '-':
              case '^':
              case '?':
              case '*':
              case '+':
              case '{':
              case '}':
              case '(':
              case ')':
              case '[':
              case ']':
                return new Saxon.Hej.regex.charclass.SingletonCharacterClass(escapeChar);
              case '$':
                if (isXPath) {
                    return new Saxon.Hej.regex.charclass.SingletonCharacterClass(escapeChar);
                } else {
                    syntaxError("In XSD, '$' must not be escaped");
                }
                break ;
              case 's':
                return Saxon.Hej.regex.charclass.Categories.ESCAPE_s;
              case 'S':
                return Saxon.Hej.regex.charclass.Categories.ESCAPE_S;
              case 'i':
                return Saxon.Hej.regex.charclass.Categories.ESCAPE_i;
              case 'I':
                return Saxon.Hej.regex.charclass.Categories.ESCAPE_I;
              case 'c':
                return Saxon.Hej.regex.charclass.Categories.ESCAPE_c;
              case 'C':
                return Saxon.Hej.regex.charclass.Categories.ESCAPE_C;
              case 'd':
                return Saxon.Hej.regex.charclass.Categories.ESCAPE_d;
              case 'D':
                return Saxon.Hej.regex.charclass.Categories.ESCAPE_D;
              case 'w':
                return Saxon.Hej.regex.charclass.Categories.ESCAPE_w;
              case 'W':
                return Saxon.Hej.regex.charclass.Categories.ESCAPE_W;
              case 'p':
              case 'P':
                if ((idx == len)) {
                    syntaxError(("Expected '{' after \\"+escapeChar));
                }
                if ((pattern.codePointAt(idx) != '{')) {
                    syntaxError(("Expected '{' after \\"+escapeChar));
                }
                int from = idx++;
                int close = (int)(pattern.indexOf('}', from));
                if ((close == -1)) {
                    syntaxError(("No closing '}' after \\"+escapeChar));
                }
                string block = pattern.substring(idx, close).ToString();
                if (((block.Length == 1) || (block.Length == 2))) {
                    Saxon.Hej.regex.charclass.CharacterClass primary = Saxon.Hej.regex.charclass.Categories.getCategory(block);
                    if ((primary == null)) {
                        syntaxError(("Unknown character category "+block));
                    }
                    idx = (close+1);
                    if ((escapeChar == 'p')) {
                        return primary;
                    } else {
                        return makeComplement(primary);
                    }
                } else {
                    if (block.StartsWith("Is")) {
                        string blockName = block.Substring(2);
                        Saxon.Hej.z.IntSet uniBlock = Saxon.Hej.regex.UnicodeBlocks.getBlock(blockName);
                        if ((uniBlock == null)) {
                            if (reFlags.isAllowUnknownBlockNames()) {
                                warning(("Unknown Unicode block: "+blockName));
                                idx = (close+1);
                                return Saxon.Hej.regex.charclass.EmptyCharacterClass.getComplement();
                            } else {
                                syntaxError(("Unknown Unicode block: "+blockName));
                            }
                        }
                        idx = (close+1);
                        Saxon.Hej.regex.charclass.IntSetCharacterClass primary = new Saxon.Hej.regex.charclass.IntSetCharacterClass(uniBlock);
                        if ((escapeChar == 'p')) {
                            return primary;
                        } else {
                            return makeComplement(primary);
                        }
                    } else {
                        syntaxError(("Unknown character category: "+block));
                    }
                }
                break ;
              case '0':
                syntaxError("Octal escapes not allowed");
                break ;
              case '1':
              case '2':
              case '3':
              case '4':
              case '5':
              case '6':
              case '7':
              case '8':
              case '9':
                if (inSquareBrackets) {
                    syntaxError("Backreference not allowed within character class");
                } else {
                    if (isXPath) {
                        int backRef = (escapeChar-'0');
                        while ((idx<len)) {
                            int c1 = (int)(Saxon.Hej.str.StringConstants.ZERO_TO_NINE.indexOf(pattern.codePointAt(idx)));
                            if ((c1<0)) {
                                break ;
                            } else {
                                int backRef2 = ((backRef*10)+c1);
                                if ((backRef2>((capturingOpenParenCount-1)))) {
                                    break ;
                                } else {
                                    backRef = backRef2;
                                    idx++;
                                }
                            }
                        }
                        if (!(captures.contains(backRef))) {
                            string explanation = ((backRef>((capturingOpenParenCount-1))) ? "(no such group)" : "(group not yet closed)");
                            syntaxError(((("invalid backreference \\"+backRef)+" ")+explanation));
                        }
                        hasBackReferences = true;
                        return new Saxon.Hej.regex.RECompiler.BackReference(backRef);
                    } else {
                        syntaxError("digit not allowed after \\");
                    }
                }
                break ;
              default:
                syntaxError((("Escape character '"+(char)(escapeChar))+"' not allowed"));
                break ;
            }
            return null;
        }
        internal class BackReference : Saxon.Hej.regex.charclass.SingletonCharacterClass {
            public BackReference(int number) : base (number) {
            }
        
        }
        internal Saxon.Hej.regex.charclass.CharacterClass parseCharacterClass() {
            if ((pattern.codePointAt(idx) != '[')) {
                internalError();
            }
            int index = ++idx;
            if (((((idx+1))>=len) || (pattern.codePointAt(index) == ']'))) {
                syntaxError("Missing ']'");
            }
            int simpleChar;
            bool positive = true;
            bool definingRange = false;
            int rangeStart = -1;
            int rangeEnd;
            Saxon.Hej.z.IntRangeSet range = new Saxon.Hej.z.IntRangeSet();
            Saxon.Hej.regex.charclass.CharacterClass addend = null;
            Saxon.Hej.regex.charclass.CharacterClass subtrahend = null;
            if (thereFollows('^')) {
                if (thereFollows('^', '-', '[')) {
                    syntaxError("Nothing before subtraction operator");
                } else {
                    if (thereFollows('^', ']')) {
                        syntaxError("Empty negative character group");
                    } else {
                        positive = false;
                        idx++;
                    }
                }
            } else {
                if (thereFollows('-', '[')) {
                    syntaxError("Nothing before subtraction operator");
                }
            }
            while (((idx<len) && (pattern.codePointAt(idx) != ']'))) {
                int ch = pattern.codePointAt(idx);
                simpleChar = -1;
                switch (ch) {
                  case '[':
                    syntaxError("Unescaped '[' within square brackets");
                    break ;
                  case '\\':
                    {
                        Saxon.Hej.regex.charclass.CharacterClass cc = escape(true);
                        if ((cc is Saxon.Hej.regex.charclass.SingletonCharacterClass)) {
                            simpleChar = ((Saxon.Hej.regex.charclass.SingletonCharacterClass)(cc)).getCodepoint();
                            break ;
                        } else {
                            if (definingRange) {
                                syntaxError("Multi-character escape cannot follow '-'");
                            } else {
                                if ((addend == null)) {
                                    addend = cc;
                                } else {
                                    addend = makeUnion(addend, cc);
                                }
                            }
                            continue ;
                        }
                    }
                  case '-':
                    if (thereFollows('-', '[')) {
                        idx++;
                        subtrahend = parseCharacterClass();
                        if (!(thereFollows(']'))) {
                            syntaxError("Expected closing ']' after subtraction");
                        }
                    } else {
                        if (thereFollows('-', ']')) {
                            simpleChar = '-';
                            idx++;
                        } else {
                            if ((rangeStart>=0)) {
                                definingRange = true;
                                idx++;
                                continue ;
                            } else {
                                if (definingRange) {
                                    syntaxError("Bad range");
                                } else {
                                    if ((thereFollows('-', '-') && !(thereFollows('-', '-', '[')))) {
                                        syntaxError("Unescaped hyphen as start of range");
                                    } else {
                                        if (((((!(isXSD11) && (pattern.codePointAt((idx-1)) != '[')) && (pattern.codePointAt((idx-1)) != '^')) && !(thereFollows(']'))) && !(thereFollows('-', '[')))) {
                                            syntaxError("In XSD 1.0, hyphen is allowed only at the beginning or end of a positive character group");
                                        } else {
                                            simpleChar = '-';
                                            idx++;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    break ;
                  default:
                    simpleChar = ch;
                    idx++;
                    break ;
                }
                if (definingRange) {
                    rangeEnd = simpleChar;
                    if ((rangeStart>rangeEnd)) {
                        syntaxError("Bad character range: start > end");
                    }
                    range.addRange(rangeStart, rangeEnd);
                    if (reFlags.isCaseIndependent()) {
                        if (((rangeStart == 'a') && (rangeEnd == 'z'))) {
                            range.addRange('A', 'Z');
                            for (int v = 0; (v<Saxon.Hej.regex.CaseVariants.ROMAN_VARIANTS.Length); v++) {
                                range.add(Saxon.Hej.regex.CaseVariants.ROMAN_VARIANTS[v]);
                            }
                        } else {
                            if (((rangeStart == 'A') && (rangeEnd == 'Z'))) {
                                range.addRange('a', 'z');
                                for (int v = 0; (v<Saxon.Hej.regex.CaseVariants.ROMAN_VARIANTS.Length); v++) {
                                    range.add(Saxon.Hej.regex.CaseVariants.ROMAN_VARIANTS[v]);
                                }
                            } else {
                                for (int k = rangeStart; (k<=rangeEnd); k++) {
                                    int[] variants = Saxon.Hej.regex.CaseVariants.getCaseVariants(k);
                                    foreach (int variant in variants) {
                                        range.add(variant);
                                    }
                                }
                            }
                        }
                    }
                    definingRange = false;
                    rangeStart = -1;
                } else {
                    if (thereFollows('-')) {
                        if (thereFollows('-', '[')) {
                            range.add(simpleChar);
                        } else {
                            if (thereFollows('-', ']')) {
                                range.add(simpleChar);
                            } else {
                                if (thereFollows('-', '-', '[')) {
                                    range.add(simpleChar);
                                } else {
                                    if (thereFollows('-', '-')) {
                                        syntaxError("Unescaped hyphen cannot act as end of range");
                                    } else {
                                        rangeStart = simpleChar;
                                    }
                                }
                            }
                        }
                    } else {
                        range.add(simpleChar);
                        if (reFlags.isCaseIndependent()) {
                            int[] variants = Saxon.Hej.regex.CaseVariants.getCaseVariants(simpleChar);
                            foreach (int variant in variants) {
                                range.add(variant);
                            }
                        }
                    }
                }
            }
            if ((idx == len)) {
                syntaxError("Unterminated character class");
            }
            idx++;
            Saxon.Hej.regex.charclass.CharacterClass result = new Saxon.Hej.regex.charclass.IntSetCharacterClass(range);
            if ((addend != null)) {
                result = makeUnion(result, addend);
            }
            if (!(positive)) {
                result = makeComplement(result);
            }
            if ((subtrahend != null)) {
                result = makeDifference(result, subtrahend);
            }
            return result;
        }
        private bool thereFollows(params int[]  chars) {
            if (((idx+chars.Length)>len)) {
                return false;
            }
            for (int i = 0; (i<chars.Length); i++) {
                if ((pattern.codePointAt((idx+i)) != chars[i])) {
                    return false;
                }
            }
            return true;
        }
        public static Saxon.Hej.regex.charclass.CharacterClass makeUnion(Saxon.Hej.regex.charclass.CharacterClass p1, Saxon.Hej.regex.charclass.CharacterClass p2) {
            if ((p1 == Saxon.Hej.regex.charclass.EmptyCharacterClass.getInstance())) {
                return p2;
            }
            if ((p2 == Saxon.Hej.regex.charclass.EmptyCharacterClass.getInstance())) {
                return p1;
            }
            Saxon.Hej.z.IntSet is1 = p1.getIntSet();
            Saxon.Hej.z.IntSet is2 = p2.getIntSet();
            if (((is1 == null) || (is2 == null))) {
                return new Saxon.Hej.regex.charclass.PredicateCharacterClass(ch => (p1.test(ch) || p2.test(ch)));
            } else {
                return new Saxon.Hej.regex.charclass.IntSetCharacterClass(is1.union(is2));
            }
        }
        public static Saxon.Hej.regex.charclass.CharacterClass makeDifference(Saxon.Hej.regex.charclass.CharacterClass p1, Saxon.Hej.regex.charclass.CharacterClass p2) {
            if ((p1 == Saxon.Hej.regex.charclass.EmptyCharacterClass.getInstance())) {
                return p1;
            }
            if ((p2 == Saxon.Hej.regex.charclass.EmptyCharacterClass.getInstance())) {
                return p1;
            }
            Saxon.Hej.z.IntSet is1 = p1.getIntSet();
            Saxon.Hej.z.IntSet is2 = p2.getIntSet();
            if (((is1 == null) || (is2 == null))) {
                return new Saxon.Hej.regex.charclass.PredicateCharacterClass(ch => Saxon.Hej.z.IntExceptPredicate.makeDifference(p1, p2).test(ch));
            } else {
                return new Saxon.Hej.regex.charclass.IntSetCharacterClass(is1.except(is2));
            }
        }
        public static Saxon.Hej.regex.charclass.CharacterClass makeComplement(Saxon.Hej.regex.charclass.CharacterClass p1) {
            if ((p1 is Saxon.Hej.regex.charclass.InverseCharacterClass)) {
                return ((Saxon.Hej.regex.charclass.InverseCharacterClass)(p1)).getComplement();
            } else {
                return new Saxon.Hej.regex.charclass.InverseCharacterClass(p1);
            }
        }
        internal Saxon.Hej.regex.Operation parseAtom() {
            int lenAtom = 0;
            Saxon.Hej.str.UnicodeBuilder ub = new Saxon.Hej.str.UnicodeBuilder();
            bool breakAtomLoop = false;
            while ((idx<len)) {
                if ((((idx+1))<len)) {
                    int c = pattern.codePointAt((idx+1));
                    if ((pattern.codePointAt(idx) == '\\')) {
                        int idxEscape = idx;
                        escape(false);
                        if ((idx<len)) {
                            c = pattern.codePointAt(idx);
                        }
                        idx = idxEscape;
                    }
                    switch (c) {
                      case '{':
                      case '?':
                      case '*':
                      case '+':
                        if ((lenAtom != 0)) {
                            breakAtomLoop = true;
                        }
                        break ;
                    }
                }
                if (breakAtomLoop) {
                    break ;
                }
                switch (pattern.codePointAt(idx)) {
                  case ']':
                  case '.':
                  case '[':
                  case '(':
                  case ')':
                  case '|':
                    breakAtomLoop = true;
                    break ;
                  case '{':
                  case '?':
                  case '*':
                  case '+':
                    if ((lenAtom == 0)) {
                        syntaxError("No expression before quantifier");
                    }
                    breakAtomLoop = true;
                    break ;
                  case '}':
                    syntaxError("Unescaped right curly brace");
                    breakAtomLoop = true;
                    break ;
                  case '\\':
                    {
                        int idxBeforeEscape = idx;
                        Saxon.Hej.regex.charclass.CharacterClass charClass = escape(false);
                        if (!(((charClass is Saxon.Hej.z.IntValuePredicate)))) {
                            idx = idxBeforeEscape;
                            breakAtomLoop = true;
                            break ;
                        }
                        ub.append(((Saxon.Hej.z.IntValuePredicate)(charClass)).getTarget());
                        lenAtom++;
                        break ;
                    }
                  case '^':
                  case '$':
                    if (isXPath) {
                        breakAtomLoop = true;
                        break ;
                    }
                    goto default;
                  default:
                    int index = idx++;
                    ub.append(pattern.codePointAt(index));
                    lenAtom++;
                    break ;
                }
                if (breakAtomLoop) {
                    break ;
                }
            }
            if (ub.isEmpty()) {
                internalError();
            }
            return trace(new Saxon.Hej.regex.OpAtom(ub.toUnicodeString()));
        }
        internal Saxon.Hej.regex.Operation parseTerminal(int[] flags) {
            switch (pattern.codePointAt(idx)) {
              case '$':
                if (isXPath) {
                    idx++;
                    return trace(new Saxon.Hej.regex.OpEOL());
                }
                break ;
              case '^':
                if (isXPath) {
                    idx++;
                    return trace(new Saxon.Hej.regex.OpBOL());
                }
                break ;
              case '.':
                idx++;
                Saxon.Hej.z.IntPredicateProxy predicate;
                if (reFlags.isSingleLine()) {
                    predicate = Saxon.Hej.z.IntSetPredicate.ALWAYS_TRUE;
                } else {
                    predicate = Saxon.Hej.z.IntPredicateLambda.of(value => ((value != '\n') && (value != '\r')));
                }
                return trace(new Saxon.Hej.regex.OpCharClass(predicate));
              case '[':
                Saxon.Hej.regex.charclass.CharacterClass range = parseCharacterClass();
                return trace(new Saxon.Hej.regex.OpCharClass(range));
              case '(':
                return parseExpr(flags);
              case ')':
                syntaxError("Unexpected closing ')'");
                break ;
              case '|':
                internalError();
                break ;
              case ']':
                syntaxError("Unexpected closing ']'");
                break ;
              case 0:
                syntaxError("Unexpected end of input");
                break ;
              case '?':
              case '+':
              case '{':
              case '*':
                syntaxError("No expression before quantifier");
                break ;
              case '\\':
                {
                    int idxBeforeEscape = idx;
                    Saxon.Hej.regex.charclass.CharacterClass esc = escape(false);
                    if ((esc is Saxon.Hej.regex.RECompiler.BackReference)) {
                        int backreference = ((Saxon.Hej.regex.RECompiler.BackReference)(esc)).getCodepoint();
                        if ((capturingOpenParenCount<=backreference)) {
                            syntaxError("Bad backreference");
                        }
                        return trace(new Saxon.Hej.regex.OpBackReference(backreference));
                    } else {
                        if ((esc is Saxon.Hej.z.IntSingletonSet)) {
                            idx = idxBeforeEscape;
                        } else {
                            return trace(new Saxon.Hej.regex.OpCharClass(esc));
                        }
                    }
                    break ;
                }
            }
            return parseAtom();
        }
        internal Saxon.Hej.regex.Operation piece(int[] flags) {
            int[] terminalFlags = {NODE_NORMAL};
            Saxon.Hej.regex.Operation ret = parseTerminal(terminalFlags);
            flags[0] |= terminalFlags[0];
            if ((idx>=len)) {
                return ret;
            }
            bool greedy = true;
            int quantifierType = pattern.codePointAt(idx);
            switch (quantifierType) {
              case '?':
              case '*':
              case '+':
                idx++;
                goto case '{';
              case '{':
                if ((quantifierType == '{')) {
                    bracket();
                }
                if (((ret is Saxon.Hej.regex.OpBOL) || (ret is Saxon.Hej.regex.OpEOL))) {
                    if ((((quantifierType == '?') || (quantifierType == '*')) || (((quantifierType == '{') && (bracketMin == 0))))) {
                        return new Saxon.Hej.regex.OpNothing();
                    } else {
                        quantifierType = 0;
                    }
                }
                if ((ret.matchesEmptyString() == Saxon.Hej.regex.Operation.MATCHES_ZLS_ANYWHERE)) {
                    if ((quantifierType == '?')) {
                        quantifierType = 0;
                    } else {
                        if ((quantifierType == '+')) {
                            quantifierType = '*';
                        } else {
                            if ((quantifierType == '{')) {
                                quantifierType = '*';
                            }
                        }
                    }
                }
                break ;
            }
            if (((idx<len) && (pattern.codePointAt(idx) == '?'))) {
                if (!(isXPath)) {
                    syntaxError("Reluctant quantifiers are not allowed in XSD");
                }
                idx++;
                greedy = false;
            }
            int min = 1;
            int max = 1;
            switch (quantifierType) {
              case '{':
                min = this.bracketMin;
                max = this.bracketMax;
                break ;
              case '?':
                min = 0;
                max = 1;
                break ;
              case '+':
                min = 1;
                max = System.Int32.MaxValue;
                break ;
              case '*':
                min = 0;
                max = System.Int32.MaxValue;
                break ;
            }
            Saxon.Hej.regex.Operation result;
            if ((max == 0)) {
                result = new Saxon.Hej.regex.OpNothing();
            } else {
                if (((min == 1) && (max == 1))) {
                    return ret;
                } else {
                    if (greedy) {
                        if ((ret.getMatchLength() == -1)) {
                            result = trace(new Saxon.Hej.regex.OpRepeat(ret, min, max, true));
                        } else {
                            result = new Saxon.Hej.regex.OpGreedyFixed(ret, min, max, ret.getMatchLength());
                        }
                    } else {
                        if ((ret.getMatchLength() == -1)) {
                            result = new Saxon.Hej.regex.OpRepeat(ret, min, max, false);
                        } else {
                            result = new Saxon.Hej.regex.OpReluctantFixed(ret, min, max, ret.getMatchLength());
                        }
                    }
                }
            }
            return trace(result);
        }
        internal Saxon.Hej.regex.Operation parseBranch() {
            Saxon.Hej.regex.Operation current = null;
            int[] quantifierFlags = new int[1];
            while ((((idx<len) && (pattern.codePointAt(idx) != '|')) && (pattern.codePointAt(idx) != ')'))) {
                quantifierFlags[0] = NODE_NORMAL;
                Saxon.Hej.regex.Operation op = piece(quantifierFlags);
                if ((current == null)) {
                    current = op;
                } else {
                    current = makeSequence(current, op);
                }
            }
            if ((current == null)) {
                return new Saxon.Hej.regex.OpNothing();
            }
            return current;
        }
        private Saxon.Hej.regex.Operation parseExpr(int[] compilerFlags) {
            int paren = -1;
            int group = 0;
            System.Collections.Generic.IList<Saxon.Hej.regex.Operation> branches = new System.Collections.Generic.List<Saxon.Hej.regex.Operation>();
            int closeParens = capturingOpenParenCount;
            bool capturing = true;
            if (((((compilerFlags[0]&NODE_TOPLEVEL)) == 0) && (pattern.codePointAt(idx) == '('))) {
                if (((((idx+2)<len) && (pattern.codePointAt((idx+1)) == '?')) && (pattern.codePointAt((idx+2)) == ':'))) {
                    if (!(isXPath30)) {
                        syntaxError("Non-capturing groups allowed only in XPath3.0");
                    }
                    paren = 2;
                    idx += 3;
                    capturing = false;
                } else {
                    paren = 1;
                    idx++;
                    group = capturingOpenParenCount++;
                }
            }
            compilerFlags[0] &= ~(NODE_TOPLEVEL);
            branches.Add(parseBranch());
            while (((idx<len) && (pattern.codePointAt(idx) == '|'))) {
                idx++;
                branches.Add(parseBranch());
            }
            Saxon.Hej.regex.Operation op;
            if ((branches.Count == 1)) {
                op = branches[0];
            } else {
                op = new Saxon.Hej.regex.OpChoice(branches);
            }
            if ((paren>0)) {
                if (((idx<len) && (pattern.codePointAt(idx) == ')'))) {
                    idx++;
                } else {
                    syntaxError("Missing close paren");
                }
                if (capturing) {
                    op = new Saxon.Hej.regex.OpCapture(op, group);
                    captures.add(closeParens);
                }
            } else {
                op = makeSequence(op, new Saxon.Hej.regex.OpEndProgram());
            }
            return op;
        }
        private static Saxon.Hej.regex.Operation makeSequence(Saxon.Hej.regex.Operation o1, Saxon.Hej.regex.Operation o2) {
            if ((o1 is Saxon.Hej.regex.OpSequence)) {
                if ((o2 is Saxon.Hej.regex.OpSequence)) {
                    System.Collections.Generic.IList<Saxon.Hej.regex.Operation> list1 = ((Saxon.Hej.regex.OpSequence)(o1)).getOperations();
                    System.Collections.Generic.IList<Saxon.Hej.regex.Operation> list2 = ((Saxon.Hej.regex.OpSequence)(o2)).getOperations();
                    Saxon.Impl.Helpers.ListUtils.AddAll(list1, list2);
                    return o1;
                }
                System.Collections.Generic.IList<Saxon.Hej.regex.Operation> l1 = ((Saxon.Hej.regex.OpSequence)(o1)).getOperations();
                l1.Add(o2);
                return o1;
            } else {
                if ((o2 is Saxon.Hej.regex.OpSequence)) {
                    System.Collections.Generic.IList<Saxon.Hej.regex.Operation> l2 = ((Saxon.Hej.regex.OpSequence)(o2)).getOperations();
                    l2.Insert(0, o1);
                    return o2;
                } else {
                    System.Collections.Generic.IList<Saxon.Hej.regex.Operation> list = new System.Collections.Generic.List<Saxon.Hej.regex.Operation>(4);
                    list.Add(o1);
                    list.Add(o2);
                    return trace(new Saxon.Hej.regex.OpSequence(list));
                }
            }
        }
        public Saxon.Hej.regex.REProgram compile(Saxon.Hej.str.UnicodeString pattern) {
            this.pattern = pattern;
            len = this.pattern.length32();
            idx = 0;
            capturingOpenParenCount = 1;
            if (reFlags.isLiteral()) {
                Saxon.Hej.regex.Operation ret = new Saxon.Hej.regex.OpAtom(this.pattern);
                Saxon.Hej.regex.Operation endNode = new Saxon.Hej.regex.OpEndProgram();
                Saxon.Hej.regex.Operation seq = makeSequence(ret, endNode);
                return new Saxon.Hej.regex.REProgram(seq, capturingOpenParenCount, reFlags);
            } else {
                if (reFlags.isAllowWhitespace()) {
                    Saxon.Hej.str.UnicodeBuilder sb = new Saxon.Hej.str.UnicodeBuilder();
                    int nesting = 0;
                    bool escaped = false;
                    Saxon.Hej.z.IntIterator iter = pattern.codePoints();
                    while (iter.hasNext()) {
                        int ch = iter.next();
                        if (((ch == '\\') && !(escaped))) {
                            escaped = true;
                            sb.append(ch);
                        } else {
                            if (((ch == '[') && !(escaped))) {
                                nesting++;
                                escaped = false;
                                sb.append(ch);
                            } else {
                                if (((ch == ']') && !(escaped))) {
                                    nesting--;
                                    escaped = false;
                                    sb.append(ch);
                                } else {
                                    if (((nesting == 0) && Saxon.Hej.value.Whitespace.isWhite(ch))) {
                                    } else {
                                        escaped = false;
                                        sb.append(ch);
                                    }
                                }
                            }
                        }
                    }
                    this.pattern = sb.toUnicodeString();
                    this.len = this.pattern.length32();
                }
                int[] compilerFlags = {NODE_TOPLEVEL};
                Saxon.Hej.regex.Operation exp = parseExpr(compilerFlags);
                if ((idx != len)) {
                    if ((pattern.codePointAt(idx) == ')')) {
                        syntaxError("Unmatched close paren");
                    }
                    syntaxError("Unexpected input remains");
                }
                Saxon.Hej.regex.REProgram program = new Saxon.Hej.regex.REProgram(exp, capturingOpenParenCount, reFlags);
                if (hasBackReferences) {
                    program.optimizationFlags |= Saxon.Hej.regex.REProgram.OPT_HASBACKREFS;
                }
                return program;
            }
        }
        public static bool noAmbiguity(Saxon.Hej.regex.Operation op0, Saxon.Hej.regex.Operation op1, bool caseBlind, bool reluctant) {
            if ((op1 is Saxon.Hej.regex.OpEndProgram)) {
                return !(reluctant);
            }
            if (((op1 is Saxon.Hej.regex.OpBOL) || (op1 is Saxon.Hej.regex.OpEOL))) {
                return true;
            }
            if (((op1 is Saxon.Hej.regex.OpRepeat) && (((Saxon.Hej.regex.OpRepeat)(op1)).min == 0))) {
                return false;
            }
            Saxon.Hej.regex.charclass.CharacterClass c0 = op0.getInitialCharacterClass(caseBlind);
            Saxon.Hej.regex.charclass.CharacterClass c1 = op1.getInitialCharacterClass(caseBlind);
            return c0.isDisjoint(c1);
        }
    
    }
}