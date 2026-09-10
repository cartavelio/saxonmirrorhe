////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2022 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.expr;

import net.sf.saxon.event.Outputter;
import net.sf.saxon.event.ReceiverOption;
import net.sf.saxon.expr.elab.*;
import net.sf.saxon.expr.instruct.TailCall;
import net.sf.saxon.expr.instruct.UserFunction;
import net.sf.saxon.expr.parser.*;
import net.sf.saxon.om.*;
import net.sf.saxon.trace.ExpressionPresenter;
import net.sf.saxon.trans.*;
import net.sf.saxon.tree.iter.EmptyIterator;
import net.sf.saxon.type.AnyItemType;
import net.sf.saxon.type.ItemType;
import net.sf.saxon.type.UType;
import net.sf.saxon.value.Cardinality;
import net.sf.saxon.value.SequenceType;

import java.util.ArrayList;
import java.util.List;


/**
 * This class represents a call to a user-defined function in the stylesheet or query.
 */

public class UserFunctionCall extends FunctionCall implements UserFunctionResolvable, ComponentInvocation, ContextOriginator {

    private SequenceType staticType;
    private UserFunction function;
    private int bindingSlot = -1;
    private int tailCall = NOT_TAIL_CALL;
    private StructuredQName name;

    public boolean isBeingInlined() {
        return beingInlined;
    }

    public void setBeingInlined(boolean beingInlined) {
        this.beingInlined = beingInlined;
    }

    private boolean beingInlined = false;


    // Only used in cases that don't yet use elaboration, e.g. streaming and XQuery Update
    private SequenceEvaluator[] argumentEvaluators = null;

    public static final int NOT_TAIL_CALL = 0;
    public static final int FOREIGN_TAIL_CALL = 1;
    public static final int SELF_TAIL_CALL = 2;

    /**
     * Create a function call to a user-written function in a query or stylesheet
     */

    public UserFunctionCall() {
    }


    /**
     * Set the name of the function being called
     *
     * @param name the name of the function
     */

    public final void setFunctionName(StructuredQName name) {
        this.name = name;
    }


    /**
     * Set the static type
     *
     * @param type the static type of the result of the function call
     */

    public void setStaticType(SequenceType type) {
        staticType = type;
    }

    /**
     * Create the reference to the function to be called
     *
     * @param compiledFunction the function being called
     */

    @Override
    public void setFunction(UserFunction compiledFunction) {
        function = compiledFunction;
    }

    /**
     * Set the binding slot to be used. This is the offset within the binding vector of the containing
     * component where the actual target template is to be found. The target function is not held directly
     * in the UserFunctionCall expression itself because it can be overridden in a using package.
     *
     * @param slot the offset in the binding vector of the containing package where the target template
     *             can be found.
     */


    @Override
    public void setBindingSlot(int slot) {
        this.bindingSlot = slot;
    }

    /**
     * Get the binding slot to be used. This is the offset within the binding vector of the containing
     * component where the actual target template is to be found.
     *
     * @return the offset in the binding vector of the containing package where the target template
     * can be found.
     */

    @Override
    public int getBindingSlot() {
        return bindingSlot;
    }


    /**
     * Get the function that is being called by this function call. This is the provisional
     * binding: the actual function might be an override of this one.
     *
     * @return the function being called
     */

    public UserFunction getFunction() {
        return function;
    }

    @Override
    public Component getFixedTarget() {
        Visibility v = function.getDeclaringComponent().getVisibility();
        if (v == Visibility.PRIVATE || v == Visibility.FINAL) {
            return function.getDeclaringComponent();
        } else {
            return null;
        }
    }

    /**
     * Determine whether this is a tail call (not necessarily a recursive tail call)
     *
     * @return true if this function call is a tail call
     */

    public boolean isTailCall() {
        return tailCall != NOT_TAIL_CALL;
    }

    public boolean isRecursiveTailCall() {
        return tailCall == SELF_TAIL_CALL;
    }

    /**
     * Get the qualified of the function being called
     *
     * @return the qualified name
     */

    @Override
    public final StructuredQName getFunctionName() {
        if (name == null) {
            return function.getFunctionName();
        } else {
            return name;
        }
    }

    @Override
    public SymbolicName getSymbolicName() {
        return new SymbolicName.F(getFunctionName(), getArity());
    }

    public Component getTarget() {
        return function.getDeclaringComponent();
    }


