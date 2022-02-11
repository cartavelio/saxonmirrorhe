using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public sealed class XSLCopyOf : Saxon.Hej.style.StyleElement {
        private Saxon.Hej.expr.Expression select;
        private bool copyNamespaces;
        private bool copyAccumulators;
        private int validation = Saxon.Hej.lib.Validation.PRESERVE;
        private Saxon.Hej.type.SchemaType schemaType;
        public override bool isInstruction() {
            return true;
        }
        protected internal override void prepareAttributes() {
            string selectAtt = null;
            string copyNamespacesAtt = null;
            string copyAccumulatorsAtt = null;
            string validationAtt = null;
            string typeAtt = null;
            foreach (Saxon.Hej.om.AttributeInfo att in attributes()) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                string value = att.getValue();
                string f = attName.getDisplayName();
                if (f.Equals("select")) {
                    selectAtt = value;
                    select = makeExpression(selectAtt, att);
                } else {
                    if (f.Equals("copy-namespaces")) {
                        copyNamespacesAtt = Saxon.Hej.value.Whitespace.trim(value);
                    } else {
                        if (f.Equals("copy-accumulators")) {
                            copyAccumulatorsAtt = Saxon.Hej.value.Whitespace.trim(value);
                        } else {
                            if (f.Equals("validation")) {
                                validationAtt = Saxon.Hej.value.Whitespace.trim(value);
                            } else {
                                if (f.Equals("type")) {
                                    typeAtt = Saxon.Hej.value.Whitespace.trim(value);
                                } else {
                                    if ((attName.getLocalPart().Equals("read-once") && attName.hasURI(Saxon.Hej.lib.NamespaceConstant.SAXON))) {
                                        compileError("The saxon:read-once attribute is no longer available - use xsl:stream");
                                    } else {
                                        checkUnknownAttribute(attName);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if ((selectAtt == null)) {
                reportAbsence("select");
            }
            if ((copyAccumulatorsAtt == null)) {
                copyAccumulators = false;
            } else {
                copyAccumulators = processBooleanAttribute("copy-accumulators", copyAccumulatorsAtt);
                if ((copyAccumulators && isConstructingComplexContent())) {
                    compileWarning((("Copying accumulators is pointless when the copied element "+"is immediately attached to a new parent, since that action ")+"will lose the accumulator values"), Saxon.Hej.trans.SaxonErrorCode.SXWN9017);
                    copyAccumulators = false;
                }
            }
            if ((copyNamespacesAtt == null)) {
                copyNamespaces = true;
            } else {
                copyNamespaces = processBooleanAttribute("copy-namespaces", copyNamespacesAtt);
            }
            if ((validationAtt != null)) {
                validation = validateValidationAttribute(validationAtt);
            } else {
                validation = getDefaultValidation();
            }
            if ((typeAtt != null)) {
                schemaType = getSchemaType(typeAtt);
                if (!(isSchemaAware())) {
                    compileError("The @type attribute is available only with a schema-aware XSLT processor", "XTSE1660");
                }
                validation = Saxon.Hej.lib.Validation.BY_TYPE;
            }
            if (((typeAtt != null) && (validationAtt != null))) {
                compileError("The @validation and @type attributes are mutually exclusive", "XTSE1505");
            }
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            checkEmpty();
            select = typeCheck("select", select);
        }
        public override Saxon.Hej.expr.Expression compile(Saxon.Hej.style.Compilation exec, Saxon.Hej.style.ComponentDeclaration decl) {
            Saxon.Hej.expr.instruct.CopyOf inst = new Saxon.Hej.expr.instruct.CopyOf(select, copyNamespaces, validation, schemaType, false);
            inst.setCopyAccumulators(copyAccumulators);
            inst.setSchemaAware(exec.isSchemaAware());
            return inst;
        }
    
    }
}