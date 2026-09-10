using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.instruct {
    public sealed class ComputedAttribute : Saxon.Hej.expr.instruct.AttributeCreator {
        private readonly Saxon.Hej.expr.Operand nameOp;
        private Saxon.Hej.expr.Operand namespaceOp;
        private readonly bool allowNameAsQName;
        public ComputedAttribute(Saxon.Hej.expr.Expression attributeName, Saxon.Hej.expr.Expression @namespace, int validationAction, Saxon.Hej.type.SimpleType schemaType, bool allowNameAsQName)  {
            nameOp = new Saxon.Hej.expr.Operand(this, attributeName, Saxon.Hej.expr.OperandRole.SINGLE_ATOMIC);
            if ((@namespace != null)) {
                namespaceOp = new Saxon.Hej.expr.Operand(this, @namespace, Saxon.Hej.expr.OperandRole.SINGLE_ATOMIC);
            }
            setSchemaType(schemaType);
            setValidationAction(validationAction);
            setOptions(Saxon.Hej.@event.ReceiverOption.NONE);
            this.allowNameAsQName = allowNameAsQName;
        }
        public override void setRejectDuplicates() {
            setOptions((getOptions()|Saxon.Hej.@event.ReceiverOption.REJECT_DUPLICATES));
        }
        public override int getInstructionNameCode() {
            return Saxon.Hej.om.StandardNames.XSL_ATTRIBUTE;
        }
        public Saxon.Hej.expr.Expression getNameExp() {
            return nameOp.getChildExpression();
        }
        public Saxon.Hej.expr.Expression getNamespaceExp() {
            return ((namespaceOp == null) ? null : namespaceOp.getChildExpression());
        }
        public void setNameExp(Saxon.Hej.expr.Expression attributeName) {
            nameOp.setChildExpression(attributeName);
        }
        public void setNamespace(Saxon.Hej.expr.Expression @namespace) {
            if ((@namespace != null)) {
                if ((namespaceOp == null)) {
                    namespaceOp = new Saxon.Hej.expr.Operand(this, @namespace, Saxon.Hej.expr.OperandRole.SINGLE_ATOMIC);
                } else {
                    namespaceOp.setChildExpression(@namespace);
                }
            }
        }
        public override System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> operands() {
            return operandSparseList(selectOp, nameOp, namespaceOp);
        }
        public Saxon.Hej.om.NamespaceResolver getNamespaceResolver() {
            return getRetainedStaticContext();
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            return Saxon.Hej.pattern.NodeKindTest.ATTRIBUTE;
        }
        public override int getCardinality() {
            return Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_ONE;
        }
        public bool isAllowNameAsQName() {
            return allowNameAsQName;
        }
        protected internal override int computeSpecialProperties() {
            return (base.computeSpecialProperties()|Saxon.Hej.expr.StaticProperty.SINGLE_DOCUMENT_NODESET);
        }
        public override void localTypeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType) {
            nameOp.typeCheck(visitor, contextItemType);
            Saxon.Ejava.util.function.Supplier<Saxon.Hej.expr.parser.RoleDiagnostic> role = () => new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.INSTRUCTION, "attribute/name", 0);
            Saxon.Hej.Configuration config = visitor.getConfiguration();
            Saxon.Hej.type.TypeHierarchy th = config.getTypeHierarchy();
            if (allowNameAsQName) {
                setNameExp(config.getTypeChecker(false).staticTypeCheck(getNameExp(), Saxon.Hej.value.SequenceType.SINGLE_ATOMIC, role, visitor));
                Saxon.Hej.type.ItemType nameItemType = getNameExp().getItemType();
                bool maybeString = ((th.relationship(nameItemType, Saxon.Hej.type.BuiltInAtomicType.STRING) != Saxon.Hej.type.Affinity.DISJOINT) || (th.relationship(nameItemType, Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC) != Saxon.Hej.type.Affinity.DISJOINT));
                bool maybeQName = (th.relationship(nameItemType, Saxon.Hej.type.BuiltInAtomicType.QNAME) != Saxon.Hej.type.Affinity.DISJOINT);
                if (!(((maybeString || maybeQName)))) {
                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("The attribute name must be either an xs:string, an xs:QName, or untyped atomic");
                    err.setErrorCode("XPTY0004");
                    err.setIsTypeError(true);
                    err.setLocation(getLocation());
                    throw err;
                }
            } else {
                if (!(th.isSubType(getNameExp().getItemType(), Saxon.Hej.type.BuiltInAtomicType.STRING))) {
                    setNameExp(Saxon.Hej.functions.SystemFunction.makeCall("string", getRetainedStaticContext(), getNameExp()));
                }
            }
            if ((getNamespaceExp() != null)) {
                namespaceOp.typeCheck(visitor, contextItemType);
            }
            if (Saxon.Hej.expr.Literal.isAtomic(getNameExp())) {
                try {
                    Saxon.Hej.value.AtomicValue val = (Saxon.Hej.value.AtomicValue)(((Saxon.Hej.expr.Literal)(getNameExp())).getGroundedValue());
                    if ((val is Saxon.Hej.value.StringValue)) {
                        string[] parts = Saxon.Hej.om.NameChecker.checkQNameParts(val.getStringValue());
                        if ((getNamespaceExp() == null)) {
                            Saxon.Hej.om.NamespaceUri uri = getNamespaceResolver().getURIForPrefix(parts[0], false);
                            if ((uri == null)) {
                                Saxon.Hej.trans.XPathException se = new Saxon.Hej.trans.XPathException((("Prefix "+parts[0])+" has not been declared"));
                                if (isXSLT()) {
                                    se.setErrorCode("XTDE0860");
                                    se.setIsStaticError(true);
                                    throw se;
                                } else {
                                    se.setErrorCode("XQDY0074");
                                    se.setIsStaticError(false);
                                    throw se;
                                }
                            }
                            setNamespace(new Saxon.Hej.expr.StringLiteral(uri.ToString()));
                        }
                    }
                } catch (Saxon.Hej.trans.XPathException e) {
                    if (((e.getErrorCodeQName() == null) || e.getErrorCodeLocalPart().Equals("FORG0001"))) {
                        e.setErrorCode((isXSLT() ? "XTDE0850" : "XQDY0074"));
                    }
                    e.maybeSetLocation(getLocation());
                    e.setIsStaticError(true);
                    throw;
                }
            }
        }
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType) {
            Saxon.Hej.expr.Expression exp = base.optimize(visitor, contextItemType);
            if ((exp != this)) {
                return exp;
            }
            if (((getNameExp() is Saxon.Hej.expr.Literal) && (((getNamespaceExp() == null) || (getNamespaceExp() is Saxon.Hej.expr.Literal))))) {
                Saxon.Hej.expr.XPathContext context = visitor.getStaticContext().makeEarlyEvaluationContext();
                Saxon.Hej.om.NodeName nc = evaluateNodeName(context);
                Saxon.Hej.expr.instruct.FixedAttribute fa = new Saxon.Hej.expr.instruct.FixedAttribute(nc, getValidationAction(), ((/*1430*/Saxon.Hej.type.SimpleType)getSchemaType()));
                fa.setSelect(getSelect());
                return fa;
            }
            return this;
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.instruct.ComputedAttribute exp = new Saxon.Hej.expr.instruct.ComputedAttribute(((getNameExp() == null) ? null : getNameExp().copy(rebindings)), ((getNamespaceExp() == null) ? null : getNamespaceExp().copy(rebindings)), getValidationAction(), ((/*1430*/Saxon.Hej.type.SimpleType)getSchemaType()), allowNameAsQName);
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, exp);
            exp.setSelect(getSelect().copy(rebindings));
            exp.setInstruction(isInstruction());
            return exp;
        }
        public override void checkPermittedContents(Saxon.Hej.type.SchemaType parentType, bool whole) {
            if ((parentType is Saxon.Hej.type.SimpleType)) {
                string msg = "Attributes are not permitted here: ";
                if (parentType.isAnonymousType()) {
                    msg += "the containing element is defined to have a simple type";
                } else {
                    msg += ("the containing element is of simple type "+parentType.getDescription());
                }
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(msg);
                err.setIsTypeError(true);
                err.setLocation(getLocation());
                throw err;
            }
        }
        public override Saxon.Hej.om.NodeName evaluateNodeName(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.om.Item nameValue = getNameExp().evaluateItem(context);
            return validateNodeName(nameValue, context);
        }
        private Saxon.Hej.om.NodeName validateNodeName(Saxon.Hej.om.Item nameValue, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.om.NamePool pool = context.getNamePool();
            string prefix;
            string localName;
            Saxon.Hej.om.NamespaceUri uri = null;
            if ((nameValue is Saxon.Hej.value.StringValue)) {
                string rawName = nameValue.getStringValue();
                rawName = Saxon.Hej.value.Whitespace.trim(rawName);
                if ((rawName.StartsWith("Q{") && allowNameAsQName)) {
                    try {
                        Saxon.Hej.om.StructuredQName qn = Saxon.Hej.om.StructuredQName.fromEQName(rawName);
                        prefix = "";
                        localName = qn.getLocalPart();
                        uri = qn.getNamespaceUri();
                    } catch (Saxon.Ejava.lang.IllegalArgumentException e) {
                        throw new Saxon.Hej.trans.XPathException(("Invalid EQName in computed attribute constructor: "+e.Message), "XQDY0074");
                    }
                    if (!(Saxon.Hej.om.NameChecker.isValidNCName(localName))) {
                        throw new Saxon.Hej.trans.XPathException("Local part of EQName in computed attribute constructor is invalid", "XQDY0074");
                    }
                } else {
                    try {
                        string[] parts = Saxon.Hej.om.NameChecker.getQNameParts(rawName);
                        prefix = parts[0];
                        localName = parts[1];
                    } catch (Saxon.Hej.om.QNameException err) {
                        string errorCode = (isXSLT() ? "XTDE0850" : "XQDY0074");
                        Saxon.Hej.trans.XPathException err1 = new Saxon.Hej.trans.XPathException(("Invalid attribute name: "+rawName), errorCode, this.getLocation());
                        throw dynamicError(getLocation(), err1, context);
                    }
                    if (rawName.Equals("xmlns")) {
                        if ((getNamespaceExp() == null)) {
                            string errorCode = (isXSLT() ? "XTDE0855" : "XQDY0044");
                            Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(("Invalid attribute name: "+rawName), errorCode, this.getLocation());
                            throw dynamicError(getLocation(), err, context);
                        }
                    }
                    if (prefix.Equals("xmlns")) {
                        if ((getNamespaceExp() == null)) {
                            string errorCode = (isXSLT() ? "XTDE0860" : "XQDY0044");
                            Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(("Invalid attribute name: "+rawName), errorCode, this.getLocation());
                            throw dynamicError(getLocation(), err, context);
                        } else {
                            prefix = "";
                        }
                    }
                }
            } else {
                if (((nameValue is Saxon.Hej.value.QNameValue) && allowNameAsQName)) {
                    localName = ((Saxon.Hej.value.QNameValue)(nameValue)).getLocalName();
                    uri = ((Saxon.Hej.value.QNameValue)(nameValue)).getNamespaceURI();
                    if ((localName.Equals("xmlns") && uri.isEmpty())) {
                        Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Invalid attribute name: xmlns", "XQDY0044", this.getLocation());
                        throw dynamicError(getLocation(), err, context);
                    }
                    if (uri.isEmpty()) {
                        prefix = "";
                    } else {
                        prefix = ((Saxon.Hej.value.QNameValue)(nameValue)).getPrefix();
                        if (prefix.Length==0) {
                            prefix = pool.suggestPrefixForURI(uri);
                            if ((prefix == null)) {
                                prefix = "ns0";
                            }
                        }
                        if ((uri.Equals(Saxon.Hej.om.NamespaceUri.XML) != "xml".Equals(prefix))) {
                            string message;
                            if ("xml".Equals(prefix)) {
                                message = ("When the prefix is 'xml', the namespace URI must be "+Saxon.Hej.lib.NamespaceConstant.XML);
                            } else {
                                message = (("When the namespace URI is "+Saxon.Hej.lib.NamespaceConstant.XML)+", the prefix must be 'xml'");
                            }
                            string errorCode = (isXSLT() ? "XTDE0835" : "XQDY0044");
                            Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(message, errorCode, this.getLocation());
                            throw dynamicError(getLocation(), err, context);
                        }
                    }
                    if ("xmlns".Equals(prefix)) {
                        Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Invalid attribute namespace: http://www.w3.org/2000/xmlns/", "XQDY0044", this.getLocation());
                        throw dynamicError(getLocation(), err, context);
                    }
                } else {
                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Attribute name must be either a string or a QName", "XPTY0004", this.getLocation());
                    err.setIsTypeError(true);
                    throw dynamicError(getLocation(), err, context);
                }
            }
            if (((getNamespaceExp() == null) && (uri == null))) {
                if (prefix.Length==0) {
                    uri = Saxon.Hej.om.NamespaceUri.NULL;
                } else {
                    uri = getRetainedStaticContext().getURIForPrefix(prefix, false);
                    if ((uri == null)) {
                        string errorCode = (isXSLT() ? "XTDE0860" : "XQDY0074");
                        Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(("Undeclared prefix in attribute name: "+prefix), errorCode, this.getLocation());
                        throw dynamicError(getLocation(), err, context);
                    }
                }
            } else {
                if ((uri == null)) {
                    if ((getNamespaceExp() is Saxon.Hej.expr.StringLiteral)) {
                        uri = Saxon.Hej.om.NamespaceUri.of(((Saxon.Hej.expr.StringLiteral)(getNamespaceExp())).stringify());
                    } else {
                        uri = Saxon.Hej.om.NamespaceUri.of(getNamespaceExp().evaluateAsString(context).ToString());
                        if (!(Saxon.Hej.lib.StandardURIChecker.getInstance().isValidURI(uri.ToString()))) {
                            Saxon.Hej.trans.XPathException de = new Saxon.Hej.trans.XPathException("The value of the namespace attribute must be a valid URI", "XTDE0865", this.getLocation());
                            throw dynamicError(getLocation(), de, context);
                        }
                    }
                }
                if (uri.isEmpty()) {
                    prefix = "";
                } else {
                    if (prefix.Length==0) {
                        prefix = pool.suggestPrefixForURI(uri);
                        if ((prefix == null)) {
                            prefix = "ns0";
                        }
                    }
                }
            }
            if (uri.Equals(Saxon.Hej.om.NamespaceUri.XMLNS)) {
                string errorCode = (isXSLT() ? "XTDE0865" : "XQDY0044");
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(("Cannot create attribute in namespace "+uri), errorCode, this.getLocation());
                throw dynamicError(getLocation(), err, context);
            }
            return new Saxon.Hej.om.FingerprintedQName(prefix, uri, localName);
        }
        public override Saxon.Hej.om.Item evaluateItem(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.om.Item node = base.evaluateItem(context);
            validateOrphanAttribute((Saxon.Hej.tree.util.Orphan)(node), context);
            return node;
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("compAtt", this);
            if ((getValidationAction() != Saxon.Hej.lib.Validation.SKIP)) {
                @out.emitAttribute("validation", Saxon.Hej.lib.Validation.describe(getValidationAction()));
            }
            Saxon.Hej.type.SimpleType type = ((/*1430*/Saxon.Hej.type.SimpleType)getSchemaType());
            if ((type != null)) {
                @out.emitAttribute("type", type.getStructuredQName());
            }
            string flags = "";
            if (isLocal()) {
                flags += "l";
            }
            if (!(flags.Length==0)) {
                @out.emitAttribute("flags", flags);
            }
            @out.setChildRole("name");
            getNameExp().export(@out);
            if ((getNamespaceExp() != null)) {
                @out.setChildRole("namespace");
                getNamespaceExp().export(@out);
            }
            @out.setChildRole("select");
            getSelect().export(@out);
            @out.endElement();
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Hej.expr.instruct.ComputedAttribute.ComputedAttributeElaborator();
        }
        private class ComputedAttributeElaborator : Saxon.Hej.expr.elab.SimpleNodePushElaborator {
            public override Saxon.Hej.expr.elab.PushEvaluator elaborateForPush() {
                Saxon.Hej.expr.instruct.ComputedAttribute expr = (Saxon.Hej.expr.instruct.ComputedAttribute)(getExpression());
                Saxon.Hej.s9api.Location loc = expr.getLocation();
                int options = expr.getOptions();
                Saxon.Hej.expr.elab.ItemEvaluator nameEval = expr.getNameExp().makeElaborator().elaborateForItem();
                if ((((((/*1430*/Saxon.Hej.type.SimpleType)expr.getSchemaType()) != null) || (expr.getValidationAction() == Saxon.Hej.lib.Validation.STRICT)) || (expr.getValidationAction() == Saxon.Hej.lib.Validation.LAX))) {
                    Saxon.Hej.expr.elab.UnicodeStringEvaluator contentEval = expr.getSelect().makeElaborator().elaborateForUnicodeString(true);
                    return (output, context) => {
                        Saxon.Hej.om.Item nameItem = /*Delegate.eval()*/nameEval(context);
                        Saxon.Hej.om.NodeName name = expr.validateNodeName(nameItem, context);
                        Saxon.Hej.str.UnicodeString content = /*Delegate.eval()*/contentEval(context);
                        Saxon.Hej.type.SimpleType ann = expr.validate(name, content, context);
                        if (name.Equals(Saxon.Hej.om.StandardNames.XML_ID_NAME)) {
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
                        Saxon.Hej.om.Item nameItem = /*Delegate.eval()*/nameEval(context);
                        Saxon.Hej.om.NodeName name = expr.validateNodeName(nameItem, context);
                        string content = /*Delegate.eval()*/contentEval(context);
                        if (name.Equals(Saxon.Hej.om.StandardNames.XML_ID_NAME)) {
                            content = Saxon.Hej.value.Whitespace.collapseWhitespace(content);
                        }
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
                Saxon.Hej.expr.instruct.ComputedAttribute expr = (Saxon.Hej.expr.instruct.ComputedAttribute)(getExpression());
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