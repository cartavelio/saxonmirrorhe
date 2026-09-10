using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.serialize {
    public class HTMLIndenterPE : Saxon.Hej.serialize.HTMLIndenter {
        private int indentSpaces = 3;
        private int lineLength = 80;
        public HTMLIndenterPE(Saxon.Hej.@event.Receiver next, string method) : base (next, method) {
        }
        public override void setOutputProperties(Saxon.Ejava.util.Properties props) {
            base.setOutputProperties(props);
            string s = props.getProperty(Saxon.Hej.lib.SaxonOutputKeys.INDENT_SPACES);
            if ((s == null)) {
                indentSpaces = 3;
            } else {
                try {
                    indentSpaces = Saxon.Impl.Helpers.Utils.ParseInt(s);
                } catch (Saxon.Ejava.lang.NumberFormatException err) {
                    indentSpaces = 3;
                }
            }
            s = props.getProperty(Saxon.Hej.lib.SaxonOutputKeys.LINE_LENGTH);
            if ((s == null)) {
                lineLength = 80;
            } else {
                try {
                    lineLength = Saxon.Impl.Helpers.Utils.ParseInt(s);
                } catch (Saxon.Ejava.lang.NumberFormatException err) {
                    lineLength = 80;
                }
            }
        }
        protected internal override int getIndentation() {
            return indentSpaces;
        }
        protected internal override int getLineLength() {
            return lineLength;
        }
    
    }
}