    private static final int UNHANDLED_DEPENDENCIES =
        StaticProperty.DEPENDS_ON_POSITION | StaticProperty.DEPENDS_ON_LAST |
            StaticProperty.DEPENDS_ON_XSLT_CONTEXT | StaticProperty.DEPENDS_ON_USER_FUNCTIONS;

    public void allocateArgumentEvaluators() {
        // Retained for paths such as streaming, bytecode generation, XQuery updated, that have
        // not yet converted to use elaboration mechanisms.
        argumentEvaluators = new SequenceEvaluator[getArity()];
        int i=0;
        for (Operand o : operands()) {
            Expression arg = o.getChildExpression();
            SequenceType required = function.getArgumentType(i);
            int cardinality = required.getCardinality();

            if (i == 0 && function.getDeclaredStreamability().isConsuming()) {
                argumentEvaluators[i] =
                        context -> context.getConfiguration().obtainOptimizer().evaluateStreamingArgument(arg, context);
            } else if (function.getParameterDefinitions()[i].isIndexedVariable()) {
                PullEvaluator argPull = arg.makeElaborator().elaborateForPull();
                argumentEvaluators[i] =
                        context -> context.getConfiguration().obtainOptimizer().makeIndexedValue(argPull.iterate(context));
            } else {
                argumentEvaluators[i] = arg.makeElaborator().lazily(true);
            }
            i++;
        }
    }

    public SequenceEvaluator[] getArgumentEvaluators() {
        return argumentEvaluators;
    }

//    public static class ArgumentEvaluationModeAdjuster implements Consumer<MemoClosure.MemoClosureStatistics> {
//
//        private final UserFunctionCall functionCall;
//        private final int argument;
//        private int completed;
//        private int abandoned;
//
//        public ArgumentEvaluationModeAdjuster(UserFunctionCall functionCall, int argument) {
//            this.functionCall = functionCall;
//            this.argument = argument;
//        }
//
//        @Override
//        public void accept(MemoClosure.MemoClosureStatistics statistics) {
//            if (functionCall.argumentEvaluators[argument].getCode() == Evaluators.MAKE_MEMO_CLOSURE) {
//                if (statistics.allRead) {
//                    completed++;
//                } else {
//                    abandoned++;
//                }
//                if (completed > 10 && abandoned == 0) {
//                    //System.err.println("Argument evaluator set to eager");
//                    functionCall.argumentEvaluators[argument] = Evaluator.EagerSequence.INSTANCE;
//                }
//            }
//        }
//
//    }


    /**
     * Pre-evaluate a function at compile time. This version of the method suppresses
     * early evaluation by doing nothing.
     *
     * @param visitor an expression visitor
     */

    @Override
    public Expression preEvaluate(ExpressionVisitor visitor) {
        return this;
    }

    /**
     * Determine the data type of the expression, if possible
     *
     * @return Type.ITEM (meaning not known in advance)
     */

    /*@NotNull*/
    @Override
    public ItemType getItemType() {
        if (staticType == null) {
            // the actual type is not known yet, so we return an approximation
            return AnyItemType.getInstance();
        } else {
            return staticType.getPrimaryType();
        }
    }

    /**
     * Get the static type of the expression as a UType, following precisely the type
     * inference rules defined in the XSLT 3.0 specification.
     *
     * @param contextItemType the static type of the context item
     * @return the static item type of the expression according to the XSLT 3.0 defined rules
     */
    @Override
    public UType getStaticUType(UType contextItemType) {
        UserFunction f = getFunction();
        if (f == null) {
            // Happens when called during parsing
            return UType.ANY;
        }
        return f.getResultType().getPrimaryType().getUType();
    }

    @Override
    public int getIntrinsicDependencies() {
        return StaticProperty.DEPENDS_ON_USER_FUNCTIONS;
    }

    /**
     * Determine whether this is an updating expression as defined in the XQuery update specification
     *
     * @return true if this is an updating expression
     */

    @Override
    public boolean isUpdatingExpression() {
        return function.isUpdating();
    }

    /**
     * Compute the special properties of this expression. These properties are denoted by a bit-significant
     * integer, possible values are in class {@link net.sf.saxon.expr.StaticProperty}. The "special" properties are properties
     * other than cardinality and dependencies, and most of them relate to properties of node sequences, for
     * example whether the nodes are in document order.
     *
     * @return the special properties, as a bit-significant integer
     */

