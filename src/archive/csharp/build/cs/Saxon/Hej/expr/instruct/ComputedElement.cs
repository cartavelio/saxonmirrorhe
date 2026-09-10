using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.instruct {
    public class ComputedElement : Saxon.Hej.expr.instruct.ElementCreator {
        private readonly Saxon.Hej.expr.Operand nameOp;
        private Saxon.Hej.expr.Operand namespaceOp;
        private readonly bool allowNameAsQName;
        private Saxon.Hej.type.ItemType itemType;
        public ComputedElement(Saxon.Hej.expr.Expression elementName, Saxon.Hej.expr.Expression @namespace, Saxon.Hej.type.SchemaType schemaType, int validation, bool inheritNamespaces, bool allowQName)  {
            nameOp = new Saxon.Hej.expr.Operand(this, elementName, Saxon.Hej.expr.OperandRole.SINGLE_ATOMIC);
            if ((@namespace != null)) {
                namespaceOp = new Saxon.Hej.expr.Operand(this, @namespace, Saxon.Hej.expr.OperandRole.SINGLE_ATOMIC);
            }
            setValidationAction(validation, schemaType);
            preservingTypes = ((schemaType == null) && (validation == Saxon.Hej.lib.Validation.PRESERVE));
            this.bequeathNamespacesToChildren = inheritNamespaces;
            allowNameAsQName = allowQName;
        }
        public Saxon.Hej.expr.Expression getNameExp() {
            return nameOp.getChildExpression();
        }
        public Saxon.Hej.expr.Expression getNamespaceExp() {
            return ((namespaceOp == null) ? null : namespaceOp.getChildExpression());
        }
        protected internal void setNameExp(Saxon.Hej.expr.Expression elementName) {
            nameOp.setChildExpression(elementName);
        }
        protected internal void setNamespaceExp(Saxon.Hej.expr.Expression @namespace) {
            if ((namespaceOp == null)) {
                namespaceOp = new Saxon.Hej.expr.Operand(this, @namespace, Saxon.Hej.expr.OperandRole.SINGLE_ATOMIC);
            } else {
                namespaceOp.setChildExpression(@namespace);
            }
        }
        public override System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> operands() {
            return operandSparseList(contentOp, nameOp, namespaceOp);
        }
        public Saxon.Hej.om.NamespaceResolver getNamespaceResolver() {
            return getRetainedStaticContext();
        }
        public override Saxon.Hej.expr.Expression simplify() {
            setNameExp(getNameExp().simplify());
            if ((getNamespaceExp() != null)) {
                setNamespaceExp(getNamespaceExp().simplify());
            }
            Saxon.Hej.Configuration config = getConfiguration();
            bool schemaAware = getPackageData().isSchemaAware();
            preservingTypes |= !(schemaAware);
            Saxon.Hej.type.SchemaType schemaType = getSchemaType();
            if ((schemaType != null)) {
                itemType = new Saxon.Hej.pattern.ContentTypeTest(Saxon.Hej.type.Type.ELEMENT, schemaType, config, false);
                schemaType.analyzeContentExpression(getContentExpression(), Saxon.Hej.type.Type.ELEMENT);
            } else {
                if (((getValidationAction() == Saxon.Hej.lib.Validation.STRIP) || !(schemaAware))) {
                    itemType = new Saxon.Hej.pattern.ContentTypeTest(Saxon.Hej.type.Type.ELEMENT, Saxon.Hej.type.Untyped.getInstance(), config, false);
                } else {
                    itemType = Saxon.Hej.pattern.NodeKindTest.ELEMENT;
                }
            }
            return base.simplify();
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            base.typeCheck(visitor, contextInfo);
            Saxon.Hej.Configuration config = visitor.getConfiguration();
            Saxon.Hej.type.TypeHierarchy th = config.getTypeHierarchy();
            if (allowNameAsQName) {
                Saxon.Ejava.util.function.Supplier<Saxon.Hej.expr.parser.RoleDiagnostic> role = () => new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.INSTRUCTION, "element/name", 0);
                setNameExp(config.getTypeChecker(false).staticTypeCheck(getNameExp(), Saxon.Hej.value.SequenceType.SINGLE_ATOMIC, role, visitor));
                Saxon.Hej.type.ItemType supplied = getNameExp().getItemType();
                if ((((th.relationship(supplied, Saxon.Hej.type.BuiltInAtomicType.STRING) == Saxon.Hej.type.Affinity.DISJOINT) && (th.relationship(supplied, Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC) == Saxon.Hej.type.Affinity.DISJOINT)) && (th.relationship(supplied, Saxon.Hej.type.BuiltInAtomicType.QNAME) == Saxon.Hej.type.Affinity.DISJOINT))) {
                    Saxon.Hej.trans.XPathException de = new Saxon.Hej.trans.XPathException("The name of a constructed element must be a string, QName, or untypedAtomic");
                    de.setErrorCode("XPTY0004");
                    de.setIsTypeError(true);
                    de.setLocation(getLocation());
                    throw de;
                }
            } else {
                if (!(th.isSubType(getNameExp().getItemType(), Saxon.Hej.type.BuiltInAtomicType.STRING))) {
                    setNameExp(Saxon.Hej.functions.SystemFunction.makeCall("string", getRetainedStaticContext(), getNameExp()));
                }
            }
            if (Saxon.Hej.expr.Literal.isAtomic(getNameExp())) {
                try {
                    Saxon.Hej.value.AtomicValue val = (Saxon.Hej.value.AtomicValue)(((Saxon.Hej.expr.Literal)(getNameExp())).getGroundedValue());
                    if ((val is Saxon.Hej.value.StringValue)) {
                        string[] parts = Saxon.Hej.om.NameChecker.checkQNameParts(val.getStringValue());
                        if ((getNamespaceExp() == null)) {
                            string prefix = parts[0];
                            Saxon.Hej.om.NamespaceUri uri = getNamespaceResolver().getURIForPrefix(prefix, true);
                            if ((uri == null)) {
                                Saxon.Hej.trans.XPathException se = new Saxon.Hej.trans.XPathException((("Prefix "+prefix)+" has not been declared"));
                                se.setErrorCode("XPST0081");
                                se.setIsStaticError(true);
                                throw se;
                            }
                            setNamespaceExp(new Saxon.Hej.expr.StringLiteral(uri.ToString()));
                        }
                    }
                } catch (Saxon.Hej.trans.XPathException e) {
                    string code = e.getErrorCodeLocalPart();
                    if (((code == null) || code.Equals("FORG0001"))) {
                        e.setErrorCode((isXSLT() ? "XTDE0820" : "XQDY0074"));
                    } else {
                        if (code.Equals("XPST0081")) {
                            e.setErrorCode((isXSLT() ? "XTDE0830" : "XQDY0074"));
                        }
                    }
                    e.maybeSetLocation(getLocation());
                    e.setIsStaticError(true);
                    throw;
                }
            }
            return base.typeCheck(visitor, contextInfo);
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.instruct.ComputedElement ce = new Saxon.Hej.expr.instruct.ComputedElement(getNameExp().copy(rebindings), ((getNamespaceExp() == null) ? null : getNamespaceExp().copy(rebindings)), getSchemaType(), getValidationAction(), bequeathNamespacesToChildren, allowNameAsQName);
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, ce);
            ce.setContentExpression(getContentExpression().copy(rebindings));
            return ce;
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            if ((itemType == null)) {
                return base.getItemType();
            }
            return itemType;
        }
        public override void checkPermittedContents(Saxon.Hej.type.SchemaType parentType, bool whole) {
            if (((parentType is Saxon.Hej.type.SimpleType) || ((Saxon.Hej.type.ComplexType)(parentType)).isSimpleContent())) {
                string msg = "Elements are not permitted here: the containing element ";
                if ((parentType is Saxon.Hej.type.SimpleType)) {
                    if (parentType.isAnonymousType()) {
                        msg += "is defined to have a simple type";
                    } else {
                        msg += ("is of simple type "+parentType.getDescription());
                    }
                } else {
                    msg += "has a complex type with simple content";
                }
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(msg);
                err.setIsTypeError(true);
                err.setLocation(getLocation());
                throw err;
            }
        }
        public Saxon.Hej.om.NodeName getElementName(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.Controller controller = context.getController();
            System.Diagnostics.Debug.Assert((controller != null));
            string prefix;
            string localName;
            Saxon.Hej.om.NamespaceUri uri = null;
            Saxon.Hej.value.AtomicValue nameValue = (Saxon.Hej.value.AtomicValue)(getNameExp().evaluateItem(context));
            if ((nameValue == null)) {
                string errorCode = (isXSLT() ? "XTDE0820" : "XPTY0004");
                Saxon.Hej.trans.XPathException err1 = new Saxon.Hej.trans.XPathException("Invalid element name (empty sequence)", errorCode, getLocation());
                throw dynamicError(getLocation(), err1, context);
            }
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
                        throw new Saxon.Hej.trans.XPathException(("Invalid EQName in computed element constructor: "+e.Message), "XQDY0074");
                    }
                    if (!(Saxon.Hej.om.NameChecker.isValidNCName(localName))) {
                        throw new Saxon.Hej.trans.XPathException("Local part of EQName in computed element constructor is invalid", "XQDY0074");
                    }
                } else {
                    try {
                        string[] parts = Saxon.Hej.om.NameChecker.getQNameParts(rawName);
                        prefix = parts[0];
                        localName = parts[1];
                    } catch (Saxon.Hej.om.QNameException err) {
                        string message = ("Invalid element name. "+err.Message);
                        if ((rawName.Length == 0)) {
                            message = "Supplied element name is a zero-length string";
                        }
                        string errorCode = (isXSLT() ? "XTDE0820" : "XQDY0074");
                        Saxon.Hej.trans.XPathException err1 = new Saxon.Hej.trans.XPathException(message, errorCode, getLocation());
                        throw dynamicError(getLocation(), err1, context);
                    }
                }
            } else {
                if (((nameValue is Saxon.Hej.value.QNameValue) && allowNameAsQName)) {
                    localName = ((Saxon.Hej.value.QNameValue)(nameValue)).getLocalName();
                    uri = ((Saxon.Hej.value.QNameValue)(nameValue)).getNamespaceURI();
                    prefix = ((Saxon.Hej.value.QNameValue)(nameValue)).getPrefix();
                    if (prefix.Equals("xmlns")) {
                        Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Computed element name has prefix xmlns", "XQDY0096", getLocation());
                        throw dynamicError(getLocation(), err, context);
                    }
                } else {
                    string errorCode = (isXSLT() ? "XTDE0820" : "XPTY0004");
                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Computed element name has incorrect type", errorCode, getLocation());
                    err.setIsTypeError(true);
                    throw dynamicError(getLocation(), err, context);
                }
            }
            if (((getNamespaceExp() == null) && (uri == null))) {
                uri = getRetainedStaticContext().getURIForPrefix(prefix, true);
                if ((uri == null)) {
                    string errorCode = (isXSLT() ? "XTDE0830" : (prefix.Equals("xmlns") ? "XQDY0096" : "XQDY0074"));
                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(("Undeclared prefix in element name: "+prefix), errorCode, getLocation());
                    throw dynamicError(getLocation(), err, context);
                }
            } else {
                if ((uri == null)) {
                    if ((getNamespaceExp() is Saxon.Hej.expr.StringLiteral)) {
                        uri = Saxon.Hej.om.NamespaceUri.of(((Saxon.Hej.expr.StringLiteral)(getNamespaceExp())).stringify());
                    } else {
                        uri = Saxon.Hej.om.NamespaceUri.of(getNamespaceExp().evaluateAsString(context).ToString());
                        if (!(Saxon.Hej.lib.StandardURIChecker.getInstance().isValidURI(uri.ToString()))) {
                            Saxon.Hej.trans.XPathException de = new Saxon.Hej.trans.XPathException("The value of the namespace attribute must be a valid URI", "XTDE0835", getLocation());
                            throw dynamicError(getLocation(), de, context);
                        }
                    }
                }
                if (uri.isEmpty()) {
                    prefix = "";
                }
                if (prefix.Equals("xmlns")) {
                    prefix = "x-xmlns";
                }
            }
            if (uri.Equals(Saxon.Hej.om.NamespaceUri.XMLNS)) {
                string errorCode = (isXSLT() ? "XTDE0835" : "XQDY0096");
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(("Cannot create element in namespace "+uri), errorCode, getLocation());
                throw dynamicError(getLocation(), err, context);
            }
            if ((uri.Equals(Saxon.Hej.om.NamespaceUri.XML) != prefix.Equals("xml"))) {
                string message;
                if (prefix.Equals("xml")) {
                    message = ("When the prefix is 'xml', the namespace URI must be "+Saxon.Hej.lib.NamespaceConstant.XML);
                } else {
                    message = (("When the namespace URI is "+Saxon.Hej.lib.NamespaceConstant.XML)+", the prefix must be 'xml'");
                }
                string errorCode = (isXSLT() ? "XTDE0835" : "XQDY0096");
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(message, errorCode, getLocation());
                throw dynamicError(getLocation(), err, context);
            }
            return new Saxon.Hej.om.FingerprintedQName(prefix, uri, localName);
        }
        public bool isAllowNameAsQName() {
            return allowNameAsQName;
        }
        public override Saxon.Hej.expr.instruct.ElementCreator.ElementCreationDetails makeElementCreationDetails() {
            return new Inner_ElementCreationDetails_141(this);
        }
        public override void outputNamespaceNodes(Saxon.Hej.@event.Outputter @out, Saxon.Hej.om.NodeName nodeName, Saxon.Hej.expr.instruct.ElementCreator.ElementCreationDetails details) {
        }
        public override int getInstructionNameCode() {
            return Saxon.Hej.om.StandardNames.XSL_ELEMENT;
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("compElem", this);
            string flags = getInheritanceFlags();
            if (isLocal()) {
                flags += "l";
            }
            if (!(flags.Length==0)) {
                @out.emitAttribute("flags", flags);
            }
            exportValidationAndType(@out);
            @out.setChildRole("name");
            getNameExp().export(@out);
            if ((getNamespaceExp() != null)) {
                @out.setChildRole("namespace");
                getNamespaceExp().export(@out);
            }
            @out.setChildRole("content");
            getContentExpression().export(@out);
            @out.endElement();
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Hej.expr.instruct.ComputedElement.ComputedElementElaborator();
        }
        public class ComputedElementElaborator : Saxon.Hej.expr.elab.ComplexNodePushElaborator {
            public override Saxon.Hej.expr.elab.PushEvaluator elaborateForPush() {
                Saxon.Hej.expr.instruct.ComputedElement expr = (Saxon.Hej.expr.instruct.ComputedElement)(getExpression());
                bool isXsltInstruction = expr.isXSLT();
                Saxon.Hej.expr.elab.PushEvaluator contentPusher = expr.getContentExpression().makeElaborator().elaborateForPush();
                Saxon.Hej.expr.elab.StringEvaluator namespaceEvaluator = ((expr.getNamespaceExp() == null) ? null : expr.getNamespaceExp().makeElaborator().elaborateForString(true));
                Saxon.Hej.expr.elab.ItemEvaluator localNameEvaluator = expr.getNameExp().makeElaborator().elaborateForItem();
                Saxon.Hej.expr.elab.ItemEvaluator nodeNameEvaluator = context => {
                    Saxon.Hej.Controller controller = context.getController();
                    System.Diagnostics.Debug.Assert((controller != null));
                    string prefix;
                    string localName;
                    Saxon.Hej.om.NamespaceUri uri = null;
                    Saxon.Hej.value.AtomicValue nameValue = (Saxon.Hej.value.AtomicValue)(/*Delegate.eval()*/localNameEvaluator(context));
                    if ((nameValue == null)) {
                        string errorCode = (isXsltInstruction ? "XTDE0820" : "XPTY0004");
                        Saxon.Hej.trans.XPathException err1 = new Saxon.Hej.trans.XPathException("Invalid element name (empty sequence)", errorCode, expr.getLocation());
                        throw dynamicError(expr.getLocation(), err1, context);
                    }
                    if ((nameValue is Saxon.Hej.value.StringValue)) {
                        string rawName = nameValue.getStringValue();
                        rawName = Saxon.Hej.value.Whitespace.trim(rawName);
                        if ((rawName.StartsWith("Q{") && expr.allowNameAsQName)) {
                            try {
                                Saxon.Hej.om.StructuredQName qn = Saxon.Hej.om.StructuredQName.fromEQName(rawName);
                                prefix = "";
                                localName = qn.getLocalPart();
                                uri = qn.getNamespaceUri();
                            } catch (Saxon.Ejava.lang.IllegalArgumentException e) {
                                throw new Saxon.Hej.trans.XPathException(("Invalid EQName in computed element constructor: "+e.Message), "XQDY0074");
                            }
                            if (!(Saxon.Hej.om.NameChecker.isValidNCName(localName))) {
                                throw new Saxon.Hej.trans.XPathException("Local part of EQName in computed element constructor is invalid", "XQDY0074");
                            }
                        } else {
                            try {
                                string[] parts = Saxon.Hej.om.NameChecker.getQNameParts(rawName);
                                prefix = parts[0];
                                localName = parts[1];
                            } catch (Saxon.Hej.om.QNameException err) {
                                string message = ("Invalid element name. "+err.Message);
                                if ((rawName.Length == 0)) {
                                    message = "Supplied element name is a zero-length string";
                                }
                                string errorCode = (isXsltInstruction ? "XTDE0820" : "XQDY0074");
                                Saxon.Hej.trans.XPathException err1 = new Saxon.Hej.trans.XPathException(message, errorCode, expr.getLocation());
                                throw dynamicError(expr.getLocation(), err1, context);
                            }
                        }
                    } else {
                        if (((nameValue is Saxon.Hej.value.QNameValue) && expr.allowNameAsQName)) {
                            localName = ((Saxon.Hej.value.QNameValue)(nameValue)).getLocalName();
                            uri = ((Saxon.Hej.value.QNameValue)(nameValue)).getNamespaceURI();
                            prefix = ((Saxon.Hej.value.QNameValue)(nameValue)).getPrefix();
                            if (prefix.Equals("xmlns")) {
                                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Computed element name has prefix xmlns", "XQDY0096", expr.getLocation());
                                throw dynamicError(expr.getLocation(), err, context);
                            }
                        } else {
                            string errorCode = (isXsltInstruction ? "XTDE0820" : "XPTY0004");
                            Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Computed element name has incorrect type", errorCode, expr.getLocation());
                            err.setIsTypeError(true);
                            throw dynamicError(expr.getLocation(), err, context);
                        }
                    }
                    if (((namespaceEvaluator == null) && (uri == null))) {
                        uri = expr.getRetainedStaticContext().getURIForPrefix(prefix, true);
                        if ((uri == null)) {
                            string errorCode = (isXsltInstruction ? "XTDE0830" : (prefix.Equals("xmlns") ? "XQDY0096" : "XQDY0074"));
                            Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(("Undeclared prefix in element name: "+prefix), errorCode, expr.getLocation());
                            throw dynamicError(expr.getLocation(), err, context);
                        }
                    } else {
                        if ((uri == null)) {
                            string nsUri = /*Delegate.eval()*/namespaceEvaluator(context);
                            uri = Saxon.Hej.om.NamespaceUri.of(nsUri);
                            if (!(Saxon.Hej.lib.StandardURIChecker.getInstance().isValidURI(uri.ToString()))) {
                                Saxon.Hej.trans.XPathException de = new Saxon.Hej.trans.XPathException("The value of the namespace attribute must be a valid URI", "XTDE0835", expr.getLocation());
                                throw dynamicError(expr.getLocation(), de, context);
                            }
                        }
                        if (uri.isEmpty()) {
                            prefix = "";
                        }
                        if (prefix.Equals("xmlns")) {
                            prefix = "x-xmlns";
                        }
                    }
                    if (uri.Equals(Saxon.Hej.om.NamespaceUri.XMLNS)) {
                        string errorCode = (isXsltInstruction ? "XTDE0835" : "XQDY0096");
                        Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(("Cannot create element in namespace "+uri), errorCode, expr.getLocation());
                        throw dynamicError(expr.getLocation(), err, context);
                    }
                    if ((uri.Equals(Saxon.Hej.om.NamespaceUri.XML) != prefix.Equals("xml"))) {
                        string message;
                        if (prefix.Equals("xml")) {
                            message = ("When the prefix is 'xml', the namespace URI must be "+Saxon.Hej.lib.NamespaceConstant.XML);
                        } else {
                            message = (("When the namespace URI is "+Saxon.Hej.lib.NamespaceConstant.XML)+", the prefix must be 'xml'");
                        }
                        string errorCode = (isXsltInstruction ? "XTDE0835" : "XQDY0096");
                        Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(message, errorCode, expr.getLocation());
                        throw dynamicError(expr.getLocation(), err, context);
                    }
                    return new Saxon.Hej.value.QNameValue(prefix, uri, localName);
                } ;
                Saxon.Hej.type.SchemaType typeCode = ((expr.getValidationAction() == Saxon.Hej.lib.Validation.PRESERVE) ? Saxon.Hej.type.AnyType.getInstance() : Saxon.Hej.type.Untyped.getInstance());
                return (@out, context) => {
                    try {
                        Saxon.Hej.value.QNameValue elemName = (Saxon.Hej.value.QNameValue)(/*Delegate.eval()*/nodeNameEvaluator(context));
                        Saxon.Hej.@event.Receiver elemOut = @out;
                        if (!(expr.preservingTypes)) {
                            Saxon.Hej.lib.ParseOptions options = expr.getValidationOptions().withTopLevelElement(elemName.getStructuredQName());
                            context.getConfiguration().prepareValidationReporting(context, options);
                            Saxon.Hej.@event.Receiver validator = context.getConfiguration().getElementValidator(elemOut, options, expr.getLocation());
                            if ((validator != elemOut)) {
                                @out = new Saxon.Hej.@event.ComplexContentOutputter(validator);
                            }
                        }
                        if ((@out.getSystemId() == null)) {
                            @out.setSystemId(expr.getStaticBaseURIString());
                        }
                        int properties = Saxon.Hej.@event.ReceiverOption.NONE;
                        if (!(expr.bequeathNamespacesToChildren)) {
                            properties |= Saxon.Hej.@event.ReceiverOption.DISINHERIT_NAMESPACES;
                        }
                        if (!(expr.inheritNamespacesFromParent)) {
                            properties |= Saxon.Hej.@event.ReceiverOption.REFUSE_NAMESPACES;
                        }
                        properties |= Saxon.Hej.@event.ReceiverOption.ALL_NAMESPACES;
                        @out.startElement(new Saxon.Hej.om.FingerprintedQName(elemName.getStructuredQName()), typeCode, expr.getLocation(), properties);
                        Saxon.Hej.expr.instruct.TailCall tc = /*Delegate.processLeavingTail()*/contentPusher(@out, context);
                        Saxon.Hej.expr.Expression.dispatchTailCall(tc);
                        @out.endElement();
                    } catch (Saxon.Hej.trans.XPathException e) {
                        e.maybeSetLocation(expr.getLocation());
                        e.maybeSetContext(context);
                        throw;
                    }
                    return null;
                } ;
            }
        
        }
    
        private class Inner_ElementCreationDetails_141 : Saxon.Hej.expr.instruct.ElementCreator.ElementCreationDetails {
            private readonly ComputedElement outerInstance;
            public Inner_ElementCreationDetails_141(ComputedElement outerInstance) {
                this.outerInstance = outerInstance;
            }
            public override Saxon.Hej.om.NodeName getNodeName(Saxon.Hej.expr.XPathContext context) {
                return outerInstance.getElementName(/*localparam*/context);
            }
            public override string getSystemId(Saxon.Hej.expr.XPathContext context) {
                return /*guess*/outerInstance.getStaticBaseURIString();
            }
            public override void processContent(Saxon.Hej.@event.Outputter outputter, Saxon.Hej.expr.XPathContext context) {
                /*guess*/outerInstance.getContentExpression().process(/*localparam*/outputter, /*localparam*/context);
            }
        }

    }
}