using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class XSLDocument : Saxon.Hej.style.StyleElement {
        private int validationAction = Saxon.Hej.lib.Validation.STRIP;
        private Saxon.Hej.type.SchemaType schemaType = null;
        public override bool isInstruction() {
            return true;
        }
        protected internal override bool mayContainSequenceConstructor() {
            return true;
        }
        protected internal override void prepareAttributes() {
            string validationAtt = null;
            string typeAtt = null;
            foreach (Saxon.Hej.om.AttributeInfo att in attributes()) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                string f = attName.getDisplayName();
                string value = att.getValue();
                if (f.Equals("validation")) {
                    validationAtt = Saxon.Hej.value.Whitespace.trim(value);
                } else {
                    if (f.Equals("type")) {
                        typeAtt = Saxon.Hej.value.Whitespace.trim(value);
                    } else {
                        checkUnknownAttribute(attName);
                    }
                }
            }
            if ((validationAtt == null)) {
                validationAction = getDefaultValidation();
            } else {
                validationAction = validateValidationAttribute(validationAtt);
            }
            if ((typeAtt != null)) {
                if (!(isSchemaAware())) {
                    compileError("The @type attribute is available only with a schema-aware XSLT processor", "XTSE1660");
                }
                schemaType = getSchemaType(typeAtt);
                validationAction = Saxon.Hej.lib.Validation.BY_TYPE;
            }
            if (((typeAtt != null) && (validationAtt != null))) {
                compileError("The @validation and @type attributes are mutually exclusive", "XTSE1505");
            }
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
        }
        public override Saxon.Hej.expr.Expression compile(Saxon.Hej.style.Compilation exec, Saxon.Hej.style.ComponentDeclaration decl) {
            Saxon.Hej.expr.instruct.DocumentInstr inst = new Saxon.Hej.expr.instruct.DocumentInstr(false, null);
            inst.setValidationAction(validationAction, schemaType);
            Saxon.Hej.expr.Expression b = compileSequenceConstructor(exec, decl, true);
            if ((b == null)) {
                b = Saxon.Hej.expr.Literal.makeEmptySequence();
            }
            inst.setContentExpression(b);
            inst.setLocation(allocateLocation());
            return inst;
        }
    
    }
}