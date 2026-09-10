
package com.saxonica.ee.optim;

import com.saxonica.ee.bytecode.util.CompilerService;
import com.saxonica.ee.stream.*;
import com.saxonica.ee.stream.watch.StreamWatch;
import com.saxonica.ee.stream.watch.Watch;
import com.saxonica.ee.stream.watch.WatchManager;
import com.saxonica.ee.trans.ContextItemStaticInfoEE;
import net.sf.saxon.Configuration;
import net.sf.saxon.Controller;
import net.sf.saxon.event.ComplexContentOutputter;
import net.sf.saxon.event.PipelineConfiguration;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.event.Sender;
import net.sf.saxon.expr.*;
import net.sf.saxon.expr.instruct.GlobalContextRequirement;
import net.sf.saxon.expr.instruct.GlobalVariable;
import net.sf.saxon.expr.parser.ExpressionTool;
import net.sf.saxon.expr.parser.Optimizer;
import net.sf.saxon.expr.parser.PathMap;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.lib.Logger;
import net.sf.saxon.lib.SerializerFactory;
import net.sf.saxon.lib.StandardLogger;
import net.sf.saxon.om.MutableNodeInfo;
import net.sf.saxon.om.TreeModel;
import net.sf.saxon.query.DynamicQueryContext;
import net.sf.saxon.query.QueryModule;
import net.sf.saxon.query.UpdateAgent;
import net.sf.saxon.query.XQueryExpression;
import net.sf.saxon.s9api.HostLanguage;
import net.sf.saxon.serialize.SerializationProperties;
import net.sf.saxon.trans.QuitParsingException;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.trans.XmlProcessingException;
import net.sf.saxon.type.AnyItemType;
import net.sf.saxon.type.ItemType;

import javax.xml.transform.Result;
import javax.xml.transform.Source;
import javax.xml.transform.stream.StreamResult;
import java.util.*;

/**
 * Saxon-EE version of XQueryExpression. Supports additional EE features including
 * XQuery update, bytecode generation, document projection, streaming
 */


public class XQueryExpressionEE extends XQueryExpression {

    private boolean allowDocumentProjection;
    private final boolean isUpdating;
    private PathMap pathMap;
    private Inversion inversion;

