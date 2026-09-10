using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.query {
    public class XQueryExpression : Saxon.Hej.s9api.Location, Saxon.Hej.expr.ExpressionOwner, Saxon.Hej.trace.TraceableComponent {
        protected internal Saxon.Hej.expr.Expression expression;
        protected internal Saxon.Hej.expr.instruct.SlotManager stackFrameMap;
        protected internal Saxon.Hej.expr.instruct.Executable executable;
        protected internal Saxon.Hej.query.QueryModule mainModule;
        public XQueryExpression(Saxon.Hej.expr.Expression exp, Saxon.Hej.query.QueryModule mainModule, bool streaming)  {
            Saxon.Hej.expr.instruct.Executable exec = mainModule.getExecutable();
            Saxon.Hej.Configuration config = mainModule.getConfiguration();
            stackFrameMap = config.makeSlotManager();
            executable = exec;
            this.mainModule = mainModule;
            exp.setRetainedStaticContext(mainModule.makeRetainedStaticContext());
            try {
                Saxon.Hej.expr.parser.ExpressionVisitor visitor = Saxon.Hej.expr.parser.ExpressionVisitor.make(mainModule);
                Saxon.Hej.expr.parser.Optimizer optimizer = visitor.obtainOptimizer();
                visitor.setOptimizeForStreaming(streaming);
                exp = exp.simplify();
                exp.checkForUpdatingSubexpressions();
                Saxon.Hej.expr.instruct.GlobalContextRequirement contextReq = exec.getGlobalContextRequirement();
                Saxon.Hej.type.ItemType req = ((contextReq == null) ? Saxon.Hej.type.AnyItemType.getInstance() : contextReq.getRequiredItemType());
                Saxon.Hej.expr.parser.ContextItemStaticInfo cit = config.makeContextItemStaticInfo(req, true);
                Saxon.Hej.expr.Expression e2 = exp.typeCheck(visitor, cit);
                if ((e2 != exp)) {
                    e2.setRetainedStaticContext(exp.getRetainedStaticContext());
                    e2.setParentExpression(null);
                    exp = e2;
                }
                if (optimizer.isOptionSet(Saxon.Hej.expr.parser.OptimizerOptions.MISCELLANEOUS)) {
                    e2 = exp.optimize(visitor, cit);
                    if ((e2 != exp)) {
                        e2.setRetainedStaticContext(exp.getRetainedStaticContext());
                        e2.setParentExpression(null);
                        exp = e2;
                    }
                }
                if (optimizer.isOptionSet(Saxon.Hej.expr.parser.OptimizerOptions.LOOP_LIFTING)) {
                    e2 = Saxon.Hej.expr.parser.LoopLifter.process(exp, visitor, cit);
                    if ((e2 != exp)) {
                        e2.setRetainedStaticContext(exp.getRetainedStaticContext());
                        e2.setParentExpression(null);
                        exp = e2;
                    }
                }
            } catch (Saxon.Hej.trans.XPathException err) {
                mainModule.reportStaticError(err);
                throw;
            }
            Saxon.Hej.expr.parser.ExpressionTool.allocateSlots(exp, 0, stackFrameMap);
            Saxon.Hej.expr.parser.ExpressionTool.computeEvaluationModesForUserFunctionCalls(exp);
            foreach (Saxon.Hej.expr.instruct.GlobalVariable var in getPackageData().getGlobalVariableList()) {
                Saxon.Hej.expr.Expression top = var.getBody();
                if ((top != null)) {
                    Saxon.Hej.expr.parser.ExpressionTool.computeEvaluationModesForUserFunctionCalls(top);
                }
            }
            expression = exp;
            executable.setConfiguration(config);
        }
        public Saxon.Hej.expr.Expression getExpression() {
            return expression;
        }
        public Saxon.Hej.expr.Expression getBody() {
            return getExpression();
        }
        public Saxon.Hej.expr.Expression getChildExpression() {
            return expression;
        }
        public void setBody(Saxon.Hej.expr.Expression expression) {
            setChildExpression(expression);
        }
        public Saxon.Hej.om.StructuredQName getObjectName() {
            return null;
        }
        public string getTracingTag() {
            return "query";
        }
        public Saxon.Hej.s9api.Location getLocation() {
            return this;
        }
        public Saxon.Hej.expr.PackageData getPackageData() {
            return mainModule.getPackageData();
        }
        public Saxon.Hej.Configuration getConfiguration() {
            return mainModule.getConfiguration();
        }
        public bool usesContextItem() {
            if (Saxon.Hej.expr.parser.ExpressionTool.dependsOnFocus(expression)) {
                return true;
            }
            System.Collections.Generic.IList<Saxon.Hej.expr.instruct.GlobalVariable> map = getPackageData().getGlobalVariableList();
            if ((map != null)) {
                foreach (Saxon.Hej.expr.instruct.GlobalVariable var in map) {
                    Saxon.Hej.expr.Expression select = var.getBody();
                    if (((select != null) && Saxon.Hej.expr.parser.ExpressionTool.dependsOnFocus(select))) {
                        return true;
                    }
                }
            }
            return false;
        }
        public virtual bool isUpdateQuery() {
            return false;
        }
        public Saxon.Hej.expr.instruct.SlotManager getStackFrameMap() {
            return stackFrameMap;
        }
        public virtual void explainPathMap() {
        }
        public Saxon.Hej.query.QueryModule getMainModule() {
            return mainModule;
        }
        public Saxon.Hej.om.StructuredQName[] getExternalVariableNames() {
            System.Collections.Generic.IList<Saxon.Hej.om.StructuredQName> list = stackFrameMap.getVariableMap();
            Saxon.Hej.om.StructuredQName[] names = new Saxon.Hej.om.StructuredQName[stackFrameMap.getNumberOfVariables()];
            for (int i = 0; (i<names.Length); i++) {
                names[i] = list[i];
            }
            return names;
        }
        public Saxon.Hej.om.SequenceIterator iterator(Saxon.Hej.query.DynamicQueryContext env) {
            if (isUpdateQuery()) {
                throw new Saxon.Hej.trans.XPathException("Cannot call iterator() on an updating query");
            }
            if (!(env.getConfiguration().isCompatible(getExecutable().getConfiguration()))) {
                throw new Saxon.Hej.trans.XPathException("The query must be compiled and executed under the same Configuration", Saxon.Hej.trans.SaxonErrorCode.SXXP0004);
            }
            Saxon.Hej.Controller controller = newController(env);
            try {
                Saxon.Hej.om.Item contextItem = controller.getGlobalContextItem();
                if ((((contextItem is Saxon.Hej.om.NodeInfo) && ((Saxon.Hej.om.NodeInfo)(contextItem)).getTreeInfo().isTyped()) && !(getExecutable().isSchemaAware()))) {
                    throw new Saxon.Hej.trans.XPathException("A typed input document can only be used with a schema-aware query");
                }
                Saxon.Hej.expr.XPathContextMajor context = initialContext(env, controller);
                if ((controller.getTraceListener() != null)) {
                    controller.preEvaluateGlobals(context);
                }
                context.openStackFrame(stackFrameMap);
                Saxon.Hej.om.SequenceIterator iterator = expression.iterate(context);
                if (((iterator is Saxon.Hej.tree.iter.GroundedIterator) && ((Saxon.Hej.tree.iter.GroundedIterator)(iterator)).isActuallyGrounded())) {
                    return iterator;
                } else {
                    return new Saxon.Hej.query.XQueryExpression.ErrorReportingIterator(iterator, controller.getErrorReporter(), getLocation());
                }
            } catch (Saxon.Hej.trans.XPathException err) {
                Saxon.Ejavax.xml.transform.TransformerException terr = err;
                while ((terr.getException() is Saxon.Ejavax.xml.transform.TransformerException)) {
                    terr = (Saxon.Ejavax.xml.transform.TransformerException)(terr.getException());
                }
                Saxon.Hej.trans.XPathException de = Saxon.Hej.trans.XPathException.makeXPathException(terr);
                controller.reportFatalError(de);
                throw de;
            }
        }
        public void run(Saxon.Hej.query.DynamicQueryContext env, Saxon.Ejavax.xml.transform.Result result, Saxon.Ejava.util.Properties outputProperties) {
            if (isUpdateQuery()) {
                throw new Saxon.Hej.trans.XPathException("Cannot call run() on an updating query");
            }
            if (!(env.getConfiguration().isCompatible(getExecutable().getConfiguration()))) {
                throw new Saxon.Hej.trans.XPathException("The query must be compiled and executed under the same Configuration", Saxon.Hej.trans.SaxonErrorCode.SXXP0004);
            }
            Saxon.Hej.om.Item contextItem = env.getContextItem();
            if ((((contextItem is Saxon.Hej.om.NodeInfo) && ((Saxon.Hej.om.NodeInfo)(contextItem)).getTreeInfo().isTyped()) && !(getExecutable().isSchemaAware()))) {
                throw new Saxon.Hej.trans.XPathException("A typed input document can only be used with a schema-aware query");
            }
            Saxon.Hej.Controller controller = newController(env);
            if ((result is Saxon.Hej.@event.Receiver)) {
                ((Saxon.Hej.@event.Receiver)(result)).getPipelineConfiguration().setController(controller);
            }
            Saxon.Ejava.util.Properties actualProperties = validateOutputProperties(controller, outputProperties);
            Saxon.Hej.expr.XPathContextMajor context = initialContext(env, controller);
            Saxon.Hej.lib.TraceListener tracer = controller.getTraceListener();
            if ((tracer != null)) {
                controller.preEvaluateGlobals(context);
            }
            context.openStackFrame(stackFrameMap);
            bool mustClose = ((result is Saxon.Ejavax.xml.transform.stream.StreamResult) && (((Saxon.Ejavax.xml.transform.stream.StreamResult)(result)).getOutputStream() == null));
            Saxon.Hej.@event.Receiver @out;
            if ((result is Saxon.Hej.@event.Receiver)) {
                @out = (Saxon.Hej.@event.Receiver)(result);
            } else {
                Saxon.Hej.lib.SerializerFactory sf = context.getConfiguration().getSerializerFactory();
                Saxon.Hej.@event.PipelineConfiguration pipe = controller.makePipelineConfiguration();
                pipe.setHostLanguage(Saxon.Api.HostLanguage.XQUERY);
                @out = sf.getReceiver(result, new Saxon.Hej.serialize.SerializationProperties(actualProperties), pipe);
            }
            Saxon.Hej.@event.ComplexContentOutputter dest = new Saxon.Hej.@event.ComplexContentOutputter(@out);
            dest.open();
            try {
                expression.process(dest, context);
            } catch (Saxon.Hej.trans.XPathException err) {
                controller.reportFatalError(err);
                throw;
            } finally {
                try {
                    if ((tracer != null)) {
                        tracer.close();
                    }
                    dest.close();
                } catch (Saxon.Hej.trans.XPathException e) {
                    System.Console.Error.WriteLine(e);
                }
            }
            if ((result is Saxon.Ejavax.xml.transform.stream.StreamResult)) {
                closeStreamIfNecessary((Saxon.Ejavax.xml.transform.stream.StreamResult)(result), mustClose);
            }
        }
        protected internal void closeStreamIfNecessary(Saxon.Ejavax.xml.transform.stream.StreamResult result, bool mustClose) {
            if (mustClose) {
                System.IO.Stream os = result.getOutputStream();
                if ((os != null)) {
                    try {
                        os.Close();
                    } catch (System.IO.IOException err) {
                        throw new Saxon.Hej.trans.XPathException(err);
                    }
                }
            }
        }
        public virtual void runStreamed(Saxon.Hej.query.DynamicQueryContext dynamicEnv, Saxon.Ejavax.xml.transform.Source source, Saxon.Ejavax.xml.transform.Result result, Saxon.Ejava.util.Properties outputProperties) {
            throw new Saxon.Hej.trans.XPathException("Streaming requires Saxon-EE");
        }
        protected internal Saxon.Ejava.util.Properties validateOutputProperties(Saxon.Hej.Controller controller, Saxon.Ejava.util.Properties outputProperties) {
            Saxon.Ejava.util.Properties baseProperties = controller.getExecutable().getPrimarySerializationProperties().getProperties();
            Saxon.Hej.lib.SerializerFactory sf = controller.getConfiguration().getSerializerFactory();
            if ((outputProperties != null)) {
                foreach (string key in outputProperties.stringPropertyNames()) {
                    string value = outputProperties.getProperty(key);
                    try {
                        value = sf.checkOutputProperty(key, value);
                        baseProperties.setProperty(key, value);
                    } catch (Saxon.Hej.trans.XPathException dynamicError) {
                        outputProperties.remove(key);
                        Saxon.Hej.trans.XmlProcessingException err = new Saxon.Hej.trans.XmlProcessingException(dynamicError);
                        err.setWarning(true);
                        controller.getErrorReporter().report(err);
                    }
                }
            }
            if ((baseProperties.getProperty("method") == null)) {
                baseProperties.setProperty("method", "xml");
            }
            return baseProperties;
        }
        public virtual System.Collections.Generic.ISet<Saxon.Hej.om.MutableNodeInfo> runUpdate(Saxon.Hej.query.DynamicQueryContext dynamicEnv) {
            throw new Saxon.Hej.trans.XPathException("Calling runUpdate() on a non-updating query");
        }
        public virtual void runUpdate(Saxon.Hej.query.DynamicQueryContext dynamicEnv, Saxon.Hej.query.UpdateAgent agent) {
            throw new Saxon.Hej.trans.XPathException("Calling runUpdate() on a non-updating query");
        }
        protected internal Saxon.Hej.expr.XPathContextMajor initialContext(Saxon.Hej.query.DynamicQueryContext dynamicEnv, Saxon.Hej.Controller controller) {
            Saxon.Hej.om.Item contextItem = controller.getGlobalContextItem();
            Saxon.Hej.expr.XPathContextMajor context = controller.newXPathContext();
            if ((contextItem != null)) {
                Saxon.Hej.tree.iter.ManualIterator single = new Saxon.Hej.tree.iter.ManualIterator(contextItem);
                context.setCurrentIterator(single);
                controller.setGlobalContextItem(contextItem);
            }
            return context;
        }
        public virtual Saxon.Hej.Controller newController(Saxon.Hej.query.DynamicQueryContext env) {
            Saxon.Hej.Controller controller = new Saxon.Hej.Controller(executable.getConfiguration(), executable);
            env.initializeController(controller);
            return controller;
        }
        public void explain(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("query");
            mainModule.getKeyManager().exportKeys(@out, null);
            getExecutable().explainGlobalVariables(@out);
            mainModule.explainGlobalFunctions(@out);
            @out.startElement("body");
            expression.export(@out);
            @out.endElement();
            @out.endElement();
            @out.close();
        }
        public Saxon.Hej.expr.instruct.Executable getExecutable() {
            return executable;
        }
        public virtual void setAllowDocumentProjection(bool allowed) {
            if (allowed) {
                throw new Saxon.Ejava.lang.UnsupportedOperationException("Document projection requires Saxon-EE");
            }
        }
        public virtual bool isDocumentProjectionAllowed() {
            return false;
        }
        public string getPublicId() {
            return null;
        }
        public string getSystemId() {
            return mainModule.getSystemId();
        }
        public int getLineNumber() {
            return -1;
        }
        public int getColumnNumber() {
            return -1;
        }
        public Saxon.Hej.s9api.Location saveLocation() {
            return this;
        }
        public Saxon.Api.HostLanguage getHostLanguage() {
            return Saxon.Api.HostLanguage.XQUERY;
        }
        public void setChildExpression(Saxon.Hej.expr.Expression expr) {
            expression = expr;
        }
        private class ErrorReportingIterator : Saxon.Hej.om.SequenceIterator {
            private readonly Saxon.Hej.om.SequenceIterator @base;
            private readonly Saxon.Hej.lib.ErrorReporter reporter;
            private readonly Saxon.Hej.s9api.Location location;
            public ErrorReportingIterator(Saxon.Hej.om.SequenceIterator @base, Saxon.Hej.lib.ErrorReporter reporter, Saxon.Hej.s9api.Location location)  {
                this.@base = @base;
                this.reporter = reporter;
                this.location = location;
            }
            public Saxon.Hej.om.Item next() {
                return @base.next();
            }
            public void close() {
                @base.close();
            }
        
        }
    
        /* Expanding default method gatherProperties from interface Traceable */
        public virtual void gatherProperties(Saxon.Ejava.util.function.BiConsumer<string,System.Object> consumer) {
        }

    }
}