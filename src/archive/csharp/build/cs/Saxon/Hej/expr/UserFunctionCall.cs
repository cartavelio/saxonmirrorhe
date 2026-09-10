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
        public bool isBeingInlined() {
            return beingInlined;
        }
        public void setBeingInlined(bool beingInlined) {
            this.beingInlined = beingInlined;
        }
        private bool beingInlined = false;
        private Saxon.Hej.expr.parser.Evaluator[] argumentEvaluators = null;
        public const int NOT_TAIL_CALL = 0;
        public const int FOREIGN_TAIL_CALL = 1;
        public const int SELF_TAIL_CALL = 2;
        public UserFunctionCall()  {
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
        public void setArgumentEvaluators(Saxon.Hej.expr.parser.Evaluator[] evalModes) {
            argumentEvaluators = evalModes;
        }
        private static readonly int UNHANDLED_DEPENDENCIES = (((Saxon.Hej.expr.StaticProperty.DEPENDS_ON_POSITION|Saxon.Hej.expr.StaticProperty.DEPENDS_ON_LAST)|Saxon.Hej.expr.StaticProperty.DEPENDS_ON_XSLT_CONTEXT)|Saxon.Hej.expr.StaticProperty.DEPENDS_ON_USER_FUNCTIONS);
        public void allocateArgumentEvaluators() {
            argumentEvaluators = new Saxon.Hej.expr.parser.Evaluator[getArity()];
            int i = 0;
            foreach (Saxon.Hej.expr.Operand o in operands()) {
                Saxon.Hej.expr.Expression arg = o.getChildExpression();
                Saxon.Hej.value.SequenceType required = function.getArgumentType(i);
                int cardinality = required.getCardinality();
                if (((i == 0) && function.getDeclaredStreamability().isConsuming())) {
                    argumentEvaluators[i] = Saxon.Hej.expr.parser.Evaluator.StreamingArgument.INSTANCE;
                } else {
                    if (function.getParameterDefinitions()[i].isIndexedVariable()) {
                        argumentEvaluators[i] = Saxon.Hej.expr.parser.Evaluator.MakeIndexedVariable.INSTANCE;
                    } else {
                        if ((arg is Saxon.Hej.expr.Literal)) {
                            argumentEvaluators[i] = Saxon.Hej.expr.parser.Evaluator.Literal.INSTANCE;
                        } else {
                            if ((arg is Saxon.Hej.expr.VariableReference)) {
                                argumentEvaluators[i] = Saxon.Hej.expr.parser.Evaluator.Variable.INSTANCE;
                            } else {
                                if ((cardinality == Saxon.Hej.expr.StaticProperty.EXACTLY_ONE)) {
                                    argumentEvaluators[i] = Saxon.Hej.expr.parser.Evaluator.SingleItem.INSTANCE;
                                } else {
                                    if ((((arg.getDependencies()&UNHANDLED_DEPENDENCIES)) != 0)) {
                                        argumentEvaluators[i] = new Saxon.Hej.expr.parser.Evaluator.EagerSequence();
                                    } else {
                                        if ((!(Saxon.Hej.value.Cardinality.allowsMany(arg.getCardinality())) && (arg.getCost()<20))) {
                                            argumentEvaluators[i] = new Saxon.Hej.expr.parser.Evaluator.EagerSequence();
                                        } else {
                                            if ((cardinality == Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_ONE)) {
                                                argumentEvaluators[i] = Saxon.Hej.expr.parser.Evaluator.OptionalItem.INSTANCE;
                                            } else {
                                                if (((arg is Saxon.Hej.expr.instruct.Block) && ((Saxon.Hej.expr.instruct.Block)(arg)).isCandidateForSharedAppend())) {
                                                    argumentEvaluators[i] = Saxon.Hej.expr.parser.Evaluator.SharedAppend.INSTANCE;
                                                } else {
                                                    argumentEvaluators[i] = Saxon.Hej.expr.parser.Evaluator.MemoClosureEvaluator.INSTANCE;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                i++;
            }
        }
        public Saxon.Hej.expr.parser.Evaluator[] getArgumentEvaluators() {
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
            argumentEvaluators = null;
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
            return callFunction(c).head();
        }
        public override Saxon.Hej.om.SequenceIterator iterate(Saxon.Hej.expr.XPathContext c) {
            return callFunction(c).iterate();
        }
        public override void evaluatePendingUpdates(Saxon.Hej.expr.XPathContext context, Saxon.Hej.expr.PendingUpdateList pul) {
            Saxon.Hej.om.Sequence[] actualArgs = evaluateArguments(context);
            Saxon.Hej.expr.XPathContextMajor c2 = context.newCleanContext();
            c2.setOrigin(this);
            function.callUpdating(actualArgs, c2, pul);
        }
        private const int depth = 0;
        private Saxon.Hej.om.Sequence callFunction(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.expr.instruct.UserFunction targetFunction;
            Saxon.Hej.om.Sequence[] actualArgs = evaluateArguments(context);
            Saxon.Hej.expr.XPathContextMajor c2;
            if (isTailCall()) {
                requestTailCall(context, actualArgs);
                return Saxon.Hej.value.EmptySequence.getInstance();
            }
            if ((bindingSlot>=0)) {
                Saxon.Hej.expr.Component target = getTargetComponent(context);
                if (target.isHiddenAbstractComponent()) {
                    throw new Saxon.Hej.trans.XPathException((("Cannot call an abstract function ("+name.getDisplayName())+") with no implementation"), "XTDE3052");
                }
                targetFunction = (Saxon.Hej.expr.instruct.UserFunction)(target.getActor());
                c2 = ((/*1430*/Saxon.Hej.expr.XPathContextMajor)targetFunction.makeNewContext(context, this));
                c2.setCurrentComponent(target);
                c2.setOrigin(this);
            } else {
                targetFunction = function;
                c2 = ((/*1430*/Saxon.Hej.expr.XPathContextMajor)targetFunction.makeNewContext(context, this));
                c2.setOrigin(this);
            }
            try {
                return targetFunction.call(c2, actualArgs);
            } catch (Saxon.Hej.trans.UncheckedXPathException e) {
                Saxon.Hej.trans.XPathException xe = e.getXPathException();
                xe.maybeSetLocation(getLocation());
                throw xe;
            } catch (System.StackOverflowException err) {
                throw new Saxon.Hej.trans.XPathException.StackOverflow("Too many nested function calls. May be due to infinite recursion", Saxon.Hej.trans.SaxonErrorCode.SXLM0001, getLocation());
            }
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
            Saxon.Hej.om.Sequence[] actualArgs = evaluateArguments(context);
            if (isTailCall()) {
                requestTailCall(context, actualArgs);
                return ;
            }
            if ((bindingSlot>=0)) {
                Saxon.Hej.expr.Component target = getTargetComponent(context);
                Saxon.Hej.expr.instruct.UserFunction targetFunction = (Saxon.Hej.expr.instruct.UserFunction)(target.getActor());
                if ((target.getVisibility() == Saxon.Hej.trans.Visibility.ABSTRACT)) {
                    throw new Saxon.Hej.trans.XPathException("Cannot call a function defined with visibility=abstract", "XTDE3052");
                }
                Saxon.Hej.expr.XPathContextMajor c2 = ((/*1430*/Saxon.Hej.expr.XPathContextMajor)targetFunction.makeNewContext(context, this));
                c2.setCurrentComponent(target);
                c2.setOrigin(this);
                targetFunction.process(c2, actualArgs, output);
            } else {
                Saxon.Hej.expr.XPathContextMajor c2 = ((/*1430*/Saxon.Hej.expr.XPathContextMajor)function.makeNewContext(context, this));
                c2.setOrigin(this);
                function.process(c2, actualArgs, output);
            }
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
        public override Saxon.Hej.om.Sequence[] evaluateArguments(Saxon.Hej.expr.XPathContext c) {
            return evaluateArguments(c, false);
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
                Saxon.Hej.expr.parser.Evaluator eval = argumentEvaluators[i];
                if (((eval == null) || ((eval == Saxon.Hej.expr.parser.Evaluator.StreamingArgument.INSTANCE) && !(streamed)))) {
                    eval = Saxon.Hej.expr.parser.Evaluator.EagerSequence.INSTANCE;
                }
                actualArgs[i] = eval.evaluate(getArg(i), c);
                if ((actualArgs[i] == null)) {
                    actualArgs[i] = Saxon.Hej.value.EmptySequence.getInstance();
                }
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
            if (((argumentEvaluators != null) && (getArity()>0))) {
                Saxon.Ejava.lang.StringBuilder fsb = new Saxon.Ejava.lang.StringBuilder(64);
                foreach (Saxon.Hej.expr.parser.Evaluator e in argumentEvaluators) {
                    fsb.append((e.getCode()+" "));
                }
                @out.emitAttribute("eval", Saxon.Hej.value.Whitespace.trim(fsb.ToString()));
            }
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
    
    }
}