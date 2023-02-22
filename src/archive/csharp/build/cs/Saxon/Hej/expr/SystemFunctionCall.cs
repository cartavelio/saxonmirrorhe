using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public class SystemFunctionCall : Saxon.Hej.expr.StaticFunctionCall, Saxon.Hej.expr.Negatable {
        public SystemFunctionCall(Saxon.Hej.functions.SystemFunction target, Saxon.Hej.expr.Expression[] arguments) : base (target, arguments) {
        }
        public override void setRetainedStaticContext(Saxon.Hej.expr.parser.RetainedStaticContext rsc) {
            base.setRetainedStaticContext(rsc);
            getTargetFunction().setRetainedStaticContext(rsc);
        }
        public override Saxon.Hej.expr.Expression preEvaluate(Saxon.Hej.expr.parser.ExpressionVisitor visitor) {
            Saxon.Hej.functions.SystemFunction target = getTargetFunction();
            if ((((target.getDetails().properties&Saxon.Hej.functions.registry.BuiltInFunctionSet.LATE)) == 0)) {
                return base.preEvaluate(visitor);
            } else {
                return this;
            }
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            typeCheckChildren(visitor, contextInfo);
            checkFunctionCall(getTargetFunction(), visitor);
            getTargetFunction().supplyTypeInformation(visitor, contextInfo, getArguments());
            if ((((getTargetFunction().getDetails().properties&Saxon.Hej.functions.registry.BuiltInFunctionSet.LATE)) == 0)) {
                return preEvaluateIfConstant(visitor);
            }
            return this;
        }
        public override Saxon.Hej.functions.SystemFunction getTargetFunction() {
            return (Saxon.Hej.functions.SystemFunction)(base.getTargetFunction());
        }
        public override int getIntrinsicDependencies() {
            int properties = getTargetFunction().getDetails().properties;
            int dep = 0;
            if ((((properties&Saxon.Hej.functions.registry.BuiltInFunctionSet.LATE)) != 0)) {
                dep = Saxon.Hej.expr.StaticProperty.DEPENDS_ON_RUNTIME_ENVIRONMENT;
            }
            if ((((properties&Saxon.Hej.functions.registry.BuiltInFunctionSet.FOCUS)) != 0)) {
                if ((((properties&Saxon.Hej.functions.registry.BuiltInFunctionSet.CDOC)) != 0)) {
                    dep |= Saxon.Hej.expr.StaticProperty.DEPENDS_ON_CONTEXT_DOCUMENT;
                }
                if ((((properties&Saxon.Hej.functions.registry.BuiltInFunctionSet.CITEM)) != 0)) {
                    dep |= Saxon.Hej.expr.StaticProperty.DEPENDS_ON_CONTEXT_ITEM;
                }
                if ((((properties&Saxon.Hej.functions.registry.BuiltInFunctionSet.POSN)) != 0)) {
                    dep |= Saxon.Hej.expr.StaticProperty.DEPENDS_ON_POSITION;
                }
                if ((((properties&Saxon.Hej.functions.registry.BuiltInFunctionSet.LAST)) != 0)) {
                    dep |= Saxon.Hej.expr.StaticProperty.DEPENDS_ON_LAST;
                }
            }
            if ((((properties&Saxon.Hej.functions.registry.BuiltInFunctionSet.BASE)) != 0)) {
                dep |= Saxon.Hej.expr.StaticProperty.DEPENDS_ON_STATIC_CONTEXT;
            }
            if ((((properties&Saxon.Hej.functions.registry.BuiltInFunctionSet.DCOLL)) != 0)) {
                dep |= Saxon.Hej.expr.StaticProperty.DEPENDS_ON_STATIC_CONTEXT;
            }
            if (((isCallOn(typeof(Saxon.Hej.functions.RegexGroup)) || isCallOn(typeof(Saxon.Hej.functions.CurrentMergeGroup))) || isCallOn(typeof(Saxon.Hej.functions.CurrentMergeKey)))) {
                dep |= Saxon.Hej.expr.StaticProperty.DEPENDS_ON_CURRENT_GROUP;
            }
            return dep;
        }
        protected internal override int computeCardinality() {
            return getTargetFunction().getCardinality(getArguments());
        }
        protected internal override int computeSpecialProperties() {
            return getTargetFunction().getSpecialProperties(getArguments());
        }
        public override int getNetCost() {
            return getTargetFunction().getNetCost();
        }
        public override Saxon.Hej.expr.Expression getScopingExpression() {
            if (isCallOn(typeof(Saxon.Hej.functions.RegexGroup))) {
                Saxon.Hej.expr.Expression parent = getParentExpression();
                while ((parent != null)) {
                    if ((parent is Saxon.Hej.expr.instruct.AnalyzeString)) {
                        return parent;
                    }
                    parent = parent.getParentExpression();
                }
                return null;
            } else {
                return base.getScopingExpression();
            }
        }
        public override bool isLiftable(bool forStreaming) {
            return (((base.isLiftable(forStreaming) && !(isCallOn(typeof(Saxon.Hej.functions.CurrentMergeGroup)))) && !(isCallOn(typeof(Saxon.Hej.functions.CurrentMergeKey)))) && ((!(forStreaming) || !(isCallOn(typeof(Saxon.Hej.ma.map.MapFunctionSet.MapEntry))))));
        }
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            int properties = getTargetFunction().getDetails().properties;
            if ((((properties&Saxon.Hej.functions.registry.BuiltInFunctionSet.CTRL)) != 0)) {
                Saxon.Hej.expr.Expression sfo = getTargetFunction().makeOptimizedFunctionCall(visitor, contextInfo, getArguments());
                if ((sfo != null)) {
                    sfo.setParentExpression(getParentExpression());
                    Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, sfo);
                    return sfo;
                }
            }
            Saxon.Hej.expr.Expression sf = base.optimize(visitor, contextInfo);
            if ((sf == this)) {
                Saxon.Hej.expr.Expression sfo = getTargetFunction().makeOptimizedFunctionCall(visitor, contextInfo, getArguments());
                if ((sfo != null)) {
                    sfo.setParentExpression(getParentExpression());
                    Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, sfo);
                    return sfo;
                }
            }
            Saxon.Hej.expr.parser.Optimizer opt = visitor.obtainOptimizer();
            if (((sf is Saxon.Hej.expr.SystemFunctionCall) && opt.isOptionSet(Saxon.Hej.expr.parser.OptimizerOptions.CONSTANT_FOLDING))) {
                Saxon.Hej.functions.registry.BuiltInFunctionSet.Entry details = ((Saxon.Hej.expr.SystemFunctionCall)(sf)).getTargetFunction().getDetails();
                if ((((details.properties&Saxon.Hej.functions.registry.BuiltInFunctionSet.UO)) != 0)) {
                    setArg(0, getArg(0).unordered(true, visitor.isOptimizeForStreaming()));
                }
                if ((getArity()<=details.resultIfEmpty.Length)) {
                    for (int i = 0; (i<getArity()); i++) {
                        if ((Saxon.Hej.expr.Literal.isEmptySequence(getArg(i)) && (details.resultIfEmpty[i] != null))) {
                            return Saxon.Hej.expr.Literal.makeLiteral(details.resultIfEmpty[i].materialize(), this);
                        }
                    }
                }
            }
            return sf;
        }
        public override bool isVacuousExpression() {
            return isCallOn(typeof(Saxon.Hej.functions.Error));
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            return getTargetFunction().getResultItemType(getArguments());
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.Expression[] args = new Saxon.Hej.expr.Expression[getArity()];
            for (int i = 0; (i<args.Length); i++) {
                args[i] = getArg(i).copy(rebindings);
            }
            Saxon.Hej.functions.SystemFunction target = getTargetFunction();
            if ((target is Saxon.Hej.functions.StatefulSystemFunction)) {
                target = ((Saxon.Hej.functions.StatefulSystemFunction)(target)).copy();
            }
            return target.makeFunctionCall(args);
        }
        public override Saxon.Hej.value.IntegerValue[] getIntegerBounds() {
            Saxon.Hej.functions.SystemFunction fn = getTargetFunction();
            if ((((fn.getDetails().properties&Saxon.Hej.functions.registry.BuiltInFunctionSet.FILTER)) != 0)) {
                return getArg(0).getIntegerBounds();
            }
            return fn.getIntegerBounds();
        }
        public bool isNegatable(Saxon.Hej.type.TypeHierarchy th) {
            return (((isCallOn(typeof(Saxon.Hej.functions.NotFn)) || isCallOn(typeof(Saxon.Hej.functions.BooleanFn))) || isCallOn(typeof(Saxon.Hej.functions.Empty))) || isCallOn(typeof(Saxon.Hej.functions.Exists)));
        }
        public Saxon.Hej.expr.Expression negate() {
            Saxon.Hej.functions.SystemFunction fn = getTargetFunction();
            if ((fn is Saxon.Hej.functions.NotFn)) {
                Saxon.Hej.expr.Expression arg = getArg(0);
                if (((arg.getItemType() == Saxon.Hej.type.BuiltInAtomicType.BOOLEAN) && (arg.getCardinality() == Saxon.Hej.expr.StaticProperty.EXACTLY_ONE))) {
                    return arg;
                } else {
                    return Saxon.Hej.functions.SystemFunction.makeCall("boolean", getRetainedStaticContext(), arg);
                }
            } else {
                if ((fn is Saxon.Hej.functions.BooleanFn)) {
                    return Saxon.Hej.functions.SystemFunction.makeCall("not", getRetainedStaticContext(), getArg(0));
                } else {
                    if ((fn is Saxon.Hej.functions.Exists)) {
                        return Saxon.Hej.functions.SystemFunction.makeCall("empty", getRetainedStaticContext(), getArg(0));
                    } else {
                        if ((fn is Saxon.Hej.functions.Empty)) {
                            return Saxon.Hej.functions.SystemFunction.makeCall("exists", getRetainedStaticContext(), getArg(0));
                        }
                    }
                }
            }
            throw new Saxon.Ejava.lang.UnsupportedOperationException();
        }
        public override Saxon.Hej.expr.Expression unordered(bool retainAllNodes, bool forStreaming) {
            Saxon.Hej.functions.SystemFunction fn = getTargetFunction();
            if ((fn is Saxon.Hej.functions.Reverse)) {
                return getArg(0);
            }
            if ((fn is Saxon.Hej.functions.TreatFn)) {
                setArg(0, getArg(0).unordered(retainAllNodes, forStreaming));
            }
            return this;
        }
        public override Saxon.Hej.expr.parser.PathMap.PathMapNodeSet addToPathMap(Saxon.Hej.expr.parser.PathMap pathMap, Saxon.Hej.expr.parser.PathMap.PathMapNodeSet pathMapNodeSet) {
            if (((isCallOn(typeof(Saxon.Hej.functions.Doc)) || isCallOn(typeof(Saxon.Hej.functions.DocumentFn))) || isCallOn(typeof(Saxon.Hej.functions.CollectionFn)))) {
                getArg(0).addToPathMap(pathMap, pathMapNodeSet);
                return new Saxon.Hej.expr.parser.PathMap.PathMapNodeSet(pathMap.makeNewRoot(this));
            } else {
                if (isCallOn(typeof(Saxon.Hej.functions.KeyFn))) {
                    return ((Saxon.Hej.functions.KeyFn)(getTargetFunction())).addToPathMap(pathMap, pathMapNodeSet);
                } else {
                    return base.addToPathMap(pathMap, pathMapNodeSet);
                }
            }
        }
        public override Saxon.Hej.pattern.Pattern toPattern(Saxon.Hej.Configuration config) {
            Saxon.Hej.functions.SystemFunction fn = getTargetFunction();
            if ((fn is Saxon.Hej.functions.Root_1)) {
                if (((getArg(0) is Saxon.Hej.expr.ContextItemExpression) || (((getArg(0) is Saxon.Hej.expr.ItemChecker) && (((Saxon.Hej.expr.ItemChecker)(getArg(0))).getBaseExpression() is Saxon.Hej.expr.ContextItemExpression))))) {
                    return new Saxon.Hej.pattern.NodeSetPattern(this);
                }
            }
            return base.toPattern(config);
        }
        public override void process(Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context) {
            /*Delegate.processLeavingTail()*/makeElaborator().elaborateForPush()(output, context);
        }
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            return base.call(context, arguments);
        }
        public override string getExpressionName() {
            return "sysFuncCall";
        }
        public override bool isUpdatingExpression() {
            return (getTargetFunction() is Saxon.Hej.functions.Put);
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            if (getFunctionName().hasURI(Saxon.Hej.om.NamespaceUri.FN)) {
                @out.startElement("fn", this);
                string localPart = getFunctionName().getLocalPart();
                @out.emitAttribute("name", localPart);
                getTargetFunction().exportAttributes(@out);
                if (((((localPart.Equals("concat") && "JS".Equals(@out.getOptions().target)) && (@out.getOptions().targetVersion == 2)) && (getArity() == 1)) && (getArg(0) is Saxon.Hej.expr.instruct.Block))) {
                    foreach (Saxon.Hej.expr.Operand o in getArg(0).operands()) {
                        if ((o.getChildExpression() is Saxon.Hej.expr.Literal)) {
                            foreach (Saxon.Hej.om.Item it in ((Saxon.Hej.expr.Literal)(o.getChildExpression())).getGroundedValue().asIterable()) {
                                Saxon.Hej.expr.Literal.exportValue(it, @out);
                            }
                        } else {
                            o.getChildExpression().export(@out);
                        }
                    }
                } else {
                    foreach (Saxon.Hej.expr.Operand o in operands()) {
                        o.getChildExpression().export(@out);
                    }
                }
                getTargetFunction().exportAdditionalArguments(this, @out);
                @out.endElement();
            } else {
                @out.startElement("ifCall", this);
                @out.emitAttribute("name", getFunctionName());
                @out.emitAttribute("type", getTargetFunction().getFunctionItemType().getResultType().toAlphaCode());
                getTargetFunction().exportAttributes(@out);
                foreach (Saxon.Hej.expr.Operand o in operands()) {
                    o.getChildExpression().export(@out);
                }
                getTargetFunction().exportAdditionalArguments(this, @out);
                @out.endElement();
            }
        }
        public abstract class Optimized : Saxon.Hej.expr.SystemFunctionCall {
            public Optimized(Saxon.Hej.functions.SystemFunction target, Saxon.Hej.expr.Expression[] arguments) : base (target, arguments) {
            }
            public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
                return this;
            }
        
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            Saxon.Hej.functions.SystemFunction fn = getTargetFunction();
            Saxon.Hej.expr.elab.Elaborator fnElaborator = fn.getElaborator();
            if ((fnElaborator != null)) {
                return fnElaborator;
            } else {
                return new Saxon.Hej.expr.SystemFunctionCall.SystemFunctionCallElaborator();
            }
        }
        public class SystemFunctionCallElaborator : Saxon.Hej.expr.FunctionCall.FunctionCallElaborator {
            public override void setExpression(Saxon.Hej.expr.Expression expr) {
                base.setExpression(expr);
                allocateArgumentEvaluators((Saxon.Hej.expr.FunctionCall)(expr), false);
            }
            public override Saxon.Hej.expr.elab.PullEvaluator elaborateForPull() {
                Saxon.Hej.expr.SystemFunctionCall expr = (Saxon.Hej.expr.SystemFunctionCall)(getExpression());
                Saxon.Hej.functions.SystemFunction fn = expr.getTargetFunction();
                switch (argumentEvaluators.Length) {
                  case 0:
                    return context => {
                        try {
                            return fn.call(context, Saxon.Hej.expr.StackFrame.EMPTY_ARRAY_OF_SEQUENCE).iterate();
                        } catch (Saxon.Hej.trans.XPathException err) {
                            err.maybeSetContext(context);
                            err.maybeSetLocation(expr.getLocation());
                            throw;
                        }
                    } ;
                  case 1:
                    return context => {
                        try {
                            return fn.call(context, new Saxon.Hej.om.Sequence[]{argumentEvaluators[0].evaluate(context)}).iterate();
                        } catch (Saxon.Hej.trans.XPathException err) {
                            err.maybeSetContext(context);
                            err.maybeSetLocation(expr.getLocation());
                            throw;
                        }
                    } ;
                  case 2:
                    return context => {
                        try {
                            return fn.call(context, new Saxon.Hej.om.Sequence[]{argumentEvaluators[0].evaluate(context), argumentEvaluators[1].evaluate(context)}).iterate();
                        } catch (Saxon.Hej.trans.XPathException err) {
                            err.maybeSetContext(context);
                            err.maybeSetLocation(expr.getLocation());
                            throw;
                        }
                    } ;
                  default:
                    return context => {
                        try {
                            return fn.call(context, evaluateArguments(context)).iterate();
                        } catch (Saxon.Hej.trans.XPathException err) {
                            err.maybeSetContext(context);
                            err.maybeSetLocation(expr.getLocation());
                            throw;
                        }
                    } ;
                }
            }
            public override Saxon.Hej.expr.elab.ItemEvaluator elaborateForItem() {
                Saxon.Hej.expr.SystemFunctionCall expr = (Saxon.Hej.expr.SystemFunctionCall)(getExpression());
                Saxon.Hej.functions.SystemFunction fn = expr.getTargetFunction();
                switch (argumentEvaluators.Length) {
                  case 0:
                    return context => fn.call(context, Saxon.Hej.expr.StackFrame.EMPTY_ARRAY_OF_SEQUENCE).head();
                  case 1:
                    return context => fn.call(context, new Saxon.Hej.om.Sequence[]{argumentEvaluators[0].evaluate(context)}).head();
                  case 2:
                    return context => fn.call(context, new Saxon.Hej.om.Sequence[]{argumentEvaluators[0].evaluate(context), argumentEvaluators[1].evaluate(context)}).head();
                  default:
                    return context => fn.call(context, evaluateArguments(context)).head();
                }
            }
            public override Saxon.Hej.expr.elab.PushEvaluator elaborateForPush() {
                Saxon.Hej.expr.SystemFunctionCall expr = (Saxon.Hej.expr.SystemFunctionCall)(getExpression());
                Saxon.Hej.functions.SystemFunction fn = expr.getTargetFunction();
                if ((fn is Saxon.Hej.functions.PushableFunction)) {
                    return (output, context) => {
                        Saxon.Hej.om.Sequence[] actualArgs = evaluateArguments(context);
                        try {
                            ((Saxon.Hej.functions.PushableFunction)(fn)).process(output, context, actualArgs);
                        } catch (Saxon.Hej.trans.XPathException e) {
                            e.maybeSetLocation(expr.getLocation());
                            e.maybeSetContext(context);
                            e.maybeSetFailingExpression(expr);
                            throw;
                        }
                        return null;
                    } ;
                } else {
                    return base.elaborateForPush();
                }
            }
            public override Saxon.Hej.expr.elab.UpdateEvaluator elaborateForUpdate() {
                Saxon.Hej.expr.SystemFunctionCall expr = (Saxon.Hej.expr.SystemFunctionCall)(getExpression());
                if (expr.isVacuousExpression()) {
                    Saxon.Hej.expr.elab.PullEvaluator eval = elaborateForPull();
                    return (context, pul) => {
                        /*Delegate.iterate()*/eval(context).next();
                    } ;
                } else {
                    throw new Saxon.Ejava.lang.UnsupportedOperationException((("Expression "+expr.toShortString())+" is not an updating expression"));
                }
            }
        
        }
    
    }
}