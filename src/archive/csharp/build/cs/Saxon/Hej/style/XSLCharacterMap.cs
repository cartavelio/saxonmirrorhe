using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class XSLCharacterMap : Saxon.Hej.style.StyleElement {
        internal string use;
        internal System.Collections.Generic.IList<Saxon.Hej.style.XSLCharacterMap> characterMapElements = null;
        internal bool validated = false;
        internal bool redundant = false;
        public override bool isDeclaration() {
            return true;
        }
        public Saxon.Hej.om.StructuredQName getCharacterMapName() {
            Saxon.Hej.om.StructuredQName name = getObjectName();
            if ((name == null)) {
                return makeQName(getAttributeValue(Saxon.Hej.om.NamespaceUri.NULL, "name"), null, "name");
            }
            return name;
        }
        public bool isRedundant() {
            return redundant;
        }
        protected internal override void prepareAttributes() {
            string name = null;
            use = null;
            foreach (Saxon.Hej.om.AttributeInfo att in attributes()) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                string value = att.getValue();
                string f = attName.getDisplayName();
                if (f.Equals("name")) {
                    name = Saxon.Hej.value.Whitespace.trim(value);
                } else {
                    if (f.Equals("use-character-maps")) {
                        use = value;
                    } else {
                        checkUnknownAttribute(attName);
                    }
                }
            }
            if ((name == null)) {
                reportAbsence("name");
                name = ("unnamedCharacterMap_"+GetHashCode());
            }
            setObjectName(makeQName(name, null, "name"));
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            if (validated) {
                return ;
            }
            checkTopLevel("XTSE0010", false);
            foreach (Saxon.Hej.om.NodeInfo child in children()) {
                if (!(((child is Saxon.Hej.style.XSLOutputCharacter)))) {
                    compileError("Only xsl:output-character is allowed within xsl:character-map", "XTSE0010");
                }
            }
            Saxon.Hej.style.PrincipalStylesheetModule psm = getPrincipalStylesheetModule();
            Saxon.Hej.style.ComponentDeclaration other = psm.getCharacterMap(getObjectName());
            if (((other != null) && (other.getSourceElement() != this))) {
                if ((decl.getPrecedence() == other.getPrecedence())) {
                    compileError("There are two character-maps with the same name and import precedence", "XTSE1580");
                } else {
                    if ((decl.getPrecedence()<other.getPrecedence())) {
                        redundant = true;
                    }
                }
            }
            if ((use != null)) {
                characterMapElements = new System.Collections.Generic.List<Saxon.Hej.style.XSLCharacterMap>(5);
                Saxon.Ejava.util.StringTokenizer st = new Saxon.Ejava.util.StringTokenizer(use, " \t\n\r", false);
                while (st.hasMoreTokens()) {
                    string displayname = st.nextToken();
                    try {
                        string[] parts = Saxon.Hej.om.NameChecker.getQNameParts(displayname);
                        Saxon.Hej.om.NamespaceUri uri = getURIForPrefix(parts[0], false);
                        if ((uri == null)) {
                            compileError((("Undeclared namespace prefix "+Saxon.Hej.trans.Err.wrap(parts[0]))+" in character map name"), "XTSE0280");
                        }
                        Saxon.Hej.om.StructuredQName qn = new Saxon.Hej.om.StructuredQName(parts[0], uri, parts[1]);
                        Saxon.Hej.style.ComponentDeclaration charMapDecl = psm.getCharacterMap(qn);
                        if ((charMapDecl == null)) {
                            compileError((("No character-map named '"+displayname)+"' has been defined"), "XTSE1590");
                        } else {
                            Saxon.Hej.style.XSLCharacterMap @ref = (Saxon.Hej.style.XSLCharacterMap)(charMapDecl.getSourceElement());
                            characterMapElements.Add(@ref);
                        }
                    } catch (Saxon.Hej.om.QNameException err) {
                        compileError(("Invalid character-map name. "+err.Message), "XTSE1590");
                    }
                }
                foreach (System.Object characterMapElement in characterMapElements) {
                    ((Saxon.Hej.style.XSLCharacterMap)(characterMapElement)).checkCircularity(this);
                }
            }
            validated = true;
        }
        private void checkCircularity(Saxon.Hej.style.XSLCharacterMap origin) {
            if ((this == origin)) {
                compileError("The definition of the character map is circular", "XTSE1600");
                characterMapElements = null;
            } else {
                if (!(validated)) {
                    return ;
                }
                if ((characterMapElements != null)) {
                    foreach (System.Object characterMapElement in characterMapElements) {
                        ((Saxon.Hej.style.XSLCharacterMap)(characterMapElement)).checkCircularity(origin);
                    }
                }
            }
        }
        public void assemble(Saxon.Hej.z.IntHashMap<string> map) {
            if ((characterMapElements != null)) {
                foreach (Saxon.Hej.style.XSLCharacterMap charmap in characterMapElements) {
                    charmap.assemble(map);
                }
            }
            foreach (Saxon.Hej.om.NodeInfo child in children()) {
                Saxon.Hej.style.XSLOutputCharacter oc = (Saxon.Hej.style.XSLOutputCharacter)(child);
                map.put(oc.getCodePoint(), oc.getReplacementString());
            }
        }
        public override void compileDeclaration(Saxon.Hej.style.Compilation compilation, Saxon.Hej.style.ComponentDeclaration decl) {
        }
    
    }
}