    @Override
    protected int computeSpecialProperties() {
        // Inherit the properties of the function being called if possible. But we have to prevent
        // looping when the function is recursive. For safety, we only consider the properties of the
        // function body if it contains no further function calls. Also, we can only do this safely if
        // the function is private or final
        if (function == null) {
            return super.computeSpecialProperties();
        } else if (function.getBody() != null &&
                (function.getDeclaredVisibility() == Visibility.PRIVATE || function.getDeclaredVisibility() == Visibility.FINAL)) {
            int props;
            List<UserFunction> calledFunctions = new ArrayList<>();
            ExpressionTool.gatherCalledFunctions(function.getBody(), calledFunctions);
            if (calledFunctions.isEmpty()) {
                props = function.getBody().getSpecialProperties();
            } else {
                props = super.computeSpecialProperties();
            }
            if (function.getDeterminism() != UserFunction.Determinism.PROACTIVE) {
                props |= StaticProperty.NO_NODES_NEWLY_CREATED;
            }
            return props;
        } else {
            return super.computeSpecialProperties();
        }
    }

    /**
     * Copy an expression. This makes a deep copy.
     *
     * @param rebindings variable bindings that need to be changed
     * @return the copy of the original expression
     */

    /*@NotNull*/
    @Override
    public Expression copy(RebindingMap rebindings) {
        if (function == null) {
            // not bound yet, we have no way to register the new copy with the XSLFunction
            throw new UnsupportedOperationException("UserFunctionCall.copy()");
        }
        UserFunctionCall ufc = new UserFunctionCall();
        ufc.setFunction(function);
        ufc.setStaticType(staticType);
        int numArgs = getArity();
        Expression[] a2 = new Expression[numArgs];
        for (int i = 0; i < numArgs; i++) {
            a2[i] = getArg(i).copy(rebindings);
        }
        ufc.setArguments(a2);
        ExpressionTool.copyLocationInfo(this, ufc);
        return ufc;
    }

    /**
     * Determine the cardinality of the result
     */

    @Override
    protected int computeCardinality() {
        if (staticType == null) {
            // the actual type is not known yet, so we return an approximation
            return StaticProperty.ALLOWS_ZERO_OR_MORE;
        } else {
            return staticType.getCardinality();
        }
    }

    /*@NotNull*/
    @Override
    public Expression typeCheck(ExpressionVisitor visitor, ContextItemStaticInfo contextInfo) throws XPathException {
        Expression e = super.typeCheck(visitor, contextInfo);
        if (e != this) {
            return e;
        }
        if (function != null) {
            checkFunctionCall(function, visitor);
            if (staticType == null || staticType == SequenceType.ANY_SEQUENCE) {
                // try to get a better type
                staticType = function.getResultType();
            }
        }
        return this;
    }

    /*@NotNull*/
    @Override
    public Expression optimize(ExpressionVisitor visitor, ContextItemStaticInfo contextItemType) throws XPathException {
        Expression e = super.optimize(visitor, contextItemType);
        if (e == this && function != null) {
            return visitor.obtainOptimizer().tryInlineFunctionCall(
                    this, visitor, contextItemType);
        }
        return e;
    }

    /**
     * Reset the static properties of the expression to -1, so that they have to be recomputed
     * next time they are used.
     */
    @Override
    public void resetLocalStaticProperties() {
        super.resetLocalStaticProperties();
        //argumentEvaluators = null;
    }
    
    /**
     * Add a representation of this expression to a PathMap. The PathMap captures a map of the nodes visited
     * by an expression in a source tree.
     * <p>The default implementation of this method assumes that an expression does no navigation other than
     * the navigation done by evaluating its subexpressions, and that the subexpressions are evaluated in the
     * same context as the containing expression. The method must be overridden for any expression
     * where these assumptions do not hold. For example, implementations exist for AxisExpression, ParentExpression,
     * and RootExpression (because they perform navigation), and for the doc(), document(), and collection()
     * functions because they create a new navigation root. Implementations also exist for PathExpression and
     * FilterExpression because they have subexpressions that are evaluated in a different context from the
     * calling expression.</p>
     *
     * @param pathMap        the PathMap to which the expression should be added
     * @param pathMapNodeSet the PathMapNodeSet to which the paths embodied in this expression should be added
     * @return the pathMapNode representing the focus established by this expression, in the case where this
     * expression is the first operand of a path expression or filter expression. For an expression that does
     * navigation, it represents the end of the arc in the path map that describes the navigation route. For other
     * expressions, it is the same as the input pathMapNode.
     */

