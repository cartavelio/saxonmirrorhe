using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.instruct {
    public class UserFunction : Saxon.Hej.expr.instruct.Actor, Saxon.Hej.om.Function, Saxon.Hej.expr.ContextOriginator, Saxon.Hej.trace.TraceableComponent {
        public enum Determinism {
            DETERMINISTIC,
            PROACTIVE,
            ELIDABLE,
        }
        private const int MAX_INLININGS = 100;
        private Saxon.Hej.om.StructuredQName functionName;
        private bool tailCalls = false;
        private bool tailRecursive = false;
        private Saxon.Hej.expr.instruct.UserFunctionParameter[] parameterDefinitions;
        private Saxon.Hej.value.SequenceType resultType;
        private Saxon.Hej.value.SequenceType declaredResultType;
        protected internal Saxon.Hej.expr.parser.Evaluator evaluator = null;
        private bool updating = false;
        private int inlineable = -1;
        private int inliningCount = 0;
        private bool overrideExtensionFunction = true;
        private Saxon.Hej.query.AnnotationList annotations = Saxon.Hej.query.AnnotationList.EMPTY;
        private Saxon.Hej.trans.FunctionStreamability declaredStreamability = Saxon.Hej.trans.FunctionStreamability.UNCLASSIFIED;
        private Saxon.Hej.expr.instruct.UserFunction.Determinism determinism = Saxon.Hej.expr.instruct.UserFunction.Determinism.PROACTIVE;
        private int refCount = 0;
        public UserFunction()  {
        }
        public void setFunctionName(Saxon.Hej.om.StructuredQName name) {
            functionName = name;
        }
        public Saxon.Hej.om.StructuredQName getFunctionName() {
            return functionName;
        }
        public string getDescription() {
            Saxon.Hej.om.StructuredQName name = getFunctionName();
            if (name.hasURI(Saxon.Hej.lib.NamespaceConstant.ANONYMOUS)) {
                bool first = true;
                Saxon.Ejava.lang.StringBuilder sb = new Saxon.Ejava.lang.StringBuilder("function(");
                foreach (Saxon.Hej.expr.instruct.UserFunctionParameter param in getParameterDefinitions()) {
                    if (first) {
                        first = false;
                    } else {
                        sb.append(", ");
                    }
                    sb.append("$").append(param.getVariableQName().getDisplayName());
                }
                sb.append("){");
                Saxon.Hej.expr.Expression body = getBody();
                if ((body == null)) {
                    sb.append("...");
                } else {
                    sb.append(body.toShortString());
                }
                sb.append("}");
                return sb.ToString();
            } else {
                return name.getDisplayName();
            }
        }
        public override string getTracingTag() {
            return "xsl:function";
        }
        public void gatherProperties(Saxon.Ejava.util.function.BiConsumer<string,System.Object> consumer) {
            consumer("name", getFunctionName());
            consumer("arity", getArity());
        }
        public Saxon.Hej.om.StructuredQName getObjectName() {
            return functionName;
        }
        public override Saxon.Hej.trans.SymbolicName.F getSymbolicName() {
            return new Saxon.Hej.trans.SymbolicName.F(functionName, getArity());
        }
        public Saxon.Hej.type.FunctionItemType getFunctionItemType() {
            Saxon.Hej.value.SequenceType[] argTypes = new Saxon.Hej.value.SequenceType[parameterDefinitions.Length];
            for (int i = 0; (i<parameterDefinitions.Length); i++) {
                Saxon.Hej.expr.instruct.UserFunctionParameter ufp = parameterDefinitions[i];
                argTypes[i] = ufp.getRequiredType();
            }
            return new Saxon.Hej.type.SpecificFunctionType(argTypes, resultType, annotations);
        }
        public Saxon.Hej.expr.OperandRole[] getOperandRoles() {
            Saxon.Hej.expr.OperandRole[] roles = new Saxon.Hej.expr.OperandRole[getArity()];
            Saxon.Hej.expr.OperandUsage first = Saxon.Hej.expr.OperandUsage.TRANSMISSION;
            switch (declaredStreamability.innerEnumValue) {
              case Saxon.Hej.trans.FunctionStreamability.InnerEnum.UNCLASSIFIED:
                Saxon.Hej.value.SequenceType required = getArgumentType(0);
                first = Saxon.Hej.expr.OperandRole.getTypeDeterminedUsage(required.getPrimaryType());
                break ;
              case Saxon.Hej.trans.FunctionStreamability.InnerEnum.ABSORBING:
                first = Saxon.Hej.expr.OperandUsage.ABSORPTION;
                break ;
              case Saxon.Hej.trans.FunctionStreamability.InnerEnum.INSPECTION:
                first = Saxon.Hej.expr.OperandUsage.INSPECTION;
                break ;
              case Saxon.Hej.trans.FunctionStreamability.InnerEnum.FILTER:
                first = Saxon.Hej.expr.OperandUsage.TRANSMISSION;
                break ;
              case Saxon.Hej.trans.FunctionStreamability.InnerEnum.SHALLOW_DESCENT:
                first = Saxon.Hej.expr.OperandUsage.TRANSMISSION;
                break ;
              case Saxon.Hej.trans.FunctionStreamability.InnerEnum.DEEP_DESCENT:
                first = Saxon.Hej.expr.OperandUsage.TRANSMISSION;
                break ;
              case Saxon.Hej.trans.FunctionStreamability.InnerEnum.ASCENT:
                first = Saxon.Hej.expr.OperandUsage.TRANSMISSION;
                break ;
            }
            roles[0] = new Saxon.Hej.expr.OperandRole(0, first, getArgumentType(0));
            for (int i = 1; (i<roles.Length); i++) {
                Saxon.Hej.value.SequenceType required = getArgumentType(i);
                roles[i] = new Saxon.Hej.expr.OperandRole(0, Saxon.Hej.expr.OperandRole.getTypeDeterminedUsage(required.getPrimaryType()), required);
            }
            return roles;
        }
        public bool acceptsNodesWithoutAtomization() {
            for (int i = 0; (i<getArity()); i++) {
                Saxon.Hej.type.ItemType type = getArgumentType(i).getPrimaryType();
                if (((type is Saxon.Hej.pattern.NodeTest) || (type == Saxon.Hej.type.AnyItemType.getInstance()))) {
                    return true;
                }
            }
            return false;
        }
        public bool isOverrideExtensionFunction() {
            return overrideExtensionFunction;
        }
        public void setOverrideExtensionFunction(bool overrideExtensionFunction) {
            this.overrideExtensionFunction = overrideExtensionFunction;
        }
        public void setAnnotations(Saxon.Hej.query.AnnotationList list) {
            this.annotations = Saxon.Ejava.util.Objects.requireNonNull(list);
        }
        public Saxon.Hej.query.AnnotationList getAnnotations() {
            return annotations;
        }
        public void setDeterminism(Saxon.Hej.expr.instruct.UserFunction.Determinism determinism) {
            this.determinism = determinism;
        }
        public Saxon.Hej.expr.instruct.UserFunction.Determinism getDeterminism() {
            return determinism;
        }
        public virtual void computeEvaluationMode() {
            if (tailRecursive) {
                evaluator = Saxon.Hej.expr.parser.ExpressionTool.eagerEvaluator(getBody());
            } else {
                evaluator = Saxon.Hej.expr.parser.ExpressionTool.lazyEvaluator(getBody(), true);
            }
        }
        public Saxon.Ejava.util.Optional<bool> isInlineable() {
            if ((inlineable != -1)) {
                return Saxon.Ejava.util.Optional<bool>.of(((inlineable>0) && (inliningCount<MAX_INLININGS)));
            }
            if ((body == null)) {
                return Saxon.Ejava.util.Optional<bool>.empty();
            }
            if ((body.hasSpecialProperty(Saxon.Hej.expr.StaticProperty.HAS_SIDE_EFFECTS) || tailCalls)) {
                return Saxon.Ejava.util.Optional<bool>.of(false);
            }
            Saxon.Hej.expr.Component component = getDeclaringComponent();
            if ((component != null)) {
                Saxon.Hej.trans.Visibility visibility = getDeclaringComponent().getVisibility();
                if (((visibility == Saxon.Hej.trans.Visibility.PRIVATE) || (visibility == Saxon.Hej.trans.Visibility.FINAL))) {
                    if ((inlineable<0)) {
                        return Saxon.Ejava.util.Optional<bool>.empty();
                    } else {
                        return Saxon.Ejava.util.Optional<bool>.of((inlineable>0));
                    }
                } else {
                    return Saxon.Ejava.util.Optional<bool>.of(false);
                }
            } else {
                return Saxon.Ejava.util.Optional<bool>.empty();
            }
        }
        public void setInlineable(bool inlineable) {
            this.inlineable = (inlineable ? 1 : 0);
        }
        public void markAsInlined() {
            inliningCount++;
        }
        public void setParameterDefinitions(Saxon.Hej.expr.instruct.UserFunctionParameter[] @params) {
            parameterDefinitions = @params;
        }
        public Saxon.Hej.expr.instruct.UserFunctionParameter[] getParameterDefinitions() {
            return parameterDefinitions;
        }
        public void setResultType(Saxon.Hej.value.SequenceType resultType) {
            this.declaredResultType = resultType;
            this.resultType = resultType;
        }
        public void setTailRecursive(bool tailCalls, bool recursiveTailCalls) {
            this.tailCalls = tailCalls;
            tailRecursive = recursiveTailCalls;
        }
        public bool containsTailCalls() {
            return tailCalls;
        }
        public bool isTailRecursive() {
            return tailRecursive;
        }
        public void setUpdating(bool isUpdating) {
            this.updating = isUpdating;
        }
        public bool isUpdating() {
            return updating;
        }
        public void setDeclaredStreamability(Saxon.Hej.trans.FunctionStreamability streamability) {
            this.declaredStreamability = ((streamability == null) ? Saxon.Hej.trans.FunctionStreamability.UNCLASSIFIED : streamability);
        }
        public Saxon.Hej.trans.FunctionStreamability getDeclaredStreamability() {
            return this.declaredStreamability;
        }
        public Saxon.Hej.value.SequenceType getResultType() {
            if (((resultType == Saxon.Hej.value.SequenceType.ANY_SEQUENCE) && (getBody() != null))) {
                if (!(containsUserFunctionCalls(getBody()))) {
                    resultType = Saxon.Hej.value.SequenceType.makeSequenceType(getBody().getItemType(), getBody().getCardinality());
                }
            }
            return resultType;
        }
        public Saxon.Hej.value.SequenceType getDeclaredResultType() {
            return declaredResultType;
        }
        private static bool containsUserFunctionCalls(Saxon.Hej.expr.Expression exp) {
            if ((exp is Saxon.Hej.expr.UserFunctionCall)) {
                return true;
            }
            foreach (Saxon.Hej.expr.Operand o in exp.operands()) {
                if (containsUserFunctionCalls(o.getChildExpression())) {
                    return true;
                }
            }
            return false;
        }
        public Saxon.Hej.value.SequenceType getArgumentType(int n) {
            return parameterDefinitions[n].getRequiredType();
        }
        public Saxon.Hej.expr.parser.Evaluator getEvaluator() {
            if ((evaluator == null)) {
                computeEvaluationMode();
            }
            return evaluator;
        }
        public void setEvaluator(Saxon.Hej.expr.parser.Evaluator mode) {
            evaluator = mode;
        }
        public int getArity() {
            return parameterDefinitions.Length;
        }
        public virtual bool isMemoFunction() {
            return false;
        }
        public void typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor) {
            Saxon.Hej.expr.Expression exp = getBody();
            if (((((exp is Saxon.Hej.expr.instruct.ValueOf) && ((Saxon.Hej.expr.instruct.ValueOf)(exp)).getSelect().getItemType().isAtomicType()) && declaredResultType.getPrimaryType().isAtomicType()) && (declaredResultType.getPrimaryType() != Saxon.Hej.type.BuiltInAtomicType.STRING))) {
                visitor.getStaticContext().issueWarning("A function that computes atomic values should use xsl:sequence rather than xsl:value-of", getLocation());
            }
            Saxon.Hej.expr.parser.ExpressionTool.resetPropertiesWithinSubtree(exp);
            Saxon.Hej.expr.Expression exp2 = exp;
            try {
                Saxon.Hej.expr.parser.ContextItemStaticInfo info = Saxon.Hej.expr.parser.ContextItemStaticInfo.ABSENT;
                exp2 = exp.typeCheck(visitor, info);
                if ((resultType != null)) {
                    Saxon.Hej.expr.parser.RoleDiagnostic role = new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.FUNCTION_RESULT, ((functionName == null) ? "" : ((functionName.getDisplayName()+"#")+getArity())), 0);
                    role.setErrorCode(((getPackageData().isXSLT() && (getFunctionName() != null)) ? "XTTE0780" : "XPTY0004"));
                    exp2 = visitor.getConfiguration().getTypeChecker(false).staticTypeCheck(exp2, resultType, role, visitor);
                }
            } catch (Saxon.Hej.trans.XPathException err) {
                err.maybeSetLocation(getLocation());
                throw;
            }
            if ((exp2 != exp)) {
                setBody(exp2);
            }
        }
        public Saxon.Hej.expr.XPathContext makeNewContext(Saxon.Hej.expr.XPathContext oldContext, Saxon.Hej.expr.ContextOriginator originator) {
            Saxon.Hej.expr.XPathContextMajor c2 = oldContext.newCleanContext();
            c2.setTemporaryOutputState(Saxon.Hej.om.StandardNames.XSL_FUNCTION);
            c2.setCurrentOutputUri(null);
            c2.setCurrentComponent(getDeclaringComponent());
            c2.setOrigin(originator);
            return c2;
        }
        public virtual Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] actualArgs) {
            if ((evaluator == null)) {
                computeEvaluationMode();
            }
            Saxon.Hej.expr.XPathContextMajor c2 = (Saxon.Hej.expr.XPathContextMajor)(context);
            c2.setStackFrame(getStackFrameMap(), actualArgs);
            Saxon.Hej.om.Sequence result;
            try {
                result = evaluator.evaluate(getBody(), c2);
            } catch (Saxon.Hej.trans.XPathException err) {
                err.maybeSetLocation(getLocation());
                err.maybeSetContext(c2);
                throw;
            } catch (System.Exception err2) {
                string message = ((("Internal error evaluating function "+(((functionName == null) ? "(unnamed)" : functionName.getDisplayName())))+(((getLineNumber()>0) ? (" at line "+getLineNumber()) : "")))+(((getSystemId() != null) ? (" in module "+getSystemId()) : "")));
                throw new System.Exception(message, err2);
            }
            return result;
        }
        public void process(Saxon.Hej.expr.XPathContextMajor context, Saxon.Hej.om.Sequence[] actualArgs, Saxon.Hej.@event.Outputter output) {
            context.setStackFrame(getStackFrameMap(), actualArgs);
            getBody().process(output, context);
        }
        public Saxon.Hej.om.Sequence call(Saxon.Hej.om.Sequence[] actualArgs, Saxon.Hej.Controller controller) {
            return call(controller.newXPathContext(), actualArgs);
        }
        public void callUpdating(Saxon.Hej.om.Sequence[] actualArgs, Saxon.Hej.expr.XPathContextMajor context, Saxon.Hej.expr.PendingUpdateList pul) {
            context.setStackFrame(getStackFrameMap(), actualArgs);
            try {
                getBody().evaluatePendingUpdates(context, pul);
            } catch (Saxon.Hej.trans.XPathException err) {
                err.maybeSetLocation(getLocation());
                throw;
            }
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter presenter) {
            presenter.startElement("function");
            if ((getFunctionName() != null)) {
                presenter.emitAttribute("name", getFunctionName());
                presenter.emitAttribute("line", (getLineNumber()+""));
                presenter.emitAttribute("module", getSystemId());
                presenter.emitAttribute("eval", (getEvaluator().getCode()+""));
            }
            string flags = "";
            if ((determinism == Saxon.Hej.expr.instruct.UserFunction.Determinism.PROACTIVE)) {
                flags += "p";
            } else {
                if ((determinism == Saxon.Hej.expr.instruct.UserFunction.Determinism.ELIDABLE)) {
                    flags += "e";
                } else {
                    flags += "d";
                }
            }
            if (isMemoFunction()) {
                flags += "m";
            }
            switch (declaredStreamability.innerEnumValue) {
              case Saxon.Hej.trans.FunctionStreamability.InnerEnum.UNCLASSIFIED:
                flags += "U";
                break ;
              case Saxon.Hej.trans.FunctionStreamability.InnerEnum.ABSORBING:
                flags += "A";
                break ;
              case Saxon.Hej.trans.FunctionStreamability.InnerEnum.INSPECTION:
                flags += "I";
                break ;
              case Saxon.Hej.trans.FunctionStreamability.InnerEnum.FILTER:
                flags += "F";
                break ;
              case Saxon.Hej.trans.FunctionStreamability.InnerEnum.SHALLOW_DESCENT:
                flags += "S";
                break ;
              case Saxon.Hej.trans.FunctionStreamability.InnerEnum.DEEP_DESCENT:
                flags += "D";
                break ;
              case Saxon.Hej.trans.FunctionStreamability.InnerEnum.ASCENT:
                flags += "C";
                break ;
            }
            presenter.emitAttribute("flags", flags);
            presenter.emitAttribute("as", getDeclaredResultType().toAlphaCode());
            presenter.emitAttribute("slots", (getStackFrameMap().getNumberOfVariables()+""));
            foreach (Saxon.Hej.expr.instruct.UserFunctionParameter p in parameterDefinitions) {
                presenter.startElement("arg");
                presenter.emitAttribute("name", p.getVariableQName());
                presenter.emitAttribute("as", p.getRequiredType().toAlphaCode());
                presenter.endElement();
            }
            presenter.setChildRole("body");
            getBody().export(presenter);
            presenter.endElement();
        }
        public override bool isExportable() {
            return (((refCount>0) || (((getDeclaredVisibility() != Saxon.Hej.trans.Visibility.UNDEFINED) && (getDeclaredVisibility() != Saxon.Hej.trans.Visibility.PRIVATE)))) || ((Saxon.Hej.style.StylesheetPackage)(getPackageData())).isRetainUnusedFunctions());
        }
        public bool isTrustedResultType() {
            return false;
        }
        public bool isMap() {
            return false;
        }
        public bool isArray() {
            return false;
        }
        public bool deepEquals(Saxon.Hej.om.Function other, Saxon.Hej.expr.XPathContext context, Saxon.Hej.expr.sort.AtomicComparer comparer, int flags) {
            Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Cannot compare functions using deep-equal", "FOTY0015");
            err.setIsTypeError(true);
            err.setXPathContext(context);
            throw err;
        }
        public Saxon.Hej.om.Item itemAt(int n) {
            return ((n == 0) ? this : null);
        }
        public Saxon.Hej.om.GroundedValue subsequence(int start, int length) {
            return (((start<=0) && (((start+length))>0)) ? (Saxon.Hej.om.GroundedValue)(this) : (Saxon.Hej.om.GroundedValue)(Saxon.Hej.value.EmptySequence.getInstance()));
        }
        public int getLength() {
            return 1;
        }
        public bool effectiveBooleanValue() {
            return Saxon.Hej.expr.parser.ExpressionTool.effectiveBooleanValue(this);
        }
        public Saxon.Hej.om.GroundedValue reduce() {
            return this;
        }
        public Saxon.Hej.om.Item head() {
            return this;
        }
        public Saxon.Hej.str.UnicodeString getUnicodeStringValue() {
            throw new Saxon.Ejava.lang.UnsupportedOperationException("A function has no string value");
        }
        public Saxon.Hej.om.AtomicSequence atomize() {
            throw new Saxon.Hej.trans.XPathException("Functions cannot be atomized", "FOTY0013");
        }
        public void incrementReferenceCount() {
            refCount++;
        }
        public int getReferenceCount() {
            return refCount;
        }
        public virtual void prepareForStreaming() {
        }
    
        /* Expanding default method isSequenceVariadic from interface Function */
        public virtual bool isSequenceVariadic() {
            return false;
        }

        /* Expanding default method toShortString from interface Item */
        public virtual string toShortString() {
            return ToString();
        }

        /* Expanding default method getGenre from interface Function */
        public virtual Saxon.Hej.om.Genre getGenre() {
            return Saxon.Hej.om.Genre.FUNCTION;
        }

        /* Expanding default method materialize from interface Sequence */
        public virtual Saxon.Hej.om.GroundedValue materialize() {
            return Saxon.Hej.om.SequenceTool.toGroundedValue(iterate());
        }

        /* Expanding default method asIterable from interface GroundedValue */
        public virtual System.Collections.Generic.IEnumerable<Saxon.Hej.om.Item> asIterable() {
            return new Saxon.Hej.om.GroundedValueAsIterable<Saxon.Hej.om.Item>(this);
        }

        /* Expanding default method containsNode from interface GroundedValue */
        public virtual bool containsNode(Saxon.Hej.om.NodeInfo sought) {
            return Saxon.Hej.expr.SingletonIntersectExpression.containsNode(iterate(), sought);
        }

        /* Expanding default method concatenate from interface GroundedValue */
        public virtual Saxon.Hej.om.GroundedValue concatenate(params Saxon.Hej.om.GroundedValue[]  others) {
            Saxon.Hej.ma.zeno.ZenoSequence chain = new Saxon.Hej.ma.zeno.ZenoSequence();
            chain.appendSequence(this);
            foreach (Saxon.Hej.om.GroundedValue seq in others) {
                chain.appendSequence(seq);
            }
            return chain;
        }

        /* Expanding default method getStringValue from interface Item */
        public virtual string getStringValue() {
            return getUnicodeStringValue().ToString();
        }

        /* Expanding default method iterate from interface Item */
        public virtual Saxon.Hej.om.SequenceIterator iterate() {
            return new Saxon.Hej.tree.iter.SingletonIterator(this);
        }

        /* Expanding default method isStreamed from interface Item */
        public virtual bool isStreamed() {
            return false;
        }

        /* Expanding default method makeRepeatable from interface Sequence */
        public virtual Saxon.Hej.om.Sequence makeRepeatable() {
            return this;
        }

    }
}