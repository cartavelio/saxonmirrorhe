using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.serialize {
    public class JSONEmitter {
        private Saxon.Hej.Configuration config;
        private Saxon.Hej.str.UnicodeWriter writer;
        private bool normalize;
        private Saxon.Ejava.text.Normalizer.Form normalizationForm;
        private Saxon.Hej.serialize.CharacterMap characterMap;
        private Saxon.Ejava.util.Properties outputProperties;
        private Saxon.Hej.serialize.charcode.CharacterSet characterSet;
        private bool isIndenting;
        private int indentSpaces = 2;
        private int maxLineLength;
        private bool first = true;
        private bool afterKey = false;
        private int level;
        private readonly System.Collections.Generic.Stack<bool> oneLinerStack = new System.Collections.Generic.Stack<bool>();
        private bool unfailing = false;
        public JSONEmitter(Saxon.Hej.@event.PipelineConfiguration pipe, Saxon.Hej.str.UnicodeWriter writer, Saxon.Ejava.util.Properties outputProperties)  {
            config = pipe.getConfiguration();
            setOutputProperties(outputProperties);
            this.writer = writer;
        }
        public void setOutputProperties(Saxon.Ejava.util.Properties details) {
            this.outputProperties = details;
            if ("yes".Equals(details.getProperty(Saxon.Ejavax.xml.transform.OutputKeys.INDENT))) {
                isIndenting = true;
            }
            if ("yes".Equals(details.getProperty(Saxon.Hej.lib.SaxonOutputKeys.UNFAILING))) {
                unfailing = true;
            }
            string max = details.getProperty(Saxon.Hej.lib.SaxonOutputKeys.LINE_LENGTH);
            if ((max != null)) {
                try {
                    maxLineLength = Saxon.Impl.Helpers.Utils.ParseInt(max);
                } catch (Saxon.Ejava.lang.NumberFormatException err) {
                }
            }
            string spaces = details.getProperty(Saxon.Hej.lib.SaxonOutputKeys.INDENT_SPACES);
            if ((spaces != null)) {
                try {
                    indentSpaces = Saxon.Impl.Helpers.Utils.ParseInt(spaces);
                } catch (Saxon.Ejava.lang.NumberFormatException err) {
                }
            }
            string encoding = details.getProperty(Saxon.Ejavax.xml.transform.OutputKeys.ENCODING);
            try {
                characterSet = config.getCharacterSetFactory().getCharacterSet(encoding);
            } catch (Saxon.Hej.trans.XPathException e) {
                characterSet = Saxon.Hej.serialize.charcode.UTF8CharacterSet.getInstance();
            }
        }
        public Saxon.Ejava.util.Properties getOutputProperties() {
            return outputProperties;
        }
        public void setNormalizationForm(Saxon.Ejava.text.Normalizer.Form form) {
            this.normalize = true;
            this.normalizationForm = form;
        }
        public void setCharacterMap(Saxon.Hej.serialize.CharacterMap map) {
            this.characterMap = map;
        }
        public void writeKey(string key) {
            conditionalComma(false);
            emit('"');
            emit(escape(key));
            emit("\":");
            if (isIndenting) {
                emit(" ");
            }
            afterKey = true;
        }
        public void writeAtomicValue(Saxon.Hej.value.AtomicValue item) {
            conditionalComma(false);
            if ((item == null)) {
                emit("null");
            } else {
                if ((item is Saxon.Hej.value.NumericValue)) {
                    Saxon.Hej.value.NumericValue num = (Saxon.Hej.value.NumericValue)(item);
                    if (num.isNaN()) {
                        if (unfailing) {
                            emit("NaN");
                        } else {
                            throw new Saxon.Hej.trans.XPathException("JSON has no way of representing NaN", "SERE0020");
                        }
                    } else {
                        if (System.Double.IsInfinity(num.getDoubleValue())) {
                            if (unfailing) {
                                emit(((num.getDoubleValue()<0) ? "-INF" : "INF"));
                            } else {
                                throw new Saxon.Hej.trans.XPathException("JSON has no way of representing Infinity", "SERE0020");
                            }
                        } else {
                            if ((item is Saxon.Hej.value.IntegerValue)) {
                                emit((num.longValue()+""));
                            } else {
                                if (((num.isWholeNumber() && !(num.isNegativeZero())) && (num.abs().compareTo(1_000_000_000_000_000_000L)<0))) {
                                    emit((num.longValue()+""));
                                } else {
                                    emit(num.getStringValue());
                                }
                            }
                        }
                    }
                } else {
                    if ((item is Saxon.Hej.value.BooleanValue)) {
                        emit(item.getStringValue());
                    } else {
                        emit('"');
                        emit(escape(item.getStringValue()));
                        emit('"');
                    }
                }
            }
        }
        public void startArray(bool oneLiner) {
            emitOpen('[', oneLiner);
            level++;
        }
        public void endArray() {
            emitClose(']', level--);
        }
        public void startMap(bool oneLiner) {
            emitOpen('{', oneLiner);
            level++;
        }
        public void endMap() {
            emitClose('}', level--);
        }
        private void emitOpen(char bracket, bool oneLiner) {
            conditionalComma(true);
            oneLinerStack.Push(oneLiner);
            emit(bracket);
            first = true;
            if ((isIndenting && oneLiner)) {
                emit(' ');
            }
        }
        private void emitClose(char bracket, int level) {
            bool oneLiner = oneLinerStack.Pop();
            if (isIndenting) {
                if (oneLiner) {
                    emit(' ');
                } else {
                    indent((level-1));
                }
            }
            emit(bracket);
            first = false;
        }
        private void conditionalComma(bool opening) {
            bool wasFirst = first;
            bool actuallyIndenting = ((isIndenting && (level != 0)) && !(oneLinerStack.Peek()));
            if (first) {
                first = false;
            } else {
                if (!(afterKey)) {
                    emit(',');
                }
            }
            if (((wasFirst && afterKey))) {
                emit(' ');
            } else {
                if ((actuallyIndenting && !(afterKey))) {
                    emit('\n');
                    for (int i = 0; (i<(indentSpaces*level)); i++) {
                        emit(' ');
                    }
                }
            }
            afterKey = false;
        }
        private void indent(int level) {
            emit('\n');
            for (int i = 0; (i<(indentSpaces*level)); i++) {
                emit(' ');
            }
        }
        private string escape(string cs) {
            if ((characterMap != null)) {
                Saxon.Ejava.lang.StringBuilder @out = new Saxon.Ejava.lang.StringBuilder(cs.Length);
                string s = characterMap.map(Saxon.Hej.str.StringView.of(cs).tidy(), true).ToString();
                int prev = 0;
                while (true) {
                    int start = s.IndexOf((char)(0), prev);
                    if ((start>=0)) {
                        @out.append(simpleEscape(s.Substring(prev, start - prev)));
                        int end = s.IndexOf((char)(0), (start+1));
                        @out.append(s, (start+1), end);
                        prev = (end+1);
                    } else {
                        @out.append(simpleEscape(s.Substring(prev)));
                        return @out.ToString();
                    }
                }
            } else {
                return simpleEscape(cs);
            }
        }
        private string simpleEscape(string cs) {
            if (normalize) {
                cs = Saxon.Ejava.text.Normalizer.normalize(cs, normalizationForm);
            }
            return Saxon.Hej.ma.json.JsonReceiver.escape(cs, false, c => (((c<31) || (((c>=127) && (c<=159)))) || !(characterSet.inCharset(c))));
        }
        private void emit(string s) {
            System.Diagnostics.Debug.Assert((writer != null));
            try {
                writer.write(s);
            } catch (System.IO.IOException e) {
                throw new Saxon.Hej.trans.XPathException(e);
            }
        }
        private void emit(char c) {
            System.Diagnostics.Debug.Assert((writer != null));
            try {
                writer.writeCodePoint(c);
            } catch (System.IO.IOException e) {
                throw new Saxon.Hej.trans.XPathException(e);
            }
        }
        public void close() {
            if (first) {
                emit("null");
            }
            if ((writer != null)) {
                try {
                    writer.close();
                } catch (System.IO.IOException e) {
                }
            }
        }
    
    }
}