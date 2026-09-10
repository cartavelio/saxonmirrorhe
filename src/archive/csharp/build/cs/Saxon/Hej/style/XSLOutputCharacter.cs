using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class XSLOutputCharacter : Saxon.Hej.style.StyleElement {
        private int codepoint = -1;
        private string replacementString = null;
        protected internal override void prepareAttributes() {
            foreach (Saxon.Hej.om.AttributeInfo att in attributes()) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                string f = attName.getDisplayName();
                string value = att.getValue();
                if (f.Equals("character")) {
                    string s = value;
                    switch (s.Length) {
                      case 0:
                        compileError("character attribute must not be zero-length", "XTSE0020");
                        codepoint = 256;
                        break ;
                      case 1:
                        codepoint = s[0];
                        break ;
                      case 2:
                        if ((Saxon.Hej.serialize.charcode.UTF16CharacterSet.isHighSurrogate(s[0]) && Saxon.Hej.serialize.charcode.UTF16CharacterSet.isLowSurrogate(s[1]))) {
                            codepoint = Saxon.Hej.serialize.charcode.UTF16CharacterSet.combinePair(s[0], s[1]);
                        } else {
                            compileError("character attribute must be a single XML character", "XTSE0020");
                            codepoint = 256;
                        }
                        break ;
                      default:
                        compileError("character attribute must be a single XML character", "XTSE0020");
                        codepoint = 256;
                        break ;
                    }
                } else {
                    if (f.Equals("string")) {
                        replacementString = value;
                    } else {
                        checkUnknownAttribute(attName);
                    }
                }
            }
            if ((codepoint == -1)) {
                reportAbsence("character");
                return ;
            }
            if ((replacementString == null)) {
                reportAbsence("string");
            }
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            if (!(((((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent()) is Saxon.Hej.style.XSLCharacterMap)))) {
                compileError("xsl:output-character may appear only as a child of xsl:character-map", "XTSE0010");
            }
        }
        public override Saxon.Hej.expr.Expression compile(Saxon.Hej.style.Compilation exec, Saxon.Hej.style.ComponentDeclaration decl) {
            return null;
        }
        public int getCodePoint() {
            return codepoint;
        }
        public string getReplacementString() {
            return replacementString;
        }
    
    }
}