    @Override
    public PathMap.PathMapNodeSet addToPathMap(PathMap pathMap, PathMap.PathMapNodeSet pathMapNodeSet) {
        return addExternalFunctionCallToPathMap(pathMap, pathMapNodeSet);
    }

    /**
     * Mark tail-recursive calls on stylesheet functions. This marks the function call as tailRecursive if
     * if is a call to the containing function, and in this case it also returns "true" to the caller to indicate
     * that a tail call was found.
     */

    @Override
    public int markTailFunctionCalls(StructuredQName qName, int arity) {
        tailCall = getFunctionName().equals(qName) &&
                arity == getArity() ? SELF_TAIL_CALL : FOREIGN_TAIL_CALL;
        return tailCall;
    }

    @Override
    public int getImplementationMethod() {
        if (Cardinality.allowsMany(getCardinality())) {
            return ITERATE_METHOD | PROCESS_METHOD;
        } else {
            return EVALUATE_METHOD;
        }
    }

    /**
     * Call the function, returning the value as an item. This method will be used
     * only when the cardinality is zero or one. If the function is tail recursive,
     * it returns an Object representing the arguments to the next (recursive) call
     */

    @Override
    public Item evaluateItem(XPathContext c) throws XPathException {
        return makeElaborator().elaborateForItem().eval(c);
    }

    /**
     * Call the function, returning an iterator over the results. (But if the function is
     * tail recursive, it returns an iterator over the arguments of the recursive call)
     */

    /*@NotNull*/
    @Override
    public SequenceIterator iterate(XPathContext c) throws XPathException {
        return makeElaborator().elaborateForPull().iterate(c);
    }


    private void requestTailCall(XPathContext context, Sequence[] actualArgs) throws XPathException {
        if (bindingSlot >= 0) {
            TailCallLoop.TailCallComponent info = new TailCallLoop.TailCallComponent();
            Component target = getTargetComponent(context);
            info.component = target;
            info.function = (UserFunction) target.getActor();
            if (target.isHiddenAbstractComponent()) {
                throw new XPathException("Cannot call an abstract function (" +
                                                 name.getDisplayName() +
                                                 ") with no implementation", "XTDE3052");
            }
            ((XPathContextMajor) context).requestTailCall(info, actualArgs);

        } else {
            TailCallLoop.TailCallFunction info = new TailCallLoop.TailCallFunction();
            info.function = function;
            ((XPathContextMajor) context).requestTailCall(info, actualArgs);
        }

    }

    /**
     * Process the function call in push mode
     *
     *
     * @param output the destination for the result
     * @param context the XPath dynamic context
     * @throws XPathException if a dynamic error occurs
     */

    @Override
    public void process(Outputter output, XPathContext context) throws XPathException {
        TailCall tc = makeElaborator().elaborateForPush().processLeavingTail(output, context);
        dispatchTailCall(tc);
    }

    public Component getTargetComponent(XPathContext context) {
        if (bindingSlot == -1) {
            // fallback for non-package code
            return function.getDeclaringComponent();
        } else {
            return context.getTargetComponent(bindingSlot);
        }
    }

    @Override
    public UserFunction getTargetFunction(XPathContext context) {
        return (UserFunction) getTargetComponent(context).getActor();
    }



    public Sequence[] evaluateArguments(XPathContext c, boolean streamed) throws XPathException {
        int numArgs = getArity();
        Sequence[] actualArgs = SequenceTool.makeSequenceArray(numArgs);
        synchronized(this) {
            if (argumentEvaluators == null) {
                allocateArgumentEvaluators();
            }
        }
        for (int i = 0; i < numArgs; i++) {
            SequenceEvaluator eval = argumentEvaluators[i];
//            if (eval == null || eval == Evaluator.StreamingArgument.INSTANCE && !streamed) {
//                eval = Evaluator.EagerSequence.INSTANCE;
//            }
            actualArgs[i] = eval.evaluate(c);
//            if (actualArgs[i] == null) {
//                actualArgs[i] = EmptySequence.getInstance();
//            }
//            if (eval instanceof Evaluator.MemoClosureEvaluator && actualArgs[i] instanceof MemoClosure) {
//                ((MemoClosure)actualArgs[i]).setBindingExpression(((Evaluator.MemoClosureEvaluator)eval).statisticsGatherer);
//            }
        }
        return actualArgs;
    }

    /**
     * Diagnostic print of expression structure. The abstract expression tree
     * is written to the supplied output destination.
     */

