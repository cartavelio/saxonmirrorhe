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
                Saxon.Hej.str.UnicodeString val = ((Saxon.Hej.expr.StringLiteral)(select)).getString();
                bool special = (val.indexWhere(c => (((((c<33) || (c>126)) || (c == '<')) || (c == '>')) || (c == '&')), 0)>=0);
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
        public override void processValue(Saxon.Hej.str.UnicodeString value, Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context) {
            output.characters(value, getLocation(), options);
        }
        public override Saxon.Hej.om.NodeInfo evaluateItem(Saxon.Hej.expr.XPathContext context) {
            try {
                Saxon.Hej.om.Item content = /*Delegate.eval()*/getSelect().makeElaborator().elaborateForItem()(context);
                return makeTextNode(content, noNodeIfEmpty, isDisableOutputEscaping(), context);
            } catch (Saxon.Hej.trans.XPathException err) {
                err.maybeSetLocation(getLocation());
                throw;
            }
        }
        private static Saxon.Hej.om.NodeInfo makeTextNode(Saxon.Hej.om.Item content, bool noNodeIfEmpty, bool doe, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.str.UnicodeString val;
            if ((content == null)) {
                if (noNodeIfEmpty) {
                    return null;
                } else {
                    val = Saxon.Hej.str.EmptyUnicodeString.getInstance();
                }
            } else {
                val = content.getUnicodeStringValue();
            }
            Saxon.Hej.tree.util.Orphan o = new Saxon.Hej.tree.util.Orphan(context.getConfiguration());
            o.setNodeKind(Saxon.Hej.type.Type.TEXT);
            o.setStringValue(val);
            if (doe) {
                o.setDisableOutputEscaping(true);
            }
            return o;
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
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Hej.expr.instruct.ValueOf.ValueOfElaborator();
        }
        private class ValueOfElaborator : Saxon.Hej.expr.elab.SimpleNodePushElaborator {
            public override Saxon.Hej.expr.elab.PushEvaluator elaborateForPush() {
                Saxon.Hej.expr.instruct.ValueOf expr = (Saxon.Hej.expr.instruct.ValueOf)(getExpression());
                Saxon.Hej.s9api.Location loc = expr.getLocation();
                bool noNodeIfEmpty = expr.noNodeIfEmpty;
                int options = expr.getOptions();
                if (noNodeIfEmpty) {
                    Saxon.Hej.expr.elab.ItemEvaluator contentEval = expr.getSelect().makeElaborator().elaborateForItem();
                    return (output, context) => {
                        Saxon.Hej.om.Item value = /*Delegate.eval()*/contentEval(context);
                        if ((value != null)) {
                            output.characters(value.getUnicodeStringValue(), loc, options);
                        }
                        return null;
                    } ;
                } else {
                    if ((((expr.getSelect().getItemType() == Saxon.Hej.type.BuiltInAtomicType.STRING) && !(expr.isDisableOutputEscaping())) && !(Saxon.Hej.value.Cardinality.allowsZero(expr.getCardinality())))) {
                        Saxon.Hej.expr.elab.PushEvaluator contentEval = expr.getSelect().makeElaborator().elaborateForPush();
                        return (output, context) => {
                            Saxon.Hej.@event.Outputter streamer = new Saxon.Hej.expr.instruct.ValueOf.TextNodeOutputStreamer(output, loc, options);
                            return /*Delegate.processLeavingTail()*/contentEval(streamer, context);
                        } ;
                    } else {
                        Saxon.Hej.expr.elab.UnicodeStringEvaluator contentEval = expr.getSelect().makeElaborator().elaborateForUnicodeString(true);
                        return (output, context) => {
                            Saxon.Hej.str.UnicodeString value = /*Delegate.eval()*/contentEval(context);
                            output.characters(value, loc, options);
                            return null;
                        } ;
                    }
                }
            }
            public override Saxon.Hej.expr.elab.ItemEvaluator elaborateForItem() {
                Saxon.Hej.expr.instruct.ValueOf expr = (Saxon.Hej.expr.instruct.ValueOf)(getExpression());
                Saxon.Hej.expr.elab.ItemEvaluator contentEval = expr.getSelect().makeElaborator().elaborateForItem();
                bool noNodeIfEmpty = expr.noNodeIfEmpty;
                bool doe = expr.isDisableOutputEscaping();
                return context => Saxon.Hej.expr.instruct.ValueOf.makeTextNode(/*Delegate.eval()*/contentEval(context), noNodeIfEmpty, doe, context);
            }
        
        }
        private class TextNodeOutputStreamer : Saxon.Hej.@event.ProxyOutputter {
            private readonly Saxon.Hej.s9api.Location instructionLoc;
            private readonly int options;
            public TextNodeOutputStreamer(Saxon.Hej.@event.Outputter output, Saxon.Hej.s9api.Location instructionLoc, int options) : base (output) {
                this.instructionLoc = instructionLoc;
                this.options = options;
            }
            public override void append(Saxon.Hej.om.Item item) {
                getNextOutputter().characters(item.getUnicodeStringValue(), instructionLoc, options);
            }
            public override void append(Saxon.Hej.om.Item item, Saxon.Hej.s9api.Location loc, int properties) {
                Saxon.Hej.s9api.Location location = ((loc.getLineNumber() == -1) ? instructionLoc : loc);
                getNextOutputter().characters(item.getUnicodeStringValue(), location, (properties|options));
            }
            public override  /* REPLACING public override */ Saxon.Hej.str.UniStringConsumer getStringReceiver(bool asTextNode, Saxon.Hej.s9api.Location loc) {
                Saxon.Hej.s9api.Location location = ((loc.getLineNumber() == -1) ? instructionLoc : loc);
                return getNextOutputter().getStringReceiver(true, location);
            }
        
        }
    
    }
}