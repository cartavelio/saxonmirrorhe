using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.instruct {
    public class Copy : Saxon.Hej.expr.instruct.ElementCreator {
        private bool copyNamespaces;
        private Saxon.Hej.type.ItemType selectItemType = Saxon.Hej.type.AnyItemType.getInstance();
        private Saxon.Hej.type.ItemType resultItemType;
        public Copy(bool copyNamespaces, bool inheritNamespaces, Saxon.Hej.type.SchemaType schemaType, int validation)  {
            this.copyNamespaces = copyNamespaces;
            this.bequeathNamespacesToChildren = inheritNamespaces;
            setValidationAction(validation, schemaType);
            preservingTypes = ((schemaType == null) && (validation == Saxon.Hej.lib.Validation.PRESERVE));
        }
        public void setCopyNamespaces(bool copy) {
            copyNamespaces = copy;
        }
        public bool isCopyNamespaces() {
            return copyNamespaces;
        }
        public override Saxon.Hej.expr.Expression simplify() {
            preservingTypes |= !(getPackageData().isSchemaAware());
            return base.simplify();
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            typeCheckChildren(visitor, contextInfo);
            selectItemType = contextInfo.getItemType();
            if ((selectItemType == Saxon.Hej.type.ErrorType.getInstance())) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("No context item supplied for xsl:copy", "XTTE0945");
                err.setIsTypeError(true);
                err.setLocation(getLocation());
                throw err;
            }
            if ((selectItemType is Saxon.Hej.pattern.NodeTest)) {
                switch (selectItemType.getPrimitiveType()) {
                  case Saxon.Hej.type.Type.ELEMENT:
                    this.resultItemType = Saxon.Hej.pattern.NodeKindTest.ELEMENT;
                    break ;
                  case Saxon.Hej.type.Type.DOCUMENT:
                    this.resultItemType = Saxon.Hej.pattern.NodeKindTest.DOCUMENT;
                    break ;
                  case Saxon.Hej.type.Type.ATTRIBUTE:
                  case Saxon.Hej.type.Type.TEXT:
                  case Saxon.Hej.type.Type.COMMENT:
                  case Saxon.Hej.type.Type.PROCESSING_INSTRUCTION:
                  case Saxon.Hej.type.Type.NAMESPACE:
                    Saxon.Hej.expr.ContextItemExpression dot = new Saxon.Hej.expr.ContextItemExpression();
                    Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, dot);
                    Saxon.Hej.expr.instruct.CopyOf c = new Saxon.Hej.expr.instruct.CopyOf(dot, copyNamespaces, getValidationAction(), getSchemaType(), false);
                    Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, c);
                    return c.typeCheck(visitor, contextInfo);
                  default:
                    this.resultItemType = selectItemType;
                    break ;
                }
            } else {
                this.resultItemType = selectItemType;
            }
            checkContentSequence(visitor.getStaticContext());
            return this;
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.instruct.Copy copy = new Saxon.Hej.expr.instruct.Copy(copyNamespaces, bequeathNamespacesToChildren, getSchemaType(), getValidationAction());
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, copy);
            copy.setContentExpression(getContentExpression().copy(rebindings));
            copy.resultItemType = resultItemType;
            return copy;
        }
        public void setSelectItemType(Saxon.Hej.type.ItemType type) {
            selectItemType = type;
        }
        public override int getIntrinsicDependencies() {
            return Saxon.Hej.expr.StaticProperty.DEPENDS_ON_CONTEXT_ITEM;
        }
        public override int getInstructionNameCode() {
            return Saxon.Hej.om.StandardNames.XSL_COPY;
        }
        public override System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> operands() {
            return contentOp;
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            if ((resultItemType != null)) {
                return resultItemType;
            } else {
                Saxon.Hej.type.TypeHierarchy th = getConfiguration().getTypeHierarchy();
                resultItemType = computeItemType(th);
                return resultItemType;
            }
        }
        private Saxon.Hej.type.ItemType computeItemType(Saxon.Hej.type.TypeHierarchy th) {
            Saxon.Hej.type.ItemType selectItemType = this.selectItemType;
            if (!(getPackageData().isSchemaAware())) {
                return selectItemType;
            }
            if (selectItemType.getUType().overlaps(Saxon.Hej.type.UType.ANY_ATOMIC.union(Saxon.Hej.type.UType.FUNCTION))) {
                return selectItemType;
            }
            Saxon.Hej.Configuration config = th.getConfiguration();
            if ((getSchemaType() != null)) {
                Saxon.Hej.type.Affinity e = th.relationship(selectItemType, Saxon.Hej.pattern.NodeKindTest.ELEMENT);
                if (((e == Saxon.Hej.type.Affinity.SAME_TYPE) || (e == Saxon.Hej.type.Affinity.SUBSUMED_BY))) {
                    return new Saxon.Hej.pattern.ContentTypeTest(Saxon.Hej.type.Type.ELEMENT, getSchemaType(), config, false);
                }
                Saxon.Hej.type.Affinity a = th.relationship(selectItemType, Saxon.Hej.pattern.NodeKindTest.ATTRIBUTE);
                if (((a == Saxon.Hej.type.Affinity.SAME_TYPE) || (a == Saxon.Hej.type.Affinity.SUBSUMED_BY))) {
                    return new Saxon.Hej.pattern.ContentTypeTest(Saxon.Hej.type.Type.ATTRIBUTE, getSchemaType(), config, false);
                }
                return Saxon.Hej.pattern.AnyNodeTest.getInstance();
            } else {
                switch (getValidationAction()) {
                  case Saxon.Hej.lib.Validation.PRESERVE:
                    return selectItemType;
                  case Saxon.Hej.lib.Validation.STRIP:
                    {
                        Saxon.Hej.type.Affinity e = th.relationship(selectItemType, Saxon.Hej.pattern.NodeKindTest.ELEMENT);
                        if (((e == Saxon.Hej.type.Affinity.SAME_TYPE) || (e == Saxon.Hej.type.Affinity.SUBSUMED_BY))) {
                            return new Saxon.Hej.pattern.ContentTypeTest(Saxon.Hej.type.Type.ELEMENT, Saxon.Hej.type.Untyped.getInstance(), config, false);
                        }
                        Saxon.Hej.type.Affinity a = th.relationship(selectItemType, Saxon.Hej.pattern.NodeKindTest.ATTRIBUTE);
                        if (((a == Saxon.Hej.type.Affinity.SAME_TYPE) || (a == Saxon.Hej.type.Affinity.SUBSUMED_BY))) {
                            return new Saxon.Hej.pattern.ContentTypeTest(Saxon.Hej.type.Type.ATTRIBUTE, Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC, config, false);
                        }
                        if (((e != Saxon.Hej.type.Affinity.DISJOINT) || (a != Saxon.Hej.type.Affinity.DISJOINT))) {
                            return Saxon.Hej.pattern.AnyNodeTest.getInstance();
                        } else {
                            return selectItemType;
                        }
                    }
                  case Saxon.Hej.lib.Validation.STRICT:
                  case Saxon.Hej.lib.Validation.LAX:
                    if ((selectItemType is Saxon.Hej.pattern.NodeTest)) {
                        int fp = ((Saxon.Hej.pattern.NodeTest)(selectItemType)).getFingerprint();
                        if ((fp != -1)) {
                            Saxon.Hej.type.Affinity e = th.relationship(selectItemType, Saxon.Hej.pattern.NodeKindTest.ELEMENT);
                            if (((e == Saxon.Hej.type.Affinity.SAME_TYPE) || (e == Saxon.Hej.type.Affinity.SUBSUMED_BY))) {
                                Saxon.Hej.type.SchemaDeclaration elem = config.getElementDeclaration(fp);
                                if ((elem != null)) {
                                    try {
                                        return new Saxon.Hej.pattern.ContentTypeTest(Saxon.Hej.type.Type.ELEMENT, elem.getType(), config, false);
                                    } catch (Saxon.Hej.type.MissingComponentException e1) {
                                        return new Saxon.Hej.pattern.ContentTypeTest(Saxon.Hej.type.Type.ELEMENT, Saxon.Hej.type.AnyType.getInstance(), config, false);
                                    }
                                } else {
                                    return new Saxon.Hej.pattern.ContentTypeTest(Saxon.Hej.type.Type.ELEMENT, Saxon.Hej.type.AnyType.getInstance(), config, false);
                                }
                            }
                            Saxon.Hej.type.Affinity a = th.relationship(selectItemType, Saxon.Hej.pattern.NodeKindTest.ATTRIBUTE);
                            if (((a == Saxon.Hej.type.Affinity.SAME_TYPE) || (a == Saxon.Hej.type.Affinity.SUBSUMED_BY))) {
                                Saxon.Hej.type.SchemaDeclaration attr = config.getElementDeclaration(fp);
                                if ((attr != null)) {
                                    try {
                                        return new Saxon.Hej.pattern.ContentTypeTest(Saxon.Hej.type.Type.ATTRIBUTE, attr.getType(), config, false);
                                    } catch (Saxon.Hej.type.MissingComponentException e1) {
                                        return new Saxon.Hej.pattern.ContentTypeTest(Saxon.Hej.type.Type.ATTRIBUTE, Saxon.Hej.type.AnySimpleType.getInstance(), config, false);
                                    }
                                } else {
                                    return new Saxon.Hej.pattern.ContentTypeTest(Saxon.Hej.type.Type.ATTRIBUTE, Saxon.Hej.type.AnySimpleType.getInstance(), config, false);
                                }
                            }
                        } else {
                            Saxon.Hej.type.Affinity e = th.relationship(selectItemType, Saxon.Hej.pattern.NodeKindTest.ELEMENT);
                            if (((e == Saxon.Hej.type.Affinity.SAME_TYPE) || (e == Saxon.Hej.type.Affinity.SUBSUMED_BY))) {
                                return Saxon.Hej.pattern.NodeKindTest.ELEMENT;
                            }
                            Saxon.Hej.type.Affinity a = th.relationship(selectItemType, Saxon.Hej.pattern.NodeKindTest.ATTRIBUTE);
                            if (((a == Saxon.Hej.type.Affinity.SAME_TYPE) || (a == Saxon.Hej.type.Affinity.SUBSUMED_BY))) {
                                return Saxon.Hej.pattern.NodeKindTest.ATTRIBUTE;
                            }
                        }
                        return Saxon.Hej.pattern.AnyNodeTest.getInstance();
                    } else {
                        if ((selectItemType is Saxon.Hej.type.AtomicType)) {
                            return selectItemType;
                        } else {
                            return Saxon.Hej.type.AnyItemType.getInstance();
                        }
                    }
                  default:
                    throw new Saxon.Ejava.lang.IllegalStateException();
                }
            }
        }
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType) {
            Saxon.Hej.expr.Expression exp = base.optimize(visitor, contextItemType);
            if ((exp == this)) {
                if ((resultItemType == null)) {
                    resultItemType = computeItemType(visitor.getConfiguration().getTypeHierarchy());
                }
                if (visitor.isOptimizeForStreaming()) {
                    Saxon.Hej.type.UType type = contextItemType.getItemType().getUType();
                    if (!(type.intersection(Saxon.Hej.pattern.MultipleNodeKindTest.LEAF.getUType()).Equals(Saxon.Hej.type.UType.VOID))) {
                        Saxon.Hej.expr.Expression p = getParentExpression();
                        if (((((p is Saxon.Hej.expr.instruct.Choose) && (((Saxon.Hej.expr.instruct.Choose)(p)).size() == 2)) && (((Saxon.Hej.expr.instruct.Choose)(p)).getAction(1) == this)) && (((Saxon.Hej.expr.instruct.Choose)(p)).getAction(0) is Saxon.Hej.expr.instruct.CopyOf))) {
                            return exp;
                        }
                        Saxon.Hej.expr.Expression copyOf = new Saxon.Hej.expr.instruct.CopyOf(new Saxon.Hej.expr.ContextItemExpression(), false, getValidationAction(), getSchemaType(), false);
                        Saxon.Hej.pattern.NodeTest leafTest = new Saxon.Hej.pattern.MultipleNodeKindTest(type.intersection(Saxon.Hej.pattern.MultipleNodeKindTest.LEAF.getUType()));
                        Saxon.Hej.expr.Expression[] conditions = new Saxon.Hej.expr.Expression[]{new Saxon.Hej.expr.InstanceOfExpression(new Saxon.Hej.expr.ContextItemExpression(), Saxon.Hej.value.SequenceType.makeSequenceType(leafTest, Saxon.Hej.expr.StaticProperty.EXACTLY_ONE)), Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.BooleanValue.TRUE, this)};
                        Saxon.Hej.expr.Expression[] actions = new Saxon.Hej.expr.Expression[]{copyOf, this};
                        Saxon.Hej.expr.instruct.Choose choose = new Saxon.Hej.expr.instruct.Choose(conditions, actions);
                        Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, choose);
                        return choose;
                    }
                }
            }
            return exp;
        }
        public override void outputNamespaceNodes(Saxon.Hej.@event.Outputter receiver, Saxon.Hej.om.NodeName nodeName, Saxon.Hej.expr.instruct.ElementCreator.ElementCreationDetails details) {
            if (copyNamespaces) {
                receiver.namespaces(((Saxon.Hej.expr.instruct.Copy.CopyElementDetails)(details)).getCopiedNode().getAllNamespaces(), Saxon.Hej.@event.ReceiverOption.NAMESPACE_OK);
            } else {
                Saxon.Hej.om.NamespaceBinding ns = nodeName.getNamespaceBinding();
                if (!(ns.isDefaultUndeclaration())) {
                    receiver.@namespace(ns.getPrefix(), ns.getNamespaceUri(), Saxon.Hej.@event.ReceiverOption.NONE);
                }
            }
        }
        public static void copyUnparsedEntities(Saxon.Hej.om.NodeInfo source, Saxon.Hej.@event.Outputter @out) {
            System.Collections.Generic.IEnumerator<string> unparsedEntities = source.getTreeInfo().getUnparsedEntityNames();
            while (unparsedEntities.MoveNext()) {
                string n = unparsedEntities.Current;
                string[] details = source.getTreeInfo().getUnparsedEntity(n);
                @out.setUnparsedEntity(n, details[0], details[1]);
            }
        }
        public override Saxon.Hej.om.Item evaluateItem(Saxon.Hej.expr.XPathContext context) {
            return /*Delegate.eval()*/makeElaborator().elaborateForItem()(context);
        }
        public override string getStreamerName() {
            return "Copy";
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("copy", this);
            exportValidationAndType(@out);
            string flags = "";
            if (copyNamespaces) {
                flags = "c";
            }
            if (bequeathNamespacesToChildren) {
                flags += "i";
            }
            if (inheritNamespacesFromParent) {
                flags += "n";
            }
            if (isLocal()) {
                flags += "l";
            }
            @out.emitAttribute("flags", flags);
            string sType = Saxon.Hej.value.SequenceType.makeSequenceType(selectItemType, getCardinality()).toAlphaCode();
            @out.emitAttribute("sit", sType);
            @out.setChildRole("content");
            getContentExpression().export(@out);
            @out.endElement();
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Hej.expr.instruct.Copy.CopyElaborator();
        }
        public class CopyElementDetails : Saxon.Hej.expr.instruct.ElementCreator.ElementCreationDetails {
            private readonly Saxon.Hej.expr.elab.PushEvaluator contentEvaluator;
            private readonly Saxon.Hej.om.NodeInfo copiedNode;
            public CopyElementDetails(Saxon.Hej.expr.elab.PushEvaluator contentEvaluator, Saxon.Hej.om.NodeInfo copiedNode)  {
                this.contentEvaluator = contentEvaluator;
                this.copiedNode = copiedNode;
            }
            public Saxon.Hej.om.NodeInfo getCopiedNode() {
                return copiedNode;
            }
            public override Saxon.Hej.om.NodeName getNodeName(Saxon.Hej.expr.XPathContext context) {
                return Saxon.Hej.om.NameOfNode.makeName(copiedNode);
            }
            public override string getSystemId(Saxon.Hej.expr.XPathContext context) {
                return copiedNode.getBaseURI();
            }
            public override void processContent(Saxon.Hej.@event.Outputter @out, Saxon.Hej.expr.XPathContext context) {
                Saxon.Hej.expr.Expression.dispatchTailCall(/*Delegate.processLeavingTail()*/contentEvaluator(@out, context));
            }
        
        }
        public class CopyElaborator : Saxon.Hej.expr.elab.PushElaborator {
            public override Saxon.Hej.expr.elab.PushEvaluator elaborateForPush() {
                Saxon.Hej.expr.instruct.Copy expr = (Saxon.Hej.expr.instruct.Copy)(getExpression());
                Saxon.Hej.expr.elab.PushEvaluator contentPush = expr.getContentExpression().makeElaborator().elaborateForPush();
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
                return (output, context) => {
                    Saxon.Hej.Controller controller = context.getController();
                    Saxon.Hej.om.Item item = context.getContextItem();
                    if ((item == null)) {
                        Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("There is no context item for xsl:copy", "XTTE0945");
                        err.setIsTypeError(true);
                        err.setLocation(expr.getLocation());
                        err.setXPathContext(context);
                        throw err;
                    }
                    if (!(((item is Saxon.Hej.om.NodeInfo)))) {
                        output.append(item, expr.getLocation(), Saxon.Hej.@event.ReceiverOption.ALL_NAMESPACES);
                        return null;
                    }
                    Saxon.Hej.om.NodeInfo source = (Saxon.Hej.om.NodeInfo)(item);
                    switch (source.getNodeKind()) {
                      case Saxon.Hej.type.Type.ELEMENT:
                        try {
                            Saxon.Hej.om.NodeName elemName = Saxon.Hej.om.NameOfNode.makeName(source);
                            Saxon.Hej.@event.Receiver elemOut = output;
                            if (!(expr.preservingTypes)) {
                                Saxon.Hej.lib.ParseOptions options = expr.getValidationOptions().withTopLevelElement(elemName.getStructuredQName());
                                context.getConfiguration().prepareValidationReporting(context, options);
                                Saxon.Hej.@event.Receiver validator = context.getConfiguration().getElementValidator(elemOut, options, expr.getLocation());
                                if ((validator != elemOut)) {
                                    output = new Saxon.Hej.@event.ComplexContentOutputter(validator);
                                }
                            }
                            if ((output.getSystemId() == null)) {
                                output.setSystemId(source.getBaseURI());
                            }
                            output.startElement(elemName, typeCode, expr.getLocation(), finalProperties);
                            if (expr.copyNamespaces) {
                                output.namespaces(source.getAllNamespaces(), Saxon.Hej.@event.ReceiverOption.NAMESPACE_OK);
                            } else {
                                Saxon.Hej.om.NamespaceBinding ns = elemName.getNamespaceBinding();
                                if (!(ns.isDefaultUndeclaration())) {
                                    output.@namespace(ns.getPrefix(), ns.getNamespaceUri(), Saxon.Hej.@event.ReceiverOption.NONE);
                                }
                            }
                            Saxon.Hej.expr.Expression.dispatchTailCall(/*Delegate.processLeavingTail()*/contentPush(output, context));
                            output.endElement();
                        } catch (Saxon.Hej.trans.XPathException e) {
                            e.maybeSetLocation(expr.getLocation());
                            e.maybeSetContext(context);
                            throw;
                        }
                        return null;
                      case Saxon.Hej.type.Type.ATTRIBUTE:
                        if ((expr.getSchemaType() is Saxon.Hej.type.ComplexType)) {
                            expr.dynamicError("Cannot copy an attribute when the type requested for validation is a complex type", "XTTE1535", context);
                        }
                        try {
                            Saxon.Hej.expr.instruct.CopyOf.copyAttribute(source, (Saxon.Hej.type.SimpleType)(expr.getSchemaType()), expr.getValidationAction(), expr, output, context, false);
                        } catch (Saxon.Hej.@event.NoOpenStartTagException err) {
                            err.setXPathContext(context);
                            throw dynamicError(expr.getLocation(), err, context);
                        }
                        break ;
                      case Saxon.Hej.type.Type.TEXT:
                        Saxon.Hej.str.UnicodeString tval = source.getUnicodeStringValue();
                        output.characters(tval, expr.getLocation(), Saxon.Hej.@event.ReceiverOption.NONE);
                        break ;
                      case Saxon.Hej.type.Type.PROCESSING_INSTRUCTION:
                        Saxon.Hej.str.UnicodeString pval = source.getUnicodeStringValue();
                        output.processingInstruction(source.getDisplayName(), pval, expr.getLocation(), Saxon.Hej.@event.ReceiverOption.NONE);
                        break ;
                      case Saxon.Hej.type.Type.COMMENT:
                        Saxon.Hej.str.UnicodeString cval = source.getUnicodeStringValue();
                        output.comment(cval, expr.getLocation(), Saxon.Hej.@event.ReceiverOption.NONE);
                        break ;
                      case Saxon.Hej.type.Type.NAMESPACE:
                        output.@namespace(((Saxon.Hej.om.NodeInfo)(item)).getLocalPart(), Saxon.Hej.om.NamespaceUri.of(item.getStringValue()), Saxon.Hej.@event.ReceiverOption.NONE);
                        break ;
                      case Saxon.Hej.type.Type.DOCUMENT:
                        if (!(expr.preservingTypes)) {
                            Saxon.Hej.lib.ParseOptions options = expr.getValidationOptions().withSpaceStrippingRule(Saxon.Hej.om.NoElementsSpaceStrippingRule.getInstance());
                            controller.getConfiguration().prepareValidationReporting(context, options);
                            Saxon.Hej.@event.Receiver val = controller.getConfiguration().getDocumentValidator(output, source.getBaseURI(), options, expr.getLocation());
                            output = new Saxon.Hej.@event.ComplexContentOutputter(val);
                        }
                        if ((output.getSystemId() == null)) {
                            output.setSystemId(source.getBaseURI());
                        }
                        output.startDocument(Saxon.Hej.@event.ReceiverOption.NONE);
                        copyUnparsedEntities(source, output);
                        Saxon.Hej.expr.Expression.dispatchTailCall(/*Delegate.processLeavingTail()*/contentPush(output, context));
                        output.endDocument();
                        break ;
                      default:
                        throw new Saxon.Ejava.lang.IllegalArgumentException(("Unknown node kind "+source.getNodeKind()));
                    }
                    return null;
                } ;
            }
        
        }
    
    }
}