    @Override
    public void export(ExpressionPresenter out) throws XPathException {
        out.startElement("ufCall", this);
        if (getFunctionName() != null) {
            out.emitAttribute("name", getFunctionName());
            out.emitAttribute("tailCall",
                              tailCall == NOT_TAIL_CALL ? "false" : tailCall == SELF_TAIL_CALL ? "self" : "foreign");
        }
        out.emitAttribute("bSlot", "" + getBindingSlot());
        for (Operand o : operands()) {
            o.getChildExpression().export(out);
        }
        if (getFunctionName() == null) {
            out.setChildRole("inline");
            function.getBody().export(out);
            out.endElement();
        }
        out.endElement();
    }

    /**
     * Get a name identifying the kind of expression, in terms meaningful to a user.
     *
     * @return a name identifying the kind of expression, in terms meaningful to a user.
     * The name will always be in the form of a lexical XML QName, and should match the name used
     * in explain() output displaying the expression.
     */
    @Override
    public String getExpressionName() {
        return "userFunctionCall";
    }

    @Override
    public Object getProperty(String name) {
        if (name.equals("target")) {
            return function;
        }
        return super.getProperty(name);
    }

    @Override
    public StructuredQName getObjectName() {
        return getFunctionName();
    }

    @Override
    public Elaborator getElaborator() {
        if (isTailCall()) {
            return new TailCallElaborator();
        } else {
            return new UserFunctionCallElaborator();
        }
    }
    public static abstract class UserFunctionCallAbstractElaborator extends FunctionCallElaborator {

        @Override
        public void allocateArgumentEvaluators(FunctionCall expr, boolean allowRepeatedUse) {
            argumentEvaluators = new SequenceEvaluator[expr.getArity()];
            UserFunction target = ((UserFunctionCall)expr).getFunction();
            int i = 0;
            for (Operand o : expr.operands()) {
                Expression arg = o.getChildExpression();
                if (target.getParameterDefinitions()[i].isIndexedVariable()) {
                    Optimizer opt = expr.getConfiguration().obtainOptimizer();
                    argumentEvaluators[i] =
                            context -> opt.makeIndexedValue(arg.iterate(context));
                } else {
                    argumentEvaluators[i] = arg.makeElaborator().lazily(allowRepeatedUse);
                }
                i++;
            }
        }

    }

    private static class TailCallElaborator extends UserFunctionCallAbstractElaborator {

        public PullEvaluator elaborateForPull() {
            final UserFunctionCall expr = (UserFunctionCall) getExpression();

            if (expr.bindingSlot >= 0) {
                return context -> {
                    TailCallLoop.TailCallComponent info = new TailCallLoop.TailCallComponent();
                    Component target = expr.getTargetComponent(context);
                    info.component = target;
                    info.function = (UserFunction) target.getActor();
                    if (target.isHiddenAbstractComponent()) {
                        throw new XPathException("Cannot call an abstract function (" +
                                                         expr.getFunctionName().getDisplayName() +
                                                         ") with no implementation", "XTDE3052");
                    }
                    Sequence[] actualArgs = evaluateArguments(context);
                    ((XPathContextMajor) context).requestTailCall(info, actualArgs);
                    return EmptyIterator.emptyIterator();
                };
            } else {
                TailCallLoop.TailCallFunction info = new TailCallLoop.TailCallFunction();
                info.function = expr.getFunction();
                return context -> {
                    Sequence[] actualArgs = evaluateArguments(context);
                    ((XPathContextMajor) context).requestTailCall(info, actualArgs);
                    return EmptyIterator.emptyIterator();
                };
            }
        }
    }

    private static class UserFunctionCallElaborator extends UserFunctionCallAbstractElaborator {

        private Sequence callTargetFunction(
                UserFunctionCall expr, Component target, XPathContext context) throws XPathException {
            try {
                Sequence[] actualArgs = evaluateArguments(context);
                UserFunction targetFunction = (UserFunction) target.getActor();
                XPathContextMajor c2 = targetFunction.makeNewContext(context, expr);
                c2.setCurrentComponent(target);
                return targetFunction.call(c2, actualArgs);
            } catch (UncheckedXPathException e) {
                XPathException xe = e.getXPathException();
                xe.maybeSetLocation(expr.getLocation());
                throw xe;
            } catch (StackOverflowError err) {
                throw new XPathException.StackOverflow("Too many nested function calls. May be due to infinite recursion",
                                                       SaxonErrorCode.SXLM0001, expr.getLocation());
            }
        }

