using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    #pragma warning disable 659
    public sealed class AxisExpression : Saxon.Hej.expr.Expression {
        private int axis;
        private Saxon.Hej.pattern.NodeTest test;
        private Saxon.Hej.type.ItemType itemType = null;
        private Saxon.Hej.expr.parser.ContextItemStaticInfo staticInfo = Saxon.Hej.expr.parser.ContextItemStaticInfo.DEFAULT;
        private bool doneTypeCheck = false;
        private bool doneOptimize = false;
        public AxisExpression(int axis, Saxon.Hej.pattern.NodeTest nodeTest)  {
            this.axis = axis;
            this.test = nodeTest;
        }
        public void setAxis(int axis) {
            this.axis = axis;
        }
        public override string getExpressionName() {
            return "axisStep";
        }
        public override Saxon.Hej.expr.Expression simplify() {
            Saxon.Hej.expr.Expression e2 = base.simplify();
            if ((e2 != this)) {
                return e2;
            }
            if (((((test == null) || (test == Saxon.Hej.pattern.AnyNodeTest.getInstance()))) && (((axis == Saxon.Hej.om.AxisInfo.PARENT) || (axis == Saxon.Hej.om.AxisInfo.ANCESTOR))))) {
                test = Saxon.Hej.pattern.MultipleNodeKindTest.PARENT_NODE;
            }
            return this;
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            Saxon.Hej.type.ItemType contextItemType = contextInfo.getItemType();
            bool noWarnings = (doneOptimize || ((doneTypeCheck && this.staticInfo.getItemType().Equals(contextItemType))));
            doneTypeCheck = true;
            if ((contextItemType == Saxon.Hej.type.ErrorType.getInstance())) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException((("Axis step "+this)+" cannot be used here: the context item is absent"));
                err.setErrorCode("XPDY0002");
                err.setLocation(getLocation());
                throw err;
            } else {
                staticInfo = contextInfo;
            }
            Saxon.Hej.Configuration config = visitor.getConfiguration();
            if ((contextItemType.getGenre() != Saxon.Hej.om.Genre.NODE)) {
                Saxon.Hej.type.TypeHierarchy th = config.getTypeHierarchy();
                Saxon.Hej.type.Affinity relation = th.relationship(contextItemType, Saxon.Hej.pattern.AnyNodeTest.getInstance());
                if ((relation == Saxon.Hej.type.Affinity.DISJOINT)) {
                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException((("Axis step "+this)+" cannot be used here: the context item is not a node"));
                    err.setIsTypeError(true);
                    err.setErrorCode("XPTY0020");
                    err.setLocation(getLocation());
                    throw err;
                } else {
                    if (((relation == Saxon.Hej.type.Affinity.OVERLAPS) || (relation == Saxon.Hej.type.Affinity.SUBSUMES))) {
                        Saxon.Hej.expr.Expression thisExp = checkPlausibility(visitor, contextInfo, !(noWarnings));
                        if (Saxon.Hej.expr.Literal.isEmptySequence(thisExp)) {
                            return thisExp;
                        }
                        Saxon.Hej.expr.ContextItemExpression exp = new Saxon.Hej.expr.ContextItemExpression();
                        Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, exp);
                        Saxon.Hej.expr.parser.RoleDiagnostic role = new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.AXIS_STEP, "", axis);
                        role.setErrorCode("XPTY0020");
                        Saxon.Hej.expr.ItemChecker checker = new Saxon.Hej.expr.ItemChecker(exp, Saxon.Hej.pattern.AnyNodeTest.getInstance(), role);
                        Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, checker);
                        Saxon.Hej.expr.SimpleStepExpression step = new Saxon.Hej.expr.SimpleStepExpression(checker, thisExp);
                        Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, step);
                        return step;
                    }
                }
            }
            if (visitor.getStaticContext().getOptimizerOptions().isSet(Saxon.Hej.expr.parser.OptimizerOptions.VOID_EXPRESSIONS)) {
                return checkPlausibility(visitor, contextInfo, !(noWarnings));
            } else {
                return this;
            }
        }
        private Saxon.Hej.expr.Expression checkPlausibility(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo, bool warnings) {
            Saxon.Hej.expr.StaticContext env = visitor.getStaticContext();
            Saxon.Hej.Configuration config = env.getConfiguration();
            Saxon.Hej.type.ItemType contextType = contextInfo.getItemType();
            if (!(((contextType is Saxon.Hej.pattern.NodeTest)))) {
                contextType = Saxon.Hej.pattern.AnyNodeTest.getInstance();
            }
            if (((test != null) && !(Saxon.Hej.om.AxisInfo.getTargetUType(Saxon.Hej.type.UType.ANY_NODE, axis).overlaps(test.getUType())))) {
                if (warnings) {
                    visitor.issueWarning(((("The "+Saxon.Hej.om.AxisInfo.axisName[axis])+" axis will never select ")+test.getUType().toStringWithIndefiniteArticle()), getLocation());
                }
                return Saxon.Hej.expr.Literal.makeEmptySequence();
            }
            if ((((test is Saxon.Hej.pattern.NameTest) && (axis == Saxon.Hej.om.AxisInfo.NAMESPACE)) && !(((Saxon.Hej.pattern.NameTest)(test)).getNamespaceURI().Length==0))) {
                if (warnings) {
                    visitor.issueWarning("The names of namespace nodes are never prefixed, so this axis step will never select anything", getLocation());
                }
                return Saxon.Hej.expr.Literal.makeEmptySequence();
            }
            Saxon.Hej.type.UType originUType = contextType.getUType();
            Saxon.Hej.type.UType targetUType = Saxon.Hej.om.AxisInfo.getTargetUType(originUType, axis);
            Saxon.Hej.type.UType testUType = ((test == null) ? Saxon.Hej.type.UType.ANY_NODE : test.getUType());
            if (targetUType.Equals(Saxon.Hej.type.UType.VOID)) {
                if (warnings) {
                    visitor.issueWarning((((("The "+Saxon.Hej.om.AxisInfo.axisName[axis])+" axis starting at ")+originUType.toStringWithIndefiniteArticle())+" will never select anything"), getLocation());
                }
                return Saxon.Hej.expr.Literal.makeEmptySequence();
            }
            if ((contextInfo.isParentless() && (((axis == Saxon.Hej.om.AxisInfo.PARENT) || (axis == Saxon.Hej.om.AxisInfo.ANCESTOR))))) {
                if (warnings) {
                    visitor.issueWarning((("The "+Saxon.Hej.om.AxisInfo.axisName[axis])+" axis will never select anything because the context item is parentless"), getLocation());
                }
                return Saxon.Hej.expr.Literal.makeEmptySequence();
            }
            if (!(targetUType.overlaps(testUType))) {
                if (warnings) {
                    visitor.issueWarning(((((("The "+Saxon.Hej.om.AxisInfo.axisName[axis])+" axis starting at ")+originUType.toStringWithIndefiniteArticle())+" will never select ")+test.getUType().toStringWithIndefiniteArticle()), getLocation());
                }
                return Saxon.Hej.expr.Literal.makeEmptySequence();
            }
            int nonSelf = Saxon.Hej.om.AxisInfo.excludeSelfAxis[axis];
            Saxon.Hej.type.UType kind = ((test == null) ? Saxon.Hej.type.UType.ANY_NODE : test.getUType());
            if ((axis != nonSelf)) {
                Saxon.Hej.type.UType nonSelfTarget = Saxon.Hej.om.AxisInfo.getTargetUType(originUType, nonSelf);
                if (!(nonSelfTarget.overlaps(testUType))) {
                    axis = Saxon.Hej.om.AxisInfo.SELF;
                    targetUType = Saxon.Hej.om.AxisInfo.getTargetUType(originUType, axis);
                }
            }
            Saxon.Hej.type.ItemType target = targetUType.toItemType();
            if (((test == null) || (test is Saxon.Hej.pattern.AnyNodeTest))) {
                itemType = target;
            } else {
                if (((target is Saxon.Hej.pattern.AnyNodeTest) || targetUType.subsumes(test.getUType()))) {
                    itemType = test;
                } else {
                    itemType = new Saxon.Hej.pattern.CombinedNodeTest((Saxon.Hej.pattern.NodeTest)(target), Saxon.Hej.expr.parser.Token.INTERSECT, test);
                }
            }
            int origin = contextType.getPrimitiveType();
            if ((test != null)) {
                if (((contextType is Saxon.Hej.pattern.DocumentNodeTest) && kind.Equals(Saxon.Hej.type.UType.ELEMENT))) {
                    Saxon.Hej.pattern.NodeTest elementTest = ((Saxon.Hej.pattern.DocumentNodeTest)(contextType)).getElementTest();
                    Saxon.Ejava.util.Optional<Saxon.Hej.z.IntSet> outermostElementNames = elementTest.getRequiredNodeNames();
                    if (outermostElementNames.isPresent()) {
                        Saxon.Ejava.util.Optional<Saxon.Hej.z.IntSet> selectedElementNames = test.getRequiredNodeNames();
                        if (selectedElementNames.isPresent()) {
                            if ((axis == Saxon.Hej.om.AxisInfo.CHILD)) {
                                if (selectedElementNames.get().intersect(outermostElementNames.get()).isEmpty()) {
                                    if (warnings) {
                                        visitor.issueWarning(("Starting at a document node, the step is selecting an element whose name "+"is not among the names of child elements permitted for this document node type"), getLocation());
                                    }
                                    return Saxon.Hej.expr.Literal.makeEmptySequence();
                                }
                                if (((env.getPackageData().isSchemaAware() && (elementTest is Saxon.Hej.pattern.SchemaNodeTest)) && (outermostElementNames.get().size() == 1))) {
                                    Saxon.Hej.z.IntIterator oeni = outermostElementNames.get().iterator();
                                    int outermostElementName = (oeni.hasNext() ? oeni.next() : -1);
                                    Saxon.Hej.type.SchemaDeclaration decl = config.getElementDeclaration(outermostElementName);
                                    if ((decl == null)) {
                                        if (warnings) {
                                            visitor.issueWarning((("Element "+config.getNamePool().getEQName(outermostElementName))+" is not declared in the schema"), getLocation());
                                        }
                                        itemType = elementTest;
                                    } else {
                                        itemType = new Saxon.Hej.pattern.CombinedNodeTest(elementTest, Saxon.Hej.expr.parser.Token.INTERSECT, new Saxon.Hej.pattern.ContentTypeTest(Saxon.Hej.type.Type.ELEMENT, decl.getType(), config, true));
                                    }
                                } else {
                                    itemType = elementTest;
                                }
                                return this;
                            } else {
                                if ((axis == Saxon.Hej.om.AxisInfo.DESCENDANT)) {
                                    bool canMatchOutermost = !(selectedElementNames.get().intersect(outermostElementNames.get()).isEmpty());
                                    if (!(canMatchOutermost)) {
                                        Saxon.Hej.expr.Expression path = Saxon.Hej.expr.parser.ExpressionTool.makePathExpression(new Saxon.Hej.expr.AxisExpression(Saxon.Hej.om.AxisInfo.CHILD, elementTest), new Saxon.Hej.expr.AxisExpression(Saxon.Hej.om.AxisInfo.DESCENDANT, test));
                                        Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, path);
                                        return path.typeCheck(visitor, contextInfo);
                                    }
                                }
                            }
                        }
                    }
                }
                Saxon.Hej.type.SchemaType contentType = ((Saxon.Hej.pattern.NodeTest)(contextType)).getContentType();
                if ((contentType == Saxon.Hej.type.AnyType.getInstance())) {
                    return this;
                }
                if (!(env.getPackageData().isSchemaAware())) {
                    Saxon.Hej.type.SchemaType ct = test.getContentType();
                    if (!((((((((ct == Saxon.Hej.type.AnyType.getInstance()) || (ct == Saxon.Hej.type.Untyped.getInstance())) || (ct == Saxon.Hej.type.AnySimpleType.getInstance())) || (ct == Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC)) || (ct == Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC)) || (ct == Saxon.Hej.type.BuiltInAtomicType.STRING))))) {
                        if (warnings) {
                            visitor.issueWarning(((("The "+Saxon.Hej.om.AxisInfo.axisName[axis])+" axis will never select any typed nodes, ")+"because the expression is being compiled in an environment that is not schema-aware"), getLocation());
                        }
                        return Saxon.Hej.expr.Literal.makeEmptySequence();
                    }
                }
                int targetfp = test.getFingerprint();
                Saxon.Hej.om.StructuredQName targetName = test.getMatchingNodeName();
                if (contentType.isSimpleType()) {
                    if (warnings) {
                        if ((((((axis == Saxon.Hej.om.AxisInfo.CHILD) || (axis == Saxon.Hej.om.AxisInfo.DESCENDANT)) || (axis == Saxon.Hej.om.AxisInfo.DESCENDANT_OR_SELF))) && Saxon.Hej.type.UType.PARENT_NODE_KINDS.union(Saxon.Hej.type.UType.ATTRIBUTE).subsumes(kind))) {
                            visitor.issueWarning(((((("The "+Saxon.Hej.om.AxisInfo.axisName[axis])+" axis will never select any ")+kind)+" nodes when starting at ")+(((origin == Saxon.Hej.type.Type.ATTRIBUTE) ? "an attribute node" : getStartingNodeDescription(contentType)))), getLocation());
                        } else {
                            if ((((axis == Saxon.Hej.om.AxisInfo.CHILD) && kind.Equals(Saxon.Hej.type.UType.TEXT)) && ((getParentExpression() is Saxon.Hej.expr.Atomizer)))) {
                                visitor.issueWarning((("Selecting the text nodes of an element with simple content may give the "+"wrong answer in the presence of comments or processing instructions. It is usually ")+"better to omit the '/text()' step"), getLocation());
                            } else {
                                if ((axis == Saxon.Hej.om.AxisInfo.ATTRIBUTE)) {
                                    bool found = false;
                                    if ((targetfp == -1)) {
                                        foreach (Saxon.Hej.type.SchemaType extension in config.getExtensionsOfType(contentType)) {
                                            if (((Saxon.Hej.type.ComplexType)(extension)).allowsAttributes()) {
                                                found = true;
                                                break ;
                                            }
                                        }
                                    } else {
                                        foreach (Saxon.Hej.type.SchemaType extension in config.getExtensionsOfType(contentType)) {
                                            try {
                                                if ((((Saxon.Hej.type.ComplexType)(extension)).getAttributeUseType(targetName) != null)) {
                                                    found = true;
                                                    break ;
                                                }
                                            } catch (Saxon.Hej.type.SchemaException e) {
                                            }
                                        }
                                    }
                                    if (!(found)) {
                                        visitor.issueWarning(((((("The "+Saxon.Hej.om.AxisInfo.axisName[axis])+" axis will never select ")+(((targetName == null) ? "any attribute nodes" : ("an attribute node named "+getDiagnosticName(targetName, env)))))+" when starting at ")+getStartingNodeDescription(contentType)), getLocation());
                                    }
                                }
                            }
                        }
                    }
                } else {
                    if (((((Saxon.Hej.type.ComplexType)(contentType)).isSimpleContent() && ((((axis == Saxon.Hej.om.AxisInfo.CHILD) || (axis == Saxon.Hej.om.AxisInfo.DESCENDANT)) || (axis == Saxon.Hej.om.AxisInfo.DESCENDANT_OR_SELF)))) && Saxon.Hej.type.UType.PARENT_NODE_KINDS.subsumes(kind))) {
                        if (warnings) {
                            visitor.issueWarning((((((("The "+Saxon.Hej.om.AxisInfo.axisName[axis])+" axis will never select any ")+kind)+" nodes when starting at ")+getStartingNodeDescription(contentType))+", as this type requires simple content"), getLocation());
                        }
                        return Saxon.Hej.expr.Literal.makeEmptySequence();
                    } else {
                        if ((((Saxon.Hej.type.ComplexType)(contentType)).isEmptyContent() && ((((axis == Saxon.Hej.om.AxisInfo.CHILD) || (axis == Saxon.Hej.om.AxisInfo.DESCENDANT)) || (axis == Saxon.Hej.om.AxisInfo.DESCENDANT_OR_SELF))))) {
                            foreach (Saxon.Hej.type.SchemaType extension in config.getExtensionsOfType(contentType)) {
                                if (!(((Saxon.Hej.type.ComplexType)(extension)).isEmptyContent())) {
                                    return this;
                                }
                            }
                            if (warnings) {
                                visitor.issueWarning(((((("The "+Saxon.Hej.om.AxisInfo.axisName[axis])+" axis will never select any")+" nodes when starting at ")+getStartingNodeDescription(contentType))+", as this type requires empty content"), getLocation());
                            }
                            return Saxon.Hej.expr.Literal.makeEmptySequence();
                        } else {
                            if ((axis == Saxon.Hej.om.AxisInfo.ATTRIBUTE)) {
                                if ((targetfp == -1)) {
                                    if (warnings) {
                                        if (!(((Saxon.Hej.type.ComplexType)(contentType)).allowsAttributes())) {
                                            visitor.issueWarning((("The complex type "+contentType.getDescription())+" allows no attributes other than the standard attributes in the xsi namespace"), getLocation());
                                        }
                                    }
                                } else {
                                    try {
                                        Saxon.Hej.type.SchemaType schemaType;
                                        if ((targetfp == Saxon.Hej.om.StandardNames.XSI_TYPE)) {
                                            schemaType = Saxon.Hej.type.BuiltInAtomicType.QNAME;
                                        } else {
                                            if ((targetfp == Saxon.Hej.om.StandardNames.XSI_SCHEMA_LOCATION)) {
                                                schemaType = Saxon.Hej.type.BuiltInListType.ANY_URIS;
                                            } else {
                                                if ((targetfp == Saxon.Hej.om.StandardNames.XSI_NO_NAMESPACE_SCHEMA_LOCATION)) {
                                                    schemaType = Saxon.Hej.type.BuiltInAtomicType.ANY_URI;
                                                } else {
                                                    if ((targetfp == Saxon.Hej.om.StandardNames.XSI_NIL)) {
                                                        schemaType = Saxon.Hej.type.BuiltInAtomicType.BOOLEAN;
                                                    } else {
                                                        schemaType = ((Saxon.Hej.type.ComplexType)(contentType)).getAttributeUseType(targetName);
                                                    }
                                                }
                                            }
                                        }
                                        if ((schemaType == null)) {
                                            if (warnings) {
                                                visitor.issueWarning(((("The complex type "+contentType.getDescription())+" does not allow an attribute named ")+getDiagnosticName(targetName, env)), getLocation());
                                                return Saxon.Hej.expr.Literal.makeEmptySequence();
                                            }
                                        } else {
                                            itemType = new Saxon.Hej.pattern.CombinedNodeTest(test, Saxon.Hej.expr.parser.Token.INTERSECT, new Saxon.Hej.pattern.ContentTypeTest(Saxon.Hej.type.Type.ATTRIBUTE, schemaType, config, false));
                                        }
                                    } catch (Saxon.Hej.type.SchemaException e) {
                                    }
                                }
                            } else {
                                if (((axis == Saxon.Hej.om.AxisInfo.CHILD) && kind.Equals(Saxon.Hej.type.UType.ELEMENT))) {
                                    try {
                                        int childfp = targetfp;
                                        if ((targetName == null)) {
                                            if (((Saxon.Hej.type.ComplexType)(contentType)).containsElementWildcard()) {
                                                return this;
                                            }
                                            Saxon.Hej.z.IntHashSet children = new Saxon.Hej.z.IntHashSet();
                                            ((Saxon.Hej.type.ComplexType)(contentType)).gatherAllPermittedChildren(children, false);
                                            if (children.isEmpty()) {
                                                if (warnings) {
                                                    visitor.issueWarning((("The complex type "+contentType.getDescription())+" does not allow children"), getLocation());
                                                }
                                                return Saxon.Hej.expr.Literal.makeEmptySequence();
                                            }
                                            if ((children.size() == 1)) {
                                                Saxon.Hej.z.IntIterator iter = children.iterator();
                                                if (iter.hasNext()) {
                                                    childfp = iter.next();
                                                }
                                            } else {
                                                return this;
                                            }
                                        }
                                        Saxon.Hej.type.SchemaType schemaType = ((Saxon.Hej.type.ComplexType)(contentType)).getElementParticleType(childfp, true);
                                        if ((schemaType == null)) {
                                            if (warnings) {
                                                Saxon.Hej.om.StructuredQName childElement = getConfiguration().getNamePool().getStructuredQName(childfp);
                                                string message = ((("The complex type "+contentType.getDescription())+" does not allow a child element named ")+getDiagnosticName(childElement, env));
                                                Saxon.Hej.z.IntHashSet permitted = new Saxon.Hej.z.IntHashSet();
                                                ((Saxon.Hej.type.ComplexType)(contentType)).gatherAllPermittedChildren(permitted, false);
                                                if (!(permitted.contains(-1))) {
                                                    Saxon.Hej.z.IntIterator kids = permitted.iterator();
                                                    while (kids.hasNext()) {
                                                        int kid = kids.next();
                                                        Saxon.Hej.om.StructuredQName sq = getConfiguration().getNamePool().getStructuredQName(kid);
                                                        if ((sq.getLocalPart().Equals(childElement.getLocalPart()) && (kid != childfp))) {
                                                            message += ((((". Perhaps the namespace is "+((childElement.hasURI("") ? "missing" : "wrong")))+", and ")+sq.getEQName())+" was intended?");
                                                            break ;
                                                        }
                                                    }
                                                }
                                                visitor.issueWarning(message, getLocation());
                                            }
                                            return Saxon.Hej.expr.Literal.makeEmptySequence();
                                        } else {
                                            itemType = new Saxon.Hej.pattern.CombinedNodeTest(test, Saxon.Hej.expr.parser.Token.INTERSECT, new Saxon.Hej.pattern.ContentTypeTest(Saxon.Hej.type.Type.ELEMENT, schemaType, config, true));
                                            int computedCardinality = ((Saxon.Hej.type.ComplexType)(contentType)).getElementParticleCardinality(childfp, true);
                                            Saxon.Hej.expr.parser.ExpressionTool.resetStaticProperties(this);
                                            if ((computedCardinality == Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO)) {
                                                Saxon.Hej.om.StructuredQName childElement = getConfiguration().getNamePool().getStructuredQName(childfp);
                                                visitor.issueWarning(((("The complex type "+contentType.getDescription())+" appears not to allow a child element named ")+getDiagnosticName(childElement, env)), getLocation());
                                                return Saxon.Hej.expr.Literal.makeEmptySequence();
                                            }
                                            if (((!(Saxon.Hej.value.Cardinality.allowsMany(computedCardinality)) && !(((getParentExpression() is Saxon.Hej.expr.FirstItemExpression)))) && !(visitor.isOptimizeForPatternMatching()))) {
                                                return Saxon.Hej.expr.FirstItemExpression.makeFirstItemExpression(this);
                                            }
                                        }
                                    } catch (Saxon.Hej.type.SchemaException e) {
                                    }
                                } else {
                                    if ((((axis == Saxon.Hej.om.AxisInfo.DESCENDANT) && kind.Equals(Saxon.Hej.type.UType.ELEMENT)) && (targetfp != -1))) {
                                        try {
                                            Saxon.Hej.z.IntHashSet descendants = new Saxon.Hej.z.IntHashSet();
                                            ((Saxon.Hej.type.ComplexType)(contentType)).gatherAllPermittedDescendants(descendants);
                                            if (descendants.contains(-1)) {
                                                return this;
                                            }
                                            if (descendants.contains(targetfp)) {
                                                Saxon.Hej.z.IntHashSet children = new Saxon.Hej.z.IntHashSet();
                                                ((Saxon.Hej.type.ComplexType)(contentType)).gatherAllPermittedChildren(children, false);
                                                Saxon.Hej.z.IntHashSet usefulChildren = new Saxon.Hej.z.IntHashSet();
                                                bool considerSelf = false;
                                                bool considerDescendants = false;
                                                Saxon.Hej.z.IntIterator kids = children.iterator();
                                                while (kids.hasNext()) {
                                                    int c = kids.next();
                                                    if ((c == targetfp)) {
                                                        usefulChildren.add(c);
                                                        considerSelf = true;
                                                    }
                                                    Saxon.Hej.type.SchemaType st = ((Saxon.Hej.type.ComplexType)(contentType)).getElementParticleType(c, true);
                                                    if ((st == null)) {
                                                        throw new Saxon.Ejava.lang.AssertionError(("Can't find type for child element "+c));
                                                    }
                                                    if ((st is Saxon.Hej.type.ComplexType)) {
                                                        Saxon.Hej.z.IntHashSet subDescendants = new Saxon.Hej.z.IntHashSet();
                                                        ((Saxon.Hej.type.ComplexType)(st)).gatherAllPermittedDescendants(subDescendants);
                                                        if (subDescendants.contains(targetfp)) {
                                                            usefulChildren.add(c);
                                                            considerDescendants = true;
                                                        }
                                                    }
                                                }
                                                itemType = test;
                                                if (considerDescendants) {
                                                    Saxon.Hej.type.SchemaType st = ((Saxon.Hej.type.ComplexType)(contentType)).getDescendantElementType(targetfp);
                                                    if ((st != Saxon.Hej.type.AnyType.getInstance())) {
                                                        itemType = new Saxon.Hej.pattern.CombinedNodeTest(test, Saxon.Hej.expr.parser.Token.INTERSECT, new Saxon.Hej.pattern.ContentTypeTest(Saxon.Hej.type.Type.ELEMENT, st, config, true));
                                                    }
                                                }
                                                if ((usefulChildren.size()<children.size())) {
                                                    Saxon.Hej.pattern.NodeTest childTest = makeUnionNodeTest(usefulChildren, config.getNamePool());
                                                    Saxon.Hej.expr.AxisExpression first = new Saxon.Hej.expr.AxisExpression(Saxon.Hej.om.AxisInfo.CHILD, childTest);
                                                    Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, first);
                                                    int nextAxis;
                                                    if (considerSelf) {
                                                        nextAxis = (considerDescendants ? Saxon.Hej.om.AxisInfo.DESCENDANT_OR_SELF : Saxon.Hej.om.AxisInfo.SELF);
                                                    } else {
                                                        nextAxis = Saxon.Hej.om.AxisInfo.DESCENDANT;
                                                    }
                                                    Saxon.Hej.expr.AxisExpression next = new Saxon.Hej.expr.AxisExpression(nextAxis, (Saxon.Hej.pattern.NodeTest)(itemType));
                                                    Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, next);
                                                    Saxon.Hej.expr.Expression path = Saxon.Hej.expr.parser.ExpressionTool.makePathExpression(first, next);
                                                    Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, path);
                                                    return path.typeCheck(visitor, contextInfo);
                                                }
                                            } else {
                                                if (warnings) {
                                                    visitor.issueWarning(((("The complex type "+contentType.getDescription())+" does not allow a descendant element named ")+getDiagnosticName(targetName, env)), getLocation());
                                                }
                                            }
                                        } catch (Saxon.Hej.type.SchemaException e) {
                                            throw new Saxon.Ejava.lang.AssertionError(e);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return this;
        }
        private static string getDiagnosticName(Saxon.Hej.om.StructuredQName name, Saxon.Hej.expr.StaticContext env) {
            string uri = name.getURI();
            if (uri.Equals("")) {
                return name.getLocalPart();
            } else {
                Saxon.Hej.om.NamespaceResolver resolver = env.getNamespaceResolver();
                for (System.Collections.Generic.IEnumerator<string> it = resolver.iteratePrefixes(); it.MoveNext(); ) {
                    string prefix = it.Current;
                    if (uri.Equals(resolver.getURIForPrefix(prefix, true))) {
                        if (prefix.Length==0) {
                            return ((("Q{"+uri)+"}")+name.getLocalPart());
                        } else {
                            return ((prefix+":")+name.getLocalPart());
                        }
                    }
                }
            }
            return ((("Q{"+uri)+"}")+name.getLocalPart());
        }
        private static string getStartingNodeDescription(Saxon.Hej.type.SchemaType type) {
            string s = type.getDescription();
            if (s.StartsWith("of element")) {
                return ("a valid element named"+s.Substring("of element".Length));
            } else {
                if (s.StartsWith("of attribute")) {
                    return ("a valid attribute named"+s.Substring("of attribute".Length));
                } else {
                    return ((("a node with "+((type.isSimpleType() ? "simple" : "complex")))+" type ")+s);
                }
            }
        }
        private Saxon.Hej.pattern.NodeTest makeUnionNodeTest(Saxon.Hej.z.IntHashSet elements, Saxon.Hej.om.NamePool pool) {
            Saxon.Hej.pattern.NodeTest test = null;
            Saxon.Hej.z.IntIterator iter = elements.iterator();
            while (iter.hasNext()) {
                int fp = iter.next();
                Saxon.Hej.pattern.NodeTest nextTest = new Saxon.Hej.pattern.NameTest(Saxon.Hej.type.Type.ELEMENT, fp, pool);
                if ((test == null)) {
                    test = nextTest;
                } else {
                    test = new Saxon.Hej.pattern.CombinedNodeTest(test, Saxon.Hej.expr.parser.Token.UNION, nextTest);
                }
            }
            return test;
        }
        public Saxon.Hej.type.ItemType getContextItemType() {
            return staticInfo.getItemType();
        }
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            doneOptimize = true;
            staticInfo = contextInfo;
            return this;
        }
        public override double getCost() {
            switch (axis) {
              case Saxon.Hej.om.AxisInfo.SELF:
              case Saxon.Hej.om.AxisInfo.PARENT:
              case Saxon.Hej.om.AxisInfo.ATTRIBUTE:
                return 1;
              case Saxon.Hej.om.AxisInfo.CHILD:
              case Saxon.Hej.om.AxisInfo.FOLLOWING_SIBLING:
              case Saxon.Hej.om.AxisInfo.PRECEDING_SIBLING:
              case Saxon.Hej.om.AxisInfo.ANCESTOR:
              case Saxon.Hej.om.AxisInfo.ANCESTOR_OR_SELF:
                return 5;
              default:
                return 20;
            }
        }
        public override bool Equals(System.Object other) {
            return (((other is Saxon.Hej.expr.AxisExpression) && (axis == ((Saxon.Hej.expr.AxisExpression)(other)).axis)) && Saxon.Ejava.util.Objects.Equals(test, ((Saxon.Hej.expr.AxisExpression)(other)).test));
        }
        protected internal override int computeHashCode() {
            int h = ((9375162+axis)<<20);
            if ((test != null)) {
                h ^= (test.getPrimitiveType()<<16);
                h ^= test.getFingerprint();
            }
            return h;
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.AxisExpression a2 = new Saxon.Hej.expr.AxisExpression(axis, test);
            a2.itemType = itemType;
            a2.staticInfo = staticInfo;
            a2.doneTypeCheck = doneTypeCheck;
            a2.doneOptimize = doneOptimize;
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, a2);
            return a2;
        }
        protected internal override int computeSpecialProperties() {
            return ((((((Saxon.Hej.expr.StaticProperty.CONTEXT_DOCUMENT_NODESET|Saxon.Hej.expr.StaticProperty.SINGLE_DOCUMENT_NODESET)|Saxon.Hej.expr.StaticProperty.NO_NODES_NEWLY_CREATED)|((Saxon.Hej.om.AxisInfo.isForwards[axis] ? Saxon.Hej.expr.StaticProperty.ORDERED_NODESET : Saxon.Hej.expr.StaticProperty.REVERSE_DOCUMENT_ORDER)))|(((Saxon.Hej.om.AxisInfo.isPeerAxis[axis] || isPeerNodeTest(test)) ? Saxon.Hej.expr.StaticProperty.PEER_NODESET : 0)))|((Saxon.Hej.om.AxisInfo.isSubtreeAxis[axis] ? Saxon.Hej.expr.StaticProperty.SUBTREE_NODESET : 0)))|((((axis == Saxon.Hej.om.AxisInfo.ATTRIBUTE) || (axis == Saxon.Hej.om.AxisInfo.NAMESPACE)) ? Saxon.Hej.expr.StaticProperty.ATTRIBUTE_NS_NODESET : 0)));
        }
        private static bool isPeerNodeTest(Saxon.Hej.pattern.NodeTest test) {
            if ((test == null)) {
                return false;
            }
            Saxon.Hej.type.UType uType = test.getUType();
            if (uType.overlaps(Saxon.Hej.type.UType.ELEMENT)) {
                return false;
            } else {
                if (uType.overlaps(Saxon.Hej.type.UType.DOCUMENT)) {
                    return uType.Equals(Saxon.Hej.type.UType.DOCUMENT);
                } else {
                    return true;
                }
            }
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            if ((itemType != null)) {
                return itemType;
            }
            int p = Saxon.Hej.om.AxisInfo.principalNodeType[axis];
            switch (p) {
              case Saxon.Hej.type.Type.ATTRIBUTE:
              case Saxon.Hej.type.Type.NAMESPACE:
                return Saxon.Hej.pattern.NodeKindTest.makeNodeKindTest(p);
              default:
                if ((test == null)) {
                    return Saxon.Hej.pattern.AnyNodeTest.getInstance();
                } else {
                    return test;
                }
            }
        }
        public override Saxon.Hej.type.UType getStaticUType(Saxon.Hej.type.UType contextItemType) {
            Saxon.Hej.type.UType reachable = Saxon.Hej.om.AxisInfo.getTargetUType(contextItemType, axis);
            if ((test == null)) {
                return reachable;
            } else {
                return reachable.intersection(test.getUType());
            }
        }
        public override int getIntrinsicDependencies() {
            return Saxon.Hej.expr.StaticProperty.DEPENDS_ON_CONTEXT_ITEM;
        }
        protected internal override int computeCardinality() {
            Saxon.Hej.pattern.NodeTest originNodeType;
            Saxon.Hej.pattern.NodeTest nodeTest = test;
            Saxon.Hej.type.ItemType contextItemType = staticInfo.getItemType();
            if ((contextItemType is Saxon.Hej.pattern.NodeTest)) {
                originNodeType = (Saxon.Hej.pattern.NodeTest)(contextItemType);
            } else {
                if ((contextItemType is Saxon.Hej.type.AnyItemType)) {
                    originNodeType = Saxon.Hej.pattern.AnyNodeTest.getInstance();
                } else {
                    return Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_MORE;
                }
            }
            if (((axis == Saxon.Hej.om.AxisInfo.ATTRIBUTE) && (nodeTest is Saxon.Hej.pattern.NameTest))) {
                Saxon.Hej.type.SchemaType contentType = originNodeType.getContentType();
                if ((contentType is Saxon.Hej.type.ComplexType)) {
                    try {
                        return ((Saxon.Hej.type.ComplexType)(contentType)).getAttributeUseCardinality(nodeTest.getMatchingNodeName());
                    } catch (Saxon.Hej.type.SchemaException err) {
                        return Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_ONE;
                    }
                } else {
                    if ((contentType is Saxon.Hej.type.SimpleType)) {
                        return Saxon.Hej.expr.StaticProperty.EMPTY;
                    }
                }
                return Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_ONE;
            } else {
                if ((((axis == Saxon.Hej.om.AxisInfo.DESCENDANT) && (nodeTest is Saxon.Hej.pattern.NameTest)) && (nodeTest.getPrimitiveType() == Saxon.Hej.type.Type.ELEMENT))) {
                    Saxon.Hej.type.SchemaType contentType = originNodeType.getContentType();
                    if ((contentType is Saxon.Hej.type.ComplexType)) {
                        try {
                            return ((Saxon.Hej.type.ComplexType)(contentType)).getDescendantElementCardinality(nodeTest.getFingerprint());
                        } catch (Saxon.Hej.type.SchemaException err) {
                            return Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_MORE;
                        }
                    } else {
                        return Saxon.Hej.expr.StaticProperty.EMPTY;
                    }
                } else {
                    if ((axis == Saxon.Hej.om.AxisInfo.SELF)) {
                        return Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_ONE;
                    } else {
                        return Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_MORE;
                    }
                }
            }
        }
        public override bool isSubtreeExpression() {
            return Saxon.Hej.om.AxisInfo.isSubtreeAxis[axis];
        }
        public int getAxis() {
            return axis;
        }
        public Saxon.Hej.pattern.NodeTest getNodeTest() {
            return test;
        }
        public override Saxon.Hej.expr.parser.PathMap.PathMapNodeSet addToPathMap(Saxon.Hej.expr.parser.PathMap pathMap, Saxon.Hej.expr.parser.PathMap.PathMapNodeSet pathMapNodeSet) {
            if ((pathMapNodeSet == null)) {
                Saxon.Hej.expr.ContextItemExpression cie = new Saxon.Hej.expr.ContextItemExpression();
                pathMapNodeSet = new Saxon.Hej.expr.parser.PathMap.PathMapNodeSet(pathMap.makeNewRoot(cie));
            }
            return pathMapNodeSet.createArc(axis, ((test == null) ? Saxon.Hej.pattern.AnyNodeTest.getInstance() : test));
        }
        public bool isContextPossiblyUndefined() {
            return staticInfo.isPossiblyAbsent();
        }
        public Saxon.Hej.expr.parser.ContextItemStaticInfo getContextItemStaticInfo() {
            return staticInfo;
        }
        public override Saxon.Hej.pattern.Pattern toPattern(Saxon.Hej.Configuration config) {
            Saxon.Hej.pattern.NodeTest test = getNodeTest();
            Saxon.Hej.pattern.Pattern pat;
            if ((test == null)) {
                test = Saxon.Hej.pattern.AnyNodeTest.getInstance();
            }
            if (((test is Saxon.Hej.pattern.AnyNodeTest) && ((((axis == Saxon.Hej.om.AxisInfo.CHILD) || (axis == Saxon.Hej.om.AxisInfo.DESCENDANT)) || (axis == Saxon.Hej.om.AxisInfo.SELF))))) {
                test = Saxon.Hej.pattern.MultipleNodeKindTest.CHILD_NODE;
            }
            int kind = test.getPrimitiveType();
            if ((axis == Saxon.Hej.om.AxisInfo.SELF)) {
                pat = new Saxon.Hej.pattern.NodeTestPattern(test);
            } else {
                if ((axis == Saxon.Hej.om.AxisInfo.ATTRIBUTE)) {
                    if ((kind == Saxon.Hej.type.Type.NODE)) {
                        pat = new Saxon.Hej.pattern.NodeTestPattern(Saxon.Hej.pattern.NodeKindTest.ATTRIBUTE);
                    } else {
                        if (!(Saxon.Hej.om.AxisInfo.containsNodeKind(axis, kind))) {
                            pat = new Saxon.Hej.pattern.NodeTestPattern(Saxon.Hej.type.ErrorType.getInstance());
                        } else {
                            pat = new Saxon.Hej.pattern.NodeTestPattern(test);
                        }
                    }
                } else {
                    if ((((axis == Saxon.Hej.om.AxisInfo.CHILD) || (axis == Saxon.Hej.om.AxisInfo.DESCENDANT)) || (axis == Saxon.Hej.om.AxisInfo.DESCENDANT_OR_SELF))) {
                        if (((kind != Saxon.Hej.type.Type.NODE) && !(Saxon.Hej.om.AxisInfo.containsNodeKind(axis, kind)))) {
                            pat = new Saxon.Hej.pattern.NodeTestPattern(Saxon.Hej.type.ErrorType.getInstance());
                        } else {
                            pat = new Saxon.Hej.pattern.NodeTestPattern(test);
                        }
                    } else {
                        if ((axis == Saxon.Hej.om.AxisInfo.NAMESPACE)) {
                            if ((kind == Saxon.Hej.type.Type.NODE)) {
                                pat = new Saxon.Hej.pattern.NodeTestPattern(Saxon.Hej.pattern.NodeKindTest.NAMESPACE);
                            } else {
                                if (!(Saxon.Hej.om.AxisInfo.containsNodeKind(axis, kind))) {
                                    pat = new Saxon.Hej.pattern.NodeTestPattern(Saxon.Hej.type.ErrorType.getInstance());
                                } else {
                                    pat = new Saxon.Hej.pattern.NodeTestPattern(test);
                                }
                            }
                        } else {
                            throw new Saxon.Hej.trans.XPathException("Only downwards axes are allowed in a pattern", "XTSE0340");
                        }
                    }
                }
            }
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, pat);
            return pat;
        }
        public override int getImplementationMethod() {
            return ITERATE_METHOD;
        }
        public override Saxon.Hej.om.SequenceIterator iterate(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.om.Item item = context.getContextItem();
            if ((item == null)) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException((("The context item for axis step "+this)+" is absent"));
                err.setErrorCode("XPDY0002");
                err.setXPathContext(context);
                err.setLocation(getLocation());
                err.setIsTypeError(true);
                throw err;
            }
            try {
                if ((test == null)) {
                    return ((Saxon.Hej.om.NodeInfo)(item)).iterateAxis(axis);
                } else {
                    return ((Saxon.Hej.om.NodeInfo)(item)).iterateAxis(axis, test);
                }
            } catch (System.InvalidCastException cce) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException((("The context item for axis step "+this)+" is not a node"));
                err.setErrorCode("XPTY0020");
                err.setXPathContext(context);
                err.setLocation(getLocation());
                err.setIsTypeError(true);
                throw err;
            } catch (Saxon.Ejava.lang.UnsupportedOperationException err) {
                if ((err.InnerException is Saxon.Hej.trans.XPathException)) {
                    Saxon.Hej.trans.XPathException ec = (Saxon.Hej.trans.XPathException)(err.InnerException);
                    ec.maybeSetLocation(getLocation());
                    ec.maybeSetContext(context);
                    throw ec;
                } else {
                    dynamicError(err.Message, "XPST0010", context);
                    return null;
                }
            }
        }
        public Saxon.Hej.tree.iter.AxisIterator iterate(Saxon.Hej.om.NodeInfo origin) {
            if ((test == null)) {
                return origin.iterateAxis(axis);
            } else {
                return origin.iterateAxis(axis, test);
            }
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter destination) {
            destination.startElement("axis", this);
            destination.emitAttribute("name", Saxon.Hej.om.AxisInfo.axisName[axis]);
            destination.emitAttribute("nodeTest", Saxon.Hej.type.AlphaCode.fromItemType(((test == null) ? Saxon.Hej.pattern.AnyNodeTest.getInstance() : test)));
            destination.endElement();
        }
        public override string ToString() {
            Saxon.Ejava.lang.StringBuilder fsb = new Saxon.Ejava.lang.StringBuilder(16);
            fsb.append(Saxon.Hej.om.AxisInfo.axisName[axis]);
            fsb.append("::");
            fsb.append(((test == null) ? "node()" : test.ToString()));
            return fsb.ToString();
        }
        public override string toShortString() {
            Saxon.Ejava.lang.StringBuilder fsb = new Saxon.Ejava.lang.StringBuilder(16);
            if ((axis == Saxon.Hej.om.AxisInfo.CHILD)) {
            } else {
                if ((axis == Saxon.Hej.om.AxisInfo.ATTRIBUTE)) {
                    fsb.append("@");
                } else {
                    fsb.append(Saxon.Hej.om.AxisInfo.axisName[axis]);
                    fsb.append("::");
                }
            }
            if ((test == null)) {
                fsb.append("node()");
            } else {
                if ((test is Saxon.Hej.pattern.NameTest)) {
                    if ((((Saxon.Hej.pattern.NameTest)(test)).getNodeKind() != Saxon.Hej.om.AxisInfo.principalNodeType[axis])) {
                        fsb.append(test.ToString());
                    } else {
                        fsb.append(test.getMatchingNodeName().getDisplayName());
                    }
                } else {
                    fsb.append(test.ToString());
                }
            }
            return fsb.ToString();
        }
        public override string getStreamerName() {
            return "AxisExpression";
        }
        public System.Collections.Generic.ISet<Saxon.Hej.expr.Expression> getPreconditions() {
            System.Collections.Generic.HashSet<Saxon.Hej.expr.Expression> pre = new System.Collections.Generic.HashSet<Saxon.Hej.expr.Expression>(1);
            Saxon.Hej.expr.Expression a = this.copy(new Saxon.Hej.expr.parser.RebindingMap());
            a.setRetainedStaticContext(getRetainedStaticContext());
            pre.Add(a);
            return pre;
        }
    
    }
}