using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.instruct {
    public sealed class ValueOf : Saxon.Hej.expr.instruct.SimpleNodeConstructor {
        private int options;
        private bool numberingInstruction = false;
        private readonly bool noNodeIfEmpty;
        public ValueOf(Saxon.Hej.expr.Expression select, bool disable, bool noNodeIfEmpty)  {
            setSelect(select);
            options = (disable ? Saxon.Hej.@event.ReceiverOption.DISABLE_ESCAPING : Saxon.Hej.@event.ReceiverOption.NONE);
            this.noNodeIfEmpty = noNodeIfEmpty;
            adoptChildExpression(select);
            if ((select is Saxon.Hej.expr.StringLiteral)) {
                bool special = false;
                Saxon.Hej.str.UnicodeString val = ((Saxon.Hej.expr.StringLiteral)(select)).getString();
                Saxon.Hej.z.IntIterator iter = val.codePoints();
                while (iter.hasNext()) {
                    int c = iter.next();
                    if ((((((c<33) || (c>126)) || (c == '<')) || (c == '>')) || (c == '&'))) {
                        special = true;
                        break ;
                    }
                }
                if (!(special)) {
                    options |= Saxon.Hej.@event.ReceiverOption.NO_SPECIAL_CHARS;
                }
            }
        }
        public void setIsNumberingInstruction() {
            numberingInstruction = true;
        }
        public bool isNumberingInstruction() {
            return numberingInstruction;
        }
        public bool isNoNodeIfEmpty() {
            return noNodeIfEmpty;
        }
        public override string toShortString() {
            if ((getSelect() is Saxon.Hej.expr.StringLiteral)) {
                return (("text{"+Saxon.Hej.trans.Err.depict(((Saxon.Hej.expr.StringLiteral)(getSelect())).getGroundedValue()))+"}");
            } else {
                return base.toShortString();
            }
        }
        public override void gatherProperties(Saxon.Ejava.util.function.BiConsumer<string,System.Object> consumer) {
            if ((getSelect() is Saxon.Hej.expr.StringLiteral)) {
                consumer("text", (((Saxon.Hej.expr.StringLiteral)(getSelect())).getGroundedValue().getUnicodeStringValue()));
            }
        }
        public override int getInstructionNameCode() {
            if (numberingInstruction) {
                return Saxon.Hej.om.StandardNames.XSL_NUMBER;
            } else {
                if ((getSelect() is Saxon.Hej.expr.StringLiteral)) {
                    return Saxon.Hej.om.StandardNames.XSL_TEXT;
                } else {
                    return Saxon.Hej.om.StandardNames.XSL_VALUE_OF;
                }
            }
        }
        public int getOptions() {
            return options;
        }
        public bool isDisableOutputEscaping() {
            return Saxon.Hej.@event.ReceiverOption.contains(options, Saxon.Hej.@event.ReceiverOption.DISABLE_ESCAPING);
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            return Saxon.Hej.pattern.NodeKindTest.TEXT;
        }
        protected internal override int computeCardinality() {
            if (noNodeIfEmpty) {
                return Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_ONE;
            } else {
                return Saxon.Hej.expr.StaticProperty.EXACTLY_ONE;
            }
        }
        public override void localTypeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType) {
        }
        public override int getIntrinsicDependencies() {
            int d = base.getIntrinsicDependencies();
            if (isDisableOutputEscaping()) {
                d |= Saxon.Hej.expr.StaticProperty.DEPENDS_ON_ASSIGNABLE_GLOBALS;
            }
            return d;
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.instruct.ValueOf exp = new Saxon.Hej.expr.instruct.ValueOf(getSelect().copy(rebindings), Saxon.Hej.@event.ReceiverOption.contains(options, Saxon.Hej.@event.ReceiverOption.DISABLE_ESCAPING), noNodeIfEmpty);
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, exp);
            if (numberingInstruction) {
                exp.setIsNumberingInstruction();
            }
            return exp;
        }
        public override void checkPermittedContents(Saxon.Hej.type.SchemaType parentType, bool whole) {
            if ((getSelect() is Saxon.Hej.expr.Literal)) {
                Saxon.Hej.om.GroundedValue selectValue = ((Saxon.Hej.expr.Literal)(getSelect())).getGroundedValue();
                Saxon.Hej.type.SimpleType stype = null;
                if (((parentType is Saxon.Hej.type.SimpleType) && whole)) {
                    stype = (Saxon.Hej.type.SimpleType)(parentType);
                } else {
                    if (((parentType is Saxon.Hej.type.ComplexType) && ((Saxon.Hej.type.ComplexType)(parentType)).isSimpleContent())) {
                        stype = ((Saxon.Hej.type.ComplexType)(parentType)).getSimpleContentType();
                    }
                }
                if (((whole && (stype != null)) && !(stype.isNamespaceSensitive()))) {
                    Saxon.Hej.type.ValidationFailure err = stype.validateContent(selectValue.getUnicodeStringValue(), null, getConfiguration().getConversionRules());
                    if ((err != null)) {
                        err.setLocator(getLocation());
                        err.setErrorCode((isXSLT() ? "XTTE1540" : "XQDY0027"));
                        throw err.makeException();
                    }
                    return ;
                }
                if (((((parentType is Saxon.Hej.type.ComplexType) && !(((Saxon.Hej.type.ComplexType)(parentType)).isSimpleContent())) && !(((Saxon.Hej.type.ComplexType)(parentType)).isMixedContent())) && !(Saxon.Hej.value.Whitespace.isAllWhite(selectValue.getUnicodeStringValue())))) {
                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(((("The containing element must be of type "+parentType.getDescription())+", which does not allow text content ")+Saxon.Hej.trans.Err.wrap(selectValue.getUnicodeStringValue())));
                    err.setLocation(getLocation());
                    err.setIsTypeError(true);
                    throw err;
                }
            } else {
            }
        }
        public Saxon.Hej.expr.Expression convertToCastAsString() {
            if ((noNodeIfEmpty || !(Saxon.Hej.value.Cardinality.allowsZero(getSelect().getCardinality())))) {
                return new Saxon.Hej.expr.CastExpression(getSelect(), Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC, true);
            } else {
                Saxon.Hej.expr.Expression sf = Saxon.Hej.functions.SystemFunction.makeCall("string", getRetainedStaticContext(), getSelect());
                return new Saxon.Hej.expr.CastExpression(sf, Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC, false);
            }
        }
        public override Saxon.Hej.expr.instruct.TailCall processLeavingTail(Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context) {
            if (noNodeIfEmpty) {
                Saxon.Hej.om.Item value = getSelect().evaluateItem(context);
                if ((value != null)) {
                    processValue(value.getUnicodeStringValue(), output, context);
                }
                return null;
            } else {
                if ((((getSelect().getItemType() == Saxon.Hej.type.BuiltInAtomicType.STRING) && !(isDisableOutputEscaping())) && !(Saxon.Hej.value.Cardinality.allowsZero(getCardinality())))) {
                    Saxon.Hej.s9api.Location instructionLoc = getLocation();
                    Saxon.Hej.@event.Outputter toText = new Inner_ProxyOutputter_14(this, output, instructionLoc);
                    getSelect().process(toText, context);
                    return null;
                } else {
                    return base.processLeavingTail(output, context);
                }
            }
        }
        public override void processValue(Saxon.Hej.str.UnicodeString value, Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context) {
            output.characters(value, getLocation(), options);
        }
        public override Saxon.Hej.om.NodeInfo evaluateItem(Saxon.Hej.expr.XPathContext context) {
            try {
                Saxon.Hej.str.UnicodeString val;
                Saxon.Hej.om.Item item = getSelect().evaluateItem(context);
                if ((item == null)) {
                    if (noNodeIfEmpty) {
                        return null;
                    } else {
                        val = Saxon.Hej.str.EmptyUnicodeString.getInstance();
                    }
                } else {
                    val = item.getUnicodeStringValue();
                }
                Saxon.Hej.Controller controller = context.getController();
                System.Diagnostics.Debug.Assert((controller != null));
                Saxon.Hej.tree.util.Orphan o = new Saxon.Hej.tree.util.Orphan(controller.getConfiguration());
                o.setNodeKind(Saxon.Hej.type.Type.TEXT);
                o.setStringValue(val);
                if (isDisableOutputEscaping()) {
                    o.setDisableOutputEscaping(true);
                }
                return o;
            } catch (Saxon.Hej.trans.XPathException err) {
                err.maybeSetLocation(getLocation());
                throw;
            }
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("valueOf", this);
            string flags = "";
            if (isDisableOutputEscaping()) {
                flags += "d";
            }
            if (Saxon.Hej.@event.ReceiverOption.contains(options, Saxon.Hej.@event.ReceiverOption.NO_SPECIAL_CHARS)) {
                flags += "S";
            }
            if (noNodeIfEmpty) {
                flags += "e";
            }
            if (isLocal()) {
                flags += "l";
            }
            if (!(flags.Length==0)) {
                @out.emitAttribute("flags", flags);
            }
            getSelect().export(@out);
            @out.endElement();
        }
    
        private class Inner_ProxyOutputter_14 : Saxon.Hej.@event.ProxyOutputter {
            private readonly ValueOf outerInstance;
            public Inner_ProxyOutputter_14(ValueOf outerInstance, /*AA*/Saxon.Hej.@event.Outputter arg1, Saxon.Hej.s9api.Location instructionLoc) : base(arg1) {
                this.outerInstance = outerInstance;
                this.instructionLoc = instructionLoc;
            }
            private Saxon.Hej.s9api.Location instructionLoc;
            public override void append(Saxon.Hej.om.Item item) {
                /*superMethod*/getNextOutputter().characters(/*localparam*/item.getUnicodeStringValue(), /*extraParam*/instructionLoc, outerInstance.options);
            }
            public override void append(Saxon.Hej.om.Item item, Saxon.Hej.s9api.Location loc, int properties) {
                Saxon.Hej.s9api.Location location = ((/*localparam*/loc.getLineNumber() == -1) ? /*extraParam*/instructionLoc : /*localparam*/loc);
                /*superMethod*/getNextOutputter().characters(/*localparam*/item.getUnicodeStringValue(), /*localvar*/location, (/*localparam*/properties|outerInstance.options));
            }
            public override  /* REPLACING public */ Saxon.Hej.str.UniStringConsumer getStringReceiver(bool asTextNode, Saxon.Hej.s9api.Location loc) {
                Saxon.Hej.s9api.Location location = ((/*localparam*/loc.getLineNumber() == -1) ? /*extraParam*/instructionLoc : /*localparam*/loc);
                return /*superMethod*/getNextOutputter().getStringReceiver(true, /*localvar*/location);
            }
        }

    }
}