using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    #pragma warning disable 659
    public class Literal : Saxon.Hej.expr.Expression {
        private readonly Saxon.Hej.om.GroundedValue value;
        protected internal Literal(Saxon.Hej.om.GroundedValue value)  {
            this.value = value.reduce();
        }
        public virtual Saxon.Hej.om.GroundedValue getGroundedValue() {
            return value;
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            return this;
        }
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType) {
            return this;
        }
        public override int getNetCost() {
            return 0;
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            if ((value is Saxon.Hej.value.AtomicValue)) {
                return ((Saxon.Hej.value.AtomicValue)(value)).getItemType();
            } else {
                if ((value.getLength() == 0)) {
                    return Saxon.Hej.type.ErrorType.getInstance();
                } else {
                    Saxon.Hej.type.TypeHierarchy th = getConfiguration().getTypeHierarchy();
                    return Saxon.Hej.om.SequenceTool.getItemType(value, th);
                }
            }
        }
        public override Saxon.Hej.type.UType getStaticUType(Saxon.Hej.type.UType contextItemType) {
            if ((value.getLength() == 0)) {
                return Saxon.Hej.type.UType.VOID;
            } else {
                if ((value is Saxon.Hej.value.AtomicValue)) {
                    return ((Saxon.Hej.value.AtomicValue)(value)).getUType();
                } else {
                    if ((value is Saxon.Hej.om.FunctionItem)) {
                        return Saxon.Hej.type.UType.FUNCTION;
                    } else {
                        return base.getStaticUType(contextItemType);
                    }
                }
            }
        }
        protected internal override int computeCardinality() {
            if ((value.getLength() == 0)) {
                return Saxon.Hej.expr.StaticProperty.EMPTY;
            } else {
                if ((value is Saxon.Hej.value.AtomicValue)) {
                    return Saxon.Hej.expr.StaticProperty.EXACTLY_ONE;
                }
            }
            Saxon.Hej.om.SequenceIterator iter = value.iterate();
            Saxon.Hej.om.Item next = iter.next();
            if ((next == null)) {
                return Saxon.Hej.expr.StaticProperty.EMPTY;
            } else {
                if ((iter.next() != null)) {
                    return Saxon.Hej.expr.StaticProperty.ALLOWS_MANY;
                } else {
                    return Saxon.Hej.expr.StaticProperty.EXACTLY_ONE;
                }
            }
        }
        protected internal override int computeSpecialProperties() {
            if ((value.getLength() == 0)) {
                return (Saxon.Hej.expr.StaticProperty.SPECIAL_PROPERTY_MASK&~(Saxon.Hej.expr.StaticProperty.HAS_SIDE_EFFECTS));
            }
            return Saxon.Hej.expr.StaticProperty.NO_NODES_NEWLY_CREATED;
        }
        public override bool supportsLazyEvaluation() {
            return false;
        }
        public override Saxon.Hej.value.IntegerValue[] getIntegerBounds() {
            if ((value is Saxon.Hej.value.IntegerValue)) {
                return new Saxon.Hej.value.IntegerValue[]{(Saxon.Hej.value.IntegerValue)(value), (Saxon.Hej.value.IntegerValue)(value)};
            } else {
                if ((value is Saxon.Hej.value.IntegerRange)) {
                    return new Saxon.Hej.value.IntegerValue[]{Saxon.Hej.value.Int64Value.makeIntegerValue(((Saxon.Hej.value.IntegerRange)(value)).getStart()), Saxon.Hej.value.Int64Value.makeIntegerValue(((Saxon.Hej.value.IntegerRange)(value)).getEnd())};
                } else {
                    return null;
                }
            }
        }
        public override bool isVacuousExpression() {
            return (value.getLength() == 0);
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.Literal l2 = new Saxon.Hej.expr.Literal(value);
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, l2);
            return l2;
        }
        public override Saxon.Hej.pattern.Pattern toPattern(Saxon.Hej.Configuration config) {
            if (isEmptySequence(this)) {
                return new Saxon.Hej.pattern.NodeTestPattern(Saxon.Hej.type.ErrorType.getInstance());
            } else {
                return base.toPattern(config);
            }
        }
        public override Saxon.Hej.expr.parser.PathMap.PathMapNodeSet addToPathMap(Saxon.Hej.expr.parser.PathMap pathMap, Saxon.Hej.expr.parser.PathMap.PathMapNodeSet pathMapNodeSet) {
            return pathMapNodeSet;
        }
        public override int getDependencies() {
            return 0;
        }
        public override Saxon.Hej.om.SequenceIterator iterate(Saxon.Hej.expr.XPathContext context) {
            return value.iterate();
        }
        public Saxon.Hej.om.SequenceIterator iterate() {
            return value.iterate();
        }
        public override Saxon.Hej.om.Item evaluateItem(Saxon.Hej.expr.XPathContext context) {
            return value.head();
        }
        public override void process(Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context) {
            if ((value is Saxon.Hej.om.Item)) {
                output.append((Saxon.Hej.om.Item)(value), getLocation(), Saxon.Hej.@event.ReceiverOption.ALL_NAMESPACES);
            } else {
                Saxon.Hej.om.SequenceTool.supply(value.iterate(), (Saxon.Hej.om.ItemConsumer<Saxon.Hej.om.Item>)((it => output.append(it, getLocation(), Saxon.Hej.@event.ReceiverOption.ALL_NAMESPACES))));
            }
        }
        public override int getImplementationMethod() {
            return ((ITERATE_METHOD|PROCESS_METHOD)|EVALUATE_METHOD);
        }
        public override Saxon.Hej.str.UnicodeString evaluateAsString(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.value.AtomicValue value = (Saxon.Hej.value.AtomicValue)(evaluateItem(context));
            if ((value == null)) {
                return Saxon.Hej.str.EmptyUnicodeString.getInstance();
            }
            return value.getUnicodeStringValue();
        }
        public override bool effectiveBooleanValue(Saxon.Hej.expr.XPathContext context) {
            return value.effectiveBooleanValue();
        }
        public override bool Equals(System.Object obj) {
            if (!(((obj is Saxon.Hej.expr.Literal)))) {
                return false;
            }
            Saxon.Hej.om.GroundedValue v0 = value;
            Saxon.Hej.om.GroundedValue v1 = ((Saxon.Hej.expr.Literal)(obj)).value;
            Saxon.Hej.om.SequenceIterator i0 = v0.iterate();
            Saxon.Hej.om.SequenceIterator i1 = v1.iterate();
            while (true) {
                Saxon.Hej.om.Item m0 = i0.next();
                Saxon.Hej.om.Item m1 = i1.next();
                if (((m0 == null) && (m1 == null))) {
                    return true;
                }
                if (((m0 == null) || (m1 == null))) {
                    return false;
                }
                if ((m0 == m1)) {
                    continue ;
                }
                bool n0 = (m0 is Saxon.Hej.om.NodeInfo);
                bool n1 = (m1 is Saxon.Hej.om.NodeInfo);
                if ((n0 != n1)) {
                    return false;
                }
                if (n0) {
                    if (m0.Equals(m1)) {
                        continue ;
                    } else {
                        return false;
                    }
                }
                bool a0 = (m0 is Saxon.Hej.value.AtomicValue);
                bool a1 = (m1 is Saxon.Hej.value.AtomicValue);
                if ((a0 != a1)) {
                    return false;
                }
                if (a0) {
                    if ((((Saxon.Hej.value.AtomicValue)(m0)).isIdentical((Saxon.Hej.value.AtomicValue)(m1)) && (((Saxon.Hej.value.AtomicValue)(m0)).getItemType() == ((Saxon.Hej.value.AtomicValue)(m1)).getItemType()))) {
                        continue ;
                    } else {
                        return false;
                    }
                }
                return false;
            }
        }
        protected internal override int computeHashCode() {
            if ((value is Saxon.Hej.om.AtomicSequence)) {
                return Saxon.Hej.expr.sort.SimpleTypeComparison.getInstance().hash((Saxon.Hej.om.AtomicSequence)(value));
            } else {
                return base.computeHashCode();
            }
        }
        public override string ToString() {
            return value.ToString();
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            exportValue(value, @out);
        }
        public static void exportValue(Saxon.Hej.om.Sequence value, Saxon.Hej.trace.ExpressionPresenter @out) {
            if ((value.head() == null)) {
                @out.startElement("empty");
                @out.endElement();
            } else {
                if ((value is Saxon.Hej.value.AtomicValue)) {
                    exportAtomicValue((Saxon.Hej.value.AtomicValue)(value), @out);
                } else {
                    if ((value is Saxon.Hej.value.IntegerRange)) {
                        @out.startElement("range");
                        @out.emitAttribute("from", (""+((Saxon.Hej.value.IntegerRange)(value)).getStart()));
                        @out.emitAttribute("to", (""+((Saxon.Hej.value.IntegerRange)(value)).getEnd()));
                        @out.endElement();
                    } else {
                        if ((value is Saxon.Hej.om.NodeInfo)) {
                            @out.startElement("node");
                            int nodeKind = ((Saxon.Hej.om.NodeInfo)(value)).getNodeKind();
                            @out.emitAttribute("kind", (nodeKind+""));
                            if (@out.getOptions().explaining) {
                                string name = ((Saxon.Hej.om.NodeInfo)(value)).getDisplayName();
                                if (!(name.Length==0)) {
                                    @out.emitAttribute("name", name);
                                }
                            } else {
                                switch (nodeKind) {
                                  case Saxon.Hej.type.Type.DOCUMENT:
                                  case Saxon.Hej.type.Type.ELEMENT:
                                    System.IO.StringWriter sw = new System.IO.StringWriter();
                                    Saxon.Ejava.util.Properties props = new Saxon.Ejava.util.Properties();
                                    props.setProperty("method", "xml");
                                    props.setProperty("indent", "no");
                                    props.setProperty("omit-xml-declaration", "yes");
                                    Saxon.Hej.query.QueryResult.serialize(((Saxon.Hej.om.NodeInfo)(value)), new Saxon.Ejavax.xml.transform.stream.StreamResult(sw), props);
                                    @out.emitAttribute("content", sw.ToString());
                                    @out.emitAttribute("baseUri", ((Saxon.Hej.om.NodeInfo)(value)).getBaseURI());
                                    break ;
                                  case Saxon.Hej.type.Type.TEXT:
                                  case Saxon.Hej.type.Type.COMMENT:
                                    @out.emitAttribute("content", ((Saxon.Hej.om.NodeInfo)(value)).getStringValue());
                                    break ;
                                  case Saxon.Hej.type.Type.ATTRIBUTE:
                                  case Saxon.Hej.type.Type.NAMESPACE:
                                  case Saxon.Hej.type.Type.PROCESSING_INSTRUCTION:
                                    Saxon.Hej.om.StructuredQName name = Saxon.Hej.om.NameOfNode.makeName(((Saxon.Hej.om.NodeInfo)(value))).getStructuredQName();
                                    if (!(name.getLocalPart().Length==0)) {
                                        @out.emitAttribute("localName", name.getLocalPart());
                                    }
                                    if (!(name.getPrefix().Length==0)) {
                                        @out.emitAttribute("prefix", name.getPrefix());
                                    }
                                    if (!(name.hasURI(Saxon.Hej.om.NamespaceUri.NULL))) {
                                        @out.emitAttribute("ns", name.getNamespaceUri().ToString());
                                    }
                                    @out.emitAttribute("content", ((Saxon.Hej.om.NodeInfo)(value)).getStringValue());
                                    break ;
                                  default:
                                    System.Diagnostics.Debug.Assert(false);
                                    break ;
                                }
                            }
                            @out.endElement();
                        } else {
                            if ((value is Saxon.Hej.ma.map.MapItem)) {
                                @out.startElement("map");
                                @out.emitAttribute("size", (""+((Saxon.Hej.ma.map.MapItem)(value)).size()));
                                foreach (Saxon.Hej.ma.map.KeyValuePair kvp in ((Saxon.Hej.ma.map.MapItem)(value)).keyValuePairs()) {
                                    exportAtomicValue(kvp.key, @out);
                                    exportValue(kvp.value, @out);
                                }
                                @out.endElement();
                            } else {
                                if ((value is Saxon.Hej.om.FunctionItem)) {
                                    ((Saxon.Hej.om.FunctionItem)(value)).export(@out);
                                } else {
                                    if ((value is Saxon.Hej.value.AnyExternalObject)) {
                                        if (@out.getOptions().explaining) {
                                            @out.startElement("externalObject");
                                            @out.emitAttribute("class", ((Saxon.Hej.value.AnyExternalObject)(value)).getWrappedObject().GetType().Name);
                                            @out.endElement();
                                        } else {
                                            throw new Saxon.Hej.trans.XPathException("Cannot export a stylesheet containing literal values bound to external Java objects", Saxon.Hej.trans.SaxonErrorCode.SXST0070);
                                        }
                                    } else {
                                        @out.startElement("literal");
                                        if ((value is Saxon.Hej.om.GroundedValue)) {
                                            @out.emitAttribute("count", (((Saxon.Hej.om.GroundedValue)(value)).getLength()+""));
                                        }
                                        Saxon.Hej.om.SequenceTool.supply(value.iterate(), (Saxon.Hej.om.ItemConsumer<Saxon.Hej.om.Item>)((it => exportValue(it, @out))));
                                        @out.endElement();
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        public override string getExpressionName() {
            return "literal";
        }
        public static void exportAtomicValue(Saxon.Hej.value.AtomicValue value, Saxon.Hej.trace.ExpressionPresenter @out) {
            if ("JS".Equals(@out.getOptions().target)) {
                value.checkValidInJavascript();
            }
            Saxon.Hej.type.AtomicType type = value.getItemType();
            string val = value.getStringValue();
            if (type.Equals(Saxon.Hej.type.BuiltInAtomicType.STRING)) {
                @out.startElement("str");
                @out.emitAttribute("val", val);
                @out.endElement();
            } else {
                if (type.Equals(Saxon.Hej.type.BuiltInAtomicType.INTEGER)) {
                    @out.startElement("int");
                    @out.emitAttribute("val", val);
                    @out.endElement();
                } else {
                    if (type.Equals(Saxon.Hej.type.BuiltInAtomicType.DECIMAL)) {
                        @out.startElement("dec");
                        @out.emitAttribute("val", val);
                        @out.endElement();
                    } else {
                        if (type.Equals(Saxon.Hej.type.BuiltInAtomicType.DOUBLE)) {
                            @out.startElement("dbl");
                            @out.emitAttribute("val", val);
                            @out.endElement();
                        } else {
                            if (type.Equals(Saxon.Hej.type.BuiltInAtomicType.BOOLEAN)) {
                                @out.startElement((((Saxon.Hej.value.BooleanValue)(value)).effectiveBooleanValue() ? "true" : "false"));
                                @out.endElement();
                            } else {
                                if ((value is Saxon.Hej.value.QualifiedNameValue)) {
                                    @out.startElement("qName");
                                    @out.emitAttribute("pre", ((Saxon.Hej.value.QualifiedNameValue)(value)).getPrefix());
                                    @out.emitAttribute("uri", ((Saxon.Hej.value.QualifiedNameValue)(value)).getNamespaceURI().ToString());
                                    @out.emitAttribute("loc", ((Saxon.Hej.value.QualifiedNameValue)(value)).getLocalName());
                                    if (!(type.Equals(Saxon.Hej.type.BuiltInAtomicType.QNAME))) {
                                        @out.emitAttribute("type", type.getEQName());
                                    }
                                    @out.endElement();
                                } else {
                                    @out.startElement("atomic");
                                    @out.emitAttribute("val", val);
                                    @out.emitAttribute("type", Saxon.Hej.type.AlphaCode.fromItemType(type));
                                    @out.endElement();
                                }
                            }
                        }
                    }
                }
            }
        }
        public override string toShortString() {
            if ((value.getLength() == 0)) {
                return "()";
            } else {
                if ((value.getLength() == 1)) {
                    return value.toShortString();
                } else {
                    return (((("("+value.head().toShortString())+", ...{")+value.getLength())+"})");
                }
            }
        }
        public static bool isAtomic(Saxon.Hej.expr.Expression exp) {
            return ((exp is Saxon.Hej.expr.Literal) && (((Saxon.Hej.expr.Literal)(exp)).getGroundedValue() is Saxon.Hej.value.AtomicValue));
        }
        public static bool isEmptySequence(Saxon.Hej.expr.Expression exp) {
            return ((exp is Saxon.Hej.expr.Literal) && (((Saxon.Hej.expr.Literal)(exp)).getGroundedValue().getLength() == 0));
        }
        public static bool isConstantBoolean(Saxon.Hej.expr.Expression exp, bool value) {
            if ((exp is Saxon.Hej.expr.Literal)) {
                Saxon.Hej.om.GroundedValue b = ((Saxon.Hej.expr.Literal)(exp)).getGroundedValue();
                return ((b is Saxon.Hej.value.BooleanValue) && (((Saxon.Hej.value.BooleanValue)(b)).getBooleanValue() == value));
            }
            return false;
        }
        public static bool hasEffectiveBooleanValue(Saxon.Hej.expr.Expression exp, bool value) {
            if ((exp is Saxon.Hej.expr.Literal)) {
                try {
                    return (value == ((Saxon.Hej.expr.Literal)(exp)).getGroundedValue().effectiveBooleanValue());
                } catch (Saxon.Hej.trans.XPathException err) {
                    return false;
                }
            }
            return false;
        }
        public static bool isConstantOne(Saxon.Hej.expr.Expression exp) {
            if ((exp is Saxon.Hej.expr.Literal)) {
                Saxon.Hej.om.GroundedValue v = ((Saxon.Hej.expr.Literal)(exp)).getGroundedValue();
                return ((v is Saxon.Hej.value.Int64Value) && (((Saxon.Hej.value.Int64Value)(v)).longValue() == 1));
            }
            return false;
        }
        public static bool isConstantZero(Saxon.Hej.expr.Expression exp) {
            if ((exp is Saxon.Hej.expr.Literal)) {
                Saxon.Hej.om.GroundedValue v = ((Saxon.Hej.expr.Literal)(exp)).getGroundedValue();
                return ((v is Saxon.Hej.value.Int64Value) && (((Saxon.Hej.value.Int64Value)(v)).longValue() == 0));
            }
            return false;
        }
        public override bool isSubtreeExpression() {
            return true;
        }
        public static Saxon.Hej.expr.Literal makeEmptySequence() {
            return new Saxon.Hej.expr.Literal(Saxon.Hej.value.EmptySequence.getInstance());
        }
        public static Saxon.Hej.expr.Literal makeLiteral(Saxon.Hej.om.GroundedValue value) {
            value = value.reduce();
            if ((value is Saxon.Hej.value.StringValue)) {
                return new Saxon.Hej.expr.StringLiteral((Saxon.Hej.value.StringValue)(value));
            } else {
                if (((value is Saxon.Hej.om.FunctionItem) && !((((value is Saxon.Hej.ma.map.MapItem) || (value is Saxon.Hej.ma.arrays.ArrayItem)))))) {
                    return new Saxon.Hej.functions.hof.FunctionLiteral((Saxon.Hej.om.FunctionItem)(value));
                } else {
                    return new Saxon.Hej.expr.Literal(value);
                }
            }
        }
        public static Saxon.Hej.expr.Literal makeLiteral(Saxon.Hej.om.GroundedValue value, Saxon.Hej.expr.Expression origin) {
            Saxon.Hej.expr.Literal lit = makeLiteral(value);
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(origin, lit);
            return lit;
        }
        public override string getStreamerName() {
            return "Literal";
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Hej.expr.Literal.LiteralElaborator();
        }
        public class LiteralElaborator : Saxon.Hej.expr.elab.PullElaborator {
            public override Saxon.Hej.expr.elab.SequenceEvaluator eagerly() {
                Saxon.Hej.om.GroundedValue value = ((Saxon.Hej.expr.Literal)(getExpression())).getGroundedValue();
                return new Saxon.Hej.expr.elab.LiteralEvaluator(value);
            }
            public override Saxon.Hej.expr.elab.SequenceEvaluator lazily(bool repeatable) {
                return eagerly();
            }
            public override Saxon.Hej.expr.elab.PullEvaluator elaborateForPull() {
                Saxon.Hej.om.GroundedValue value = ((Saxon.Hej.expr.Literal)(getExpression())).getGroundedValue();
                return context => value.iterate();
            }
            public override Saxon.Hej.expr.elab.PushEvaluator elaborateForPush() {
                Saxon.Hej.expr.Literal expr = (Saxon.Hej.expr.Literal)(getExpression());
                Saxon.Hej.om.GroundedValue value = ((Saxon.Hej.expr.Literal)(getExpression())).getGroundedValue();
                if ((value is Saxon.Hej.om.Item)) {
                    return (@out, context) => {
                        @out.append((Saxon.Hej.om.Item)(value), expr.getLocation(), Saxon.Hej.@event.ReceiverOption.ALL_NAMESPACES);
                        return null;
                    } ;
                } else {
                    return (@out, context) => {
                        foreach (Saxon.Hej.om.Item item in value.asIterable()) {
                            @out.append(item, expr.getLocation(), Saxon.Hej.@event.ReceiverOption.ALL_NAMESPACES);
                        }
                        return null;
                    } ;
                }
            }
            public override Saxon.Hej.expr.elab.ItemEvaluator elaborateForItem() {
                Saxon.Hej.om.GroundedValue value = ((Saxon.Hej.expr.Literal)(getExpression())).getGroundedValue();
                System.Diagnostics.Debug.Assert((value.getLength()<2));
                Saxon.Hej.om.Item item = value.head();
                return context => item;
            }
            public override Saxon.Hej.expr.elab.BooleanEvaluator elaborateForBoolean() {
                Saxon.Hej.om.GroundedValue value = ((Saxon.Hej.expr.Literal)(getExpression())).getGroundedValue();
                try {
                    bool ebv = value.effectiveBooleanValue();
                    return context => ebv;
                } catch (Saxon.Hej.trans.XPathException e) {
                    throw new Saxon.Hej.trans.UncheckedXPathException(e);
                }
            }
            public override Saxon.Hej.expr.elab.UnicodeStringEvaluator elaborateForUnicodeString(bool zeroLengthWhenAbsent) {
                Saxon.Hej.om.GroundedValue value = ((Saxon.Hej.expr.Literal)(getExpression())).getGroundedValue();
                try {
                    Saxon.Hej.str.UnicodeString str = value.getUnicodeStringValue();
                    return context => str;
                } catch (Saxon.Hej.trans.XPathException e) {
                    throw new Saxon.Hej.trans.UncheckedXPathException(e);
                }
            }
            public override Saxon.Hej.expr.elab.StringEvaluator elaborateForString(bool zeroLengthWhenAbsent) {
                Saxon.Hej.om.GroundedValue value = ((Saxon.Hej.expr.Literal)(getExpression())).getGroundedValue();
                try {
                    string str = value.getStringValue();
                    return context => str;
                } catch (Saxon.Hej.trans.XPathException e) {
                    throw new Saxon.Hej.trans.UncheckedXPathException(e);
                }
            }
            public override Saxon.Hej.expr.elab.UpdateEvaluator elaborateForUpdate() {
                Saxon.Hej.om.GroundedValue value = ((Saxon.Hej.expr.Literal)(getExpression())).getGroundedValue();
                if ((value.getLength() == 0)) {
                    return (context, pul) => {
                    } ;
                } else {
                    return base.elaborateForUpdate();
                }
            }
        
        }
    
    }
}