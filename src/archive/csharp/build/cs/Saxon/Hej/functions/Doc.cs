using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class Doc : Saxon.Hej.functions.SystemFunction, Saxon.Hej.expr.Callable {
        private Saxon.Hej.lib.ParseOptions parseOptions;
        public Saxon.Hej.lib.ParseOptions getParseOptions() {
            return parseOptions;
        }
        public void setParseOptions(Saxon.Hej.lib.ParseOptions parseOptions) {
            this.parseOptions = parseOptions;
        }
        public override int getCardinality(Saxon.Hej.expr.Expression[] arguments) {
            return (arguments[0].getCardinality()&~(Saxon.Hej.expr.StaticProperty.ALLOWS_MANY));
        }
        public override Saxon.Hej.expr.Expression makeFunctionCall(params Saxon.Hej.expr.Expression[]  arguments) {
            Saxon.Hej.expr.Expression expr = maybePreEvaluate(this, arguments);
            return ((expr == null) ? base.makeFunctionCall(arguments) : expr);
        }
        public static Saxon.Hej.expr.Expression maybePreEvaluate(Saxon.Hej.functions.SystemFunction sf, Saxon.Hej.expr.Expression[] arguments) {
            if (((arguments.Length>1) || !(sf.getRetainedStaticContext().getConfiguration().getBooleanProperty(Saxon.Hej.lib.Feature<bool>.PRE_EVALUATE_DOC_FUNCTION)))) {
                sf.getDetails().properties = (sf.getDetails().properties|Saxon.Hej.functions.registry.BuiltInFunctionSet.LATE);
                return null;
            } else {
                return new Inner_SystemFunctionCall_d1030e260(sf, arguments, sf);
            }
        }
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.value.AtomicValue hrefVal = (Saxon.Hej.value.AtomicValue)(arguments[0].head());
            if ((hrefVal == null)) {
                return Saxon.Hej.value.EmptySequence.getInstance();
            }
            string href = hrefVal.getStringValue();
            Saxon.Hej.expr.PackageData packageData = getRetainedStaticContext().getPackageData();
            Saxon.Hej.om.NodeInfo item = Saxon.Hej.functions.DocumentFn.makeDoc(href, getRetainedStaticContext().getStaticBaseUriString(), packageData, getParseOptions(), context, null, false);
            if ((item == null)) {
                throw new Saxon.Hej.trans.XPathException(("Failed to load document "+href), "FODC0002", context);
            }
            Saxon.Hej.Controller controller = context.getController();
            if (((parseOptions != null) && (controller is Saxon.Hej.trans.XsltController))) {
                ((Saxon.Hej.trans.XsltController)(controller)).getAccumulatorManager().setApplicableAccumulators(item.getTreeInfo(), parseOptions.getApplicableAccumulators());
            }
            return item;
        }
        public override int getSpecialProperties(Saxon.Hej.expr.Expression[] arguments) {
            return (((Saxon.Hej.expr.StaticProperty.ORDERED_NODESET|Saxon.Hej.expr.StaticProperty.PEER_NODESET)|Saxon.Hej.expr.StaticProperty.NO_NODES_NEWLY_CREATED)|Saxon.Hej.expr.StaticProperty.SINGLE_DOCUMENT_NODESET);
        }
    
        private class Inner_SystemFunctionCall_d1030e260 : Saxon.Hej.expr.SystemFunctionCall {
            public Inner_SystemFunctionCall_d1030e260(/*AA*/Saxon.Hej.functions.SystemFunction arg1, /*AA*/Saxon.Hej.expr.Expression[] arg2, Saxon.Hej.functions.SystemFunction sf) : base(arg1, arg2) {
                this.sf = sf;
            }
            private Saxon.Hej.functions.SystemFunction sf;
            public override Saxon.Hej.expr.Expression preEvaluate(Saxon.Hej.expr.parser.ExpressionVisitor visitor) {
                Saxon.Hej.Configuration config = /*localparam*/visitor.getConfiguration();
                try {
                    Saxon.Hej.om.GroundedValue firstArg = ((Saxon.Hej.expr.Literal)(this.getArg(0))).getGroundedValue();
                    if ((/*localvar*/firstArg.getLength() == 0)) {
                        return null;
                    } else {
                        if ((/*localvar*/firstArg.getLength()>1)) {
                            return this;
                        }
                    }
                    string href = /*localvar*/firstArg.head().getStringValue();
                    if ((/*localvar*/href.IndexOf('#')>=0)) {
                        return this;
                    }
                    Saxon.Hej.om.NodeInfo item = Saxon.Hej.functions.DocumentFn.preLoadDoc(/*localvar*/href, /*extraParam*/sf.getStaticBaseUriString(), /*localvar*/config, /*superMethod*/getLocation());
                    if ((/*localvar*/item != null)) {
                        Saxon.Hej.expr.Expression constant = Saxon.Hej.expr.Literal.makeLiteral(/*localvar*/item);
                        Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this.getArg(0), /*localvar*/constant);
                        return /*localvar*/constant;
                    }
                } catch (System.Exception err) {
                    return this;
                }
                return this;
            }
            public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType) {
                /*superMethod*/optimizeChildren(/*localparam*/visitor, /*localparam*/contextItemType);
                if ((/*superMethod*/getArg(0) is Saxon.Hej.expr.StringLiteral)) {
                    return /*method*/preEvaluate(/*localparam*/visitor);
                }
                return this;
            }
        }

    }
}