    public XQueryExpressionEE(Expression exp, QueryModule mainModule, boolean streaming)
            throws XPathException {
        super(exp, mainModule, streaming);

        Configuration config = mainModule.getConfiguration();

        if (config.isLicensedFeature(Configuration.LicenseFeature.ENTERPRISE_XQUERY)) {
            final PackageData pd = mainModule.getPackageData();
            ExpressionTool.processExpressionTree(expression, null, (expr, result) -> {
                if (expr instanceof StreamInstr) {
                    Expression fallback = ((StreamInstr) expr).prepareForStreaming(pd);
                    if (fallback != expr) {
                        if (expr == expression) {
                            // the xsl:stream instruction is top-level
                            expression = fallback;
                        } else {
                            Expression parent = expr.getParentExpression();
                            for (Operand o : parent.operands()) {
                                if (o.getChildExpression() == expr) {
                                    o.setChildExpression(fallback);
                                }
                            }
                        }
                        return true;
                    }
                }
                return false;
            });
        }

        if (streaming && config.isLicensedFeature(Configuration.LicenseFeature.ENTERPRISE_XQUERY)) {
            if (!ExpressionTool.dependsOnFocus(exp)) {
                throw new XPathException("Streaming is not possible because the query does not use the context item");
            }
            Iterator<GlobalVariable> it = mainModule.getModuleVariables();
            while (it.hasNext()) {
                GlobalVariable gVar = it.next();
                if (gVar.getBody() != null && ExpressionTool.dependsOnFocus(gVar.getBody())) {
                    throw new XPathException("Streaming is not possible because a variable $" +
                                                     gVar.getObjectName().getDisplayName() + " depends on the context item");
                }
            }
            List<String> reasons = new ArrayList<>();
            ItemType contextItemType = AnyItemType.getInstance();
            GlobalContextRequirement req = mainModule.getExecutable().getGlobalContextRequirement();
            if (req != null) {
                contextItemType = req.getRequiredItemType();
            }
            ContextItemStaticInfoEE contextInfo = new ContextItemStaticInfoEE(contextItemType, false, Posture.STRIDING);
            PostureAndSweep ps = Streamability.getStreamability(expression, contextInfo, reasons);
            if (ps.getPosture() == Posture.ROAMING || ps.getSweep() == Sweep.FREE_RANGING) {
                StringBuilder fsb = new StringBuilder(256);
                fsb.append("Streaming is not possible. ");
                for (String r : reasons) {
                    fsb.append(r);
                    fsb.append(". ");
                }
                throw new XPathException(fsb.toString(), "XTSE3430");
            }
            inversion = Inversion.invertExpression(expression, false);
        }

        boolean isUpdatingExpression = mainModule.isUpdating() && expression.isUpdatingExpression();

        if (config.getConfigurationProperty(Feature.GENERATE_BYTE_CODE)
                && config.isLicensedFeature(Configuration.LicenseFeature.ENTERPRISE_XQUERY)
                && !streaming
                && !isUpdatingExpression) {

            Optimizer optimizer = config.obtainOptimizer();
//#if BYTECODE == true
            Expression cexp;
            if (config.getCountDown() == 0) {
                if (config.isTiming()) {
                    config.getLogger().info("Generating byte code...");
                }
                CompilerService compilerService = new CompilerService(config, HostLanguage.XQUERY);
                cexp = optimizer.compileToByteCode(compilerService, getExpression(), "main",
                                                   Expression.PROCESS_METHOD | Expression.ITERATE_METHOD);
            } else {
                optimizer.injectByteCodeCandidates(getExpression());
                cexp = optimizer.makeByteCodeCandidate(this, getExpression(),
                                                       "main", Expression.PROCESS_METHOD | Expression.ITERATE_METHOD);
            }
            if (cexp != null) {
                expression = cexp;
                ExpressionTool.copyLocationInfo(exp, expression);
            }
            //#endif
        }

        isUpdating = isUpdatingExpression;
    }

//    protected void processQuery(Outputter dest, XPathContext context) throws XPathException {
//        PushEvaluator pusher = Elaborator.make(expression).getPushEvaluator();
//        pusher.process(dest, context);
//    }

    /**
     * Ask whether this is an update query
     *
     * @return true if the body of the query is an updating expression
     * (as defined by the XQuery Update specification). Note that a query can use Update syntax
     * (notably the copy-modify syntax) without being an updating expression.
     */

    @Override
    public boolean isUpdateQuery() {
        return isUpdating;
    }

    /**
     * Indicate that document projection is or is not allowed
     *
     * @param allowed true if projection is allowed
     */

    @Override
    public void setAllowDocumentProjection(boolean allowed) {
        allowDocumentProjection = allowed;
    }

    /**
     * Ask whether document projection is allowed
     *
     * @return true if document projection is allowed
     */

    @Override
    public boolean isDocumentProjectionAllowed() {
        return allowDocumentProjection;
    }

    /**
     * Get the path map for the query expression
     *
     * @return the path map (which is constructed if this has not already been done)
     */

    public PathMap getPathMap() {
        if (pathMap == null) {
            pathMap = new PathMap(expression);
        }

        for (PackageData pack : getExecutable().getPackages()) {
            for (GlobalVariable var : pack.getGlobalVariableList()) {
                if (!var.isUnused()) {
                    Expression select = var.getBody();
                    if (select != null) {
                        select.addToPathMap(pathMap, null);
                    }
                }
            }
        }

        return pathMap;
    }