        private Sequence callTargetFunction(
                UserFunctionCall expr, UserFunction targetFunction, XPathContext context) throws XPathException {
            try {
                Sequence[] actualArgs = evaluateArguments(context);
                XPathContextMajor c2 = targetFunction.makeNewContext(context, expr);
                return targetFunction.call(c2, actualArgs);
            } catch (UncheckedXPathException e) {
                XPathException xe = e.getXPathException();
                xe.maybeSetLocation(expr.getLocation());
                throw xe;
            } catch (StackOverflowError err) {
                throw new XPathException.StackOverflow("Too many nested function calls. May be due to infinite recursion",
                                                       SaxonErrorCode.SXLM0001, expr.getLocation());
            }
        }
        /**
         * Get a function that evaluates the underlying expression in the form of
         * a {@link SequenceIterator}
         *
         * @return an evaluator for the expression that returns a {@link SequenceIterator}
         */
        @Override
        public PullEvaluator elaborateForPull() {
            final UserFunctionCall expr = (UserFunctionCall) getExpression();

            if (expr.bindingSlot >= 0) {
                return context -> {
                    Component target = expr.getTargetComponent(context);
                    if (target.isHiddenAbstractComponent()) {
                        throw new XPathException("Cannot call an abstract function (" +
                                                         expr.getFunctionName().getDisplayName() +
                                                         ") with no implementation", "XTDE3052");
                    }
                    SequenceIterator result = callTargetFunction(expr, target, context).iterate();
                    return result;
                };
            } else {
                // Non-package case (XQuery)
                UserFunction targetFunction = expr.getFunction();
                return context -> {
                    try {
                        return callTargetFunction(expr, targetFunction, context).iterate();
                    } catch (UncheckedXPathException e) {
                        XPathException xpe = e.getXPathException();
                        xpe.maybeSetContext(context);
                        xpe.maybeSetLocation(expr.getLocation());
                        throw xpe;
                    }
                };
            }

        }

        /**
         * Get a function that evaluates the underlying expression in push mode, by
         * writing events to an {@link Outputter}
         *
         * @return an evaluator for the expression in push mode
         */
        @Override
        public PushEvaluator elaborateForPush() {

            final UserFunctionCall expr = (UserFunctionCall) getExpression();

            if (Cardinality.allowsMany(expr.getCardinality())) {
                PullEvaluator pullEval = elaborateForPull();
                return (output, context) -> {
                    SequenceIterator iter = pullEval.iterate(context);
                    SequenceTool.supply(iter, (ItemConsumer<? super Item>) it ->
                            output.append(it, expr.getLocation(), ReceiverOption.ALL_NAMESPACES));
                    return null;
                };
            } else {
                ItemEvaluator itemEval = elaborateForItem();
                return (output, context) -> {
                    Item item = itemEval.eval(context);
                    if (item != null) {
                        output.append(item, expr.getLocation(), ReceiverOption.ALL_NAMESPACES);
                    }
                    return null;
                };
            }

        }

        /**
         * Get a function that evaluates the underlying expression in the form of
         * a {@link Item}. This must only be called for expressions whose result
         * has cardinality zero or one.
         *
         * @return an evaluator for the expression that returns an {@link Item}.
         */
        @Override
        public ItemEvaluator elaborateForItem() {

            final UserFunctionCall expr = (UserFunctionCall) getExpression();

            if (expr.bindingSlot >= 0) {
                return context -> {
                    Component target = expr.getTargetComponent(context);
                    if (target.isHiddenAbstractComponent()) {
                        throw new XPathException("Cannot call an abstract function (" +
                                                         expr.getFunctionName().getDisplayName() +
                                                         ") with no implementation", "XTDE3052");
                    }
                    return callTargetFunction(expr, target, context).head();
                };
            } else {
                // Non-package case (XQuery)
                UserFunction targetFunction = expr.getFunction();
                return context -> callTargetFunction(expr, targetFunction, context).head();
            }

        }

        @Override
        public UpdateEvaluator elaborateForUpdate() {
            final UserFunctionCall expr = (UserFunctionCall) getExpression();
            UserFunction targetFunction = expr.getFunction(); // This is XQuery: target function is known
            return (context, pul) -> {
                Sequence[] actualArgs = evaluateArguments(context);
                XPathContextMajor c2 = context.newCleanContext();
                c2.setOrigin(expr);
                targetFunction.callUpdating(actualArgs, c2, pul);
            };
        }
    }
}