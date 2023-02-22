using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.instruct {
    public sealed class FixedAttribute : Saxon.Hej.expr.instruct.AttributeCreator {
        private readonly Saxon.Hej.om.NodeName nodeName;
        public FixedAttribute(Saxon.Hej.om.NodeName nodeName, int validationAction, Saxon.Hej.type.SimpleType schemaType)  {
            this.nodeName = nodeName;
            setSchemaType(schemaType);
            setValidationAction(validationAction);
            setOptions(Saxon.Hej.@event.ReceiverOption.NONE);
        }
        public override int getInstructionNameCode() {
            return Saxon.Hej.om.StandardNames.XSL_ATTRIBUTE;
        }
        public override string getExpressionName() {
            return "att";
        }
        public Saxon.Hej.om.NodeName getAttributeName() {
            return nodeName;
        }
        public override void gatherProperties(Saxon.Ejava.util.function.BiConsumer<string,System.Object> consumer) {
            consumer("name", getAttributeName());
        }
        public override void localTypeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType) {
            if ((nodeName.Equals(Saxon.Hej.om.StandardNames.XML_ID_NAME) && !(getSelect().isCallOn(typeof(Saxon.Hej.functions.NormalizeSpace_1))))) {
                Saxon.Hej.expr.Expression select = Saxon.Hej.functions.SystemFunction.makeCall("normalize-space", getRetainedStaticContext(), getSelect());
                setSelect(select);
            }
            Saxon.Hej.Configuration config = visitor.getConfiguration();
            Saxon.Hej.lib.ConversionRules rules = config.getConversionRules();
            Saxon.Hej.type.SimpleType schemaType = ((/*1430*/Saxon.Hej.type.SimpleType)getSchemaType());
            string errorCode = "XTTE1540";
            if ((schemaType == null)) {
                int validation = getValidationAction();
                if ((validation == Saxon.Hej.lib.Validation.STRICT)) {
                    Saxon.Hej.type.SchemaDeclaration decl = config.getAttributeDeclaration(nodeName.getStructuredQName());
                    if ((decl == null)) {
                        Saxon.Hej.trans.XPathException se = new Saxon.Hej.trans.XPathException(("Strict validation fails: there is no global attribute declaration for "+nodeName.getDisplayName()));
                        se.setErrorCode("XTTE1510");
                        se.setLocation(getLocation());
                        throw se;
                    }
                    schemaType = (Saxon.Hej.type.SimpleType)(decl.getType());
                    errorCode = "XTTE1510";
                } else {
                    if ((validation == Saxon.Hej.lib.Validation.LAX)) {
                        Saxon.Hej.type.SchemaDeclaration decl = config.getAttributeDeclaration(nodeName.getStructuredQName());
                        if ((decl != null)) {
                            schemaType = (Saxon.Hej.type.SimpleType)(decl.getType());
                            errorCode = "XTTE1515";
                        } else {
                            visitor.getStaticContext().issueWarning(("Lax validation has no effect: there is no global attribute declaration for "+nodeName.getDisplayName()), Saxon.Hej.trans.SaxonErrorCode.SXWN9031, getLocation());
                        }
                    }
                }
            }
            if (((Saxon.Hej.expr.Literal.isAtomic(getSelect()) && (schemaType != null)) && !(schemaType.isNamespaceSensitive()))) {
                Saxon.Hej.str.UnicodeString value = ((Saxon.Hej.expr.Literal)(getSelect())).getGroundedValue().getUnicodeStringValue();
                Saxon.Hej.type.ValidationFailure err = schemaType.validateContent(value, Saxon.Hej.expr.instruct.DummyNamespaceResolver.getInstance(), rules);
                if ((err != null)) {
                    Saxon.Hej.trans.XPathException se = new Saxon.Hej.trans.XPathException(((((("Attribute value "+Saxon.Hej.trans.Err.wrap(value, Saxon.Hej.trans.Err.VALUE))+" does not the match the required type ")+schemaType.getDescription())+". ")+err.getMessage()));
                    se.setErrorCode(errorCode);
                    throw se;
                }
            }
            if ((getSelect() is Saxon.Hej.expr.StringLiteral)) {
                bool special = false;
                string val = ((Saxon.Hej.expr.StringLiteral)(getSelect())).stringify();
                for (int k = 0; (k<val.Length); k++) {
                    char c = val[k];
                    if (((((((((int)(c)<33) || ((int)(c)>126)) || (c == '<')) || (c == '>')) || (c == '&')) || (c == '\"')) || (c == '\''))) {
                        special = true;
                        break ;
                    }
                }
                if (!(special)) {
                    setNoSpecialChars();
                }
            }
        }
        public int getAttributeFingerprint() {
            return nodeName.getFingerprint();
        }
        public override int getCardinality() {
            return Saxon.Hej.expr.StaticProperty.EXACTLY_ONE;
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.instruct.FixedAttribute exp = new Saxon.Hej.expr.instruct.FixedAttribute(nodeName, getValidationAction(), ((/*1430*/Saxon.Hej.type.SimpleType)getSchemaType()));
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, exp);
            exp.setSelect(getSelect().copy(rebindings));
            exp.setInstruction(isInstruction());
            return exp;
        }
        public override Saxon.Hej.om.NodeName evaluateNodeName(Saxon.Hej.expr.XPathContext context) {
            return nodeName;
        }
        public override void checkPermittedContents(Saxon.Hej.type.SchemaType parentType, bool whole) {
            int fp = nodeName.getFingerprint();
            if (((((fp == Saxon.Hej.om.StandardNames.XSI_TYPE) || (fp == Saxon.Hej.om.StandardNames.XSI_SCHEMA_LOCATION)) || (fp == Saxon.Hej.om.StandardNames.XSI_NIL)) || (fp == Saxon.Hej.om.StandardNames.XSI_NO_NAMESPACE_SCHEMA_LOCATION))) {
                return ;
            }
            if ((parentType is Saxon.Hej.type.SimpleType)) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(((("Attribute "+nodeName.getDisplayName())+" is not permitted in the content model of the simple type ")+parentType.getDescription()));
                err.setIsTypeError(true);
                err.setLocation(getLocation());
                err.setErrorCode((getPackageData().isXSLT() ? "XTTE1510" : "XQDY0027"));
                throw err;
            }
            Saxon.Hej.type.SchemaType type;
            try {
                type = ((Saxon.Hej.type.ComplexType)(parentType)).getAttributeUseType(nodeName.getStructuredQName());
            } catch (Saxon.Hej.type.SchemaException e) {
                throw new Saxon.Hej.trans.XPathException(e);
            }
            if ((type == null)) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(((("Attribute "+nodeName.getDisplayName())+" is not permitted in the content model of the complex type ")+parentType.getDescription()));
                err.setIsTypeError(true);
                err.setLocation(getLocation());
                err.setErrorCode((getPackageData().isXSLT() ? "XTTE1510" : "XQDY0027"));
                throw err;
            }
            try {
                getSelect().checkPermittedContents(type, true);
            } catch (Saxon.Hej.trans.XPathException e) {
                e.maybeSetLocation(getLocation());
                throw;
            }
        }
        public override Saxon.Hej.om.NodeInfo evaluateItem(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.tree.util.Orphan o = (Saxon.Hej.tree.util.Orphan)(base.evaluateItem(context));
            System.Diagnostics.Debug.Assert((o != null));
            validateOrphanAttribute(o, context);
            return o;
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("att", this);
            @out.emitAttribute("name", nodeName.getDisplayName());
            if (!(nodeName.getStructuredQName().hasURI(Saxon.Hej.om.NamespaceUri.NULL))) {
                @out.emitAttribute("nsuri", nodeName.getStructuredQName().getNamespaceUri().ToString());
            }
            if (((getValidationAction() != Saxon.Hej.lib.Validation.SKIP) && (getValidationAction() != Saxon.Hej.lib.Validation.BY_TYPE))) {
                @out.emitAttribute("validation", Saxon.Hej.lib.Validation.describe(getValidationAction()));
            }
            if ((((/*1430*/Saxon.Hej.type.SimpleType)getSchemaType()) != null)) {
                @out.emitAttribute("type", ((/*1430*/Saxon.Hej.type.SimpleType)getSchemaType()).getStructuredQName());
            }
            string flags = "";
            if (isLocal()) {
                flags += "l";
            }
            if (!(flags.Length==0)) {
                @out.emitAttribute("flags", flags);
            }
            getSelect().export(@out);
            @out.endElement();
        }
        public override string toShortString() {
            return (("attr{"+nodeName.getDisplayName())+"=...}");
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Hej.expr.instruct.FixedAttribute.FixedAttributeElaborator();
        }
        private class FixedAttributeElaborator : Saxon.Hej.expr.elab.SimpleNodePushElaborator {
            public override Saxon.Hej.expr.elab.PushEvaluator elaborateForPush() {
                Saxon.Hej.expr.instruct.FixedAttribute expr = (Saxon.Hej.expr.instruct.FixedAttribute)(getExpression());
                Saxon.Hej.om.NodeName name = expr.nodeName;
                Saxon.Hej.s9api.Location loc = expr.getLocation();
                int options = expr.getOptions();
                bool collapse = name.Equals(Saxon.Hej.om.StandardNames.XML_ID_NAME);
                if ((((collapse || (((/*1430*/Saxon.Hej.type.SimpleType)expr.getSchemaType()) != null)) || (expr.getValidationAction() == Saxon.Hej.lib.Validation.STRICT)) || (expr.getValidationAction() == Saxon.Hej.lib.Validation.LAX))) {
                    Saxon.Hej.expr.elab.UnicodeStringEvaluator contentEval = expr.getSelect().makeElaborator().elaborateForUnicodeString(true);
                    return (output, context) => {
                        Saxon.Hej.str.UnicodeString content = /*Delegate.eval()*/contentEval(context);
                        Saxon.Hej.type.SimpleType ann = expr.validate(name, content, context);
                        if (collapse) {
                            content = Saxon.Hej.value.Whitespace.collapseWhitespace(content);
                        }
                        try {
                            output.attribute(name, ann, content.ToString(), loc, options);
                        } catch (Saxon.Hej.trans.XPathException err) {
                            throw dynamicError(loc, err, context);
                        }
                        return null;
                    } ;
                } else {
                    Saxon.Hej.expr.elab.StringEvaluator contentEval = expr.getSelect().makeElaborator().elaborateForString(true);
                    return (output, context) => {
                        string content = /*Delegate.eval()*/contentEval(context);
                        try {
                            output.attribute(name, Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC, content, loc, options);
                        } catch (Saxon.Hej.trans.XPathException err) {
                            throw Saxon.Hej.expr.instruct.Instruction.dynamicError(loc, err, context);
                        }
                        return null;
                    } ;
                }
            }
            public override Saxon.Hej.expr.elab.ItemEvaluator elaborateForItem() {
                Saxon.Hej.expr.instruct.FixedAttribute expr = (Saxon.Hej.expr.instruct.FixedAttribute)(getExpression());
                if ((((((/*1430*/Saxon.Hej.type.SimpleType)expr.getSchemaType()) != null) || (expr.getValidationAction() == Saxon.Hej.lib.Validation.STRICT)) || (expr.getValidationAction() == Saxon.Hej.lib.Validation.LAX))) {
                    Saxon.Hej.expr.elab.ItemEvaluator superEval = base.elaborateForItem();
                    return context => {
                        Saxon.Hej.tree.util.Orphan o = (Saxon.Hej.tree.util.Orphan)(/*Delegate.eval()*/superEval(context));
                        System.Diagnostics.Debug.Assert((o != null));
                        expr.validateOrphanAttribute(o, context);
                        return o;
                    } ;
                } else {
                    return base.elaborateForItem();
                }
            }
        
        }
    
    }
}