    /**
     * Get a controller that can be used to execute functions in this compiled query.
     * Functions in the query module can be found using {@link QueryModule#getUserDefinedFunction}.
     * They can then be called directly from the Java application using {@link net.sf.saxon.expr.instruct.UserFunction#call}
     * The same Controller can be used for a series of function calls. Note that the Controller should only be used
     * in a single thread.
     *
     * @return a newly constructed Controller
     */

    /*@NotNull*/
    @Override
    public Controller newController(DynamicQueryContext env) throws XPathException {
        Controller controller = new Controller(executable.getConfiguration(), executable);
        if (isUpdateQuery() && !controller.getModel().isMutable()) {
            controller.setModel(TreeModel.LINKED_TREE);
        }
        env.initializeController(controller);
        //controller.getBindery(expression.getPackageData()).allocateGlobals(executable.getGlobalVariableMap());

        if (allowDocumentProjection) {
            controller.setUseDocumentProjection(getPathMap());
        }
        return controller;
    }

    /**
     * Run an updating query
     *
     * @param dynamicEnv the dynamic context for query execution
     * @return a set of nodes representing the roots of trees that have been modified as a result
     * of executing the update. Note that this method will never modify persistent data on disk; it returns
     * the root nodes of the affected documents (which will often be document nodes whose document-uri can
     * be ascertained), and it is the caller's responsibility to decide what to do with them.
     * <p>On completion of this method it is generally unsafe to rely on the contents or relationships
     * of NodeInfo objects that were obtained before the updating query was run. Such objects may or may not
     * reflect the results of the update operations. This is especially true in the case of nodes that
     * are part of a subtree that has been deleted (detached from its parent). Instead, the new updated
     * tree should be accessed by navigation from the root nodes returned by this method.</p>
     * @throws XPathException if evaluation of the update query fails, or it this is not an updating query
     */

    /*@NotNull*/
    @Override
    public Set<MutableNodeInfo> runUpdate(/*@NotNull*/ DynamicQueryContext dynamicEnv) throws XPathException {
        if (!isUpdating) {
            throw new XPathException("Calling runUpdate() on a non-updating query");
        }

        Configuration config = executable.getConfiguration();
        Controller controller = newController(dynamicEnv);
        XPathContextMajor context = initialContext(dynamicEnv, controller);
        try {
            PendingUpdateList pul = config.newPendingUpdateList();
            context.openStackFrame(stackFrameMap);
            expression.evaluatePendingUpdates(context, pul);
            pul.apply(context, mainModule.getRevalidationMode());
            return pul.getAffectedTrees();
        } catch (XPathException e) {
            if (!e.hasBeenReported()) {
                XmlProcessingException error = new XmlProcessingException(e);
                controller.getErrorReporter().report(error);
            }
            throw e;
        } finally {
            if (controller.getTraceListener() != null) {
                controller.getTraceListener().close();
            }
        }
    }

    /**
     * Run an updating query, writing back eligible updated node to persistent storage.
     * <p>A node is eligible to be written back to disk if it is present in the document pool,
     * which generally means that it was originally read using the doc() or collection() function.</p>
     * <p>On completion of this method it is generally unsafe to rely on the contents or relationships
     * of NodeInfo objects that were obtained before the updating query was run. Such objects may or may not
     * reflect the results of the update operations. This is especially true in the case of nodes that
     * are part of a subtree that has been deleted (detached from its parent). Instead, the new updated
     * tree should be accessed by navigation from the root nodes returned by this method.</p>
     * <p>If one or more eligible updated nodes cannot be written back to disk, perhaps because the URI
     * identifies a non-updatable location, then an exception is thrown. In this case it is undefined</p>
     *
     * @param dynamicEnv the dynamic context for query execution
     * @param agent      a callback class that is called to process each document updated by the query
     * @throws XPathException if evaluation of the update query fails, or it this is not an updating query
     */

