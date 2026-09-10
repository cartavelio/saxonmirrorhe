using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public class UserFunctionCall : Saxon.Hej.expr.FunctionCall, Saxon.Hej.expr.UserFunctionResolvable, Saxon.Hej.expr.ComponentInvocation, Saxon.Hej.expr.ContextOriginator {
        private Saxon.Hej.value.SequenceType staticType;
        private Saxon.Hej.expr.instruct.UserFunction function;
        private int bindingSlot = -1;
        private int tailCall = NOT_TAIL_CALL;
        private Saxon.Hej.om.StructuredQName name;
        private bool beingInlined = false;
        private Saxon.Hej.expr.elab.SequenceEvaluator[] argumentEvaluators = null;
        private Saxon.Hej.query.UnboundFunctionLibrary.UnboundFunctionCallDetails unboundCallDetails;
        public bool isBeingInlined() {
            return beingInlined;
        }
        public void setBeingInlined(bool beingInlined) {
            this.beingInlined = beingInlined;
        }
        public const int NOT_TAIL_CALL = 0;
        public const int FOREIGN_TAIL_CALL = 1;
        public const int SELF_TAIL_CALL = 2;
        public UserFunctionCall()  {
        }
        public UserFunctionCall(Saxon.Hej.query.UnboundFunctionLibrary.UnboundFunctionCallDetails details)  {
            this.unboundCallDetails = details;
        }
        public void copyFrom(Saxon.Hej.expr.UserFunctionCall ufc2) {
            staticType = ufc2.staticType;
            function = ufc2.function;
            bindingSlot = ufc2.bindingSlot;
            tailCall = ufc2.tailCall;
            name = ufc2.name;
            beingInlined = false;
            argumentEvaluators = ufc2.argumentEvaluators;
            unboundCallDetails = null;
        }
        public void setFunctionName(Saxon.Hej.om.StructuredQName name) {
            this.name = name;
        }
        public void setStaticType(Saxon.Hej.value.SequenceType type) {
            staticType = type;
        }
        public void setFunction(Saxon.Hej.expr.instruct.UserFunction compiledFunction) {
            function = compiledFunction;
        }
        public void setBindingSlot(int slot) {
            this.bindingSlot = slot;
        }
        public int getBindingSlot() {
            return bindingSlot;
        }
        public Saxon.Hej.expr.instruct.UserFunction getFunction() {
            return function;
        }
        public Saxon.Hej.expr.Component getFixedTarget() {
            Saxon.Hej.trans.Visibility v = function.getDeclaringComponent().getVisibility();
            if (((v == Saxon.Hej.trans.Visibility.PRIVATE) || (v == Saxon.Hej.trans.Visibility.FINAL))) {
                return function.getDeclaringComponent();
            } else {
                return null;
            }
        }
        public Saxon.Hej.query.UnboundFunctionLibrary.UnboundFunctionCallDetails getUnboundCallDetails() {
            return unboundCallDetails;
        }
        public bool isTailCall() {
            return (tailCall != NOT_TAIL_CALL);
        }
        public bool isRecursiveTailCall() {
            return (tailCall == SELF_TAIL_CALL);
        }
        public override Saxon.Hej.om.StructuredQName getFunctionName() {
            if ((name == null)) {
                return function.getFunctionName();
            } else {
                return name;
            }
        }
        public Saxon.Hej.trans.SymbolicName getSymbolicName() {
            return new Saxon.Hej.trans.SymbolicName.F(getFunctionName(), getArity());
        }
        public Saxon.Hej.expr.Component getTarget() {
            return function.getDeclaringComponent();
        }
        private static readonly int UNHANDLED_DEPENDENCIES = (((Saxon.Hej.expr.StaticProperty.DEPENDS_ON_POSITION|Saxon.Hej.expr.StaticProperty.DEPENDS_ON_LAST)|Saxon.Hej.expr.StaticProperty.DEPENDS_ON_XSLT_CONTEXT)|Saxon.Hej.expr.StaticProperty.DEPENDS_ON_USER_FUNCTIONS);
        public void allocateArgumentEvaluators() {
            argumentEvaluators = new Saxon.Hej.expr.elab.SequenceEvaluator[getArity()];
            Saxon.Hej.expr.instruct.UserFunction target = getFunction();
            int i = 0;
            foreach (Saxon.Hej.expr.Operand o in operands()) {
                Saxon.Hej.expr.Expression arg = o.getChildExpression();
                if (((arg is Saxon.Hej.expr.ErrorExpression) && ((Saxon.Hej.expr.ErrorExpression)(arg)).getErrorCodeLocalPart().Equals("UseDefault"))) {
                    arg = target.getDefaultValueExpression(i);
                    o.setChildExpression(arg);
                }
                if (((i == 0) && target.getDeclaredStreamability().isConsuming())) {
                    argumentEvaluators[i] = new Saxon.Hej.expr.elab.StreamingArgumentEvaluator(arg);
                } else {
                    if (target.getParameterDefinitions()[i].isIndexedVariable()) {
                        Saxon.Hej.expr.elab.PullEvaluator argPull = arg.makeElaborator().elaborateForPull();
                        argumentEvaluators[i] = new Saxon.Hej.expr.elab.IndexedVariableEvaluator(argPull);
                    } else {
                        if ((((arg.getDependencies()&UNHANDLED_DEPENDENCIES)) != 0)) {
                            argumentEvaluators[i] = arg.makeElaborator().eagerly();
                        } else {
                            if (((arg is Saxon.Hej.expr.instruct.Block) && ((Saxon.Hej.expr.instruct.Block)(arg)).isCandidateForSharedAppend())) {
                                argumentEvaluators[i] = new Saxon.Hej.expr.elab.SharedAppendEvaluator((Saxon.Hej.expr.instruct.Block)(arg));
                            } else {
                                argumentEvaluators[i] = new Saxon.Hej.expr.elab.LearningEvaluator(arg, arg.makeElaborator().lazily(true));
                            }
                        }
                    }
                }
                i++;
            }
        }
        public Saxon.Hej.expr.elab.SequenceEvaluator[] getArgumentEvaluators() {
            return argumentEvaluators;
        }
        public override Saxon.Hej.expr.Expression preEvaluate(Saxon.Hej.expr.parser.ExpressionVisitor visitor) {
            return this;
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            if ((staticType == null)) {
                return Saxon.Hej.type.AnyItemType.getInstance();
            } else {
                return staticType.getPrimaryType();
            }
        }
        public override Saxon.Hej.type.UType getStaticUType(Saxon.Hej.type.UType contextItemType) {
            Saxon.Hej.expr.instruct.UserFunction f = getFunction();
            if ((f == null)) {
                return Saxon.Hej.type.UType.ANY;
            }
            return f.getResultType().getPrimaryType().getUType();
        }
        public override int getIntrinsicDependencies() {
            return Saxon.Hej.expr.StaticProperty.DEPENDS_ON_USER_FUNCTIONS;
        }
        public override bool isUpdatingExpression() {
            return function.isUpdating();
        }
        protected internal override int computeSpecialProperties() {
            if ((function == null)) {
                return base.computeSpecialProperties();
            } else {
                if (((function.getBody() != null) && (((function.getDeclaredVisibility() == Saxon.Hej.trans.Visibility.PRIVATE) || (function.getDeclaredVisibility() == Saxon.Hej.trans.Visibility.FINAL))))) {
                    int props;
                    System.Collections.Generic.IList<Saxon.Hej.expr.instruct.UserFunction> calledFunctions = new System.Collections.Generic.List<Saxon.Hej.expr.instruct.UserFunction>();
                    Saxon.Hej.expr.parser.ExpressionTool.gatherCalledFunctions(function.getBody(), calledFunctions);
                    if (calledFunctions.Count==0) {
                        props = function.getBody().getSpecialProperties();
                    } else {
                        props = base.computeSpecialProperties();
                    }
                    if ((function.getDeterminism() != Saxon.Hej.expr.instruct.UserFunction.Determinism.PROACTIVE)) {
                        props |= Saxon.Hej.expr.StaticProperty.NO_NODES_NEWLY_CREATED;
                    }
                    return props;
                } else {
                    return base.computeSpecialProperties();
                }
            }
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            if ((function == null)) {
                throw new Saxon.Ejava.lang.UnsupportedOperationException("UserFunctionCall.copy()");
            }
            Saxon.Hej.expr.UserFunctionCall ufc = new Saxon.Hej.expr.UserFunctionCall();
            ufc.setFunction(function);
            ufc.setStaticType(staticType);
            int numArgs = getArity();
            Saxon.Hej.expr.Expression[] a2 = new Saxon.Hej.expr.Expression[numArgs];
            for (int i = 0; (i<numArgs); i++) {
                a2[i] = getArg(i).copy(rebindings);
            }
            ufc.setArguments(a2);
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, ufc);
            return ufc;
        }
        protected internal override int computeCardinality() {
            if ((staticType == null)) {
                return Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_MORE;
            } else {
                return staticType.getCardinality();
            }
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            Saxon.Hej.expr.Expression e = base.typeCheck(visitor, contextInfo);
            if ((e != this)) {
                return e;
            }
            if ((function != null)) {
                checkFunctionCall(function, visitor);
                if (((staticType == null) || (staticType == Saxon.Hej.value.SequenceType.ANY_SEQUENCE))) {
                    staticType = function.getResultType();
                }
            }
            return this;
        }
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType) {
            Saxon.Hej.expr.Expression e = base.optimize(visitor, contextItemType);
            if (((e == this) && (function != null))) {
                return visitor.obtainOptimizer().tryInlineFunctionCall(this, visitor, contextItemType);
            }
            return e;
        }
        public override void resetLocalStaticProperties() {
            base.resetLocalStaticProperties();
        }
        public override Saxon.Hej.expr.parser.PathMap.PathMapNodeSet addToPathMap(Saxon.Hej.expr.parser.PathMap pathMap, Saxon.Hej.expr.parser.PathMap.PathMapNodeSet pathMapNodeSet) {
            return addExternalFunctionCallToPathMap(pathMap, pathMapNodeSet);
        }
        public override int markTailFunctionCalls(Saxon.Hej.om.StructuredQName qName, int arity) {
            tailCall = ((getFunctionName().Equals(qName) && (arity == getArity())) ? SELF_TAIL_CALL : FOREIGN_TAIL_CALL);
            return tailCall;
        }
        public override int getImplementationMethod() {
            if (Saxon.Hej.value.Cardinality.allowsMany(getCardinality())) {
                return (ITERATE_METHOD|PROCESS_METHOD);
            } else {
                return EVALUATE_METHOD;
            }
        }
        public override Saxon.Hej.om.Item evaluateItem(Saxon.Hej.expr.XPathContext c) {
            return /*Delegate.eval()*/makeElaborator().elaborateForItem()(c);
        }
        public override Saxon.Hej.om.SequenceIterator iterate(Saxon.Hej.expr.XPathContext c) {
            return /*Delegate.iterate()*/makeElaborator().elaborateForPull()(c);
        }
        private void requestTailCall(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] actualArgs) {
            if ((bindingSlot>=0)) {
                Saxon.Hej.expr.TailCallLoop.TailCallComponent info = new Saxon.Hej.expr.TailCallLoop.TailCallComponent();
                Saxon.Hej.expr.Component target = getTargetComponent(context);
                info.component = target;
                info.function = (Saxon.Hej.expr.instruct.UserFunction)(target.getActor());
                if (target.isHiddenAbstractComponent()) {
                    throw new Saxon.Hej.trans.XPathException((("Cannot call an abstract function ("+name.getDisplayName())+") with no implementation"), "XTDE3052");
                }
                ((Saxon.Hej.expr.XPathContextMajor)(context)).requestTailCall(info, actualArgs);
            } else {
                Saxon.Hej.expr.TailCallLoop.TailCallFunction info = new Saxon.Hej.expr.TailCallLoop.TailCallFunction();
                info.function = function;
                ((Saxon.Hej.expr.XPathContextMajor)(context)).requestTailCall(info, actualArgs);
            }
        }
        public override void process(Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.expr.instruct.TailCall tc = /*Delegate.processLeavingTail()*/makeElaborator().elaborateForPush()(output, context);
            dispatchTailCall(tc);
        }
        public Saxon.Hej.expr.Component getTargetComponent(Saxon.Hej.expr.XPathContext context) {
            if ((bindingSlot == -1)) {
                return function.getDeclaringComponent();
            } else {
                return context.getTargetComponent(bindingSlot);
            }
        }
        public override Saxon.Hej.expr.instruct.UserFunction getTargetFunction(Saxon.Hej.expr.XPathContext context) {
            return (Saxon.Hej.expr.instruct.UserFunction)(getTargetComponent(context).getActor());
        }
        public Saxon.Hej.om.Sequence[] evaluateArguments(Saxon.Hej.expr.XPathContext c, bool streamed) {
            int numArgs = getArity();
            Saxon.Hej.om.Sequence[] actualArgs = Saxon.Hej.om.SequenceTool.makeSequenceArray(numArgs);
            lock (this) {
                if ((argumentEvaluators == null)) {
                    allocateArgumentEvaluators();
                }
            }
            for (int i = 0; (i<numArgs); i++) {
                Saxon.Hej.expr.elab.SequenceEvaluator eval = argumentEvaluators[i];
                if (((eval == null) || (((eval is Saxon.Hej.expr.elab.StreamingArgumentEvaluator) && !(streamed))))) {
                    eval = getArguments()[0].makeElaborator().eagerly();
                }
                actualArgs[i] = eval.evaluate(c);
            }
            return actualArgs;
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("ufCall", this);
            if ((getFunctionName() != null)) {
                @out.emitAttribute("name", getFunctionName());
                @out.emitAttribute("tailCall", ((tailCall == NOT_TAIL_CALL) ? "false" : ((tailCall == SELF_TAIL_CALL) ? "self" : "foreign")));
            }
            @out.emitAttribute("bSlot", (""+getBindingSlot()));
            foreach (Saxon.Hej.expr.Operand o in operands()) {
                o.getChildExpression().export(@out);
            }
            if ((getFunctionName() == null)) {
                @out.setChildRole("inline");
                function.getBody().export(@out);
                @out.endElement();
            }
            @out.endElement();
        }
        public override string getExpressionName() {
            return "userFunctionCall";
        }
        public override System.Object getProperty(string name) {
            if (name.Equals("target")) {
                return function;
            }
            return base.getProperty(name);
        }
        public override Saxon.Hej.om.StructuredQName getObjectName() {
            return getFunctionName();
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            if (isTailCall()) {
                return new Saxon.Hej.expr.UserFunctionCall.TailCallElaborator();
            } else {
                return new Saxon.Hej.expr.UserFunctionCall.UserFunctionCallElaborator();
            }
        }
        private class TailCallElaborator : Saxon.Hej.expr.elab.PullElaborator {
            public override Saxon.Hej.expr.elab.PullEvaluator elaborateForPull() {
                Saxon.Hej.expr.UserFunctionCall expr = (Saxon.Hej.expr.UserFunctionCall)(getExpression());
                if ((expr.bindingSlot>=0)) {
                    return context => {
                        Saxon.Hej.expr.TailCallLoop.TailCallComponent info = new Saxon.Hej.expr.TailCallLoop.TailCallComponent();
                        Saxon.Hej.expr.Component target = expr.getTargetComponent(context);
                        info.component = target;
                        info.function = (Saxon.Hej.expr.instruct.UserFunction)(target.getActor());
                        if (target.isHiddenAbstractComponent()) {
                            throw new Saxon.Hej.trans.XPathException((("Cannot call an abstract function ("+expr.getFunctionName().getDisplayName())+") with no implementation"), "XTDE3052");
                        }
                        Saxon.Hej.om.Sequence[] actualArgs = expr.evaluateArguments(context, false);
                        ((Saxon.Hej.expr.XPathContextMajor)(context)).requestTailCall(info, actualArgs);
                        return Saxon.Hej.tree.iter.EmptyIterator.getInstance();
                    } ;
                } else {
                    Saxon.Hej.expr.TailCallLoop.TailCallFunction info = new Saxon.Hej.expr.TailCallLoop.TailCallFunction();
                    info.function = expr.getFunction();
                    return context => {
                        Saxon.Hej.om.Sequence[] actualArgs = expr.evaluateArguments(context, false);
                        ((Saxon.Hej.expr.XPathContextMajor)(context)).requestTailCall(info, actualArgs);
                        return Saxon.Hej.tree.iter.EmptyIterator.getInstance();
                    } ;
                }
            }
        
        }
        private class UserFunctionCallElaborator : Saxon.Hej.expr.elab.PullElaborator {
            private void testNotAbstract(Saxon.Hej.expr.UserFunctionCall expr, Saxon.Hej.expr.Component target) {
                if (target.isHiddenAbstractComponent()) {
                    throw new Saxon.Hej.trans.XPathException((("Cannot call an abstract function ("+expr.getFunctionName().getDisplayName())+") with no implementation"), "XTDE3052");
                }
            }
            private Saxon.Hej.trans.XPathException.StackOverflow reportStackOverflow(Saxon.Hej.expr.Expression expr) {
                return new Saxon.Hej.trans.XPathException.StackOverflow("Too many nested function calls. May be due to infinite recursion", Saxon.Hej.trans.SaxonErrorCode.SXLM0001, expr.getLocation());
            }
            public override Saxon.Hej.expr.elab.PullEvaluator elaborateForPull() {
                Saxon.Hej.expr.UserFunctionCall expr = (Saxon.Hej.expr.UserFunctionCall)(getExpression());
                if ((expr.bindingSlot>=0)) {
                    return context => {
                        Saxon.Hej.expr.Component target = expr.getTargetComponent(context);
                        testNotAbstract(expr, target);
                        try {
                            Saxon.Hej.om.Sequence[] actualArgs = expr.evaluateArguments(context, false);
                            Saxon.Hej.expr.instruct.UserFunction targetFunction = (Saxon.Hej.expr.instruct.UserFunction)(target.getActor());
                            Saxon.Hej.expr.XPathContextMajor c2 = ((/*1430*/Saxon.Hej.expr.XPathContextMajor)targetFunction.makeNewContext(context, expr));
                            c2.setCurrentComponent(target);
                            return targetFunction.call(c2, actualArgs).iterate();
                        } catch (System.StackOverflowException err) {
                            throw reportStackOverflow(expr);
                        }
                    } ;
                } else {
                    Saxon.Hej.expr.instruct.UserFunction targetFunction = expr.getFunction();
                    return context => {
                        try {
                            Saxon.Hej.om.Sequence[] actualArgs = expr.evaluateArguments(context, false);
                            Saxon.Hej.expr.XPathContextMajor c2 = ((/*1430*/Saxon.Hej.expr.XPathContextMajor)targetFunction.makeNewContext(context, expr));
                            return targetFunction.call(c2, actualArgs).iterate();
                        } catch (System.StackOverflowException err) {
                            throw reportStackOverflow(expr);
                        }
                    } ;
                }
            }
            public override Saxon.Hej.expr.elab.ItemEvaluator elaborateForItem() {
                Saxon.Hej.expr.UserFunctionCall expr = (Saxon.Hej.expr.UserFunctionCall)(getExpression());
                if ((expr.bindingSlot>=0)) {
                    return context => {
                        Saxon.Hej.expr.Component target = expr.getTargetComponent(context);
                        testNotAbstract(expr, target);
                        try {
                            Saxon.Hej.om.Sequence[] actualArgs = expr.evaluateArguments(context, false);
                            Saxon.Hej.expr.instruct.UserFunction targetFunction = (Saxon.Hej.expr.instruct.UserFunction)(target.getActor());
                            Saxon.Hej.expr.XPathContextMajor c2 = ((/*1430*/Saxon.Hej.expr.XPathContextMajor)targetFunction.makeNewContext(context, expr));
                            c2.setCurrentComponent(target);
                            return targetFunction.call(c2, actualArgs).head();
                        } catch (System.StackOverflowException err) {
                            throw reportStackOverflow(expr);
                        }
                    } ;
                } else {
                    Saxon.Hej.expr.instruct.UserFunction targetFunction = expr.getFunction();
                    return context => {
                        try {
                            Saxon.Hej.om.Sequence[] actualArgs = expr.evaluateArguments(context, false);
                            Saxon.Hej.expr.XPathContextMajor c2 = ((/*1430*/Saxon.Hej.expr.XPathContextMajor)targetFunction.makeNewContext(context, expr));
                            return targetFunction.call(c2, actualArgs).head();
                        } catch (System.StackOverflowException err) {
                            throw reportStackOverflow(expr);
                        }
                    } ;
                }
            }
            public override Saxon.Hej.expr.elab.PushEvaluator elaborateForPush() {
                Saxon.Hej.expr.UserFunctionCall expr = (Saxon.Hej.expr.UserFunctionCall)(getExpression());
                if (expr.isTailCall()) {
                    throw new Saxon.Ejava.lang.AssertionError("Not using tail call path");
                }
                if ((expr.bindingSlot>=0)) {
                    return (output, context) => {
                        Saxon.Hej.expr.Component target = expr.getTargetComponent(context);
                        testNotAbstract(expr, target);
                        try {
                            Saxon.Hej.om.Sequence[] actualArgs = expr.evaluateArguments(context, false);
                            Saxon.Hej.expr.instruct.UserFunction targetFunction = (Saxon.Hej.expr.instruct.UserFunction)(target.getActor());
                            Saxon.Hej.expr.XPathContextMajor c2 = ((/*1430*/Saxon.Hej.expr.XPathContextMajor)targetFunction.makeNewContext(context, expr));
                            c2.setCurrentComponent(target);
                            targetFunction.process(c2, actualArgs, output);
                        } catch (System.StackOverflowException err) {
                            throw reportStackOverflow(expr);
                        }
                        return null;
                    } ;
                } else {
                    Saxon.Hej.expr.instruct.UserFunction targetFunction = expr.getFunction();
                    return (output, context) => {
                        try {
                            Saxon.Hej.om.Sequence[] actualArgs = expr.evaluateArguments(context, false);
                            Saxon.Hej.expr.XPathContextMajor c2 = ((/*1430*/Saxon.Hej.expr.XPathContextMajor)targetFunction.makeNewContext(context, expr));
                            targetFunction.process(c2, actualArgs, output);
                        } catch (System.StackOverflowException err) {
                            throw reportStackOverflow(expr);
                        }
                        return null;
                    } ;
                }
            }
            public override Saxon.Hej.expr.elab.UpdateEvaluator elaborateForUpdate() {
                Saxon.Hej.expr.UserFunctionCall expr = (Saxon.Hej.expr.UserFunctionCall)(getExpression());
                Saxon.Hej.expr.instruct.UserFunction targetFunction = expr.getFunction();
                return (context, pul) => {
                    Saxon.Hej.om.Sequence[] actualArgs = expr.evaluateArguments(context, false);
                    Saxon.Hej.expr.XPathContextMajor c2 = context.newCleanContext();
                    c2.setOrigin(expr);
                    targetFunction.callUpdating(actualArgs, c2, pul);
                } ;
            }
        
        }
    
    }
}