using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class XSLElement : Saxon.Hej.style.StyleElement {
        private Saxon.Hej.expr.Expression elementName;
        private Saxon.Hej.expr.Expression @namespace = null;
        private string use;
        private Saxon.Hej.om.StructuredQName[] attributeSets = null;
        private int validation;
        private Saxon.Hej.type.SchemaType schemaType = null;
        private bool inheritNamespaces = true;
        public override bool isInstruction() {
            return true;
        }
        protected internal override bool mayContainSequenceConstructor() {
            return true;
        }
        protected internal override void prepareAttributes() {
            string nameAtt = null;
            string namespaceAtt = null;
            string validationAtt = null;
            string typeAtt = null;
            string inheritAtt = null;
            foreach (Saxon.Hej.om.AttributeInfo att in attributes()) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                string value = att.getValue();
                string f = attName.getDisplayName();
                switch (f) {
                  case "name":
                    nameAtt = Saxon.Hej.value.Whitespace.trim(value);
                    elementName = makeAttributeValueTemplate(nameAtt, att);
                    break ;
                  case "namespace":
                    namespaceAtt = value;
                    @namespace = makeAttributeValueTemplate(namespaceAtt, att);
                    break ;
                  case "validation":
                    validationAtt = Saxon.Hej.value.Whitespace.trim(value);
                    break ;
                  case "type":
                    typeAtt = Saxon.Hej.value.Whitespace.trim(value);
                    break ;
                  case "inherit-namespaces":
                    inheritAtt = Saxon.Hej.value.Whitespace.trim(value);
                    break ;
                  case "use-attribute-sets":
                    use = value;
                    break ;
                  default:
                    checkUnknownAttribute(attName);
                    break ;
                }
            }
            if ((nameAtt == null)) {
                reportAbsence("name");
            } else {
                if ((elementName is Saxon.Hej.expr.StringLiteral)) {
                    if (!(Saxon.Hej.om.NameChecker.isQName(((Saxon.Hej.expr.StringLiteral)(elementName)).getGroundedValue().codePoints()))) {
                        compileError((("Element name "+Saxon.Hej.trans.Err.wrap(((Saxon.Hej.expr.StringLiteral)(elementName)).stringify()))+" is not a valid QName"), "XTDE0820");
                        elementName = new Saxon.Hej.expr.StringLiteral("saxon-error-element");
                    }
                }
            }
            if ((namespaceAtt != null)) {
                if ((@namespace is Saxon.Hej.expr.StringLiteral)) {
                    if (!(Saxon.Hej.lib.StandardURIChecker.getInstance().isValidURI(((Saxon.Hej.expr.StringLiteral)(@namespace)).stringify()))) {
                        compileError("The value of the namespace attribute must be a valid URI", "XTDE0835");
                    }
                }
            }
            if ((validationAtt != null)) {
                validation = validateValidationAttribute(validationAtt);
            } else {
                validation = getDefaultValidation();
            }
            if ((typeAtt != null)) {
                if (!(isSchemaAware())) {
                    compileError("The @type attribute is available only with a schema-aware XSLT processor", "XTSE1660");
                }
                schemaType = getSchemaType(typeAtt);
                validation = Saxon.Hej.lib.Validation.BY_TYPE;
            }
            if (((typeAtt != null) && (validationAtt != null))) {
                compileError("The @validation and @type attributes are mutually exclusive", "XTSE1505");
            }
            if ((inheritAtt != null)) {
                inheritNamespaces = processBooleanAttribute("inherit-namespaces", inheritAtt);
            }
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            if ((use != null)) {
                attributeSets = getUsedAttributeSets(use);
            }
            elementName = typeCheck("name", elementName);
            @namespace = typeCheck("namespace", @namespace);
        }
        public override Saxon.Hej.expr.Expression compile(Saxon.Hej.style.Compilation exec, Saxon.Hej.style.ComponentDeclaration decl) {
            if ((elementName is Saxon.Hej.expr.StringLiteral)) {
                string qName = ((Saxon.Hej.expr.StringLiteral)(elementName)).stringify();
                string[] parts;
                try {
                    parts = Saxon.Hej.om.NameChecker.getQNameParts(qName);
                } catch (Saxon.Hej.om.QNameException e) {
                    compileErrorInAttribute(("Invalid element name: "+qName), "XTDE0820", "name");
                    return null;
                }
                string nsuri = null;
                if ((@namespace is Saxon.Hej.expr.StringLiteral)) {
                    nsuri = ((Saxon.Hej.expr.StringLiteral)(@namespace)).stringify();
                    if (nsuri.Length==0) {
                        parts[0] = "";
                    }
                } else {
                    if ((@namespace == null)) {
                        nsuri = getURIForPrefix(parts[0], true);
                        if ((nsuri == null)) {
                            undeclaredNamespaceError(parts[0], "XTDE0830", "name");
                        }
                    }
                }
                if ((nsuri != null)) {
                    Saxon.Hej.om.FingerprintedQName qn = new Saxon.Hej.om.FingerprintedQName(parts[0], nsuri, parts[1]);
                    qn.obtainFingerprint(getNamePool());
                    Saxon.Hej.expr.instruct.FixedElement FixedElementInst = new Saxon.Hej.expr.instruct.FixedElement(qn, Saxon.Hej.om.NamespaceMap.emptyMap(), inheritNamespaces, true, schemaType, validation);
                    FixedElementInst.setLocation(allocateLocation());
                    return compileContentExpression(exec, decl, FixedElementInst);
                }
            }
            Saxon.Hej.expr.instruct.ComputedElement inst = new Saxon.Hej.expr.instruct.ComputedElement(elementName, @namespace, schemaType, validation, inheritNamespaces, false);
            inst.setLocation(allocateLocation());
            return compileContentExpression(exec, decl, inst);
        }
        private Saxon.Hej.expr.Expression compileContentExpression(Saxon.Hej.style.Compilation exec, Saxon.Hej.style.ComponentDeclaration decl, Saxon.Hej.expr.instruct.ElementCreator inst) {
            Saxon.Hej.expr.Expression content = compileSequenceConstructor(exec, decl, true);
            if ((attributeSets != null)) {
                Saxon.Hej.expr.Expression use = Saxon.Hej.expr.instruct.UseAttributeSet.makeUseAttributeSets(attributeSets, this);
                if ((content == null)) {
                    content = use;
                } else {
                    content = Saxon.Hej.expr.instruct.Block.makeBlock(use, content);
                    content.setLocation(allocateLocation());
                }
            }
            if ((content == null)) {
                content = Saxon.Hej.expr.Literal.makeEmptySequence();
            }
            inst.setContentExpression(content);
            return inst;
        }
    
    }
}