    @Override
    public void runUpdate(/*@NotNull*/ DynamicQueryContext dynamicEnv, /*@NotNull*/ UpdateAgent agent) throws XPathException {
        if (!isUpdating) {
            throw new XPathException("Calling runUpdate() on a non-updating query");
        }

        Configuration config = executable.getConfiguration();
        Controller controller = newController(dynamicEnv);
        XPathContextMajor context = initialContext(dynamicEnv, controller);
        try {
            PendingUpdateList pul = config.newPendingUpdateList();
            context.openStackFrame(stackFrameMap);
            expression.evaluatePendingUpdates(context, pul);
            pul.apply(context, mainModule.getRevalidationMode());
            for (MutableNodeInfo mutableNodeInfo : pul.getAffectedTrees()) {
                agent.update(mutableNodeInfo, controller);
            }
        } catch (XPathException e) {
            if (!e.hasBeenReported()) {
                controller.getErrorReporter().report(new XmlProcessingException(e));
            }
            throw e;
        } finally {
            if (controller.getTraceListener() != null) {
                controller.getTraceListener().close();
            }
        }
    }

    /**
     * Run the query in streaming mode, assuming it has been compiled for streaming
     *
     * @param dynamicEnv       the dynamic execution context
     * @param source           the input document, as a SAXSource or StreamSource
     * @param result           the destination for the query results
     * @param outputProperties serialization options for the query result
     * @throws XPathException if streamed evaluation of the query fails
     */

    @Override
    public void runStreamed(DynamicQueryContext dynamicEnv, Source source, Result result, Properties outputProperties) throws XPathException {
        if (inversion == null) {
            throw new XPathException("Query was not compiled for streaming");
        }


        Controller controller = newController(dynamicEnv);
        XPathContextMajor context = controller.newXPathContext();
        PipelineConfiguration pipe = controller.makePipelineConfiguration();
        pipe.setHostLanguage(HostLanguage.XQUERY);


        SerializerFactory sf = context.getConfiguration().getSerializerFactory();
        Properties actualProperties = validateOutputProperties(controller, outputProperties);


        Receiver receiver = sf.getReceiver(result, new SerializationProperties(actualProperties), pipe);
        //context.setReceiver(new ComplexContentOutputter(receiver));
//        String itemSeparator = actualProperties.getProperty(SaxonOutputKeys.ITEM_SEPARATOR);
//        if (receiver instanceof ComplexContentOutputter && itemSeparator != null && !"#absent".equals(itemSeparator)) {
//            receiver = new SequenceNormalizerWithItemSeparator(receiver, itemSeparator);
//            context.setReceiver(receiver);
//        }
        receiver.open();


        WatchManager wm = new WatchManager(pipe);
        wm.setXPathContext(context);
        wm.setOutputter(new ComplexContentOutputter(receiver));

        Watch streamWatch = new StreamWatch(inversion, wm, context);
        wm.addWatch(streamWatch, true);

        ContentDetector cd = new ContentDetector(wm);


        context.openStackFrame(stackFrameMap);

        boolean mustClose = result instanceof StreamResult &&
                ((StreamResult) result).getOutputStream() == null;

        // Run the query
        try {
            Sender.send(source, cd, controller.getConfiguration().getParseOptions());
        } catch (QuitParsingException e) {
            // all is well
        } catch (XPathException err) {
            controller.reportFatalError(err);
            throw err;
        } finally {
            if (controller.getTraceListener() != null) {
                controller.getTraceListener().close();
            }
        }

        receiver.close();
        if (result instanceof StreamResult) {
            closeStreamIfNecessary((StreamResult) result, mustClose);
        }
    }

    /**
     * Output the path map of the query for diagnostics
     */

    @Override
    public void explainPathMap() {
        PathMap map = getPathMap();
        Logger pw = new StandardLogger();
        pw.info("DOCUMENT PROJECTION: PATH MAP");
        //map.diagnosticDump(pw);
    }


}
// Copyright (c) 2018-2022 Saxonica Limited
