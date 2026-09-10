using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.optim {
    public class XQueryExpressionEE : Saxon.Hej.query.XQueryExpression {
        private bool allowDocumentProjection;
        private readonly bool isUpdating;
        private Saxon.Hej.expr.parser.PathMap pathMap;
        private Saxon.Eej.ee.stream.Inversion inversion;
        public XQueryExpressionEE(Saxon.Hej.expr.Expression exp, Saxon.Hej.query.QueryModule mainModule, bool streaming) : base (exp, mainModule, streaming) {
            Saxon.Hej.Configuration config = mainModule.getConfiguration();
            if (config.isLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.ENTERPRISE_XQUERY)) {
                Saxon.Hej.expr.PackageData pd = mainModule.getPackageData();
                Saxon.Hej.expr.parser.ExpressionTool.processExpressionTree(expression, null, (expr, result) => {
                    if ((expr is Saxon.Eej.ee.stream.StreamInstr)) {
                        Saxon.Hej.expr.Expression fallback = ((Saxon.Eej.ee.stream.StreamInstr)(expr)).prepareForStreaming(pd);
                        if ((fallback != expr)) {
                            if ((expr == expression)) {
                                expression = fallback;
                            } else {
                                Saxon.Hej.expr.Expression parent = expr.getParentExpression();
                                foreach (Saxon.Hej.expr.Operand o in parent.operands()) {
                                    if ((o.getChildExpression() == expr)) {
                                        o.setChildExpression(fallback);
                                    }
                                }
                            }
                            return true;
                        }
                    }
                    return false;
                } );
            }
            if ((streaming && config.isLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.ENTERPRISE_XQUERY))) {
                if (!(Saxon.Hej.expr.parser.ExpressionTool.dependsOnFocus(exp))) {
                    throw new Saxon.Hej.trans.XPathException("Streaming is not possible because the query does not use the context item");
                }
                System.Collections.Generic.IEnumerator<Saxon.Hej.expr.instruct.GlobalVariable> it = mainModule.getModuleVariables();
                while (it.MoveNext() /*UNSAFE CONVERSION*/) {
                    Saxon.Hej.expr.instruct.GlobalVariable gVar = it.Current;
                    if (((gVar.getBody() != null) && Saxon.Hej.expr.parser.ExpressionTool.dependsOnFocus(gVar.getBody()))) {
                        throw new Saxon.Hej.trans.XPathException((("Streaming is not possible because a variable $"+gVar.getObjectName().getDisplayName())+" depends on the context item"));
                    }
                }
                System.Collections.Generic.IList<string> reasons = new System.Collections.Generic.List<string>();
                Saxon.Hej.type.ItemType contextItemType = Saxon.Hej.type.AnyItemType.getInstance();
                Saxon.Hej.expr.instruct.GlobalContextRequirement req = mainModule.getExecutable().getGlobalContextRequirement();
                if ((req != null)) {
                    contextItemType = req.getRequiredItemType();
                }
                Saxon.Eej.ee.trans.ContextItemStaticInfoEE contextInfo = new Saxon.Eej.ee.trans.ContextItemStaticInfoEE(contextItemType, false, Saxon.Eej.ee.stream.Posture.STRIDING);
                Saxon.Eej.ee.stream.PostureAndSweep ps = /*134*/Saxon.Eej.ee.stream.Streamability.getStreamability(expression, contextInfo, reasons);
                if (((ps.getPosture() == Saxon.Eej.ee.stream.Posture.ROAMING) || (ps.getSweep() == Saxon.Eej.ee.stream.Sweep.FREE_RANGING))) {
                    Saxon.Ejava.lang.StringBuilder fsb = new Saxon.Ejava.lang.StringBuilder(256);
                    fsb.append("Streaming is not possible. ");
                    foreach (string r in reasons) {
                        fsb.append(r);
                        fsb.append(". ");
                    }
                    throw new Saxon.Hej.trans.XPathException(fsb.ToString(), "XTSE3430");
                }
                inversion = /*134*/Saxon.Eej.ee.stream.Inversion.invertExpression(expression, false);
            }
            bool isUpdatingExpression = (mainModule.isUpdating() && expression.isUpdatingExpression());
            isUpdating = isUpdatingExpression;
        }
        internal Saxon.Hej.expr.elab.PushEvaluator pusher = null;
        protected internal override void processQuery(Saxon.Hej.@event.Outputter dest, Saxon.Hej.expr.XPathContext context) {
            if ((pusher == null)) {
                pusher = expression.makeElaborator().elaborateForPush();
            }
            Saxon.Hej.expr.instruct.TailCall tc = /*Delegate.processLeavingTail()*/pusher(dest, context);
            Saxon.Hej.expr.Expression.dispatchTailCall(tc);
        }
        public override bool isUpdateQuery() {
            return isUpdating;
        }
        public override void setAllowDocumentProjection(bool allowed) {
            allowDocumentProjection = allowed;
        }
        public override bool isDocumentProjectionAllowed() {
            return allowDocumentProjection;
        }
        public Saxon.Hej.expr.parser.PathMap getPathMap() {
            if ((pathMap == null)) {
                pathMap = new Saxon.Hej.expr.parser.PathMap(expression);
            }
            foreach (Saxon.Hej.expr.PackageData pack in getExecutable().getPackages()) {
                foreach (Saxon.Hej.expr.instruct.GlobalVariable var in pack.getGlobalVariableList()) {
                    if (!(var.isUnused())) {
                        Saxon.Hej.expr.Expression select = var.getBody();
                        if ((select != null)) {
                            select.addToPathMap(pathMap, null);
                        }
                    }
                }
            }
            return pathMap;
        }
        public override Saxon.Hej.Controller newController(Saxon.Hej.query.DynamicQueryContext env) {
            Saxon.Hej.Controller controller = new Saxon.Hej.Controller(executable.getConfiguration(), executable);
            if ((isUpdateQuery() && !(controller.getModel().isMutable()))) {
                controller.setModel(Saxon.Hej.om.TreeModel.LINKED_TREE);
            }
            env.initializeController(controller);
            if (allowDocumentProjection) {
                controller.setUseDocumentProjection(getPathMap());
            }
            return controller;
        }
        public override System.Collections.Generic.ISet<Saxon.Hej.om.MutableNodeInfo> runUpdate(Saxon.Hej.query.DynamicQueryContext dynamicEnv) {
            if (!(isUpdating)) {
                throw new Saxon.Hej.trans.XPathException("Calling runUpdate() on a non-updating query");
            }
            Saxon.Hej.Configuration config = executable.getConfiguration();
            Saxon.Hej.Controller controller = newController(dynamicEnv);
            Saxon.Hej.expr.XPathContextMajor context = initialContext(dynamicEnv, controller);
            try {
                Saxon.Hej.expr.PendingUpdateList pul = config.newPendingUpdateList();
                context.openStackFrame(stackFrameMap);
                /*Delegate.registerUpdates()*/expression.makeElaborator().elaborateForUpdate()(context, pul);
                pul.apply(context, mainModule.getRevalidationMode());
                return pul.getAffectedTrees();
            } catch (Saxon.Hej.trans.XPathException e) {
                if (!(e.hasBeenReported())) {
                    Saxon.Hej.trans.XmlProcessingException error = new Saxon.Hej.trans.XmlProcessingException(e);
                    controller.getErrorReporter().report(error);
                }
                throw;
            } finally {
                if ((controller.getTraceListener() != null)) {
                    controller.getTraceListener().close();
                }
            }
        }
        public override void runUpdate(Saxon.Hej.query.DynamicQueryContext dynamicEnv, Saxon.Hej.query.UpdateAgent agent) {
            if (!(isUpdating)) {
                throw new Saxon.Hej.trans.XPathException("Calling runUpdate() on a non-updating query");
            }
            Saxon.Hej.Configuration config = executable.getConfiguration();
            Saxon.Hej.Controller controller = newController(dynamicEnv);
            Saxon.Hej.expr.XPathContextMajor context = initialContext(dynamicEnv, controller);
            try {
                Saxon.Hej.expr.PendingUpdateList pul = config.newPendingUpdateList();
                context.openStackFrame(stackFrameMap);
                /*Delegate.registerUpdates()*/expression.makeElaborator().elaborateForUpdate()(context, pul);
                pul.apply(context, mainModule.getRevalidationMode());
                foreach (Saxon.Hej.om.MutableNodeInfo mutableNodeInfo in pul.getAffectedTrees()) {
                    /*Delegate.update()*/agent(mutableNodeInfo, controller);
                }
            } catch (Saxon.Hej.trans.XPathException e) {
                if (!(e.hasBeenReported())) {
                    controller.getErrorReporter().report(new Saxon.Hej.trans.XmlProcessingException(e));
                }
                throw;
            } finally {
                if ((controller.getTraceListener() != null)) {
                    controller.getTraceListener().close();
                }
            }
        }
        public override void runStreamed(Saxon.Hej.query.DynamicQueryContext dynamicEnv, Saxon.Ejavax.xml.transform.Source source, Saxon.Ejavax.xml.transform.Result result, Saxon.Ejava.util.Properties outputProperties) {
            if ((inversion == null)) {
                throw new Saxon.Hej.trans.XPathException("Query was not compiled for streaming");
            }
            Saxon.Hej.Controller controller = newController(dynamicEnv);
            Saxon.Hej.expr.XPathContextMajor context = controller.newXPathContext();
            Saxon.Hej.@event.PipelineConfiguration pipe = controller.makePipelineConfiguration();
            pipe.setHostLanguage(Saxon.Api.HostLanguage.XQUERY);
            Saxon.Hej.@event.Receiver receiver;
            if ((result is Saxon.Hej.@event.Receiver)) {
                receiver = (Saxon.Hej.@event.Receiver)(result);
            } else {
                Saxon.Hej.lib.SerializerFactory sf = context.getConfiguration().getSerializerFactory();
                Saxon.Ejava.util.Properties actualProperties = validateOutputProperties(controller, outputProperties);
                receiver = sf.getReceiver(result, new Saxon.Hej.serialize.SerializationProperties(actualProperties), pipe);
            }
            receiver.open();
            Saxon.Eej.ee.stream.watch.WatchManager wm = new Saxon.Eej.ee.stream.watch.WatchManager(pipe);
            wm.setXPathContext(context);
            wm.setOutputter(new Saxon.Hej.@event.ComplexContentOutputter(receiver));
            Saxon.Eej.ee.stream.watch.Watch streamWatch = new Saxon.Eej.ee.stream.watch.StreamWatch(inversion, wm, context);
            wm.addWatch(streamWatch, true);
            Saxon.Eej.ee.stream.ContentDetector cd = new Saxon.Eej.ee.stream.ContentDetector(wm);
            context.openStackFrame(stackFrameMap);
            bool mustClose = ((result is Saxon.Ejavax.xml.transform.stream.StreamResult) && (((Saxon.Ejavax.xml.transform.stream.StreamResult)(result)).getOutputStream() == null));
            try {
                Saxon.Hej.@event.Sender.send(source, cd, controller.getConfiguration().getParseOptions());
            } catch (Saxon.Hej.trans.QuitParsingException e) {
            } catch (Saxon.Hej.trans.XPathException err) {
                controller.reportFatalError(err);
                throw;
            } finally {
                if ((controller.getTraceListener() != null)) {
                    controller.getTraceListener().close();
                }
            }
            receiver.close();
            if ((result is Saxon.Ejavax.xml.transform.stream.StreamResult)) {
                closeStreamIfNecessary((Saxon.Ejavax.xml.transform.stream.StreamResult)(result), mustClose);
            }
        }
        public override void explainPathMap() {
            Saxon.Hej.expr.parser.PathMap map = getPathMap();
            Saxon.Hej.lib.Logger pw = new Saxon.Callbacks.StandardLogger();
            pw.info("DOCUMENT PROJECTION: PATH MAP");
        }
    
    }
}