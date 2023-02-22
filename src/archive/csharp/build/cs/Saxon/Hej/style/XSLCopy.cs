using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class XSLCopy : Saxon.Hej.style.StyleElement {
        private string use;
        private Saxon.Hej.om.StructuredQName[] attributeSets = null;
        private bool copyNamespaces = true;
        private bool inheritNamespaces = true;
        private int validationAction = Saxon.Hej.lib.Validation.PRESERVE;
        private Saxon.Hej.type.SchemaType schemaType = null;
        private Saxon.Hej.expr.Expression select = null;
        private bool selectSpecified = false;
        public override bool isInstruction() {
            return true;
        }
        protected internal override bool mayContainSequenceConstructor() {
            return true;
        }
        protected internal override void prepareAttributes() {
            string copyNamespacesAtt = null;
            string validationAtt = null;
            string typeAtt = null;
            string inheritAtt = null;
            Saxon.Hej.om.AttributeInfo selectAtt = null;
            foreach (Saxon.Hej.om.AttributeInfo att in attributes()) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                string f = attName.getDisplayName();
                string value = att.getValue();
                switch (f) {
                  case "use-attribute-sets":
                    use = value;
                    break ;
                  case "copy-namespaces":
                    copyNamespacesAtt = Saxon.Hej.value.Whitespace.trim(value);
                    break ;
                  case "select":
                    selectAtt = att;
                    break ;
                  case "type":
                    typeAtt = Saxon.Hej.value.Whitespace.trim(value);
                    break ;
                  case "validation":
                    validationAtt = Saxon.Hej.value.Whitespace.trim(value);
                    break ;
                  case "inherit-namespaces":
                    inheritAtt = Saxon.Hej.value.Whitespace.trim(value);
                    break ;
                  default:
                    checkUnknownAttribute(attName);
                    break ;
                }
            }
            if ((copyNamespacesAtt != null)) {
                copyNamespaces = processBooleanAttribute("copy-namespaces", copyNamespacesAtt);
            }
            if (((typeAtt != null) && (validationAtt != null))) {
                compileError("The type and validation attributes must not both be specified", "XTSE1505");
            }
            if ((validationAtt != null)) {
                validationAction = validateValidationAttribute(validationAtt);
            } else {
                validationAction = getDefaultValidation();
            }
            if ((typeAtt != null)) {
                schemaType = getSchemaType(typeAtt);
                if (!(isSchemaAware())) {
                    compileError("The @type attribute is available only with a schema-aware XSLT processor", "XTSE1660");
                }
                validationAction = Saxon.Hej.lib.Validation.BY_TYPE;
            }
            if ((inheritAtt != null)) {
                inheritNamespaces = processBooleanAttribute("inherit-namespaces", inheritAtt);
            }
            if ((selectAtt != null)) {
                select = makeExpression(selectAtt.getValue(), selectAtt);
                selectSpecified = true;
            }
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            if ((use != null)) {
                attributeSets = getUsedAttributeSets(use);
            }
            if ((select == null)) {
                select = new Saxon.Hej.expr.ContextItemExpression();
                select.setLocation(allocateLocation());
            }
        }
        public override Saxon.Hej.expr.Expression compile(Saxon.Hej.style.Compilation exec, Saxon.Hej.style.ComponentDeclaration decl) {
            select = typeCheck("select", select);
            try {
                Saxon.Ejava.util.function.Supplier<Saxon.Hej.expr.parser.RoleDiagnostic> role = () => new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.INSTRUCTION, "xsl:copy/select", 0, "XTTE3180");
                select = getConfiguration().getTypeChecker(false).staticTypeCheck(select, Saxon.Hej.value.SequenceType.OPTIONAL_ITEM, role, makeExpressionVisitor());
            } catch (Saxon.Hej.trans.XPathException err) {
                compileError(err);
            }
            Saxon.Hej.expr.Expression content = compileSequenceConstructor(exec, decl, true);
            if ((attributeSets != null)) {
                Saxon.Hej.expr.Expression use = Saxon.Hej.expr.instruct.UseAttributeSet.makeUseAttributeSets(attributeSets, this);
                Saxon.Hej.expr.Expression condition = new Saxon.Hej.expr.InstanceOfExpression(new Saxon.Hej.expr.ContextItemExpression(), Saxon.Hej.value.SequenceType.makeSequenceType(Saxon.Hej.pattern.NodeKindTest.ELEMENT, Saxon.Hej.expr.StaticProperty.EXACTLY_ONE));
                Saxon.Hej.expr.Expression choice = Saxon.Hej.expr.instruct.Choose.makeConditional(condition, use);
                if ((content == null)) {
                    content = choice;
                } else {
                    content = Saxon.Hej.expr.instruct.Block.makeBlock(choice, content);
                    content.setLocation(allocateLocation());
                }
            }
            if ((content == null)) {
                content = Saxon.Hej.expr.Literal.makeEmptySequence();
            }
            Saxon.Hej.expr.instruct.Copy inst = new Saxon.Hej.expr.instruct.Copy(copyNamespaces, inheritNamespaces, schemaType, validationAction);
            inst.setContentExpression(content);
            if (selectSpecified) {
                return new Saxon.Hej.expr.instruct.ForEach(select, inst);
            } else {
                return inst;
            }
        }
    
    }
}