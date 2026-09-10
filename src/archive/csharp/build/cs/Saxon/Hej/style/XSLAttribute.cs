using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public sealed class XSLAttribute : Saxon.Hej.style.XSLLeafNodeConstructor {
        private Saxon.Hej.expr.Expression attributeName;
        private Saxon.Hej.expr.Expression separator;
        private Saxon.Hej.expr.Expression @namespace = null;
        private int validationAction = Saxon.Hej.lib.Validation.PRESERVE;
        private Saxon.Hej.type.SimpleType schemaType;
        protected internal override void prepareAttributes() {
            string nameAtt = null;
            string namespaceAtt = null;
            string selectAtt = null;
            string separatorAtt = null;
            string validationAtt = null;
            string typeAtt = null;
            foreach (Saxon.Hej.om.AttributeInfo att in attributes()) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                string value = att.getValue();
                string f = attName.getDisplayName();
                switch (f) {
                  case "name":
                    nameAtt = Saxon.Hej.value.Whitespace.trim(value);
                    attributeName = makeAttributeValueTemplate(nameAtt, att);
                    break ;
                  case "namespace":
                    namespaceAtt = Saxon.Hej.value.Whitespace.trim(value);
                    @namespace = makeAttributeValueTemplate(namespaceAtt, att);
                    break ;
                  case "select":
                    selectAtt = value;
                    select = makeExpression(selectAtt, att);
                    break ;
                  case "separator":
                    separatorAtt = value;
                    separator = makeAttributeValueTemplate(separatorAtt, att);
                    break ;
                  case "validation":
                    validationAtt = Saxon.Hej.value.Whitespace.trim(value);
                    break ;
                  case "type":
                    typeAtt = Saxon.Hej.value.Whitespace.trim(value);
                    break ;
                  default:
                    checkUnknownAttribute(attName);
                    break ;
                }
            }
            if ((nameAtt == null)) {
                reportAbsence("name");
                return ;
            }
            if ((attributeName is Saxon.Hej.expr.StringLiteral)) {
                if (!(Saxon.Hej.om.NameChecker.isQName(((Saxon.Hej.expr.StringLiteral)(attributeName)).getGroundedValue().codePoints()))) {
                    invalidAttributeName((("Attribute name "+Saxon.Hej.trans.Err.wrap(nameAtt))+" is not a valid QName"));
                }
                if (nameAtt.Equals("xmlns")) {
                    if ((@namespace == null)) {
                        invalidAttributeName("Invalid attribute name: xmlns");
                    }
                }
                if (nameAtt.StartsWith("xmlns:")) {
                    if ((namespaceAtt == null)) {
                        invalidAttributeName(("Invalid attribute name: "+Saxon.Hej.trans.Err.wrap(nameAtt)));
                    } else {
                        nameAtt = nameAtt.Substring(6);
                        attributeName = new Saxon.Hej.expr.StringLiteral(nameAtt);
                    }
                }
            }
            if ((namespaceAtt != null)) {
                if ((@namespace is Saxon.Hej.expr.StringLiteral)) {
                    if (!(Saxon.Hej.lib.StandardURIChecker.getInstance().isValidURI(((Saxon.Hej.expr.StringLiteral)(@namespace)).stringify()))) {
                        compileError("The value of the namespace attribute must be a valid URI", "XTDE0865");
                    }
                }
            }
            if ((separatorAtt == null)) {
                if ((selectAtt == null)) {
                    separator = new Saxon.Hej.expr.StringLiteral(Saxon.Hej.value.StringValue.EMPTY_STRING);
                } else {
                    separator = new Saxon.Hej.expr.StringLiteral(Saxon.Hej.value.StringValue.SINGLE_SPACE);
                }
            }
            if ((validationAtt != null)) {
                validationAction = validateValidationAttribute(validationAtt);
            } else {
                validationAction = getDefaultValidation();
            }
            if ((typeAtt != null)) {
                if (!(isSchemaAware())) {
                    compileError("The @type attribute is available only with a schema-aware XSLT processor", "XTSE1660");
                } else {
                    Saxon.Hej.type.SchemaType type = getSchemaType(typeAtt);
                    if ((type == null)) {
                        compileError(("Unknown attribute type "+typeAtt), "XTSE1520");
                    } else {
                        if (type.isSimpleType()) {
                            schemaType = (Saxon.Hej.type.SimpleType)(type);
                        } else {
                            compileError("Type annotation for attributes must be a simple type", "XTSE1530");
                        }
                    }
                    validationAction = Saxon.Hej.lib.Validation.BY_TYPE;
                }
            }
            if (((typeAtt != null) && (validationAtt != null))) {
                compileError("The validation and type attributes are mutually exclusive", "XTSE1505");
                validationAction = getDefaultValidation();
                schemaType = null;
            }
        }
        private void invalidAttributeName(string message) {
            compileErrorInAttribute(message, "XTDE0850", "name");
            attributeName = new Saxon.Hej.expr.StringLiteral("saxon-error-attribute");
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            if ((schemaType != null)) {
                if (schemaType.isNamespaceSensitive()) {
                    compileErrorInAttribute(("Validation at attribute level must not specify a "+"namespace-sensitive type (xs:QName or xs:NOTATION)"), "XTTE1545", "type");
                }
            }
            attributeName = typeCheck("name", attributeName);
            @namespace = typeCheck("namespace", @namespace);
            select = typeCheck("select", select);
            separator = typeCheck("separator", separator);
            base.validate(decl);
        }
        protected internal override string getErrorCodeForSelectPlusContent() {
            return "XTSE0840";
        }
        public override Saxon.Hej.expr.instruct.Instruction compile(Saxon.Hej.style.Compilation compilation, Saxon.Hej.style.ComponentDeclaration decl) {
            Saxon.Hej.om.NamespaceResolver nsContext = null;
            if ((attributeName is Saxon.Hej.expr.StringLiteral)) {
                string qName = Saxon.Hej.value.Whitespace.trim(((Saxon.Hej.expr.StringLiteral)(attributeName)).stringify());
                string[] parts;
                try {
                    parts = Saxon.Hej.om.NameChecker.getQNameParts(qName);
                } catch (Saxon.Hej.om.QNameException e) {
                    return null;
                }
                if ((@namespace == null)) {
                    string nsuri = "";
                    if (!(parts[0].Equals(""))) {
                        nsuri = getURIForPrefix(parts[0], false);
                        if ((nsuri == null)) {
                            undeclaredNamespaceError(parts[0], "XTDE0860", "name");
                            return null;
                        }
                    }
                    Saxon.Hej.om.NodeName attributeName = new Saxon.Hej.om.FingerprintedQName(parts[0], nsuri, parts[1]);
                    attributeName.obtainFingerprint(getNamePool());
                    Saxon.Hej.expr.instruct.FixedAttribute instruction = new Saxon.Hej.expr.instruct.FixedAttribute(attributeName, validationAction, schemaType);
                    instruction.setInstruction(true);
                    instruction.setLocation(allocateLocation());
                    compileContent(compilation, decl, instruction, separator);
                    return instruction;
                } else {
                    if ((@namespace is Saxon.Hej.expr.StringLiteral)) {
                        string nsuri = ((Saxon.Hej.expr.StringLiteral)(@namespace)).stringify();
                        if (nsuri.Equals("")) {
                            parts[0] = "";
                        } else {
                            if (parts[0].Equals("")) {
                                Saxon.Hej.tree.iter.AxisIterator iter = iterateAxis(Saxon.Hej.om.AxisInfo.NAMESPACE);
                                Saxon.Hej.om.NodeInfo ns;
                                while (((ns = ((/*1430*/Saxon.Hej.om.NodeInfo)iter.next())) != null)) {
                                    if (ns.getUnicodeStringValue().Equals(nsuri)) {
                                        parts[0] = ns.getLocalPart();
                                        break ;
                                    }
                                }
                                if (parts[0].Equals("")) {
                                    string p = getNamePool().suggestPrefixForURI(((Saxon.Hej.expr.StringLiteral)(@namespace)).stringify());
                                    if ((p != null)) {
                                        parts[0] = p;
                                    }
                                }
                                if (parts[0].Equals("")) {
                                    parts[0] = "ns0";
                                }
                            }
                        }
                        Saxon.Hej.om.NodeName nodeName = new Saxon.Hej.om.FingerprintedQName(parts[0], nsuri, parts[1]);
                        nodeName.obtainFingerprint(getNamePool());
                        Saxon.Hej.expr.instruct.FixedAttribute fixedAtt = new Saxon.Hej.expr.instruct.FixedAttribute(nodeName, validationAction, schemaType);
                        fixedAtt.setInstruction(true);
                        compileContent(compilation, decl, fixedAtt, separator);
                        return fixedAtt;
                    }
                }
            } else {
                if ((@namespace == null)) {
                    nsContext = getAllNamespaces();
                }
            }
            Saxon.Hej.expr.instruct.ComputedAttribute inst = new Saxon.Hej.expr.instruct.ComputedAttribute(attributeName, @namespace, nsContext, validationAction, schemaType, false);
            inst.setInstruction(true);
            inst.setLocation(allocateLocation());
            compileContent(compilation, decl, inst, separator);
            return inst;
        }
    
    }
}