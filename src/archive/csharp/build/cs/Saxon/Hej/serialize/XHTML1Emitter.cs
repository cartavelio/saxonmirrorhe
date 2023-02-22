using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.serialize {
    public class XHTML1Emitter : Saxon.Hej.serialize.XMLEmitter {

        static XHTML1Emitter() {
            Saxon.Ejava.util.Collections.addAll(emptyTags1, emptyTagNames1);
        }
        static System.Collections.Generic.ISet<string> emptyTags1 = new System.Collections.Generic.HashSet<string>(31);
        private static readonly string[] emptyTagNames1 = {"area", "base", "basefont", "br", "col", "embed", "frame", "hr", "img", "input", "isindex", "link", "meta", "param"};
        private bool isRecognizedHtmlElement(Saxon.Hej.om.NodeName name) {
            return name.hasURI(Saxon.Hej.lib.NamespaceConstant.XHTML);
        }
        protected internal override void writeEmptyElementTagCloser(string displayName, Saxon.Hej.om.NodeName name) {
            if ((isRecognizedHtmlElement(name) && emptyTags1.Contains(name.getLocalPart()))) {
                writer.writeAscii(Saxon.Hej.str.StringConstants.EMPTY_TAG_END_XHTML);
            } else {
                writer.writeAscii(Saxon.Hej.str.StringConstants.EMPTY_TAG_MIDDLE);
                writer.write(displayName);
                writer.writeCodePoint('>');
            }
        }
    
    }
}