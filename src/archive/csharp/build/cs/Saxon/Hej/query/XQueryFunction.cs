using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.query {
    public class XQueryFunction : Saxon.Hej.query.Declaration, Saxon.Hej.s9api.Location, Saxon.Hej.functions.registry.FunctionDefinition {
        private Saxon.Hej.om.StructuredQName functionName;
        private readonly System.Collections.Generic.IList<Saxon.Hej.expr.instruct.UserFunctionParameter> parameters;
        private Saxon.Hej.value.SequenceType resultType;
        private Saxon.Hej.expr.Expression body = null;
        private System.Collections.Generic.IList<Saxon.Hej.expr.UserFunctionResolvable> references = new System.Collections.Generic.List<Saxon.Hej.expr.UserFunctionResolvable>(10);
        private Saxon.Hej.s9api.Location location;
        private Saxon.Hej.expr.instruct.UserFunction compiledFunction = null;
        private bool memoFunction;
        private Saxon.Hej.om.NamespaceResolver namespaceResolver;
        private Saxon.Hej.query.QueryModule staticContext;
        private bool updating = false;
        private Saxon.Hej.query.AnnotationList annotations = Saxon.Hej.query.AnnotationList.EMPTY;
        private int mandatoryParams = 0;
        public XQueryFunction()  {
            parameters = new System.Collections.Generic.List<Saxon.Hej.expr.instruct.UserFunctionParameter>(8);
        }
        public Saxon.Hej.expr.PackageData getPackageData() {
            return staticContext.getPackageData();
        }
        public void setFunctionName(Saxon.Hej.om.StructuredQName name) {
            functionName = name;
        }
        public void addParameter(Saxon.Hej.expr.instruct.UserFunctionParameter param) {
            parameters.Add(param);
            if ((param.getDefaultValueExpression() == null)) {
                mandatoryParams++;
            }
        }
        public void setResultType(Saxon.Hej.value.SequenceType resultType) {
            this.resultType = resultType;
        }
        public void setBody(Saxon.Hej.expr.Expression body) {
            this.body = body;
        }
        public Saxon.Hej.expr.Expression getBody() {
            return body;
        }
        public void setLocation(Saxon.Hej.s9api.Location location) {
            this.location = location;
        }
        public Saxon.Hej.om.StructuredQName getFunctionName() {
            return functionName;
        }
        public string getDisplayName() {
            return functionName.getDisplayName();
        }
        public Saxon.Hej.trans.SymbolicName getIdentificationKey() {
            return new Saxon.Hej.trans.SymbolicName.F(functionName, parameters.Count);
        }
        public static Saxon.Hej.trans.SymbolicName getIdentificationKey(Saxon.Hej.om.StructuredQName qName, int arity) {
            return new Saxon.Hej.trans.SymbolicName.F(qName, arity);
        }
        public Saxon.Hej.value.SequenceType getResultType() {
            return resultType;
        }
        public void setStaticContext(Saxon.Hej.query.QueryModule env) {
            staticContext = env;
        }
        public Saxon.Hej.expr.StaticContext getStaticContext() {
            return staticContext;
        }
        public Saxon.Hej.value.SequenceType[] getArgumentTypes() {
            Saxon.Hej.value.SequenceType[] types = new Saxon.Hej.value.SequenceType[parameters.Count];
            for (int i = 0; (i<parameters.Count); i++) {
                types[i] = parameters[i].getRequiredType();
            }
            return types;
        }
        public Saxon.Hej.expr.instruct.UserFunctionParameter[] getParameterDefinitions() {
            Saxon.Hej.expr.instruct.UserFunctionParameter[] @params = new Saxon.Hej.expr.instruct.UserFunctionParameter[parameters.Count];
            return parameters.ToArray();
        }
        public int getPositionOfParameter(Saxon.Hej.om.StructuredQName name) {
            int pos = 0;
            foreach (Saxon.Hej.expr.instruct.UserFunctionParameter p in parameters) {
                if (p.getVariableQName().Equals(name)) {
                    return pos;
                }
                pos++;
            }
            return -1;
        }
        public Saxon.Hej.om.StructuredQName getParameterName(int i) {
            return parameters[i].getVariableQName();
        }
        public Saxon.Hej.expr.Expression getDefaultValueExpression(int i) {
            return parameters[i].getDefaultValueExpression();
        }
        public int getNumberOfParameters() {
            return parameters.Count;
        }
        public int getMinimumArity() {
            return mandatoryParams;
        }
        public void registerReference(Saxon.Hej.expr.UserFunctionResolvable ufc) {
            references.Add(ufc);
        }
        public void setMemoFunction(bool isMemoFunction) {
            memoFunction = isMemoFunction;
        }
        public bool isMemoFunction() {
            return memoFunction;
        }
        public void setUpdating(bool isUpdating) {
            this.updating = isUpdating;
        }
        public bool isUpdating() {
            return updating;
        }
        public void setAnnotations(Saxon.Hej.query.AnnotationList annotations) {
            this.annotations = annotations;
            if ((compiledFunction != null)) {
                compiledFunction.setAnnotations(annotations);
            }
            if (annotations.includes(Saxon.Hej.query.Annotation.UPDATING)) {
                setUpdating(true);
            }
        }
        public Saxon.Hej.query.AnnotationList getAnnotations() {
            return annotations;
        }
        public bool hasAnnotation(Saxon.Hej.om.StructuredQName name) {
            return annotations.includes(name);
        }
        public bool isPrivate() {
            return hasAnnotation(Saxon.Hej.query.Annotation.PRIVATE);
        }
        public void compile() {
            Saxon.Hej.Configuration config = staticContext.getConfiguration();
            try {
                if ((compiledFunction == null)) {
                    Saxon.Hej.expr.instruct.SlotManager map = config.makeSlotManager();
                    Saxon.Hej.expr.instruct.UserFunctionParameter[] @params = getParameterDefinitions();
                    for (int i = 0; (i<@params.Length); i++) {
                        @params[i].setSlotNumber(i);
                        map.allocateSlotNumber(@params[i].getVariableQName());
                    }
                    Saxon.Hej.expr.parser.RetainedStaticContext rsc = null;
                    try {
                        rsc = getStaticContext().makeRetainedStaticContext();
                        body.setRetainedStaticContext(rsc);
                        Saxon.Hej.expr.parser.ExpressionVisitor visitor = Saxon.Hej.expr.parser.ExpressionVisitor.make(staticContext);
                        body = body.simplify().typeCheck(visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo.ABSENT);
                        Saxon.Ejava.util.function.Supplier<Saxon.Hej.expr.parser.RoleDiagnostic> role = () => new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.FUNCTION_RESULT, functionName.getDisplayName(), 0);
                        body = config.getTypeChecker(false).staticTypeCheck(body, resultType, role, visitor);
                    } catch (Saxon.Hej.trans.XPathException e) {
                        e.maybeSetLocation(this);
                        if (e.isReportableStatically()) {
                            throw;
                        } else {
                            Saxon.Hej.expr.Expression newBody = new Saxon.Hej.expr.ErrorExpression(new Saxon.Hej.trans.XmlProcessingException(e));
                            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(body, newBody);
                            body = newBody;
                        }
                    }
                    compiledFunction = config.newUserFunction(memoFunction, Saxon.Hej.trans.FunctionStreamability.UNCLASSIFIED);
                    compiledFunction.setRetainedStaticContext(rsc);
                    compiledFunction.setPackageData(staticContext.getPackageData());
                    compiledFunction.setBody(body);
                    compiledFunction.setFunctionName(functionName);
                    compiledFunction.setParameterDefinitions(@params);
                    compiledFunction.setResultType(getResultType());
                    compiledFunction.setLineNumber(location.getLineNumber());
                    compiledFunction.setColumnNumber(location.getColumnNumber());
                    compiledFunction.setSystemId(location.getSystemId());
                    compiledFunction.setStackFrameMap(map);
                    compiledFunction.setUpdating(updating);
                    compiledFunction.setAnnotations(annotations);
                    if (staticContext.getUserQueryContext().isCompileWithTracing()) {
                        namespaceResolver = staticContext.getNamespaceResolver();
                        staticContext.getCodeInjector().process(compiledFunction);
                        body = compiledFunction.getBody();
                    }
                }
                fixupReferences();
            } catch (Saxon.Hej.trans.XPathException e) {
                e.maybeSetLocation(this);
                throw;
            }
        }
        public void optimize() {
            body.checkForUpdatingSubexpressions();
            if (updating) {
                if (Saxon.Hej.expr.parser.ExpressionTool.isNotAllowedInUpdatingContext(body)) {
                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("The body of an updating function must be an updating expression", "XUST0002");
                    err.setLocator(body.getLocation());
                    throw err;
                }
            } else {
                if (body.isUpdatingExpression()) {
                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("The body of a non-updating function must be a non-updating expression", "XUST0001");
                    err.setLocator(body.getLocation());
                    throw err;
                }
            }
            Saxon.Hej.expr.parser.ExpressionVisitor visitor = Saxon.Hej.expr.parser.ExpressionVisitor.make(staticContext);
            Saxon.Hej.Configuration config = staticContext.getConfiguration();
            Saxon.Hej.expr.parser.Optimizer opt = visitor.obtainOptimizer();
            int arity = parameters.Count;
            if (opt.isOptionSet(Saxon.Hej.expr.parser.OptimizerOptions.MISCELLANEOUS)) {
                body = body.optimize(visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo.ABSENT);
            }
            body.setParentExpression(null);
            if (opt.isOptionSet(Saxon.Hej.expr.parser.OptimizerOptions.LOOP_LIFTING)) {
                body = Saxon.Hej.expr.parser.LoopLifter.process(body, visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo.ABSENT);
            }
            if (opt.isOptionSet(Saxon.Hej.expr.parser.OptimizerOptions.EXTRACT_GLOBALS)) {
                Saxon.Hej.trans.GlobalVariableManager manager = new Inner_GlobalVariableManager_25(this);
                Saxon.Hej.expr.Expression b2 = opt.promoteExpressionsToGlobal(body, manager, visitor);
                if ((b2 != null)) {
                    body = body.optimize(visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo.ABSENT);
                }
            }
            if ((opt.getOptimizerOptions().isSet(Saxon.Hej.expr.parser.OptimizerOptions.TAIL_CALLS) && !(updating))) {
                int tailCalls = Saxon.Hej.expr.parser.ExpressionTool.markTailFunctionCalls(body, functionName, arity);
                if ((tailCalls != 0)) {
                    compiledFunction.setBody(body);
                    compiledFunction.setTailRecursive((tailCalls>0), (tailCalls>1));
                    body = new Saxon.Hej.expr.TailCallLoop(compiledFunction, body);
                }
            }
            compiledFunction.setBody(body);
            Saxon.Hej.expr.parser.ExpressionTool.allocateSlots(body, arity, compiledFunction.getStackFrameMap());
        }
        public void fixupReferences() {
            foreach (Saxon.Hej.expr.UserFunctionResolvable ufc in references) {
                ufc.setFunction(compiledFunction);
            }
        }
        public void checkReferences(Saxon.Hej.expr.parser.ExpressionVisitor visitor) {
            foreach (Saxon.Hej.expr.UserFunctionResolvable ufr in references) {
                if ((ufr is Saxon.Hej.expr.UserFunctionCall)) {
                    Saxon.Hej.expr.UserFunctionCall ufc = (Saxon.Hej.expr.UserFunctionCall)(ufr);
                    ufc.checkFunctionCall(compiledFunction, visitor);
                }
            }
            references = new System.Collections.Generic.List<Saxon.Hej.expr.UserFunctionResolvable>(0);
        }
        public void explain(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("declareFunction");
            @out.emitAttribute("name", functionName.getDisplayName());
            @out.emitAttribute("arity", (""+getNumberOfParameters()));
            if ((compiledFunction == null)) {
                @out.emitAttribute("unreferenced", "true");
            } else {
                if (compiledFunction.isMemoFunction()) {
                    @out.emitAttribute("memo", "true");
                }
                @out.emitAttribute("tailRecursive", (compiledFunction.isTailRecursive() ? "true" : "false"));
                body.export(@out);
            }
            @out.endElement();
        }
        public Saxon.Hej.expr.instruct.UserFunction getUserFunction() {
            return compiledFunction;
        }
        public Saxon.Hej.om.StructuredQName getObjectName() {
            return functionName;
        }
        public string getSystemId() {
            return location.getSystemId();
        }
        public int getLineNumber() {
            return location.getLineNumber();
        }
        public string getPublicId() {
            return null;
        }
        public int getColumnNumber() {
            return -1;
        }
        public Saxon.Hej.s9api.Location saveLocation() {
            return this;
        }
        public Saxon.Hej.om.NamespaceResolver getNamespaceResolver() {
            return namespaceResolver;
        }
        public System.Object getProperty(string name) {
            if ("name".Equals(name)) {
                return functionName.getDisplayName();
            } else {
                if ("as".Equals(name)) {
                    return resultType.ToString();
                } else {
                    return null;
                }
            }
        }
        public System.Collections.Generic.IEnumerator<string> getProperties() {
            return new Saxon.Hej.tree.jiter.PairIterator<string>("name", "as");
        }
        public Saxon.Api.HostLanguage getHostLanguage() {
            return Saxon.Api.HostLanguage.XQUERY;
        }
    
        private class Inner_GlobalVariableManager_25 : Saxon.Hej.trans.GlobalVariableManager {
            private readonly XQueryFunction outerInstance;
            public Inner_GlobalVariableManager_25(XQueryFunction outerInstance) {
                this.outerInstance = outerInstance;
            }
            public void addGlobalVariable(Saxon.Hej.expr.instruct.GlobalVariable variable) {
                Saxon.Hej.expr.PackageData pd = outerInstance.staticContext.getPackageData();
                /*localparam*/variable.setPackageData(/*localvar*/pd);
                Saxon.Hej.expr.instruct.SlotManager sm = /*localvar*/pd.getGlobalSlotManager();
                int slot = /*localvar*/sm.allocateSlotNumber(/*localparam*/variable.getVariableQName());
                /*localparam*/variable.compile(outerInstance.staticContext.getExecutable(), /*localvar*/slot);
                /*localvar*/pd.addGlobalVariable(/*localparam*/variable);
            }
            public Saxon.Hej.expr.instruct.GlobalVariable getEquivalentVariable(Saxon.Hej.expr.Expression select) {
                return null;
            }
        }

    }
}