using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.ma.json {
    public class JsonHandler {
        public bool escape;
        protected internal Saxon.Hej.z.IntPredicateProxy charChecker;
        private Saxon.Hej.expr.XPathContext context;
        private Saxon.Hej.om.Function fallbackFunction = null;
        private const string REPLACEMENT = "\ufffd";
        public void setContext(Saxon.Hej.expr.XPathContext context) {
            this.context = context;
        }
        public Saxon.Hej.expr.XPathContext getContext() {
            return context;
        }
        public virtual Saxon.Hej.om.Sequence getResult() {
            return null;
        }
        public virtual bool setKey(string unEscaped, string reEscaped) {
            return false;
        }
        public virtual void startArray() {
        }
        public virtual void endArray() {
        }
        public virtual void startMap() {
        }
        public virtual void endMap() {
        }
        public virtual void writeNumeric(string asString, Saxon.Hej.value.AtomicValue parsedValue) {
        }
        public virtual void writeString(string val) {
        }
        public string reEscape(string val) {
            string escaped;
            if (escape) {
                escaped = Saxon.Hej.ma.json.JsonReceiver.escape(val, true, value => ((((((value>=0) && (value<=0x1F))) || (((value>=0x7F) && (value<=unchecked((int)0x9F))))) || !(charChecker.test(value))) || ((value == 0x5C))));
            } else {
                Saxon.Ejava.lang.StringBuilder buffer = new Saxon.Ejava.lang.StringBuilder(val);
                handleInvalidCharacters(buffer);
                escaped = buffer.ToString();
            }
            return escaped;
        }
        public virtual void writeBoolean(bool value) {
        }
        public virtual void writeNull() {
        }
        protected internal void handleInvalidCharacters(Saxon.Ejava.lang.StringBuilder buffer) {
            Saxon.Hej.z.IntPredicateProxy charChecker = context.getConfiguration().getValidCharacterChecker();
            for (int i = 0; (i<buffer.length()); i++) {
                char ch = buffer.charAt(i);
                if (Saxon.Hej.serialize.charcode.UTF16CharacterSet.isHighSurrogate(ch)) {
                    if ((((i+1)>=buffer.length()) || !(Saxon.Hej.serialize.charcode.UTF16CharacterSet.isLowSurrogate(buffer.charAt((i+1)))))) {
                        substitute(buffer, i, 1, context);
                    }
                } else {
                    if (Saxon.Hej.serialize.charcode.UTF16CharacterSet.isLowSurrogate(ch)) {
                        if (((i == 0) || !(Saxon.Hej.serialize.charcode.UTF16CharacterSet.isHighSurrogate(buffer.charAt((i-1)))))) {
                            substitute(buffer, i, 1, context);
                        } else {
                            int pair = Saxon.Hej.serialize.charcode.UTF16CharacterSet.combinePair(buffer.charAt((i-1)), ch);
                            if (!(charChecker.test(pair))) {
                                substitute(buffer, (i-1), 2, context);
                            }
                        }
                    } else {
                        if (!(charChecker.test(ch))) {
                            substitute(buffer, i, 1, context);
                        }
                    }
                }
            }
        }
        protected internal virtual void markAsEscaped(string escaped, bool isKey) {
        }
        private void substitute(Saxon.Ejava.lang.StringBuilder buffer, int offset, int count, Saxon.Hej.expr.XPathContext context) {
            Saxon.Ejava.lang.StringBuilder escaped = new Saxon.Ejava.lang.StringBuilder((count*6));
            for (int j = 0; (j<count); j++) {
                escaped.append("\\u");
                Saxon.Ejava.lang.StringBuilder hex = new Saxon.Ejava.lang.StringBuilder(Saxon.Impl.Helpers.Utils.intToHexString(buffer.charAt((offset+j))));
                while ((hex.length()<4)) {
                    hex.insert(0, "0");
                }
                escaped.append(hex.ToString().ToUpper());
            }
            string replacement = replace(escaped.ToString(), context);
            if ((replacement.Length == count)) {
                for (int j = 0; (j<count); j++) {
                    buffer.setCharAt((offset+j), replacement[j]);
                }
            } else {
                for (int j = 0; (j<count); j++) {
                    buffer.deleteCharAt((offset+j));
                }
                for (int j = 0; (j<replacement.Length); j++) {
                    buffer.insert((offset+j), replacement[j]);
                }
            }
        }
        private string replace(string s, Saxon.Hej.expr.XPathContext context) {
            if ((fallbackFunction != null)) {
                Saxon.Hej.om.Sequence[] args = new Saxon.Hej.om.Sequence[1];
                args[0] = new Saxon.Hej.value.StringValue(s);
                Saxon.Hej.om.Sequence result = Saxon.Hej.functions.SystemFunction.dynamicCall(fallbackFunction, context, args).head();
                Saxon.Hej.om.Item first = result.head();
                return ((first == null) ? "" : first.getStringValue());
            } else {
                return REPLACEMENT;
            }
        }
        public void setFallbackFunction(System.Collections.Generic.IDictionary<string,Saxon.Hej.om.Sequence> options, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.om.Sequence val = Saxon.Impl.Helpers.MapUtils.Get(options, "fallback");
            if ((val != null)) {
                Saxon.Hej.om.Item fn = val.head();
                if ((fn is Saxon.Hej.om.Function)) {
                    fallbackFunction = (Saxon.Hej.om.Function)(fn);
                    if ((fallbackFunction.getArity() != 1)) {
                        throw new Saxon.Hej.trans.XPathException("Fallback function must have arity=1", "FOJS0005");
                    }
                    Saxon.Hej.type.SpecificFunctionType required = new Saxon.Hej.type.SpecificFunctionType(new Saxon.Hej.value.SequenceType[]{Saxon.Hej.value.SequenceType.SINGLE_STRING}, Saxon.Hej.value.SequenceType.ANY_SEQUENCE);
                    if (!(required.matches(fallbackFunction, context.getConfiguration().getTypeHierarchy()))) {
                        throw new Saxon.Hej.trans.XPathException("Fallback function does not match the required type", "FOJS0005");
                    }
                } else {
                    throw new Saxon.Hej.trans.XPathException("Value of option 'fallback' is not a function", "FOJS0005");
                }
            }
        }
    
    }
}