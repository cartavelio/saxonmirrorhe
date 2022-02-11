using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.serialize {
    public class XMLIndenterPE : Saxon.Hej.serialize.XMLIndenter {
        private int indentSpaces = 3;
        private int lineLength = 80;
        private System.Collections.Generic.ISet<Saxon.Hej.om.NodeName> doubleSpacedElements = null;
        public XMLIndenterPE(Saxon.Hej.serialize.XMLEmitter next) : base (next) {
        }
        public override void setOutputProperties(Saxon.Ejava.util.Properties props) {
            base.setOutputProperties(props);
            string s = props.getProperty(Saxon.Hej.lib.SaxonOutputKeys.INDENT_SPACES);
            if ((s == null)) {
                indentSpaces = 3;
            } else {
                try {
                    indentSpaces = Saxon.Impl.Helpers.Utils.ParseInt(Saxon.Hej.value.Whitespace.trim(s));
                } catch (/* substitute exception */ System.FormatException err) {
                    indentSpaces = 3;
                }
            }
            s = props.getProperty(Saxon.Hej.lib.SaxonOutputKeys.LINE_LENGTH);
            if ((s != null)) {
                try {
                    lineLength = Saxon.Impl.Helpers.Utils.ParseInt(Saxon.Hej.value.Whitespace.trim(s));
                } catch (/* substitute exception */ System.FormatException err) {
                    lineLength = 80;
                }
            }
            s = props.getProperty(Saxon.Hej.lib.SaxonOutputKeys.DOUBLE_SPACE);
            if ((s != null)) {
                doubleSpacedElements = new System.Collections.Generic.HashSet<Saxon.Hej.om.NodeName>(8);
                Saxon.Ejava.util.StringTokenizer st = new Saxon.Ejava.util.StringTokenizer(s, " \t\r\n");
                while (st.hasMoreTokens()) {
                    string eqName = st.nextToken();
                    doubleSpacedElements.Add(Saxon.Hej.om.FingerprintedQName.fromEQName(eqName));
                }
            }
        }
        protected internal override bool isDoubleSpaced(Saxon.Hej.om.NodeName name) {
            return ((doubleSpacedElements != null) && doubleSpacedElements.Contains(name));
        }
        protected internal override int getIndentation() {
            return indentSpaces;
        }
        protected internal override int getLineLength() {
            return lineLength;
        }
    
    }
}