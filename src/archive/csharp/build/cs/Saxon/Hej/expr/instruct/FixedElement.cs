using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.instruct {
    public class FixedElement : Saxon.Hej.expr.instruct.ElementCreator {
        private readonly Saxon.Hej.om.NodeName elementName;
        protected internal Saxon.Hej.om.NamespaceMap namespaceBindings;
        private Saxon.Hej.type.ItemType itemType;
        public FixedElement(Saxon.Hej.om.NodeName elementName, Saxon.Hej.om.NamespaceMap namespaceBindings, bool inheritNamespacesToChildren, bool inheritNamespacesFromParent, Saxon.Hej.type.SchemaType schemaType, int validation)  {
            this.elementName = elementName;
            this.namespaceBindings = namespaceBindings;
            this.bequeathNamespacesToChildren = inheritNamespacesToChildren;
            this.inheritNamespacesFromParent = inheritNamespacesFromParent;
            setValidationAction(validation, schemaType);
            preservingTypes = ((schemaType == null) && (validation == Saxon.Hej.lib.Validation.PRESERVE));
        }
        public override void setLocation(Saxon.Hej.s9api.Location id) {
            base.setLocation(id);
        }
        public override System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> operands() {
            return contentOp;
        }
        public override Saxon.Hej.expr.Expression simplify() {
            preservingTypes |= !(getPackageData().isSchemaAware());
            return base.simplify();
        }
        protected internal override void checkContentSequence(Saxon.Hej.expr.StaticContext env) {
            base.checkContentSequence(env);
            itemType = computeFixedElementItemType(this, env, getValidationAction(), getSchemaType(), elementName, getContentExpression());
        }
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType) {
            Saxon.Hej.expr.Expression e = base.optimize(visitor, contextItemType);
            if ((e != this)) {
                return e;
            }
            if (!(bequeathNamespacesToChildren)) {
                return this;
            }
            return this;
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.instruct.FixedElement fe = new Saxon.Hej.expr.instruct.FixedElement(elementName, namespaceBindings, bequeathNamespacesToChildren, inheritNamespacesFromParent, getSchemaType(), getValidationAction());
            fe.setContentExpression(getContentExpression().copy(rebindings));
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, fe);
            return fe;
        }
        private Saxon.Hej.type.ItemType computeFixedElementItemType(Saxon.Hej.expr.instruct.FixedElement instr, Saxon.Hej.expr.StaticContext env, int validation, Saxon.Hej.type.SchemaType schemaType, Saxon.Hej.om.NodeName elementName, Saxon.Hej.expr.Expression content) {
            Saxon.Hej.Configuration config = env.getConfiguration();
            Saxon.Hej.type.ItemType itemType;
            int fp = elementName.obtainFingerprint(config.getNamePool());
            if ((schemaType == null)) {
                if ((validation == Saxon.Hej.lib.Validation.STRICT)) {
                    Saxon.Hej.type.SchemaDeclaration decl = config.getElementDeclaration(fp);
                    if ((decl == null)) {
                        Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException((("There is no global element declaration for "+elementName.getStructuredQName().getEQName())+", so strict validation will fail"));
                        err.setErrorCode((instr.isXSLT() ? "XTTE1512" : "XQDY0084"));
                        err.setIsTypeError(true);
                        err.setLocation(instr.getLocation());
                        throw err;
                    }
                    if (decl.isAbstract()) {
                        Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException((("The element declaration for "+elementName.getStructuredQName().getEQName())+" is abstract, so strict validation will fail"));
                        err.setErrorCode((instr.isXSLT() ? "XTTE1512" : "XQDY0027"));
                        err.setIsTypeError(true);
                        err.setLocation(instr.getLocation());
                        throw err;
                    }
                    Saxon.Hej.type.SchemaType declaredType = decl.getType();
                    Saxon.Hej.type.SchemaType xsiType = instr.getXSIType(env);
                    if ((xsiType != null)) {
                        schemaType = xsiType;
                    } else {
                        schemaType = declaredType;
                    }
                    itemType = new Saxon.Hej.pattern.CombinedNodeTest(new Saxon.Hej.pattern.NameTest(Saxon.Hej.type.Type.ELEMENT, fp, env.getConfiguration().getNamePool()), Saxon.Hej.expr.parser.Token.INTERSECT, new Saxon.Hej.pattern.ContentTypeTest(Saxon.Hej.type.Type.ELEMENT, schemaType, config, false));
                    if (((xsiType != null) || !(decl.hasTypeAlternatives()))) {
                        instr.setValidationOptions(instr.getValidationOptions().withTopLevelType(schemaType));
                        try {
                            schemaType.analyzeContentExpression(content, Saxon.Hej.type.Type.ELEMENT);
                        } catch (Saxon.Hej.trans.XPathException e) {
                            e.setErrorCode((instr.isXSLT() ? "XTTE1510" : "XQDY0027"));
                            e.setLocation(instr.getLocation());
                            throw;
                        }
                        if ((xsiType != null)) {
                            try {
                                config.checkTypeDerivationIsOK(xsiType, declaredType, 0);
                            } catch (Saxon.Hej.type.SchemaException e) {
                                Saxon.Hej.type.ValidationFailure ve = new Saxon.Hej.type.ValidationFailure(((("The specified xsi:type "+xsiType.getDescription())+" is not validly derived from the required type ")+declaredType.getDescription()));
                                ve.setConstraintReference(1, "cvc-elt", "4.3");
                                ve.setErrorCode((instr.isXSLT() ? "XTTE1515" : "XQDY0027"));
                                ve.setLocator(instr.getLocation());
                                throw ve.makeException();
                            }
                        }
                    }
                } else {
                    if ((validation == Saxon.Hej.lib.Validation.LAX)) {
                        Saxon.Hej.type.SchemaDeclaration decl = config.getElementDeclaration(fp);
                        if ((decl == null)) {
                            env.issueWarning(("There is no global element declaration for "+elementName.getDisplayName()), Saxon.Hej.trans.SaxonErrorCode.SXWN9031, instr.getLocation());
                            itemType = new Saxon.Hej.pattern.NameTest(Saxon.Hej.type.Type.ELEMENT, fp, config.getNamePool());
                        } else {
                            schemaType = decl.getType();
                            instr.setValidationOptions(instr.getValidationOptions().withTopLevelType(schemaType));
                            itemType = new Saxon.Hej.pattern.CombinedNodeTest(new Saxon.Hej.pattern.NameTest(Saxon.Hej.type.Type.ELEMENT, fp, config.getNamePool()), Saxon.Hej.expr.parser.Token.INTERSECT, new Saxon.Hej.pattern.ContentTypeTest(Saxon.Hej.type.Type.ELEMENT, instr.getSchemaType(), config, false));
                            try {
                                schemaType.analyzeContentExpression(content, Saxon.Hej.type.Type.ELEMENT);
                            } catch (Saxon.Hej.trans.XPathException e) {
                                e.setErrorCode((instr.isXSLT() ? "XTTE1515" : "XQDY0027"));
                                e.setLocation(instr.getLocation());
                                throw;
                            }
                        }
                    } else {
                        if ((validation == Saxon.Hej.lib.Validation.PRESERVE)) {
                            itemType = new Saxon.Hej.pattern.CombinedNodeTest(new Saxon.Hej.pattern.NameTest(Saxon.Hej.type.Type.ELEMENT, fp, config.getNamePool()), Saxon.Hej.expr.parser.Token.INTERSECT, new Saxon.Hej.pattern.ContentTypeTest(Saxon.Hej.type.Type.ELEMENT, Saxon.Hej.type.AnyType.getInstance(), config, false));
                        } else {
                            itemType = new Saxon.Hej.pattern.CombinedNodeTest(new Saxon.Hej.pattern.NameTest(Saxon.Hej.type.Type.ELEMENT, fp, config.getNamePool()), Saxon.Hej.expr.parser.Token.INTERSECT, new Saxon.Hej.pattern.ContentTypeTest(Saxon.Hej.type.Type.ELEMENT, Saxon.Hej.type.Untyped.getInstance(), config, false));
                        }
                    }
                }
            } else {
                itemType = new Saxon.Hej.pattern.CombinedNodeTest(new Saxon.Hej.pattern.NameTest(Saxon.Hej.type.Type.ELEMENT, fp, config.getNamePool()), Saxon.Hej.expr.parser.Token.INTERSECT, new Saxon.Hej.pattern.ContentTypeTest(Saxon.Hej.type.Type.ELEMENT, schemaType, config, false));
                try {
                    schemaType.analyzeContentExpression(content, Saxon.Hej.type.Type.ELEMENT);
                } catch (Saxon.Hej.trans.XPathException e) {
                    e.setErrorCode((instr.isXSLT() ? "XTTE1540" : "XQDY0027"));
                    e.setLocation(instr.getLocation());
                    throw;
                }
            }
            return itemType;
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            if ((itemType == null)) {
                return base.getItemType();
            }
            return itemType;
        }
        public Saxon.Hej.om.NodeName getFixedElementName() {
            return elementName;
        }
        public override void gatherProperties(Saxon.Ejava.util.function.BiConsumer<string,System.Object> consumer) {
            consumer("name", getFixedElementName());
        }
        private Saxon.Hej.type.SchemaType getXSIType(Saxon.Hej.expr.StaticContext env) {
            if ((getContentExpression() is Saxon.Hej.expr.instruct.FixedAttribute)) {
                return testForXSIType((Saxon.Hej.expr.instruct.FixedAttribute)(getContentExpression()), env);
            } else {
                if ((getContentExpression() is Saxon.Hej.expr.instruct.Block)) {
                    foreach (Saxon.Hej.expr.Operand o in getContentExpression().operands()) {
                        Saxon.Hej.expr.Expression exp = o.getChildExpression();
                        if ((exp is Saxon.Hej.expr.instruct.FixedAttribute)) {
                            Saxon.Hej.type.SchemaType type = testForXSIType((Saxon.Hej.expr.instruct.FixedAttribute)(exp), env);
                            if ((type != null)) {
                                return type;
                            }
                        }
                    }
                    return null;
                } else {
                    return null;
                }
            }
        }
        private Saxon.Hej.type.SchemaType testForXSIType(Saxon.Hej.expr.instruct.FixedAttribute fat, Saxon.Hej.expr.StaticContext env) {
            int att = fat.getAttributeFingerprint();
            if ((att == Saxon.Hej.om.StandardNames.XSI_TYPE)) {
                Saxon.Hej.expr.Expression attValue = fat.getSelect();
                if ((attValue is Saxon.Hej.expr.StringLiteral)) {
                    try {
                        string[] parts = Saxon.Hej.om.NameChecker.getQNameParts(((Saxon.Hej.expr.StringLiteral)(attValue)).stringify());
                        Saxon.Hej.om.NamespaceUri uri = namespaceBindings.getNamespaceUri(parts[0]);
                        if ((uri == null)) {
                            return null;
                        } else {
                            return env.getConfiguration().getSchemaType(new Saxon.Hej.om.StructuredQName("", uri, parts[1]));
                        }
                    } catch (Saxon.Hej.om.QNameException e) {
                        throw new Saxon.Hej.trans.XPathException(e.Message);
                    }
                }
            }
            return null;
        }
        public override void checkPermittedContents(Saxon.Hej.type.SchemaType parentType, bool whole) {
            if ((parentType is Saxon.Hej.type.SimpleType)) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(((("Element "+elementName.getDisplayName())+" is not permitted here: the containing element is of simple type ")+parentType.getDescription()));
                err.setIsTypeError(true);
                err.setLocation(getLocation());
                throw err;
            } else {
                if (((Saxon.Hej.type.ComplexType)(parentType)).isSimpleContent()) {
                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException((("Element "+elementName.getDisplayName())+" is not permitted here: the containing element has a complex type with simple content"));
                    err.setIsTypeError(true);
                    err.setLocation(getLocation());
                    throw err;
                }
            }
            if (whole) {
                Saxon.Hej.expr.Expression parent = getParentExpression();
                Saxon.Hej.expr.instruct.Block block = new Saxon.Hej.expr.instruct.Block(new Saxon.Hej.expr.Expression[]{this});
                parentType.analyzeContentExpression(block, Saxon.Hej.type.Type.ELEMENT);
                setParentExpression(parent);
            }
            Saxon.Hej.type.SchemaType type;
            try {
                int fp = elementName.obtainFingerprint(getConfiguration().getNamePool());
                type = ((Saxon.Hej.type.ComplexType)(parentType)).getElementParticleType(fp, true);
            } catch (Saxon.Hej.type.MissingComponentException e) {
                throw new Saxon.Hej.trans.XPathException(e);
            }
            if ((type == null)) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(((("Element "+elementName.getDisplayName())+" is not permitted in the content model of the complex type ")+parentType.getDescription()));
                err.setIsTypeError(true);
                err.setLocation(getLocation());
                err.setErrorCode((isXSLT() ? "XTTE1510" : "XQDY0027"));
                throw err;
            }
            if ((type is Saxon.Hej.type.AnyType)) {
                return ;
            }
            try {
                getContentExpression().checkPermittedContents(type, true);
            } catch (Saxon.Hej.trans.XPathException e) {
                e.maybeSetLocation(getLocation());
                throw;
            }
        }
        public override Saxon.Hej.expr.instruct.ElementCreator.ElementCreationDetails makeElementCreationDetails() {
            return new Inner_ElementCreationDetails_125(this);
        }
        public override void outputNamespaceNodes(Saxon.Hej.@event.Outputter @out, Saxon.Hej.om.NodeName nodeName, Saxon.Hej.expr.instruct.ElementCreator.ElementCreationDetails details) {
            foreach (Saxon.Hej.om.NamespaceBinding ns in namespaceBindings) {
                @out.@namespace(ns.getPrefix(), ns.getNamespaceUri(), Saxon.Hej.@event.ReceiverOption.NONE);
            }
        }
        public Saxon.Hej.om.NamespaceMap getActiveNamespaces() {
            return namespaceBindings;
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("elem", this);
            @out.emitAttribute("name", elementName.getDisplayName());
            @out.emitAttribute("nsuri", elementName.getNamespaceUri().ToString());
            string flags = getInheritanceFlags();
            if ((!(elementName.getNamespaceUri().isEmpty()) && elementName.getPrefix().Length==0)) {
                flags += "d";
            }
            if (isLocal()) {
                flags += "l";
            }
            if (!(flags.Length==0)) {
                @out.emitAttribute("flags", flags);
            }
            Saxon.Ejava.lang.StringBuilder fsb = new Saxon.Ejava.lang.StringBuilder(256);
            if (!(namespaceBindings.isEmpty())) {
                foreach (Saxon.Hej.om.NamespaceBinding ns in namespaceBindings) {
                    string prefix = ns.getPrefix();
                    if (!(prefix.Equals("xml"))) {
                        fsb.append((prefix.Length==0 ? "#" : prefix));
                        if (!(ns.getNamespaceUri().Equals(getRetainedStaticContext().getURIForPrefix(prefix, true)))) {
                            fsb.append('=');
                            fsb.append(ns.getNamespaceUri());
                        }
                        fsb.append(' ');
                    }
                }
                fsb.setLength((fsb.length()-1));
                @out.emitAttribute("namespaces", fsb.ToString());
            }
            exportValidationAndType(@out);
            getContentExpression().export(@out);
            @out.endElement();
        }
        public override string ToString() {
            return (((("<"+elementName.getStructuredQName().getDisplayName())+" {")+getContentExpression().ToString())+"}/>");
        }
        public override string toShortString() {
            return (((("<"+elementName.getStructuredQName().getDisplayName())+" {")+getContentExpression().toShortString())+"}/>");
        }
        public override string getExpressionName() {
            return "element";
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Hej.expr.instruct.FixedElement.FixedElementElaborator();
        }
        public class FixedElementElaborator : Saxon.Hej.expr.elab.ComplexNodePushElaborator {
            public override Saxon.Hej.expr.elab.PushEvaluator elaborateForPush() {
                Saxon.Hej.expr.instruct.FixedElement expr = (Saxon.Hej.expr.instruct.FixedElement)(getExpression());
                Saxon.Hej.expr.elab.PushEvaluator contentPusher = expr.getContentExpression().makeElaborator().elaborateForPush();
                Saxon.Hej.type.SchemaType typeCode = ((expr.getValidationAction() == Saxon.Hej.lib.Validation.PRESERVE) ? Saxon.Hej.type.AnyType.getInstance() : Saxon.Hej.type.Untyped.getInstance());
                int properties = Saxon.Hej.@event.ReceiverOption.NONE;
                if (!(expr.bequeathNamespacesToChildren)) {
                    properties |= Saxon.Hej.@event.ReceiverOption.DISINHERIT_NAMESPACES;
                }
                if (!(expr.inheritNamespacesFromParent)) {
                    properties |= Saxon.Hej.@event.ReceiverOption.REFUSE_NAMESPACES;
                }
                properties |= Saxon.Hej.@event.ReceiverOption.ALL_NAMESPACES;
                int finalProperties = properties;
                return (@out, context) => {
                    try {
                        Saxon.Hej.om.NodeName elemName = expr.getFixedElementName();
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
                        @out.startElement(elemName, typeCode, expr.getLocation(), finalProperties);
                        foreach (Saxon.Hej.om.NamespaceBinding ns in expr.namespaceBindings) {
                            @out.@namespace(ns.getPrefix(), ns.getNamespaceUri(), Saxon.Hej.@event.ReceiverOption.NONE);
                        }
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
    
        private class Inner_ElementCreationDetails_125 : Saxon.Hej.expr.instruct.ElementCreator.ElementCreationDetails {
            private readonly FixedElement outerInstance;
            public Inner_ElementCreationDetails_125(FixedElement outerInstance) {
                this.outerInstance = outerInstance;
            }
            public override Saxon.Hej.om.NodeName getNodeName(Saxon.Hej.expr.XPathContext context) {
                return outerInstance.getFixedElementName();
            }
            public override string getSystemId(Saxon.Hej.expr.XPathContext context) {
                return /*guess*/outerInstance.getStaticBaseURIString();
            }
            public override void processContent(Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context) {
                /*guess*/outerInstance.getContentExpression().process(/*localparam*/output, /*localparam*/context);
            }
        }

    }
}