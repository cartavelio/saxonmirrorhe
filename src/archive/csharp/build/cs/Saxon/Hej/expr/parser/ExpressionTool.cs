using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.parser {
    public class ExpressionTool {
        private ExpressionTool()  {
        }
        public static Saxon.Hej.expr.Expression make(string expression, Saxon.Hej.expr.StaticContext env, int start, int terminator, Saxon.Hej.expr.parser.CodeInjector codeInjector) {
            Saxon.Hej.expr.parser.XPathParser parser = env.getConfiguration().newExpressionParser("XP", false);
            if ((codeInjector != null)) {
                parser.setCodeInjector(codeInjector);
            }
            if ((terminator == -1)) {
                terminator = Saxon.Hej.expr.parser.Token.EOF;
            }
            Saxon.Hej.expr.Expression exp = parser.parse(expression, start, terminator, env);
            setDeepRetainedStaticContext(exp, env.makeRetainedStaticContext());
            exp = exp.simplify();
            return exp;
        }
        public static void setDeepRetainedStaticContext(Saxon.Hej.expr.Expression exp, Saxon.Hej.expr.parser.RetainedStaticContext rsc) {
            if ((exp.getLocalRetainedStaticContext() == null)) {
                exp.setRetainedStaticContextLocally(rsc);
            } else {
                rsc = exp.getLocalRetainedStaticContext();
            }
            foreach (Saxon.Hej.expr.Operand o in exp.operands()) {
                setDeepRetainedStaticContext(o.getChildExpression(), rsc);
            }
        }
        public static void copyLocationInfo(Saxon.Hej.expr.Expression from, Saxon.Hej.expr.Expression to) {
            if (((from != null) && (to != null))) {
                if (((to.getLocation() == null) || (to.getLocation() == Saxon.Hej.expr.parser.Loc.NONE))) {
                    to.setLocation(from.getLocation());
                }
                if ((to.getLocalRetainedStaticContext() == null)) {
                    to.setRetainedStaticContextLocally(from.getLocalRetainedStaticContext());
                }
            }
        }
        public static Saxon.Hej.expr.Expression unsortedIfHomogeneous(Saxon.Hej.expr.Expression exp, bool forStreaming) {
            if ((exp is Saxon.Hej.expr.Literal)) {
                return exp;
            }
            if ((exp.getItemType() is Saxon.Hej.type.AnyItemType)) {
                return exp;
            } else {
                return exp.unordered(false, forStreaming);
            }
        }
        public static Saxon.Hej.expr.Expression injectCode(Saxon.Hej.expr.Expression exp, Saxon.Hej.expr.parser.CodeInjector injector) {
            if ((exp is Saxon.Hej.expr.flwor.FLWORExpression)) {
                ((Saxon.Hej.expr.flwor.FLWORExpression)(exp)).injectCode(injector);
            } else {
                if ((exp is Saxon.Hej.expr.instruct.TraceExpression)) {
                    foreach (Saxon.Hej.expr.Operand o in ((Saxon.Hej.expr.instruct.TraceExpression)(exp)).getChild().operands()) {
                        if (!(o.getOperandRole().isConstrainedClass())) {
                            o.setChildExpression(injectCode(o.getChildExpression(), injector));
                        }
                    }
                } else {
                    foreach (Saxon.Hej.expr.Operand o in exp.operands()) {
                        if (!(o.getOperandRole().isConstrainedClass())) {
                            o.setChildExpression(injectCode(o.getChildExpression(), injector));
                        }
                    }
                }
            }
            return injector.inject(exp);
        }
        public static Saxon.Hej.om.GroundedValue eagerEvaluate(Saxon.Hej.expr.Expression exp, Saxon.Hej.expr.XPathContext context) {
            return (Saxon.Hej.om.GroundedValue)(exp.makeElaborator().eagerly().evaluate(context));
        }
        public static int markTailFunctionCalls(Saxon.Hej.expr.Expression exp, Saxon.Hej.om.StructuredQName qName, int arity) {
            return exp.markTailFunctionCalls(qName, arity);
        }
        public static string indent(int level) {
            Saxon.Ejava.lang.StringBuilder fsb = new Saxon.Ejava.lang.StringBuilder(level);
            for (int i = 0; (i<level); i++) {
                fsb.append("  ");
            }
            return fsb.ToString();
        }
        public static bool contains(Saxon.Hej.expr.Expression a, Saxon.Hej.expr.Expression b) {
            Saxon.Hej.expr.Expression temp = b;
            while ((temp != null)) {
                if ((temp == a)) {
                    return true;
                } else {
                    temp = temp.getParentExpression();
                }
            }
            return false;
        }
        public static bool containsLocalParam(Saxon.Hej.expr.Expression exp) {
            return contains(exp, true, e => (e is Saxon.Hej.expr.instruct.LocalParam));
        }
        public static bool containsLocalVariableReference(Saxon.Hej.expr.Expression exp) {
            return contains(exp, false, e => {
                if ((e is Saxon.Hej.expr.LocalVariableReference)) {
                    Saxon.Hej.expr.LocalVariableReference vref = (Saxon.Hej.expr.LocalVariableReference)(e);
                    Saxon.Hej.expr.LocalBinding binding = vref.getBinding();
                    return !((((binding is Saxon.Hej.expr.Expression) && contains(exp, (Saxon.Hej.expr.Expression)(binding)))));
                }
                return false;
            } );
        }
        public static bool contains(Saxon.Hej.expr.Expression exp, bool sameFocusOnly, System.Predicate<Saxon.Hej.expr.Expression> predicate) {
            if (predicate(exp)) {
                return true;
            }
            foreach (Saxon.Hej.expr.Operand info in exp.operands()) {
                if ((((info.hasSameFocus() || !(sameFocusOnly))) && contains(info.getChildExpression(), sameFocusOnly, predicate))) {
                    return true;
                }
            }
            return false;
        }
        public static bool changesXsltContext(Saxon.Hej.expr.Expression exp) {
            if ((((((((exp is Saxon.Hej.expr.instruct.ResultDocument) || (exp is Saxon.Hej.expr.instruct.CallTemplate)) || (exp is Saxon.Hej.expr.instruct.ApplyTemplates)) || (exp is Saxon.Hej.expr.instruct.NextMatch)) || (exp is Saxon.Hej.expr.instruct.ApplyImports)) || exp.isCallOn(typeof(Saxon.Hej.functions.RegexGroup))) || exp.isCallOn(typeof(Saxon.Hej.functions.CurrentGroup)))) {
                return true;
            }
            foreach (Saxon.Hej.expr.Operand o in exp.operands()) {
                if (changesXsltContext(o.getChildExpression())) {
                    return true;
                }
            }
            return false;
        }
        public static bool isLoopingSubexpression(Saxon.Hej.expr.Expression child, Saxon.Hej.expr.Expression ancestor) {
            while (true) {
                Saxon.Hej.expr.Expression parent = child.getParentExpression();
                if ((parent == null)) {
                    return false;
                }
                if (hasLoopingSubexpression(parent, child)) {
                    return true;
                }
                if ((parent == ancestor)) {
                    return false;
                }
                child = parent;
            }
        }
        public static bool isLoopingReference(Saxon.Hej.expr.VariableReference reference, Saxon.Hej.expr.Binding binding) {
            Saxon.Hej.expr.Expression child = reference;
            Saxon.Hej.expr.Expression parent = child.getParentExpression();
            while (true) {
                if ((parent == null)) {
                    return true;
                }
                if ((parent is Saxon.Hej.expr.flwor.FLWORExpression)) {
                    if (parent.hasVariableBinding(binding)) {
                        return ((Saxon.Hej.expr.flwor.FLWORExpression)(parent)).hasLoopingVariableReference(binding);
                    } else {
                        if (hasLoopingSubexpression(parent, child)) {
                            return true;
                        }
                    }
                } else {
                    if (parent.getExpressionName().Equals("tryCatch")) {
                        return true;
                    } else {
                        if (((parent is Saxon.Hej.expr.instruct.ForEachGroup) && parent.hasVariableBinding(binding))) {
                            return false;
                        }
                        if (hasLoopingSubexpression(parent, child)) {
                            return true;
                        }
                        if (parent.hasVariableBinding(binding)) {
                            return false;
                        }
                    }
                }
                child = parent;
                parent = child.getParentExpression();
            }
        }
        public static bool hasLoopingSubexpression(Saxon.Hej.expr.Expression parent, Saxon.Hej.expr.Expression child) {
            foreach (Saxon.Hej.expr.Operand info in parent.operands()) {
                if ((info.getChildExpression() == child)) {
                    return info.isEvaluatedRepeatedly();
                }
            }
            return false;
        }
        public static Saxon.Hej.expr.Expression getFocusSettingContainer(Saxon.Hej.expr.Expression exp) {
            Saxon.Hej.expr.Expression child = exp;
            Saxon.Hej.expr.Expression parent = child.getParentExpression();
            while ((parent != null)) {
                Saxon.Hej.expr.Operand o = findOperand(parent, child);
                if ((o == null)) {
                    throw new Saxon.Ejava.lang.AssertionError();
                }
                if (!(o.hasSameFocus())) {
                    return parent;
                }
                child = parent;
                parent = child.getParentExpression();
            }
            return null;
        }
        public static Saxon.Hej.expr.Expression getContextDocumentSettingContainer(Saxon.Hej.expr.Expression exp) {
            Saxon.Hej.expr.Expression child = exp;
            Saxon.Hej.expr.Expression parent = child.getParentExpression();
            while ((parent != null)) {
                if ((parent is Saxon.Hej.expr.ContextSwitchingExpression)) {
                    Saxon.Hej.expr.ContextSwitchingExpression switcher = (Saxon.Hej.expr.ContextSwitchingExpression)(parent);
                    if ((child == switcher.getActionExpression())) {
                        if (switcher.getSelectExpression().hasSpecialProperty(Saxon.Hej.expr.StaticProperty.CONTEXT_DOCUMENT_NODESET)) {
                            parent.resetLocalStaticProperties();
                            parent.getSpecialProperties();
                            return getContextDocumentSettingContainer(parent);
                        }
                    }
                }
                Saxon.Hej.expr.Operand o = findOperand(parent, child);
                if ((o == null)) {
                    throw new Saxon.Ejava.lang.AssertionError();
                }
                if (!(o.hasSameFocus())) {
                    return parent;
                }
                child = parent;
                parent = child.getParentExpression();
            }
            return null;
        }
        public static void resetStaticProperties(Saxon.Hej.expr.Expression exp) {
            int i = 0;
            while ((exp != null)) {
                exp.resetLocalStaticProperties();
                exp = exp.getParentExpression();
                if ((i++>100000)) {
                    throw new Saxon.Ejava.lang.IllegalStateException("Loop in parent expression chain");
                }
            }
        }
        public static int getAxisNavigation(Saxon.Hej.expr.Expression exp) {
            Saxon.Hej.expr.Expression unfiltered = unfilteredExpression(exp, true);
            if ((unfiltered is Saxon.Hej.expr.AxisExpression)) {
                return ((Saxon.Hej.expr.AxisExpression)(unfiltered)).getAxis();
            }
            if ((unfiltered is Saxon.Hej.expr.VennExpression)) {
                int v1 = getAxisNavigation(((Saxon.Hej.expr.VennExpression)(unfiltered)).getLhsExpression());
                int v2 = getAxisNavigation(((Saxon.Hej.expr.VennExpression)(unfiltered)).getRhsExpression());
                if ((v1 == v2)) {
                    return v1;
                }
            }
            return -1;
        }
        public static bool equalOrNull(System.Object x, System.Object y) {
            if ((x == null)) {
                return (y == null);
            } else {
                return x.Equals(y);
            }
        }
        public static Saxon.Hej.om.SequenceIterator getIteratorFromProcessMethod(Saxon.Hej.expr.Expression exp, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.Controller controller = context.getController();
            System.Diagnostics.Debug.Assert((controller != null));
            Saxon.Hej.@event.SequenceCollector seq = controller.allocateSequenceOutputter();
            exp.process(new Saxon.Hej.@event.ComplexContentOutputter(seq), context);
            seq.close();
            return seq.iterate();
        }
        public static int allocateSlots(Saxon.Hej.expr.Expression exp, int nextFree, Saxon.Hej.expr.instruct.SlotManager frame) {
            if ((exp is Saxon.Hej.expr.Assignation)) {
                ((Saxon.Hej.expr.Assignation)(exp)).setSlotNumber(nextFree);
                int count = ((Saxon.Hej.expr.Assignation)(exp)).getRequiredSlots();
                nextFree += count;
                if ((frame != null)) {
                    frame.allocateSlotNumber(((Saxon.Hej.expr.Assignation)(exp)).getVariableQName());
                }
            }
            if (((exp is Saxon.Hej.expr.instruct.LocalParam) && (((Saxon.Hej.expr.instruct.LocalParam)(exp)).getSlotNumber()<0))) {
                ((Saxon.Hej.expr.instruct.LocalParam)(exp)).setSlotNumber(nextFree++);
            }
            if ((exp is Saxon.Hej.expr.flwor.FLWORExpression)) {
                foreach (Saxon.Hej.expr.flwor.Clause c in ((Saxon.Hej.expr.flwor.FLWORExpression)(exp)).getClauseList()) {
                    foreach (Saxon.Hej.expr.flwor.LocalVariableBinding b in c.getRangeVariables()) {
                        b.setSlotNumber(nextFree++);
                        frame.allocateSlotNumber(b.getVariableQName());
                    }
                }
            }
            if ((exp is Saxon.Hej.expr.VariableReference)) {
                Saxon.Hej.expr.VariableReference var = (Saxon.Hej.expr.VariableReference)(exp);
                Saxon.Hej.expr.Binding binding = var.getBinding();
                if ((exp is Saxon.Hej.expr.LocalVariableReference)) {
                    ((Saxon.Hej.expr.LocalVariableReference)(var)).setSlotNumber(((Saxon.Hej.expr.LocalBinding)(binding)).getLocalSlotNumber());
                }
                if (((binding is Saxon.Hej.expr.Assignation) && (((Saxon.Hej.expr.LocalBinding)(binding)).getLocalSlotNumber()<0))) {
                    Saxon.Hej.expr.Assignation decl = (Saxon.Hej.expr.Assignation)(binding);
                    Saxon.Hej.lib.Logger err;
                    try {
                        err = exp.getConfiguration().getLogger();
                    } catch (System.Exception ex) {
                        err = new Saxon.Callbacks.StandardLogger();
                    }
                    string msg = "*** Internal Saxon error: local variable encountered whose binding has been deleted";
                    err.error(msg);
                    err.error(("Variable name: "+decl.getVariableName()));
                    err.error(((("Line number of reference: "+var.getLocation().getLineNumber())+" in ")+var.getLocation().getSystemId()));
                    err.error(((("Line number of declaration: "+decl.getLocation().getLineNumber())+" in ")+decl.getLocation().getSystemId()));
                    err.error("DECLARATION:");
                    try {
                        decl.explain(err);
                    } catch (System.Exception e) {
                    }
                    throw new Saxon.Ejava.lang.IllegalStateException(msg);
                }
            }
            if ((exp is Saxon.Hej.pattern.Pattern)) {
                nextFree = ((Saxon.Hej.pattern.Pattern)(exp)).allocateSlots(frame, nextFree);
            } else {
                if ((exp is Saxon.Hej.style.ScopedBindingElement)) {
                    nextFree = ((Saxon.Hej.style.ScopedBindingElement)(exp)).allocateSlots(frame, nextFree);
                } else {
                    foreach (Saxon.Hej.expr.Operand o in exp.operands()) {
                        nextFree = allocateSlots(o.getChildExpression(), nextFree, frame);
                    }
                }
            }
            return nextFree;
        }
        public static bool effectiveBooleanValue(Saxon.Hej.om.SequenceIterator iterator) {
            Saxon.Hej.om.Item first = iterator.next();
            if ((first == null)) {
                return false;
            }
            Saxon.Hej.om.Genre genre = first.getGenre();
            switch (genre) {
              case Saxon.Hej.om.Genre.NODE:
                iterator.close();
                return true;
              case Saxon.Hej.om.Genre.ATOMIC:
                {
                    if ((first is Saxon.Hej.value.BooleanValue)) {
                        if ((iterator.next() != null)) {
                            iterator.close();
                            ebvError("a sequence of two or more items starting with a boolean");
                        }
                        iterator.close();
                        return ((Saxon.Hej.value.BooleanValue)(first)).getBooleanValue();
                    } else {
                        if ((first is Saxon.Hej.value.StringValue)) {
                            if ((iterator.next() != null)) {
                                iterator.close();
                                ebvError((("a sequence of two or more items starting with a string ('"+first.getStringValue())+"')"));
                            }
                            return !(((Saxon.Hej.value.StringValue)(first)).isEmpty());
                        } else {
                            if ((first is Saxon.Hej.value.NumericValue)) {
                                if ((iterator.next() != null)) {
                                    iterator.close();
                                    ebvError((("a sequence of two or more items starting with a numeric value ("+first.getStringValue())+")"));
                                }
                                Saxon.Hej.value.NumericValue n = (Saxon.Hej.value.NumericValue)(first);
                                return (((n.compareTo(0) != 0)) && !(n.isNaN()));
                            } else {
                                iterator.close();
                                ebvError(("a sequence starting with an atomic value of type "+((Saxon.Hej.value.AtomicValue)(first)).getItemType().getTypeName().getDisplayName()));
                                return false;
                            }
                        }
                    }
                }
              case Saxon.Hej.om.Genre.ARRAY:
                iterator.close();
                ebvError((("a sequence starting with an array item ("+first.toShortString())+")"));
                return false;
              case Saxon.Hej.om.Genre.MAP:
                iterator.close();
                ebvError((("a sequence starting with a map ("+first.toShortString())+")"));
                return false;
              case Saxon.Hej.om.Genre.FUNCTION:
                {
                    iterator.close();
                    ebvError((("a sequence starting with a function ("+first.toShortString())+")"));
                    return false;
                }
              case Saxon.Hej.om.Genre.EXTERNAL:
                {
                    if ((iterator.next() != null)) {
                        iterator.close();
                        ebvError("a sequence of two or more items starting with an external object value");
                    }
                    return true;
                }
            }
            ebvError("a sequence starting with an item of unknown kind");
            return false;
        }
        public static bool effectiveBooleanValue(Saxon.Hej.om.Item item) {
            if ((item == null)) {
                return false;
            }
            if ((item is Saxon.Hej.om.NodeInfo)) {
                return true;
            } else {
                if ((item is Saxon.Hej.value.AtomicValue)) {
                    if ((item is Saxon.Hej.value.BooleanValue)) {
                        return ((Saxon.Hej.value.BooleanValue)(item)).getBooleanValue();
                    } else {
                        if ((item is Saxon.Hej.value.StringValue)) {
                            return !(((Saxon.Hej.value.StringValue)(item)).isEmpty());
                        } else {
                            if ((item is Saxon.Hej.value.NumericValue)) {
                                Saxon.Hej.value.NumericValue n = (Saxon.Hej.value.NumericValue)(item);
                                return (((n.compareTo(0) != 0)) && !(n.isNaN()));
                            } else {
                                if ((item.getGenre() == Saxon.Hej.om.Genre.EXTERNAL)) {
                                    return true;
                                } else {
                                    ebvError(("an atomic value of type "+((Saxon.Hej.value.AtomicValue)(item)).getPrimitiveType().getDisplayName()));
                                    return false;
                                }
                            }
                        }
                    }
                } else {
                    ebvError(item.getGenre().ToString());
                    return false;
                }
            }
        }
        public static void ebvError(string reason) {
            Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(("Effective boolean value is not defined for "+reason));
            err.setErrorCode("FORG0006");
            err.setIsTypeError(true);
            throw err;
        }
        public static void ebvError(string reason, Saxon.Hej.expr.Expression cause) {
            Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(("Effective boolean value is not defined for "+reason));
            err.setErrorCode("FORG0006");
            err.setIsTypeError(true);
            err.setFailingExpression(cause);
            throw err;
        }
        public static bool dependsOnFocus(Saxon.Hej.expr.Expression exp) {
            return (((exp.getDependencies()&Saxon.Hej.expr.StaticProperty.DEPENDS_ON_FOCUS)) != 0);
        }
        public static bool dependsOnVariable(Saxon.Hej.expr.Expression exp, Saxon.Hej.expr.Binding[] bindingList) {
            return (!((((bindingList == null) || (bindingList.Length == 0)))) && contains(exp, false, e => {
                if ((e is Saxon.Hej.expr.VariableReference)) {
                    foreach (Saxon.Hej.expr.Binding binding in bindingList) {
                        if ((((Saxon.Hej.expr.VariableReference)(e)).getBinding() == binding)) {
                            return true;
                        }
                    }
                }
                return false;
            } ));
        }
        public static void gatherReferencedVariables(Saxon.Hej.expr.Expression e, System.Collections.Generic.IList<Saxon.Hej.expr.Binding> list) {
            if ((e is Saxon.Hej.expr.VariableReference)) {
                Saxon.Hej.expr.Binding binding = ((Saxon.Hej.expr.VariableReference)(e)).getBinding();
                if (!(list.Contains(binding))) {
                    list.Add(binding);
                }
            } else {
                foreach (Saxon.Hej.expr.Operand o in e.operands()) {
                    if (!(o.getOperandRole().isInChoiceGroup())) {
                        gatherReferencedVariables(o.getChildExpression(), list);
                    }
                }
            }
        }
        public static bool refersToVariableOrFunction(Saxon.Hej.expr.Expression exp) {
            return contains(exp, false, e => ((((((((e is Saxon.Hej.expr.VariableReference) || (e is Saxon.Hej.expr.UserFunctionCall)) || (e is Saxon.Hej.expr.Binding)) || (e is Saxon.Hej.expr.instruct.CallTemplate)) || (e is Saxon.Hej.expr.instruct.ApplyTemplates)) || (e is Saxon.Hej.expr.instruct.ApplyImports)) || isCallOnSystemFunction(e, "function-lookup")) || e.isCallOn(typeof(Saxon.Hej.functions.ApplyFn))));
        }
        public static bool isCallOnSystemFunction(Saxon.Hej.expr.Expression e, string localName) {
            return ((e is Saxon.Hej.expr.StaticFunctionCall) && localName.Equals(((Saxon.Hej.expr.StaticFunctionCall)(e)).getFunctionName().getLocalPart()));
        }
        public static bool callsFunction(Saxon.Hej.expr.Expression exp, Saxon.Hej.om.StructuredQName qName, bool sameFocusOnly) {
            return contains(exp, sameFocusOnly, e => ((e is Saxon.Hej.expr.FunctionCall) && qName.Equals(((Saxon.Hej.expr.FunctionCall)(e)).getFunctionName())));
        }
        public static bool containsSubexpression(Saxon.Hej.expr.Expression exp, System.Type subClass) {
            return contains(exp, false, e => subClass.IsAssignableFrom(e.GetType()));
        }
        public static void gatherCalledFunctions(Saxon.Hej.expr.Expression e, System.Collections.Generic.IList<Saxon.Hej.expr.instruct.UserFunction> list) {
            if ((e is Saxon.Hej.expr.UserFunctionCall)) {
                Saxon.Hej.expr.instruct.UserFunction function = ((Saxon.Hej.expr.UserFunctionCall)(e)).getFunction();
                if (!(list.Contains(function))) {
                    list.Add(function);
                }
            } else {
                if ((e is Saxon.Hej.functions.hof.UserFunctionReference)) {
                    Saxon.Hej.expr.instruct.UserFunction function = ((Saxon.Hej.functions.hof.UserFunctionReference)(e)).getNominalTarget();
                    if (!(list.Contains(function))) {
                        list.Add(function);
                    }
                } else {
                    foreach (Saxon.Hej.expr.Operand o in e.operands()) {
                        gatherCalledFunctions(o.getChildExpression(), list);
                    }
                }
            }
        }
        public static void gatherCalledFunctionNames(Saxon.Hej.expr.Expression e, System.Collections.Generic.IList<Saxon.Hej.trans.SymbolicName> list) {
            if ((e is Saxon.Hej.expr.UserFunctionCall)) {
                list.Add(((Saxon.Hej.expr.UserFunctionCall)(e)).getSymbolicName());
            } else {
                foreach (Saxon.Hej.expr.Operand o in e.operands()) {
                    gatherCalledFunctionNames(o.getChildExpression(), list);
                }
            }
        }
        public static Saxon.Hej.expr.Expression optimizeComponentBody(Saxon.Hej.expr.Expression body, Saxon.Hej.style.Compilation compilation, Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo cisi, bool extractGlobals) {
            Saxon.Hej.expr.parser.Optimizer opt = visitor.obtainOptimizer();
            if (opt.isOptionSet(Saxon.Hej.expr.parser.OptimizerOptions.MISCELLANEOUS)) {
                Saxon.Hej.expr.parser.ExpressionTool.resetPropertiesWithinSubtree(body);
                if (opt.isOptionSet(Saxon.Hej.expr.parser.OptimizerOptions.MISCELLANEOUS)) {
                    body = body.optimize(visitor, cisi);
                }
                body.setParentExpression(null);
                if ((extractGlobals && (compilation != null))) {
                    Saxon.Hej.expr.Expression exp2 = opt.promoteExpressionsToGlobal(body, compilation.getPrincipalStylesheetModule(), visitor);
                    if ((exp2 != null)) {
                        Saxon.Hej.expr.parser.ExpressionTool.resetPropertiesWithinSubtree(exp2);
                        body = exp2.optimize(visitor, cisi);
                    }
                }
                if (opt.isOptionSet(Saxon.Hej.expr.parser.OptimizerOptions.LOOP_LIFTING)) {
                    body = Saxon.Hej.expr.parser.LoopLifter.process(body, visitor, cisi);
                }
            } else {
                body = avoidDocumentSort(body);
            }
            if (!(visitor.isOptimizeForStreaming())) {
                body = opt.eliminateCommonSubexpressions(body);
            }
            opt.prepareForStreaming(body);
            body.restoreParentPointers();
            return body;
        }
        private static Saxon.Hej.expr.Expression avoidDocumentSort(Saxon.Hej.expr.Expression exp) {
            if ((exp is Saxon.Hej.expr.sort.DocumentSorter)) {
                Saxon.Hej.expr.Expression @base = ((Saxon.Hej.expr.sort.DocumentSorter)(exp)).getBaseExpression();
                if (@base.hasSpecialProperty(Saxon.Hej.expr.StaticProperty.ORDERED_NODESET)) {
                    return @base;
                }
                return exp;
            } else {
                if ((exp is Saxon.Hej.expr.sort.ConditionalSorter)) {
                    Saxon.Hej.expr.sort.DocumentSorter sorter = ((Saxon.Hej.expr.sort.ConditionalSorter)(exp)).getDocumentSorter();
                    Saxon.Hej.expr.Expression eliminatedSorter = avoidDocumentSort(sorter);
                    if ((eliminatedSorter != sorter)) {
                        return eliminatedSorter;
                    }
                }
            }
            foreach (Saxon.Hej.expr.Operand o in exp.operands()) {
                o.setChildExpression(avoidDocumentSort(o.getChildExpression()));
            }
            return exp;
        }
        public static void computeEvaluationModesForUserFunctionCalls(Saxon.Hej.expr.Expression exp) {
            Saxon.Hej.expr.parser.ExpressionTool.processExpressionTree(exp, null, (expression, result) => {
                if ((expression is Saxon.Hej.expr.UserFunctionCall)) {
                    ((Saxon.Hej.expr.UserFunctionCall)(expression)).allocateArgumentEvaluators();
                }
                if ((expression is Saxon.Hej.expr.instruct.LocalParam)) {
                    ((Saxon.Hej.expr.instruct.LocalParam)(expression)).computeEvaluationMode();
                }
                return false;
            } );
        }
        public static void clearStreamabilityData(Saxon.Hej.expr.Expression exp) {
            Saxon.Hej.expr.parser.ExpressionTool.processExpressionTree(exp, null, (expression, result) => {
                expression.setExtraProperty("P+S", null);
                expression.setExtraProperty("inversion", null);
                return false;
            } );
        }
        public static void resetPropertiesWithinSubtree(Saxon.Hej.expr.Expression exp) {
            exp.resetLocalStaticProperties();
            if ((exp is Saxon.Hej.expr.LocalVariableReference)) {
                Saxon.Hej.expr.LocalVariableReference @ref = (Saxon.Hej.expr.LocalVariableReference)(exp);
                Saxon.Hej.expr.Binding binding = @ref.getBinding();
                if ((binding is Saxon.Hej.expr.Assignation)) {
                    binding.addReference(@ref, @ref.isInLoop());
                }
            }
            foreach (Saxon.Hej.expr.Operand o in exp.operands()) {
                resetPropertiesWithinSubtree(o.getChildExpression());
                o.getChildExpression().setParentExpression(exp);
            }
        }
        public static Saxon.Hej.expr.Expression resolveCallsToCurrentFunction(Saxon.Hej.expr.Expression exp) {
            if (exp.isCallOn(typeof(Saxon.Hej.functions.Current))) {
                Saxon.Hej.expr.ContextItemExpression cie = new Saxon.Hej.expr.ContextItemExpression();
                copyLocationInfo(exp, cie);
                return cie;
            } else {
                if (callsFunction(exp, Saxon.Hej.functions.Current.FN_CURRENT, true)) {
                    replaceTrivialCallsToCurrent(exp);
                }
                if (callsFunction(exp, Saxon.Hej.functions.Current.FN_CURRENT, false)) {
                    Saxon.Hej.expr.LetExpression let = new Saxon.Hej.expr.LetExpression();
                    let.setVariableQName(new Saxon.Hej.om.StructuredQName("vv", Saxon.Hej.om.NamespaceUri.SAXON_GENERATED_VARIABLE, ("current"+exp.GetHashCode())));
                    let.setRequiredType(Saxon.Hej.value.SequenceType.SINGLE_ITEM);
                    let.setSequence(new Saxon.Hej.expr.CurrentItemExpression());
                    replaceCallsToCurrent(exp, let);
                    let.setAction(exp);
                    return let;
                } else {
                    return exp;
                }
            }
        }
        public static void gatherVariableReferences(Saxon.Hej.expr.Expression exp, Saxon.Hej.expr.Binding binding, System.Collections.Generic.IList<Saxon.Hej.expr.VariableReference> list) {
            if (((exp is Saxon.Hej.expr.VariableReference) && (((Saxon.Hej.expr.VariableReference)(exp)).getBinding() == binding))) {
                list.Add((Saxon.Hej.expr.VariableReference)(exp));
            } else {
                foreach (Saxon.Hej.expr.Operand o in exp.operands()) {
                    gatherVariableReferences(o.getChildExpression(), binding, list);
                }
            }
        }
        public static bool processExpressionTree(Saxon.Hej.expr.Expression root, System.Object result, Saxon.Hej.expr.parser.ExpressionAction action) {
            bool done = /*Delegate.process()*/action(root, result);
            if (!(done)) {
                foreach (Saxon.Hej.expr.Operand o in root.operands()) {
                    done = processExpressionTree(o.getChildExpression(), result, action);
                    if (done) {
                        return true;
                    }
                }
            }
            return false;
        }
        public static bool replaceSelectedSubexpressions(Saxon.Hej.expr.Expression exp, System.Predicate<Saxon.Hej.expr.Expression> selector, Saxon.Hej.expr.Expression replacement, bool mustCopy) {
            bool replaced = false;
            foreach (Saxon.Hej.expr.Operand o in exp.operands()) {
                if (replaced) {
                    mustCopy = true;
                }
                Saxon.Hej.expr.Expression child = o.getChildExpression();
                if (selector(child)) {
                    Saxon.Hej.expr.Expression e2 = (mustCopy ? replacement.copy(new Saxon.Hej.expr.parser.RebindingMap()) : replacement);
                    o.setChildExpression(e2);
                    replaced = true;
                } else {
                    replaced |= replaceSelectedSubexpressions(child, selector, replacement, mustCopy);
                }
            }
            return replaced;
        }
        public static void replaceVariableReferences(Saxon.Hej.expr.Expression exp, Saxon.Hej.expr.Binding binding, Saxon.Hej.expr.Expression replacement, bool mustCopy) {
            System.Predicate<Saxon.Hej.expr.Expression> selector = child => ((child is Saxon.Hej.expr.VariableReference) && (((Saxon.Hej.expr.VariableReference)(child)).getBinding() == binding));
            bool changed = replaceSelectedSubexpressions(exp, selector, replacement, mustCopy);
            if (changed) {
                resetPropertiesWithinSubtree(exp);
            }
        }
        public static int getReferenceCount(Saxon.Hej.expr.Expression exp, Saxon.Hej.expr.Binding binding, bool inLoop) {
            int rcount = 0;
            if (((exp is Saxon.Hej.expr.VariableReference) && (((Saxon.Hej.expr.VariableReference)(exp)).getBinding() == binding))) {
                if (((Saxon.Hej.expr.VariableReference)(exp)).isFiltered()) {
                    return Saxon.Hej.expr.FilterExpression.FILTERED;
                } else {
                    rcount += (inLoop ? 10 : 1);
                }
            } else {
                if ((((exp.getDependencies()&Saxon.Hej.expr.StaticProperty.DEPENDS_ON_LOCAL_VARIABLES)) == 0)) {
                    return 0;
                } else {
                    foreach (Saxon.Hej.expr.Operand info in exp.operands()) {
                        Saxon.Hej.expr.Expression child = info.getChildExpression();
                        bool childLoop = (inLoop || info.isEvaluatedRepeatedly());
                        rcount += getReferenceCount(child, binding, childLoop);
                        if ((rcount>=Saxon.Hej.expr.FilterExpression.FILTERED)) {
                            break ;
                        }
                    }
                }
            }
            return rcount;
        }
        public static int expressionSize(Saxon.Hej.expr.Expression exp) {
            int total = 1;
            foreach (Saxon.Hej.expr.Operand o in exp.operands()) {
                total += expressionSize(o.getChildExpression());
                if ((o.getChildExpression() is Saxon.Hej.functions.hof.UserFunctionReference)) {
                    Saxon.Hej.expr.instruct.UserFunction uf = ((Saxon.Hej.functions.hof.UserFunctionReference)(o.getChildExpression())).getNominalTarget();
                    if ((uf.getFunctionName() == null)) {
                        total += expressionSize(uf.getBody());
                    }
                }
            }
            return total;
        }
        public static void rebindVariableReferences(Saxon.Hej.expr.Expression exp, Saxon.Hej.expr.Binding oldBinding, Saxon.Hej.expr.Binding newBinding) {
            if ((exp is Saxon.Hej.expr.VariableReference)) {
                if ((((Saxon.Hej.expr.VariableReference)(exp)).getBinding() == oldBinding)) {
                    ((Saxon.Hej.expr.VariableReference)(exp)).fixup(newBinding);
                }
            } else {
                foreach (Saxon.Hej.expr.Operand o in exp.operands()) {
                    rebindVariableReferences(o.getChildExpression(), oldBinding, newBinding);
                }
            }
        }
        public static Saxon.Hej.expr.Expression makePathExpression(Saxon.Hej.expr.Expression start, Saxon.Hej.expr.Expression step) {
            if ((((start is Saxon.Hej.expr.RootExpression) && (step is Saxon.Hej.expr.AxisExpression)) && (((Saxon.Hej.expr.AxisExpression)(step)).getAxis() == Saxon.Hej.om.AxisInfo.PARENT))) {
                return Saxon.Hej.expr.Literal.makeEmptySequence();
            }
            Saxon.Hej.expr.SlashExpression expr = new Saxon.Hej.expr.SlashExpression(start, step);
            if ((step is Saxon.Hej.expr.SlashExpression)) {
                Saxon.Hej.expr.SlashExpression stepPath = (Saxon.Hej.expr.SlashExpression)(step);
                if ((isFilteredAxisPath(stepPath.getSelectExpression()) && isFilteredAxisPath(stepPath.getActionExpression()))) {
                    expr.setStart(Saxon.Hej.expr.parser.ExpressionTool.makePathExpression(start, stepPath.getSelectExpression()));
                    expr.setStep(stepPath.getActionExpression());
                }
            }
            return expr;
        }
        public static Saxon.Hej.expr.Operand findOperand(Saxon.Hej.expr.Expression parentExpression, Saxon.Hej.expr.Expression childExpression) {
            foreach (Saxon.Hej.expr.Operand o in parentExpression.operands()) {
                if ((o.getChildExpression() == childExpression)) {
                    return o;
                }
            }
            return null;
        }
        private static bool isFilteredAxisPath(Saxon.Hej.expr.Expression exp) {
            return (unfilteredExpression(exp, true) is Saxon.Hej.expr.AxisExpression);
        }
        public static Saxon.Hej.expr.Expression unfilteredExpression(Saxon.Hej.expr.Expression exp, bool allowPositional) {
            if (((exp is Saxon.Hej.expr.FilterExpression) && ((allowPositional || !(((Saxon.Hej.expr.FilterExpression)(exp)).isFilterIsPositional()))))) {
                return unfilteredExpression(((Saxon.Hej.expr.FilterExpression)(exp)).getSelectExpression(), allowPositional);
            } else {
                if (((exp is Saxon.Hej.expr.TailExpression) && allowPositional)) {
                    return unfilteredExpression(((Saxon.Hej.expr.UnaryExpression)(exp)).getBaseExpression(), allowPositional);
                } else {
                    if (((exp is Saxon.Hej.expr.SingleItemFilter) && allowPositional)) {
                        return unfilteredExpression(((Saxon.Hej.expr.SingleItemFilter)(exp)).getBaseExpression(), allowPositional);
                    } else {
                        return exp;
                    }
                }
            }
        }
        public static Saxon.Hej.expr.Expression tryToFactorOutDot(Saxon.Hej.expr.Expression exp, Saxon.Hej.type.ItemType contextItemType) {
            if ((exp is Saxon.Hej.expr.ContextItemExpression)) {
                return null;
            } else {
                if (((exp is Saxon.Hej.expr.LetExpression) && (((Saxon.Hej.expr.LetExpression)(exp)).getSequence() is Saxon.Hej.expr.ContextItemExpression))) {
                    Saxon.Hej.expr.Expression action = ((Saxon.Hej.expr.LetExpression)(exp)).getAction();
                    bool changed = factorOutDot(action, (Saxon.Hej.expr.LetExpression)(exp));
                    if (changed) {
                        exp.resetLocalStaticProperties();
                    }
                    return exp;
                } else {
                    if ((((exp.getDependencies()&((Saxon.Hej.expr.StaticProperty.DEPENDS_ON_CONTEXT_ITEM|Saxon.Hej.expr.StaticProperty.DEPENDS_ON_CONTEXT_DOCUMENT)))) != 0)) {
                        Saxon.Hej.expr.LetExpression let = new Saxon.Hej.expr.LetExpression();
                        let.setVariableQName(new Saxon.Hej.om.StructuredQName("saxon", Saxon.Hej.om.NamespaceUri.SAXON, ("dot"+exp.GetHashCode())));
                        let.setRequiredType(Saxon.Hej.value.SequenceType.makeSequenceType(contextItemType, Saxon.Hej.expr.StaticProperty.EXACTLY_ONE));
                        let.setSequence(new Saxon.Hej.expr.ContextItemExpression());
                        Saxon.Hej.expr.Expression actionCopy = exp.copy(new Saxon.Hej.expr.parser.RebindingMap());
                        let.setAction(actionCopy);
                        bool changed = factorOutDot(actionCopy, let);
                        if (changed) {
                            return let;
                        } else {
                            return exp;
                        }
                    } else {
                        return null;
                    }
                }
            }
        }
        public static bool factorOutDot(Saxon.Hej.expr.Expression exp, Saxon.Hej.expr.Binding variable) {
            bool changed = false;
            if ((((exp.getDependencies()&((Saxon.Hej.expr.StaticProperty.DEPENDS_ON_CONTEXT_ITEM|Saxon.Hej.expr.StaticProperty.DEPENDS_ON_CONTEXT_DOCUMENT)))) != 0)) {
                foreach (Saxon.Hej.expr.Operand info in exp.operands()) {
                    if (info.hasSameFocus()) {
                        Saxon.Hej.expr.Expression child = info.getChildExpression();
                        if ((child is Saxon.Hej.expr.ContextItemExpression)) {
                            Saxon.Hej.expr.VariableReference @ref = makeReference(variable);
                            copyLocationInfo(child, @ref);
                            info.setChildExpression(@ref);
                            changed = true;
                        } else {
                            if (((child is Saxon.Hej.expr.AxisExpression) || (child is Saxon.Hej.expr.RootExpression))) {
                                Saxon.Hej.expr.VariableReference @ref = makeReference(variable);
                                copyLocationInfo(child, @ref);
                                Saxon.Hej.expr.Expression path = Saxon.Hej.expr.parser.ExpressionTool.makePathExpression(@ref, child);
                                info.setChildExpression(path);
                                changed = true;
                            } else {
                                changed |= factorOutDot(child, variable);
                            }
                        }
                    }
                }
            }
            if (changed) {
                exp.resetLocalStaticProperties();
            }
            return changed;
        }
        private static Saxon.Hej.expr.VariableReference makeReference(Saxon.Hej.expr.Binding variable) {
            if (variable.isGlobal()) {
                return new Saxon.Hej.expr.GlobalVariableReference((Saxon.Hej.expr.instruct.GlobalVariable)(variable));
            } else {
                return new Saxon.Hej.expr.LocalVariableReference((Saxon.Hej.expr.LocalBinding)(variable));
            }
        }
        public static bool inlineVariableReferences(Saxon.Hej.expr.Expression expr, Saxon.Hej.expr.Binding binding, Saxon.Hej.expr.Expression replacement) {
            return inlineVariableReferencesInternal(expr, binding, replacement);
        }
        public static bool inlineVariableReferencesInternal(Saxon.Hej.expr.Expression expr, Saxon.Hej.expr.Binding binding, Saxon.Hej.expr.Expression replacement) {
            if (((expr is Saxon.Hej.expr.TryCatch) && !(((replacement is Saxon.Hej.expr.Literal))))) {
                return false;
            } else {
                bool found = false;
                foreach (Saxon.Hej.expr.Operand o in expr.operands()) {
                    Saxon.Hej.expr.Expression child = o.getChildExpression();
                    if (((child is Saxon.Hej.expr.VariableReference) && (((Saxon.Hej.expr.VariableReference)(child)).getBinding() == binding))) {
                        Saxon.Hej.expr.Expression copy;
                        try {
                            copy = replacement.copy(new Saxon.Hej.expr.parser.RebindingMap());
                            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(child, copy);
                        } catch (Saxon.Ejava.lang.UnsupportedOperationException err) {
                            copy = replacement;
                        }
                        o.setChildExpression(copy);
                        found = true;
                    } else {
                        found |= inlineVariableReferencesInternal(child, binding, replacement);
                    }
                }
                if (found) {
                    expr.resetLocalStaticProperties();
                }
                return found;
            }
        }
        public static bool replaceTrivialCallsToCurrent(Saxon.Hej.expr.Expression expr) {
            bool found = false;
            foreach (Saxon.Hej.expr.Operand o in expr.operands()) {
                if (o.hasSameFocus()) {
                    Saxon.Hej.expr.Expression child = o.getChildExpression();
                    if (child.isCallOn(typeof(Saxon.Hej.functions.Current))) {
                        Saxon.Hej.expr.CurrentItemExpression var = new Saxon.Hej.expr.CurrentItemExpression();
                        Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(child, var);
                        o.setChildExpression(var);
                        found = true;
                    } else {
                        found = replaceTrivialCallsToCurrent(child);
                    }
                }
            }
            if (found) {
                expr.resetLocalStaticProperties();
            }
            return found;
        }
        public static bool replaceCallsToCurrent(Saxon.Hej.expr.Expression expr, Saxon.Hej.expr.LocalBinding binding) {
            bool found = false;
            foreach (Saxon.Hej.expr.Operand o in expr.operands()) {
                Saxon.Hej.expr.Expression child = o.getChildExpression();
                if (child.isCallOn(typeof(Saxon.Hej.functions.Current))) {
                    Saxon.Hej.expr.LocalVariableReference var = new Saxon.Hej.expr.LocalVariableReference(binding);
                    Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(child, var);
                    o.setChildExpression(var);
                    binding.addReference(var, true);
                    found = true;
                } else {
                    found = replaceCallsToCurrent(child, binding);
                }
            }
            if (found) {
                expr.resetLocalStaticProperties();
            }
            return found;
        }
        public static bool isNotAllowedInUpdatingContext(Saxon.Hej.expr.Expression exp) {
            return (!(exp.isUpdatingExpression()) && !(exp.isVacuousExpression()));
        }
        public static string getCurrentDirectory() {
            string dir;
            try {
                dir = Saxon.Ejava.lang.System.getProperty("user.dir");
            } catch (System.Exception geterr) {
                return null;
            }
            if (!(dir.EndsWith("/"))) {
                dir = (dir+'/');
            }
            System.Uri currentDirectoryURL = new Saxon.Ejava.io.File(dir).toURI();
            return currentDirectoryURL.ToString();
        }
        public static System.Uri getBaseURI(Saxon.Hej.expr.StaticContext env, Saxon.Ejavax.xml.transform.SourceLocator locator, bool fail) {
            System.Uri expressionBaseURI = null;
            string @base = null;
            try {
                @base = env.getStaticBaseURI();
                if ((@base == null)) {
                    @base = getCurrentDirectory();
                }
                if (((@base != null) && !(@base.Length==0))) {
                    expressionBaseURI = new System.Uri(@base, System.UriKind.RelativeOrAbsolute);
                }
            } catch (System.UriFormatException e) {
                string esc = Saxon.Hej.functions.IriToUri.iriToUri(Saxon.Hej.str.StringView.tidy(@base)).ToString();
                try {
                    expressionBaseURI = new System.Uri(esc, System.UriKind.RelativeOrAbsolute);
                } catch (System.UriFormatException e2) {
                    expressionBaseURI = null;
                }
                if (((expressionBaseURI == null) && fail)) {
                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException((("The base URI "+Saxon.Hej.trans.Err.wrap(env.getStaticBaseURI(), Saxon.Hej.trans.Err.URI))+" is not a valid URI"));
                    err.setLocator(locator);
                    throw err;
                }
            }
            return expressionBaseURI;
        }
        public static string parenthesize(Saxon.Hej.expr.Expression exp) {
            if (exp.operands().GetEnumerator().MoveNext()) {
                return (("("+exp.ToString())+")");
            } else {
                return exp.ToString();
            }
        }
        public static string parenthesizeShort(Saxon.Hej.expr.Expression exp) {
            if (hasTwoOrMoreOperands(exp)) {
                return (("("+exp.toShortString())+")");
            } else {
                return exp.toShortString();
            }
        }
        private static bool hasTwoOrMoreOperands(Saxon.Hej.expr.Expression exp) {
            System.Collections.Generic.IEnumerator<Saxon.Hej.expr.Operand> ops = exp.operands().GetEnumerator();
            if (!(ops.MoveNext())) {
                return false;
            }
            Saxon.Hej.expr.Operand o = ops.Current;
            return ops.MoveNext();
        }
        public static void validateTree(Saxon.Hej.expr.Expression exp) {
            try {
                foreach (Saxon.Hej.expr.Operand o in exp.checkedOperands()) {
                    validateTree(o.getChildExpression());
                }
            } catch (Saxon.Ejava.lang.IllegalStateException e) {
                System.Console.Error.WriteLine(e);
            }
        }
        public static bool isLocalConstructor(Saxon.Hej.expr.Expression child) {
            if (!((((child is Saxon.Hej.expr.instruct.ParentNodeConstructor) || (child is Saxon.Hej.expr.instruct.SimpleNodeConstructor))))) {
                return false;
            }
            Saxon.Hej.expr.Expression parent = child.getParentExpression();
            while ((parent != null)) {
                if ((parent is Saxon.Hej.expr.instruct.ParentNodeConstructor)) {
                    return true;
                }
                Saxon.Hej.expr.Operand o = findOperand(parent, child);
                System.Diagnostics.Debug.Assert((o != null));
                if ((o.getUsage() != Saxon.Hej.expr.OperandUsage.TRANSMISSION)) {
                    return false;
                }
                child = parent;
                parent = parent.getParentExpression();
            }
            return false;
        }
    
    }
}